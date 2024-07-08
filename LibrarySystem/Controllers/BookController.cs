using LibrarySystem.Data.DbContexts;
using LibrarySystem.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Web.API.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> GetBooksByAuthor(string author)
        {
            var books = await _dataContext.Books.Where(u => u.Author == author).ToListAsync();
            if (books == null || books.Count == 0)
            {
                return NotFound();
            }
            return Ok(books);
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
        [HttpDelete("{isbn}")]
        public async Task<IActionResult> Delete(string isbn)
        {
            var book = await _dataContext.Books.FirstOrDefaultAsync(u => u.ISBN == isbn);
            if (book == null)
            {
                return NotFound();
            }

            _dataContext.Books.Remove(book);
            await _dataContext.SaveChangesAsync();
            return NoContent();
        }

        //edit book details
        [HttpPut("{isbn}")]
        public async Task<IActionResult> Edit(string isbn, [FromBody] Book updatedBook)
        {
            var book = await _dataContext.Books.FirstOrDefaultAsync(u => u.ISBN == isbn);
            if (book == null)
            {
                return NotFound();
            }

            book.BookTitle = updatedBook.BookTitle;
            book.Author = updatedBook.Author;

            await _dataContext.SaveChangesAsync();
            return NoContent();
        }
    }
}