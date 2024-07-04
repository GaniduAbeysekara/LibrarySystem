using LibrarySystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.DbContexts
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) :base(options) { }

        public DbSet<User> User => Set<User>();
        public DbSet<Auth> Auth => Set<Auth>();
       
    }
}
