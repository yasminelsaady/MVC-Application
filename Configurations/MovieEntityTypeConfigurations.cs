using CRUD_Core_MVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRUD_Core_MVC.Configurations
{
    public class MovieEntityTypeConfigurations : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.Property(m => m.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(m => m.StoryLine)
                .IsRequired()
                .HasMaxLength(2500);

            builder.Property("Poster").IsRequired();
        }
    }
}
