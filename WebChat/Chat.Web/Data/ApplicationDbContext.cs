using Chat.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Chat.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ApplicationUser> AppUsers { get; set; }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);
        //    // Apply Configurations / IEntityTypeConfiguration<*Models*>
        //    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        //}
    }
}
