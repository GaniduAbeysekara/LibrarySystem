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
        public async Task<IActionResult> Get(string isbn)
        {
            var book = await _dataContext.Books.FirstOrDefaultAsync(u => u.ISBN == isbn);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        //get book by author name

        //create book details

        //delete book

        //edit book details

    }
}