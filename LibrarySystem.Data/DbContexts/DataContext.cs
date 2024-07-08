using LibrarySystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data.DbContexts
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) :base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Auth> Auth => Set<Auth>();
       
        
        public DbSet<Book> Books => Set<Book>();

    }
}
