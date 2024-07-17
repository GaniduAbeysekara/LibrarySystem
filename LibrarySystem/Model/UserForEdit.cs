﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Web.API.Model
{
    public class UserForEdit
    {
        [DisplayName("First Name")]
        public string FirstName { get; set; } = string.Empty;

        [DisplayName("Last Name")]
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;

        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
