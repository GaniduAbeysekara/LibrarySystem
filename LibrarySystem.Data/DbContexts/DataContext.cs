using LibrarySystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LibrarySystem.Data.DbContexts
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) :base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>().HasKey(u => new { u.UserId, u.Email });

            builder.Entity<User>()
           .Property(u => u.Email)
           .HasConversion(
               email => email.ToLowerInvariant(),
               email => email
           );
        }



        public DbSet<User> Users => Set<User>();
        public DbSet<Auth> Auth => Set<Auth>();
        public DbSet<Book> Books => Set<Book>();

    }
}
