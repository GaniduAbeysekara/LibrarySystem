using LibrarySystem.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data.Repository.Interface
{
    public interface IUserRepository
    {
        public bool SaveChangers();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToAdd);
        public User GetUserByEmail(string userName);

        public User GetUserById(int Id);

        public Auth GetAuthByEmail(string email);

        List<User> SearchUsers(string searchTerm = null);


        public IEnumerable<User> SearchUsersSpefically(string firstName = null, string lastName = null, string email = null, string phoneNumber = null, string gender = null, int? userId = null);



    }
}