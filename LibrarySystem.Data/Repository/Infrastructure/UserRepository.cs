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

        public async Task<bool> SaveChangersAsync()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }
        public async Task AddEntityAsync<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                await _dataContext.AddAsync(entityToAdd);
            }
        }
        public async Task RemoveEntityAsync<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _dataContext.Remove(entityToAdd);
                await Task.CompletedTask;
            }
        }
        public async Task<Auth> GetAuthByEmailAsync(string email)
        {
            Auth? auth = await _dataContext.Auth.Where(a => a.Email == email).FirstOrDefaultAsync<Auth>();
            return auth;
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            User? user = await _dataContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync<User>();
            return user;
        }
        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _dataContext.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync(string email)
        {
            return await _dataContext.Users.Where(a => a.Email != email).ToListAsync();
        }

        public IEnumerable<User> SearchUsersSpefically(User user, string email)
        {
            var query = _dataContext.Users.AsQueryable();

            //if (user.UserId.HasValue) // Assuming UserId is nullable int (int?)
            //    query = query.Where(u => u.UserId == user.UserId.Value);

            if (!string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName))
            {
                query = query.Where(u =>
                    (!string.IsNullOrEmpty(user.FirstName) && u.FirstName.Contains(user.FirstName) && u.Email != email) ||
                    (!string.IsNullOrEmpty(user.LastName) && u.LastName.Contains(user.LastName) && u.Email != email)
                );
            }

            if (!string.IsNullOrEmpty(user.Email))
                query = query.Where(u => u.Email.Contains(user.Email) && u.Email != email);

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                query = query.Where(u => u.PhoneNumber.Contains(user.PhoneNumber) && u.Email != email);


            if (!string.IsNullOrEmpty(user.Gender))
                query = query.Where(u => u.Gender.Contains(user.Gender) && u.Email != email);



            return query.ToList();
        }

    }

}
