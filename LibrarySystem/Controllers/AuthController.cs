using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using LibrarySystem.DbContexts;
using LibrarySystem.Entities;
using LibrarySystem.Model;
using LibrarySystem.Repository.Infrastructure;
using LibrarySystem.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private IUserRepository _userRepository;
        private IMapper _mapper;
        private readonly IConfiguration _config;
        public AuthController(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;

            _mapper = new Mapper(new MapperConfiguration(cfg => {
                cfg.CreateMap<UserForRegistrationDto, User>();
            }));

            _config = config;
        }


        [HttpGet("{email}")]
        public User GetUser(string email)
        {
            User user = _userRepository.GetUserByEmail(email);
            if (user != null)
            {
                return user;   
            
            }
            throw new Exception("There is not email as " + email);

        }

        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                User existingUsers = _userRepository.GetUserByEmail(userForRegistration.Email);
                if (existingUsers == null)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);

                    Auth auth = new Auth();
                    auth.PasswordHash = passwordHash;
                    auth.PasswordSalt = passwordSalt;
                    auth.Email = userForRegistration.Email;
                    _userRepository.AddEntity<Auth>(auth);

                    if (_userRepository.SaveChangers())
                    {

                        User userDb = _mapper.Map<User>(userForRegistration);
                        _userRepository.AddEntity<User>(userDb);
                        if (_userRepository.SaveChangers())
                        {
                            return Ok();
                        }
                        return StatusCode(500, "Failed to add user.");
                    }
                    return StatusCode(500, "Failed to register user.");
                }
                return StatusCode(500, "User with this email already exists!");
            }
            return StatusCode(500, "Passwords do not match!");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            Auth userForConfirmation = _userRepository.GetAuthByEmail(userForLogin.Email);

            if(userForConfirmation.PasswordHash != null)
            {
                byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

                for (int index = 0; index < passwordHash.Length; index++)
                {
                    if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                    {
                        return StatusCode(401, "Incorrect password!");
                    }
                }
            }
                return Ok();
        }

        [HttpPut("EditUser")]
        public IActionResult EditUser(UserForEdit userForEdit)
        {
            var userDb = _userRepository.GetUserByEmail(userForEdit.Email);

            if (userDb != null)
            {
                userDb.PhonneNumber = userForEdit.PhonneNumber;
                userDb.FirstName = userForEdit.FirstName;
                userDb.LastName = userForEdit.LastName; 
                userDb.Gender = userForEdit.Gender;

                if (_userRepository.SaveChangers())
                {
                    return Ok(userDb);
                }

                return StatusCode(500, "Failed to Update User");
            }
            return StatusCode(500, "User feilds ar null");
        }

        [HttpDelete("DeleteUser")]
        public IActionResult DeleteUser(string email)
        {
            var userDb = _userRepository.GetUserByEmail(email);
            var authdb = _userRepository.GetAuthByEmail(email);

            _userRepository.RemoveEntity<User>(userDb);
            _userRepository.RemoveEntity<Auth>(authdb);
            if (_userRepository.SaveChangers())
            {
                return Ok();
            }

            return StatusCode(500,"Failed to Delete User");
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256 / 8
            );
        }

    }
}