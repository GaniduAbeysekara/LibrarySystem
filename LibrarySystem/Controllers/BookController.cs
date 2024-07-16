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
        private readonly DataContext _dataContext;
        public BookController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // search books by book name, author name, or description
        [HttpGet("searchbook")]
        public async Task<IActionResult> SearchBooks()
        {
            string keyword = HttpContext.Request.Query["Search"].ToString();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                try
                {
                    var books = await _dataContext.Books.Where(b => b.ISBN.Contains(keyword) ||
                        b.BookTitle.Contains(keyword) ||
                        b.Author.Contains(keyword) ||
                        b.Description.Contains(keyword)).ToListAsync();

                    if (books == null || !books.Any())
                    {
                        return NotFound(new { status = "error", message = "No books found matching the keyword." });
                    }

                    return Ok(books);
                }
                catch (Exception)
                {
                    return StatusCode(500, new { status = "error", message = "Internal server error. Please try again later." });
                }
            }
            else
            {
                try
                {
                    var books = await _dataContext.Books.ToListAsync();
                    if (books == null || !books.Any())
                    {
                        return Ok(new { status = "error", message = "No books available." });
                    }
                    return Ok(books);
                }
                catch (Exception)
                {
                    return StatusCode(500, new { status = "error", message = "Internal server error. Please try again later." });
                }
            }
        }

        //create book
        [HttpPost("createbook")]
        public async Task<IActionResult> Create([FromBody] Book book)
        {
            if (string.IsNullOrWhiteSpace(book.ISBN))
            {
                return BadRequest(new { status = "error", message = "The ISBN field is required." });
            }

            var (isValid, errorMessage) = ValidateISBN(book.ISBN);
            if (!isValid)
            {
                return BadRequest(new { status = "error", message = errorMessage });
            }

            if (string.IsNullOrWhiteSpace(book.BookTitle))
            {
                return BadRequest(new { status = "error", message = "The Book Title field is required." });
            }
            if (string.IsNullOrWhiteSpace(book.Author))
            {
                return BadRequest(new { status = "error", message = "The Author field is required." });
            }

            try
            {
                var existingBook = await _dataContext.Books.FirstOrDefaultAsync(b => b.ISBN == book.ISBN);
                if (existingBook != null)
                {
                    return Conflict(new { status = "error", message = "A book with the same ISBN already exists." });
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

                return StatusCode(201, new { status = "success", book = newBook });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "Internal server error occurred." });
            }
        }

        //delete book
        [HttpDelete("deletebook/{isbn}")]
        public async Task<IActionResult> Delete(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return BadRequest(new { status = "error", message = "ISBN cannot be null or empty." });
            }

            try
            {
                var book = await _dataContext.Books.FirstOrDefaultAsync(u => u.ISBN == isbn);
                if (book == null)
                {
                    return NotFound(new { status = "error", message = "No book found matching the provided ISBN." });
                }

                _dataContext.Books.Remove(book);
                await _dataContext.SaveChangesAsync();

                return Ok(new { status = "success", message = "Book deleted successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "Internal server error occurred." });
            }
        }

        //edit book
        //edit book
        [HttpPut("editbook/{isbn}")]
        public async Task<IActionResult> Edit(string isbn, [FromBody] Book updatedBook)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return BadRequest(new { status = "error", message = "The ISBN parameter is required." });
            }

            var (isValid, errorMessage) = ValidateISBN(updatedBook.ISBN);
            if (!isValid)
            {
                return BadRequest(new { status = "error", message = errorMessage });
            }

            if (string.IsNullOrWhiteSpace(updatedBook.BookTitle))
            {
                return BadRequest(new { status = "error", message = "The Book Title field is required." });
            }
            if (string.IsNullOrWhiteSpace(updatedBook.Author))
            {
                return BadRequest(new { status = "error", message = "The Author field is required." });
            }
            if (string.IsNullOrWhiteSpace(updatedBook.Description))
            {
                return BadRequest(new { status = "error", message = "The Description field is required." });
            }

            try
            {
                var book = await _dataContext.Books.FirstOrDefaultAsync(u => u.ISBN == isbn);
                if (book == null)
                {
                    return NotFound(new { status = "error", message = "No books found matching the ISBN." });
                }

                // Ensure ISBN is not updated
                if (updatedBook.ISBN != isbn)
                {
                    return BadRequest(new { status = "error", message = "The ISBN field cannot be updated." });
                }

                // Check for changes
                bool isChanged = false;

                if (book.BookTitle != updatedBook.BookTitle)
                {
                    book.BookTitle = updatedBook.BookTitle;
                    isChanged = true;
                }

                if (book.Author != updatedBook.Author)
                {
                    book.Author = updatedBook.Author;
                    isChanged = true;
                }

                if (book.Description != updatedBook.Description)
                {
                    book.Description = updatedBook.Description;
                    isChanged = true;
                }

                if (!isChanged)
                {
                    return Ok(new { status = "success", message = "No changes were made." });
                }

                await _dataContext.SaveChangesAsync();
                return Ok(new { status = "success", book });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "Internal server error occurred." });
            }
        }


        private (bool isValid, string errorMessage) ValidateISBN(string isbn)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(isbn, @"^\d{1,13}$"))
            {
                return (false, "ISBN must be a number with 1 to 13 digits.");
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(isbn, @"^0"))
            {
                return (false, "ISBN cannot start with 0.");
            }
            return (true, string.Empty);
        }
    }
}
