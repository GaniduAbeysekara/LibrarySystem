namespace LibrarySystem.Web.API.Model
{
    public class BookForCreateDto
    {
        public string ISBN { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
