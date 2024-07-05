using LibrarySystem.DbContexts;
using LibrarySystem.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

        // get all books
        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                var books = await _dataContext.Books.ToListAsync();
                if (books == null || !books.Any())
                {
                    return Ok("No books available.");
                }
                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // search books by book name or author name
        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks([Required] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest("Keyword cannot be null or empty.");
            }

            try
            {
                var books = await _dataContext.Books
                    .Where(b => b.BookTitle.Contains(keyword) || b.Author.Contains(keyword))
                    .ToListAsync();

                if (books == null || !books.Any())
                {
                    return NotFound("No books found matching the keyword.");
                }

                return Ok(books);
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "Error searching books with keyword: {keyword}", keyword);
                return StatusCode(500, "Internal server error. Please try again later.");
            }
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