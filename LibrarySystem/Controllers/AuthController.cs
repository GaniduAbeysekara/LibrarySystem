﻿using AutoMapper;
using LibrarySystem.Data.DbContexts;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repository.Interface;
using LibrarySystem.Web.API.Model;
using LibrarySystem.Web.API.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

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
        private DataContext _dataContext;

        public AuthController(IUserRepository userRepository, IConfiguration config,IAuthService authService, DataContext dataContext)
        {
            _userRepository = userRepository;
            _authService = authService;
            _dataContext = dataContext;

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
            if (!isValidEmail )
            {
                var emailCheck = userForRegistration.Email.ToString();

                if (emailCheck.ToLower() != emailCheck)
                {
                    return BadRequest(new { status = "error", message = "Email contains uppercase letters!" }); // Email contains uppercase letters
                }

                return BadRequest(new { status = "error", message = "Please enter a valid Email Address!" });
            }

            bool isValidPhoneNum = _authService.IsValidPhoneNo(userForRegistration.PhoneNumber);
            if (!isValidPhoneNum)
            {
                return BadRequest(new { status = "error", message = "Please enter a valid Phone Number!" });
            }

            bool isValidPass = _authService.isValidPassword(userForRegistration.Password);
            if (!isValidPass)
            {
                return BadRequest(new { status = "error", message = "Password must be atleast 8 to 15 characters. It contains atleast one Upper case,Lower Case,numbers and Special Characters." });
            }

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
                    auth.Email = userForRegistration.Email.ToLower();
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

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {

            User UserDetails = _userRepository.GetUserByEmail(userForLogin.Email);

            var validationResult = _authService.ValidateObjectNotNullOrEmpty(userForLogin);
            if (validationResult != null)
            {
                return validationResult;
            }
            bool isValidEmail = _authService.IsValidEmail(userForLogin.Email);
            if (isValidEmail != true)
            {
                var emailCheck = userForLogin.Email.ToString();

                if (emailCheck.ToLower() != emailCheck)
                {
                    return BadRequest(new { status = "error", message = "Email contains uppercase letters!" }); // Email contains uppercase letters
                }
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
                    token = _authService.CreateToken(userForLogin.Email, UserDetails.IsAdmin)

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

        [HttpPut("EditUser")]

        public IActionResult EditUser(UserForEdit userForEdit)
        {
            var accessToken = HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token").Result;
            var email = _authService.GetUserFromToken(accessToken);
            var userDb = _userRepository.GetUserByEmail(email);

            var validationResult = _authService.ValidateObjectNotNullOrEmpty(userForEdit);
            if (validationResult != null)
            {
                return validationResult;
            }

            bool isValidPhoneNum = _authService.IsValidPhoneNo(userForEdit.PhoneNumber);
            if (!isValidPhoneNum)
            {
                return BadRequest(new { status = "error", message = "Please enter a valid Phone Number!" });
            }

            if (userDb != null)
            {
                // List to track updated fields
                var updatedFields = new List<string>();

                if (userDb.FirstName != userForEdit.FirstName)
                {
                    userDb.FirstName = userForEdit.FirstName;
                    updatedFields.Add(nameof(userDb.FirstName));
                }

                if (userDb.LastName != userForEdit.LastName)
                {
                    userDb.LastName = userForEdit.LastName;
                    updatedFields.Add(nameof(userDb.LastName));
                }

                if (userDb.PhoneNumber != userForEdit.PhoneNumber)
                {
                    userDb.PhoneNumber = userForEdit.PhoneNumber;
                    updatedFields.Add(nameof(userDb.PhoneNumber));
                }

                if (userDb.Gender != userForEdit.Gender)
                {
                    userDb.Gender = userForEdit.Gender;
                    updatedFields.Add(nameof(userDb.Gender));
                }
                
                var responseMessage = updatedFields.Count > 0 ? $"Updated fields: {string.Join(", ", updatedFields)}"
                                            : "No fields were updated.";

                if (_userRepository.SaveChangers())
                {
                    return Ok(new { status = "success", message = responseMessage , userForEdit });
                }
                return BadRequest(new { status = "error", message = responseMessage });
            }
            return StatusCode(500, new { status = "error", message = "This user does not exist" });
        }


        [HttpDelete("DeleteUser")]
        public IActionResult DeleteUser(string email)
        {
            var accessToken = HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token").Result;
            var userStatus = _authService.GetStatusFromToken(accessToken);

            if (userStatus != false)

            {
                if (_authService.GetUserFromToken(accessToken) != email)
                {
                    var userDb = _userRepository.GetUserByEmail(email);
                    var authdb = _userRepository.GetAuthByEmail(email);

                    _userRepository.RemoveEntity<User>(userDb);
                    _userRepository.RemoveEntity<Auth>(authdb);
                    if (_userRepository.SaveChangers())
                    {
                        return Ok(new { status = "success", message = "User Deleted successfully." });
                    }

                    return BadRequest(new { status = "error", message = "Failed to Delete User" });
                }
                return BadRequest(new { status = "error", message = "Unable to delete account. You cannot delete your own account." });
            }

            return BadRequest(new { status = "error", message = "Sorry..Only Admin Can delete Users.." });
        }



        // search Users by Email, First name, Last Name, Phone Number,Gender
        [HttpGet("GetUsers")]
        public async Task<IActionResult> SearchUser()
        {
            var accessToken = HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token").Result;
            var userStatus = _authService.GetStatusFromToken(accessToken);

            if (userStatus)
            {
                string keyword = HttpContext.Request.Query["Search"].ToString();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    try
                    {
                        var users = await _dataContext.Users.Where(b => b.Email.Contains(keyword) ||
                            b.FirstName.Contains(keyword) ||
                            b.LastName.Contains(keyword) ||
                            b.Gender.Contains(keyword) ||
                            b.PhoneNumber.Contains(keyword) &&
                            b.IsAdmin == false)
                           .ToListAsync();

                        if (users == null || !users.Any())
                        {
                            return BadRequest(new { status = "error", message = "No User/Users found matching the keyword." });
                        }

                        return Ok(users);
                    }
                    catch (Exception ex)
                    {

                        return BadRequest(new { status = "error", message = "Internal server error. Please try again later." });
                    }
                }

                else
                {
                    try
                    {
                        var users = await _dataContext.Users.Where(a=>a.IsAdmin==false).ToListAsync();
                        if (users == null || !users.Any())
                        {
                            return Ok(new { status = "success", message = "No Users available." });
                        }
                        return Ok(users);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { status = "error", message = "Internal server error. Please try again later." });
                    }
                }
            }

            return BadRequest(new { status = "error", message = "Sorry ... Only Admin can View Users" });
        }



        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { status = "error", message = "Token is required." });
            }

            _authService.RevokeToken(token);
            return Ok(new { status = "success", message = "Logged out successfully" });
        }

    }
}