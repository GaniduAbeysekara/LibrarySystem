using LibrarySystem.Data.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;

namespace LibrarySystem.Data.DbContexts
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) :base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().HasKey(u => new { u.UserId, u.Email });

            builder.Entity<User>()
           .Property(u => u.Email)
           .HasConversion(
               email => email.ToLowerInvariant(),
               email => email
           );

            //Seed the first user
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = GetPasswordHash("Admin@123", passwordSalt);


            builder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "admin@email.com",
                    FirstName = "Admin",
                    LastName = "1",
                    Gender = "Male",
                    PhoneNumber = "0123456789",
                    IsDelete = false,
                    IsAdmin = true
                }
            );


            builder.Entity<Auth>().HasData(
                new Auth
                {
                    Email = "admin@email.com",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                }
            );
        }


        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = "8321n-0fd921-560f23q46v23^$F@@#^Cv4234i456ftg238C#$^VC4976t98fds" +
                Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256 / 8
            );
        }





        public DbSet<User> Users => Set<User>();
        public DbSet<Auth> Auth => Set<Auth>();
        public DbSet<Book> Books => Set<Book>();

    }
}
