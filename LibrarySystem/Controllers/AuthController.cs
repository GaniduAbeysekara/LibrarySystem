using AutoMapper;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repository.Interface;
using LibrarySystem.Web.API.Model;
using LibrarySystem.Web.API.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LibrarySystem.Web.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private IUserRepository _userRepository;
        private IAuthService _authService;
        private IMapper _mapper;
        private readonly IConfiguration _config;
        public AuthController(IUserRepository userRepository, IConfiguration config,IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService; 

            _mapper = new Mapper(new MapperConfiguration(cfg => {
                cfg.CreateMap<UserForRegistrationDto, User>();
            }));

            _config = config;
        }


        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            var validationResult = _authService.ValidateObjectNotNullOrEmpty(userForRegistration);
            if (validationResult != null)
            {
                return validationResult;
            }
            bool isValidEmail = _authService.IsValidEmail(userForRegistration.Email);
            bool isValidPhoneNum = _authService.IsValidPhoneNo(userForRegistration.PhonneNumber);
            if (isValidEmail && isValidPhoneNum)
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

                            byte[] passwordHash = _authService.GetPasswordHash(userForRegistration.Password, passwordSalt);

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
                                    return StatusCode(201, new { status = "success", message = "User created successfully." });
                                }
                                return StatusCode(500, new { status = "error", message = "Failed to register user." });
                            }
                            return StatusCode(500, new { status = "error", message = "Failed to register user." });
                        }
                        return BadRequest(new { status = "error", message = "User with this email already exists." });
                    }
                    return BadRequest(new { status = "error", message = "Passwords do not match!" });
                }
                return BadRequest(new { status = "error", message = "Please enter a valid Email Address and Phone Number!" });
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            var validationResult = _authService.ValidateObjectNotNullOrEmpty(userForLogin);
            if (validationResult != null)
            {
                return validationResult;
            }
            bool isValidEmail = _authService.IsValidEmail(userForLogin.Email);
            if (isValidEmail != true)
            {
                return BadRequest(new { status = "error", message = "Please enter a valid Email Address" });
            }

            Auth userForConfirmation = _userRepository.GetAuthByEmail(userForLogin.Email);
            if (userForConfirmation == null)
            {
                return BadRequest(new { status = "error", message = "This email is not in the database" });
            }

            if (userForConfirmation.PasswordHash != null && userForConfirmation.PasswordSalt != null)
            {
                byte[] passwordHash = _authService.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

                for (int index = 0; index < passwordHash.Length; index++)
                {
                    if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                    {
                        return StatusCode(401, new { status = "success", message = "Incorrect password!" });
                    }
                }
            }
            return Ok(
                new
                {
                    status = "success",
                    message = "Logged in Successfully",
                    token = _authService.CreateToken(userForLogin.Email)
                });
        }


        [HttpGet("GetLoggedInUserDetails")]
        public User GetLoggedInUserDetails()
        {
            var accessToken = HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token").Result;
            var email = _authService.GetUserFromToken(accessToken);
            var userDb = _userRepository.GetUserByEmail(email);

            return userDb;

        }

        [HttpGet("{email}")]

        public User GetUser(string email)
        {


            if (_authService.IsValidEmail(email))
            {
                User user = _userRepository.GetUserByEmail(email);
                if (user != null)
                {
                    return user;

                }
            }

            throw new Exception("There is not email as " + email);

        }



        [HttpPut("EditUser")]
        public IActionResult EditUser(UserForEdit userForEdit)
        {
            var accessToken = HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token").Result;
            var email = _authService.GetUserFromToken(accessToken);
            var userDb = _userRepository.GetUserByEmail(email);

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
            var accessToken = HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token").Result;
            if ( _authService.GetUserFromToken(accessToken) != email)
            {
                var userDb = _userRepository.GetUserByEmail(email);
                var authdb = _userRepository.GetAuthByEmail(email);

                _userRepository.RemoveEntity<User>(userDb);
                _userRepository.RemoveEntity<Auth>(authdb);
                if (_userRepository.SaveChangers())
                {
                    return Ok(new { status = "success", message = "User created successfully." });
                }

                return BadRequest(new { status = "error", message = "Failed to Delete User" });
            }
            return BadRequest(new { status = "error", message = "Unable to delete account. You cannot delete your own account." });
        }

        
    }
}