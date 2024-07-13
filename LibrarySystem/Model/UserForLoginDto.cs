using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Web.API.Model
{
    public class UserForLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool isAdmin { get;  }
             
        public UserForLoginDto()
        {
            if (Email == null)
            {
                Email = "";
            }
            if (Password == null)
            {
                Password = "";
            }
        }
    }
}
