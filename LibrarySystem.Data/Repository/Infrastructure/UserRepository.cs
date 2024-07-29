using LibrarySystem.Data.DbContexts;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repository.Interface;
using Microsoft.EntityFrameworkCore;


namespace LibrarySystem.Data.Repository.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private DataContext _dataContext;
        public UserRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public bool SaveChangers()
        {
            return _dataContext.SaveChanges() > 0;
        }
        
        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _dataContext.Add(entityToAdd);
            }
        }

        public void RemoveEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _dataContext.Remove(entityToAdd);
            }
        }

        public Auth GetAuthByEmail(string email)
        {
            Auth? auth = _dataContext.Auth.Where(a => a.Email == email).FirstOrDefault<Auth>();
            return auth;
        }

        public User GetUserByEmail(string email)
        {
            User? user = _dataContext.Users.Where(u => u.Email == email).FirstOrDefault<User>();
            return user;
        }


        public User GetUserById(int id)
        {
            User? user = _dataContext.Users.Where(u => u.UserId == id).FirstOrDefault<User>();
            return user;
        }


        public List<User> SearchUsers(string searchTerm = null)
        {
            // If no searchTerm is provided, return all users
            if (string.IsNullOrWhiteSpace(searchTerm))
            {               
                return _dataContext.Users.Where(a => a.IsAdmin == false).ToList();
            }

            // Normalize searchTerm
            searchTerm = searchTerm.Trim().ToLower();

            // Search based on provided searchTerm
            var query = _dataContext.Users.AsQueryable();

            query = query.Where(u => u.UserId.ToString() == searchTerm ||
                                     u.FirstName.ToLower().Contains(searchTerm) ||
                                     u.LastName.ToLower().Contains(searchTerm) ||
                                     u.PhoneNumber.Contains(searchTerm) ||
                                     u.Gender.ToLower().Contains(searchTerm) &&
                                     !u.IsAdmin);

            return query.ToList();
        }
    }

}
