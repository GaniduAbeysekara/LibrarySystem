using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Entities
{
    public class Book
    {
        [Key]
        public string ISBN { get; set; }

        [Required]
        [MaxLength(100)]
        public string BookTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;

        //[Required]
        //[MaxLength(100)]
        //public string Description { get; set; } = string.Empty ;

        //[Required]
        //[MaxLength(100)]
        //public string CategoryId { get; set; } = string.Empty;


    }
}
