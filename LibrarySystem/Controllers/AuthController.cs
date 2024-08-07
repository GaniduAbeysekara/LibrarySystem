using AutoMapper;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repository.Interface;
using LibrarySystem.Web.API.Model;
using LibrarySystem.Web.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public AuthController(IUserRepository userRepository, IConfiguration config, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;

            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserForRegistrationDto, User>();
            }));

        }


        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(UserForRegistrationDto userForRegistration)
        {
            var validationResult = await _authService.ValidateUserForRegistrationAsync(userForRegistration);
            if (!validationResult.Item1)
            {
                return new BadRequestObjectResult(new { status = "error", message = validationResult.Item2 });
            }

            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }
            byte[] passwordHash = _authService.GetPasswordHash(userForRegistration.Password, passwordSalt);

            Auth auth = new Auth
            {
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Email = userForRegistration.Email.ToLower()
            };
            User userDb = _mapper.Map<User>(userForRegistration);

            await _userRepository.AddEntityAsync<Auth>(auth);
            await _userRepository.AddEntityAsync<User>(userDb);

            if (await _userRepository.SaveChangersAsync())
            {
                return StatusCode(201, new { status = "success", message = "User created successfully.", userDb });
            }
            return StatusCode(500, new { status = "error", message = "Failed to register user." });
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync(UserForLoginDto userForLogin)
        {
            var validationResult = await _authService.ValidateUserForLoginAsync(userForLogin);
            if (!validationResult.Item1)
            {
                return new BadRequestObjectResult(new { status = "error", message = validationResult.Item2 });
            }

            User userDetails = await _userRepository.GetUserByEmailAsync(userForLogin.Email);
            Auth userForConfirmation = await _userRepository.GetAuthByEmailAsync(userForLogin.Email);

            byte[] passwordHash = _authService.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                {
                    return StatusCode(401, new { status = "error", message = "Incorrect password!" });
                }
            }

            return Ok(new
            {
                status = "success",
                message = "Logged in Successfully",
                token = _authService.CreateToken(userForLogin.Email, userDetails.IsAdmin)
            });
        }


        [HttpGet("GetLoggedInUserDetails")]
        public async Task<IActionResult> GetLoggedInUserDetailsAsync()
        {
            var email = await _authService.GetEmailFromAccessTokenAsync(HttpContext);
            var userDb = await _userRepository.GetUserByEmailAsync(email);

            return Ok(userDb);

        }


        [HttpPost("EditUser")]
        public async Task<IActionResult> EditUserAsync(UserForEdit userForEdit)
        {

            var validationResult = _authService.ValidateUserForEdit(userForEdit);
            if (!validationResult.Item1)
            {
                return new BadRequestObjectResult(new { status = "error", message = validationResult.Item2 });
            }
            var email = await _authService.GetEmailFromAccessTokenAsync(HttpContext);
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new { status = "error", message = "Invalid token" });
            }

            var userDb = await _userRepository.GetUserByEmailAsync(email);
            if (userDb == null)
            {
                return NotFound(new { status = "error", message = "This user does not exist" });
            }

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

            if (await _userRepository.SaveChangersAsync())
            {
                return Ok(new { status = "success", message = responseMessage, userForEdit });
            }
            return BadRequest(new { status = "error", message = responseMessage });
        }



        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            var userStatus = await _authService.GetUserStatusFromTokenAsync(HttpContext);

            if (!userStatus)
            {
                return StatusCode(403, new { status = "forbidden", message = "You do not have permission to delete other users." });
            }

            var validationResult = await _authService.ValidateUserForDeleteAsync(id, HttpContext);
            if (!validationResult.Item1)
            {
                return new BadRequestObjectResult(new { status = "error", message = validationResult.Item2 });
            }

            var userDb = await _userRepository.GetUserByIdAsync(id);
            var authDb = await _userRepository.GetAuthByEmailAsync(userDb.Email);

            await _userRepository.RemoveEntityAsync<User>(userDb);
            await _userRepository.RemoveEntityAsync<Auth>(authDb);
            if (await _userRepository.SaveChangersAsync())
            {
                return Ok(new { status = "success", message = "User " + id + " deleted successfully." });
            }

            return BadRequest(new { status = "error", message = "This user is not registered" });

        }




        // search Users by Email, First name, Last Name, Phone Number,Gender
        [HttpPost("GetUsers")]
        public async Task<IActionResult> Search([FromQuery] string? criteria)
        {
            var userStatus = await _authService.GetUserStatusFromTokenAsync(HttpContext);
            var email = await _authService.GetEmailFromAccessTokenAsync(HttpContext);

            if (!userStatus)
            {
                return StatusCode(403, new { status = "forbidden", message = "You do not have permission to access details of other users." });
            }
            try
            {
                if (string.IsNullOrEmpty(criteria))
                {
                    var allUsers = await _userRepository.GetAllUsersAsync(email);
                    if (allUsers == null || !allUsers.Any())
                    {
                        return Ok(new { status = "success", message = "No Users available." });
                    }
                    return Ok(allUsers);
                }

                var validationResult = _authService.ValidateUserForSearch(criteria);
                if (!validationResult.Item1)
                {
                    return new BadRequestObjectResult(new { status = "error", message = validationResult.Item2 });
                }

                User searchCriteria = (User)validationResult.Item2;
                // Perform the search
                var SearchUsers = _userRepository.SearchUsersSpefically(searchCriteria, email);

                if (!SearchUsers.Any())
                {
                    return Ok(new { status = "error", message = "No User/Users found matching the keyword." });
                }
                return Ok(SearchUsers);

            }
            catch (Exception ex)
            {
                // Log exception (ex) here if needed
                return StatusCode(500, new { status = "error", message = "Internal server error. Please try again later." });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { status = "error", message = "Token is required." });
            }

            await _authService.RevokeTokenAsync(token);

            var emailFromToken = await _authService.GetEmailFromAccessTokenAsync(HttpContext);
            return Ok(new { status = "success", message = $"{emailFromToken} Logged out successfully" });
        }

    }
}