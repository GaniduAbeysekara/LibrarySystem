using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Data.Entities
{
    public class Book
    {
        [Key]
        [Required]    
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string BookTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty ;


    }
}
