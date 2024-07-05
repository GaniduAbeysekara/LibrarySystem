using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Web.API.Model
{
    public class UserForEdit
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string PhonneNumber { get; set; } = string.Empty;
    }
}
