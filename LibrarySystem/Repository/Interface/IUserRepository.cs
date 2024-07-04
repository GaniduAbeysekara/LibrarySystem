using LibrarySystem.Entities;

namespace LibrarySystem.Repository.Interface
{
    public interface IUserRepository
    {
        public bool SaveChangers();
        public  void SaveChangesAsync();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToAdd);
        public User GetUserByEmail(string userName);
        public Auth GetAuthByEmail(string email);
    }
}
