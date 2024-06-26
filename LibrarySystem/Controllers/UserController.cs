using LibrarySystem.DbContexts;
using LibrarySystem.Entities;
using LibrarySystem.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController:ControllerBase
    {

        private DataContext _dataContext;
        public UserController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet("{userName}")]
        public async Task<IActionResult> Get(string userName)
        {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null) { 
            return NotFound();
            }
          return Ok(user);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserDetail userDetail )
        {
            if (userDetail == null) {
                return BadRequest();
            }
            var user = new User();
            user.UserName = userDetail.UserName;
            user.Email = userDetail.Email;
            user.Password = userDetail.Password;
            user.UserType = 1;
            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();

            return Ok(user);


        }


    }


}
