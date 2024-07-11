using AutoMapper;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repository.Interface;
using LibrarySystem.Web.API.Model;
using LibrarySystem.Web.API.Services.Interface;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace LibrarySystem.Web.API.Services.Infrastructure
{
    public class AuthService: IAuthService
    {
        private IUserRepository _userRepository;
        private IMapper _mapper;
        private readonly IConfiguration _config;
        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;

            _mapper = new Mapper(new MapperConfiguration(cfg => {
                cfg.CreateMap<UserForRegistrationDto, User>();
            }));

            _config = config;
        }

        private static readonly HashSet<string> _revokedTokens = new HashSet<string>();

        public void RevokeToken(string token)
        {
            _revokedTokens.Add(token);
        }

        public bool IsTokenRevoked(string token)
        {
            return _revokedTokens.Contains(token);
        }

        public string GetUserFromToken(string? jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(jwt);
            var loggedInEmail = jwtSecurityToken.Claims.First(claim => claim.Type == "email").Value;
            return loggedInEmail;
        }

        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
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

        public string CreateToken(string email)
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

        public  bool IsValidEmail(string email)
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

        public  bool IsValidPhoneNo(string phoneNo)
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

        public bool isValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            try
            {
                string passwordPattern = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";

                Regex regex = new Regex(passwordPattern);
                return regex.IsMatch(password);

            }
            catch (RegexMatchTimeoutException e)
            {

                return false;
            }
        }

        public  IActionResult ValidateObjectNotNullOrEmpty(object model)
        {
            if (model == null)
            {
                return new BadRequestObjectResult(new { status = "error", message = "The request body is missing." });
            }

            var properties = model.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(model);
                var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
                var displayName = displayNameAttribute != null ? displayNameAttribute.DisplayName : property.Name;

                if (value == null)
                {
                    return new BadRequestObjectResult(new { status = "error", message = $"The field '{displayName}' must not be null." });
                }

                if (property.PropertyType == typeof(string) && string.IsNullOrWhiteSpace((string)value))
                {
                    return new BadRequestObjectResult(new { status = "error", message = $"The field '{displayName}' must not be empty or whitespace." });
                }
            }

            return null;
        }



    }
}
