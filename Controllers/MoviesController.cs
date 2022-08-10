using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CRUD_Core_MVC.Models;
using CRUD_Core_MVC.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace CRUD_Core_MVC.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;

        private List<string> _allowedExtentions = new List<string> { ".jpg", ".png" };
        long _maxAllowedPosterSize = 1048576;

        public MoviesController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        public  async Task<ActionResult> Index()  
        {
            var movies = await _context.Movies.OrderByDescending(m => m.Rate).ToListAsync();
            return View(movies);
        }
        public async Task<ActionResult> Create()
        {
            var viewmodel = new MovieFormViewModel
            {
                Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync()
            };

            return View("Form View",viewmodel);
        }

        [HttpPost] 
        public async Task<ActionResult> Create(MovieFormViewModel model)
        {
            if(!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("Form View", model);
            }

            var files = Request.Form.Files;

            if (!files.Any())       //if poster == null
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Please select movie poster ");
                return View("Form View",model);
            }

            var poster = files.FirstOrDefault();
            //

            if (!_allowedExtentions.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "only .png, .jpg image are allowed");
                return View("Form View",model);
            }
            if(poster.Length > _maxAllowedPosterSize)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster","Poster connot be more than !MB");
                return View("Form View", model);
            }
            
            using var dataStream = new MemoryStream();
            await poster.CopyToAsync(dataStream);

            var movie = new Movie
            {
                Title = model.Title,
                GenreId = model.GenreId,
                Year = model.Year,
                Rate = model.Rate,
                StoryLine = model.StoryLine,
                Poster = dataStream.ToArray()
            };
            _context.Movies.Add(movie);
            _context.SaveChanges();

            _toastNotification.AddSuccessToastMessage("Movie created successfully");
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)  
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();

            var viewmodel = new MovieFormViewModel
            {
                Id = movie .Id,
                Title = movie.Title,
                Year = movie.Year,
                Rate = movie.Rate,
                GenreId = movie.GenreId,
                StoryLine =movie.StoryLine,
                Poster = movie.Poster,
                Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync()
            };

            return View("Form View",viewmodel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MovieFormViewModel viewmodel)
        {
            if (!ModelState.IsValid)
            {
                viewmodel.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("Form View", viewmodel);
            }

            var movie = await _context.Movies.FindAsync(viewmodel.Id);

            var files = Request.Form.Files;
            if (files.Any())
            {
                var poster = files.FirstOrDefault();
                using var dataStream = new MemoryStream();
                await poster.CopyToAsync(dataStream);

                viewmodel.Poster = dataStream.ToArray();

                if (!_allowedExtentions.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    viewmodel.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "only .png, .jpg image are allowed");
                    return View("Form View", viewmodel);
                }
                if (poster.Length > _maxAllowedPosterSize)
                {
                    viewmodel.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster connot be more than !MB");
                    return View("Form View", viewmodel);
                }

                movie.Poster = viewmodel.Poster;
            }
            movie.Title = viewmodel.Title;
            movie.Year = viewmodel.Year;
            movie.Rate = viewmodel.Rate;
            movie.StoryLine = viewmodel.StoryLine;
            movie.GenreId = viewmodel.GenreId;            

            _context.SaveChanges();


            _toastNotification.AddSuccessToastMessage("Movie updated succssefully");
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();
            var movie = await _context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return Ok();
        }
    }
}
