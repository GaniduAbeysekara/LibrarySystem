using AutoMapper;
using LibrarySystem.DbContexts;
using LibrarySystem.Entities;
using LibrarySystem.Model;
using LibrarySystem.Repository.Infrastructure;
using LibrarySystem.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController:ControllerBase
    {
        private IUserRepository _userRepository;
        private IMapper _mapper;
        public UserController( IUserRepository userRepository)
        {
            _userRepository = userRepository;

            _mapper = new Mapper(new MapperConfiguration(cfg => {
                cfg.CreateMap<UserDetail, User>();
            }));
        }

        [HttpGet("{userName}")]
        public User GetUser(string userName)
        {
            return _userRepository.GetSingleUser(userName);

        }


        [HttpPost("AddUser")]
        public IActionResult CreateUser([FromBody] UserDetail userDetail )
        {
            if (userDetail == null) {
                return BadRequest();
            }
            User userDb = _mapper.Map<User>(userDetail);
            _userRepository.AddEntity<User>(userDb);
            _userRepository.SaveChangers();

            return Ok(userDb);
        }


        [HttpPut("EditUser")]
        public IActionResult EditUser(UserDetail user)
        {
            var userDb = _userRepository.GetSingleUser(user.UserName);

            userDb.Email = user.Email;
            userDb.PhonneNumber = user.PhonneNumber;
            if (_userRepository.SaveChangers())
            {
                return Ok(userDb);
            }

            throw new Exception("Failed to Update User");
        }


        [HttpDelete("DeleteUser")]
        public IActionResult DeleteUser(string userName)
        {
            var userDb = _userRepository.GetSingleUser(userName);

                _userRepository.RemoveEntity<User>(userDb);
                if (_userRepository.SaveChangers())
                {
                    return Ok();
                }

                throw new Exception("Failed to Delete User");
        }


    }


}
