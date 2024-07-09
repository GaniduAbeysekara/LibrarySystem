using AutoMapper;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repository.Interface;
using LibrarySystem.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
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
            //bool isValidEmail = IsValidEmail(email);
            if (IsValidEmail(email))
            {
                User user = _userRepository.GetUserByEmail(email);
                if (user != null)
                {
                    return user;

                }
            }          

            throw new Exception("There is not email as " + email);

        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            bool isValidEmail = IsValidEmail(userForRegistration.Email);
            bool isValidPhoneNum = IsValidPhoneNo(userForRegistration.PhonneNumber);

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
            return StatusCode(500, "Please enter a valid Email Address and Phone Number!");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            Auth userForConfirmation = _userRepository.GetAuthByEmail(userForLogin.Email);

            if(userForConfirmation.PasswordHash != null && userForConfirmation.PasswordSalt != null)
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
            return Ok(new Dictionary<string, string> {
                {"token", CreateToken(userForLogin.Email)}
            });
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

        private string CreateToken(string email)
        {
            Claim[] claims = new Claim[] {
                new Claim("email", email)
            };

            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        tokenKeyString != null ? tokenKeyString : ""
                    )
                );

            SigningCredentials credentials = new SigningCredentials(
                    tokenKey,
                    SecurityAlgorithms.HmacSha512Signature
                );

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddHours(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);

        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";            

                Regex regex = new Regex(emailPattern);
                return regex.IsMatch(email);
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
           
        }

        public static bool IsValidPhoneNo(string phoneNo)
        {
            if (string.IsNullOrWhiteSpace(phoneNo))
                return false;

            try
            {
                string phoneNoPattern = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";

                Regex regex = new Regex(phoneNoPattern);
                return regex.IsMatch(phoneNo);
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }

        }

    }
}