using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Web.API.Model
{
    public class UserForRegistrationDto
    {
        //[EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        [DisplayName("Password Confirm")]
        public string PasswordConfirm { get; set; } = string.Empty;

        [DisplayName("First Name")]
        public string FirstName { get; set; } = string.Empty;

        [DisplayName("Last Name")]
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;

        [DisplayName("Phonne Number")]
        public string PhonneNumber { get; set; } = string.Empty;
    }
}
