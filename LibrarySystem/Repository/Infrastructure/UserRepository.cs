using AutoMapper;
using LibrarySystem.DbContexts;
using LibrarySystem.Entities;
using LibrarySystem.Repository.Interface;

namespace LibrarySystem.Repository.Infrastructure
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

        public async void SaveChangesAsync()
        {
            await _dataContext.SaveChangesAsync();
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
            User? user = _dataContext.User.Where(u => u.Email == email).FirstOrDefault<User>();
            return user;
        }


    }

}
