using LibrarySystem.Data.DbContexts;
using LibrarySystem.Data.Entities;
using LibrarySystem.Web.API.Model;
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
                if (keyword.Length < 3)
                {
                    return BadRequest(new { status = "error", message = "The search keyword must be at least 3 characters long." });
                }

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


        [HttpPost("createbook")]
        public async Task<IActionResult> Create([FromBody] BookForCreateDto bookForCreateDto)
        {
            var errorResponse = ValidateBook(bookForCreateDto);
            if (errorResponse != null)
            {
                return BadRequest(errorResponse);
            }

            try
            {
                var existingBookWithSameTitleAndAuthor = await _dataContext.Books
                    .FirstOrDefaultAsync(b => b.BookTitle == bookForCreateDto.BookTitle && b.Author == bookForCreateDto.Author && b.Description != bookForCreateDto.Description);

                if (existingBookWithSameTitleAndAuthor != null)
                {
                    return Conflict(new { status = "error", message = "Please change the book title." });
                }

                var existingBookWithSameISBN = await _dataContext.Books
                    .FirstOrDefaultAsync(b => b.ISBN == bookForCreateDto.ISBN);

                if (existingBookWithSameISBN != null)
                {
                    return Conflict(new { status = "error", message = "A book with the same ISBN already exists." });
                }

                var newBook = new Book
                {
                    ISBN = bookForCreateDto.ISBN,
                    BookTitle = bookForCreateDto.BookTitle,
                    Author = bookForCreateDto.Author,
                    Description = bookForCreateDto.Description,
                };

                _dataContext.Books.Add(newBook);
                await _dataContext.SaveChangesAsync();

                return StatusCode(201, new
                {
                    status = "success",
                    message = "Book created successfully.",
                    book = new
                    {
                        isbn = newBook.ISBN,
                        bookTitle = newBook.BookTitle,
                        author = newBook.Author,
                        description = newBook.Description
                    }
                });
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

            var (isValid, errorMessage) = ValidateISBN(isbn);
            if (!isValid)
            {
                return BadRequest(new { status = "error", message = errorMessage });
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

        [HttpPut("editbook/{isbn}")]
        public async Task<IActionResult> Edit(string isbn, [FromBody] BookForEditDto bookForEditDto)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return BadRequest(new { status = "error", message = "The ISBN parameter is required." });
            }

            var (isValid, errorMessage) = ValidateISBN(isbn);
            if (!isValid)
            {
                return BadRequest(new { status = "error", message = errorMessage });
            }

            try
            {
                var book = await _dataContext.Books.FirstOrDefaultAsync(u => u.ISBN == isbn);
                if (book == null)
                {
                    return NotFound(new { status = "error", message = "No book found matching the provided ISBN." });
                }

                var existingBookWithSameTitleAndAuthor = await _dataContext.Books
                    .FirstOrDefaultAsync(b => b.BookTitle == bookForEditDto.BookTitle && b.Author == bookForEditDto.Author && b.Description != bookForEditDto.Description);

                if (existingBookWithSameTitleAndAuthor != null)
                {
                    return Conflict(new { status = "error", message = "Please change the book title." });
                }

                bool isChanged = false;

                if (book.BookTitle != bookForEditDto.BookTitle)
                {
                    book.BookTitle = bookForEditDto.BookTitle;
                    isChanged = true;
                }

                if (book.Author != bookForEditDto.Author)
                {
                    book.Author = bookForEditDto.Author;
                    isChanged = true;
                }

                if (book.Description != bookForEditDto.Description)
                {
                    book.Description = bookForEditDto.Description;
                    isChanged = true;
                }

                if (!isChanged)
                {
                    return Ok(new { status = "success", message = "No changes were made." });
                }

                await _dataContext.SaveChangesAsync();
                return Ok(new { status = "success", message = "Book Updated successfully.", book });
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
                return (false, "The ISBN you provided is in an invalid format. Please ensure the ISBN is a 1 to 13-digit integer and try again.");
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(isbn, @"^0"))
            {
                return (false, "ISBN cannot start with 0.");
            }
            return (true, string.Empty);
        }

        private object ValidateBook(BookForCreateDto bookForCreateDto)
        {
            if (string.IsNullOrWhiteSpace(bookForCreateDto.ISBN))
            {
                return new { status = "error", message = "The ISBN field is required." };
            }

            var (isValid, errorMessage) = ValidateISBN(bookForCreateDto.ISBN);
            if (!isValid)
            {
                return new { status = "error", message = errorMessage };
            }

            if (string.IsNullOrWhiteSpace(bookForCreateDto.BookTitle))
            {
                return new { status = "error", message = "The Book Title field is required." };
            }

            if (string.IsNullOrWhiteSpace(bookForCreateDto.Author))
            {
                return new { status = "error", message = "The Author field is required." };
            }

            if (string.IsNullOrWhiteSpace(bookForCreateDto.Description))
            {
                return new { status = "error", message = "The Description field is required." };
            }

            return null;
        }

        private object ValidateBook(BookForEditDto bookForEditDto)
        {

            if (string.IsNullOrWhiteSpace(bookForEditDto.BookTitle))
            {
                return new { status = "error", message = "The Book Title field is required." };
            }

            if (string.IsNullOrWhiteSpace(bookForEditDto.Author))
            {
                return new { status = "error", message = "The Author field is required." };
            }

            if (string.IsNullOrWhiteSpace(bookForEditDto.Description))
            {
                return new { status = "error", message = "The Description field is required." };
            }

            return null;
        }
    }
}
