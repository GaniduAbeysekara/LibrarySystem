using AutoMapper;
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
        private IMapper _mapper;
        public UserController(DataContext dataContext)
        {
            _dataContext = dataContext;

            _mapper = new Mapper(new MapperConfiguration(cfg => {
                cfg.CreateMap<UserDetail, User>();
            }));
        }

        [HttpGet("{userName}")]
        public async Task<IActionResult> GetUser(string userName)
        {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null) 
            { 
                return NotFound();
            }
          return Ok(user);
        }


        [HttpPost("AddUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserDetail userDetail )
        {
            if (userDetail == null) {
                return BadRequest();
            }
            User userDb = _mapper.Map<User>(userDetail);
            //var user = new User();
            //user.UserName = userDetail.UserName;
            //user.Email = userDetail.Email;
            //user.Password = userDetail.Password;
            //user.PhonneNumber = userDetail.PhonneNumber;
            _dataContext.Users.Add(userDb);
            await _dataContext.SaveChangesAsync();

            return Ok(userDb);
        }


        [HttpPut("EditUser")]
        public async Task<IActionResult> EditUser(UserDetail user)
        {
            var userDb = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);

            if (userDb != null)
            {
                userDb.Email = user.Email;
                userDb.PhonneNumber = user.PhonneNumber;
                if (_dataContext.SaveChanges() > 0)
                {
                    return Ok(userDb);
                }

                throw new Exception("Failed to Update User");
            }

            throw new Exception("Failed to Get User");
        }


        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string userName)
        {
            var userDb = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (userDb != null)
            {
                _dataContext.Users.Remove(userDb);
                if (_dataContext.SaveChanges() > 0)
                {
                    return Ok();
                }

                throw new Exception("Failed to Delete User");
            }

            throw new Exception("Failed to Get User");
        }


    }


}
