using AutoMapper;
using LibrarySystem.DbContexts;
using LibrarySystem.Entities;
using LibrarySystem.Repository.Interface;

namespace LibrarySystem.Repository.Infrastructure
{
    public class UserRepository:IUserRepository
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

        public User GetSingleUser(string userName)
        {
            User? user = _dataContext.Users
                .Where(u => u.UserName == userName)
                .FirstOrDefault<User>();

            if (user != null)
            {
                return user;
            }

            throw new Exception("There is not user as " + userName);

        }















    }

}
