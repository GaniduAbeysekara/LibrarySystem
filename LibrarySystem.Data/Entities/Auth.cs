using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Data.Entities
{
    public class Auth
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuthId { get; set; }

        [Required]
        [MaxLength(100)]
        [Key]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public byte[]? PasswordHash { get; set; }

        [Required]
        [MaxLength(500)]
        public byte[]? PasswordSalt { get; set; }
    }
}
