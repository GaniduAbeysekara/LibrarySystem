using LibrarySystem.DbContexts;
using LibrarySystem.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private DataContext _dataContext;
        public BookController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet("{isbn}")]
        public async Task<IActionResult> GetBookByISBN(string isbn)
        {
            var book = await _dataContext.Books.FirstOrDefaultAsync(u => u.ISBN == isbn);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        //get book by author name
        [HttpGet("author/{author}")]
        public async Task<IActionResult> GetBookByAuthor(string author)
        {
            var book = await _dataContext.Books.FirstOrDefaultAsync(u => u.Author == author);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }


        //create book details
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Book book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            var newBook = new Book
            {
                ISBN = book.ISBN,
                BookTitle = book.BookTitle,
                Author = book.Author,
            };

            _dataContext.Books.Add(newBook);
            await _dataContext.SaveChangesAsync();

            return Ok(newBook);
        }

        //delete book

        //edit book details

    }
}