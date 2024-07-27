using LibrarySystem.Data.Entities;

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
    }
}
