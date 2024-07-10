using LibrarySystem.Data.DbContexts;
using LibrarySystem.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

        // search books by book name, author name, or description
        [HttpGet("searchbook")]
        public async Task<IActionResult> SearchBooks([Required] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest("Keyword cannot be null or empty.");
            }

            try
            {
                var books = await _dataContext.Books
                    .Where(b => b.BookTitle.Contains(keyword) || b.Author.Contains(keyword) || b.Description.Contains(keyword))
                    .ToListAsync();

                if (books == null || !books.Any())
                {
                    return NotFound("No books found matching the keyword.");
                }

                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }


        //create book
        [HttpPost("createbook")]
        public async Task<IActionResult> Create([FromBody] Book book)
        {
            if (string.IsNullOrWhiteSpace(book.ISBN))
            {
                return BadRequest("The ISBN field is required.");
            }
            if (string.IsNullOrWhiteSpace(book.BookTitle))
            {
                return BadRequest("The Book Title field is required.");
            }
            if (string.IsNullOrWhiteSpace(book.Author))
            {
                return BadRequest("The Author field is required.");
            }

            try
            {
                var existingBook = await _dataContext.Books.FirstOrDefaultAsync(b => b.ISBN == book.ISBN);
                if (existingBook != null)
                {
                    return Conflict("A book with the same ISBN already exists.");
                }

                var newBook = new Book
                {
                    ISBN = book.ISBN,
                    BookTitle = book.BookTitle,
                    Author = book.Author,
                    Description = book.Description,
                };

                _dataContext.Books.Add(newBook);
                await _dataContext.SaveChangesAsync();

                return StatusCode(201, newBook); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error occurred.");
            }
        }

        //delete book
        [HttpDelete("deletebook/{isbn}")]
        public async Task<IActionResult> Delete(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return BadRequest("ISBN cannot be null or empty.");
            }

            try
            {
                var book = await _dataContext.Books.FirstOrDefaultAsync(u => u.ISBN == isbn);
                if (book == null)
                {
                    return NotFound("No book found matching the provided ISBN.");
                }

                _dataContext.Books.Remove(book);
                await _dataContext.SaveChangesAsync();

                return Ok(new { status = 204, message = "Book deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error occurred.");
            }
        }



        //edit book
        [HttpPut("editbook/{isbn}")]
        public async Task<IActionResult> Edit(string isbn, [FromBody] Book updatedBook)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return BadRequest("The ISBN parameter is required.");
            }
            if (string.IsNullOrWhiteSpace(updatedBook.BookTitle))
            {
                return BadRequest("The Book Title field is required.");
            }
            if (string.IsNullOrWhiteSpace(updatedBook.Author))
            {
                return BadRequest("The Author field is required.");
            }
            if (string.IsNullOrWhiteSpace(updatedBook.Description))
            {
                return BadRequest("The Description field is required.");
            }

            try
            {
                var book = await _dataContext.Books.FirstOrDefaultAsync(u => u.ISBN == isbn);
                if (book == null)
                {
                    return NotFound("No books found matching the ISBN.");
                }

                // Ensure ISBN is not updated
                if (updatedBook.ISBN != isbn)
                {
                    return BadRequest("The ISBN field cannot be updated.");
                }

                // Update other fields
                book.BookTitle = updatedBook.BookTitle;
                book.Author = updatedBook.Author;
                book.Description = updatedBook.Description;

                await _dataContext.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error occurred.");
            }
        }


    }
}