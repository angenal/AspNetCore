using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace WebDotnetCore.db.sqlservr.yz.Models
{
    public class YzDbContext : DbContext
    {
        public YzDbContext(DbContextOptions<YzDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// 初始化表Movies
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void Initialize(IServiceScope scope)
        {
            var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<YzDbContext>>();
            using (var context = new YzDbContext(options))
            {
                if (context.Movies.Any() == false)
                {
                    context.Movies.AddRange(
                        new Movie
                        {
                            Title = "When Harry Met Sally",
                            ReleaseDate = DateTime.Parse("1989-1-11"),
                            Genre = "Romantic Comedy",
                            Price = 7.99
                        },
                        new Movie
                        {
                            Title = "Ghostbusters ",
                            ReleaseDate = DateTime.Parse("1984-3-13"),
                            Genre = "Comedy",
                            Price = 8.99
                        },
                        new Movie
                        {
                            Title = "Ghostbusters 2",
                            ReleaseDate = DateTime.Parse("1986-2-23"),
                            Genre = "Comedy",
                            Price = 9.99
                        },
                        new Movie
                        {
                            Title = "Rio Bravo",
                            ReleaseDate = DateTime.Parse("1959-4-15"),
                            Genre = "Western",
                            Price = 3.99
                        }
                    );
                    context.SaveChanges();
                }
            }
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieSchedule> MovieSchedules { get; set; }

    }
}
