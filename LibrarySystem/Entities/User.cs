using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

<<<<<<< Updated upstream:LibrarySystem/Entities/User.cs
=======
        [Key]
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
>>>>>>> Stashed changes:LibrarySystem.Data/Entities/User.cs
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Password { get; set; } = string.Empty;

        [Required]
<<<<<<< Updated upstream:LibrarySystem/Entities/User.cs
        [MaxLength(5)]
        public int UserType { get; set; }

=======
        [MaxLength(50)]
        [DataType(DataType.PhoneNumber)]
        public string PhonneNumber { get; set; } = string.Empty;
>>>>>>> Stashed changes:LibrarySystem.Data/Entities/User.cs

    }
}
