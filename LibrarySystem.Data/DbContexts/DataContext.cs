using LibrarySystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data.DbContexts
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) :base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>().HasKey(u => new { u.UserId, u.Email });
        }



        public DbSet<User> Users => Set<User>();
        public DbSet<Auth> Auth => Set<Auth>();
        public DbSet<Book> Books => Set<Book>();

    }
}
