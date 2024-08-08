using LibrarySystem.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data.Repository.Interface
{
    public interface IUserRepository
    {
        public  Task<bool> SaveChangersAsync();
        public Task AddEntityAsync<T>(T entityToAdd);
        public Task RemoveEntityAsync<T>(T entityToAdd);
        public Task<Auth> GetAuthByEmailAsync(string email);
        public Task<User> GetUserByEmailAsync(string email);
        public Task<User> GetUserByIdAsync(int id);
        public Task<IEnumerable<User>> GetAllUsersAsync(string email);
        public IEnumerable<User> SearchUsersSpefically(User user, string email);

    }
}