using LibrarySystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.DbContexts
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) :base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Book> Books => Set<Book>();

    }
}
