using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Model
{
    public class UserDetail
    {
        
        public string UserName { get; set; } = string.Empty;

       
        public string Email { get; set; } = string.Empty;

        
        public string Password { get; set; } = string.Empty;

        public string PhonneNumber { get; set; } = string.Empty;
    }
}
