using AutoMapper;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repository.Interface;
using LibrarySystem.Web.API.Helpers;
using LibrarySystem.Web.API.Model;
using LibrarySystem.Web.API.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace LibrarySystem.Web.API.Services.Infrastructure
{
    public class AuthService : IAuthService
    {
        private IUserRepository _userRepository;
        private IMapper _mapper;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;

            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserForRegistrationDto, User>();
            }));

            _config = config;
        }

        private static readonly HashSet<string> _revokedTokens = new HashSet<string>();

        public async Task RevokeTokenAsync(string token)
        {
            // Simulate asynchronous operation
            await Task.Run(() => _revokedTokens.Add(token));
        }

        public bool IsTokenRevoked(string token)
        {
            return _revokedTokens.Contains(token);
        }

        public async Task<string> GetEmailFromAccessTokenAsync(HttpContext httpContext)
        {
            var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(accessToken);
            var loggedInEmail = jwtSecurityToken.Claims.First(claim => claim.Type == "email").Value;
            return loggedInEmail;
        }

        public async Task<bool> GetUserStatusFromTokenAsync(HttpContext httpContext)
        {
            var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(accessToken);
            var isAdminStatus = jwtSecurityToken.Claims.First(claim => claim.Type == "isAdmin").Value;
            return Convert.ToBoolean(isAdminStatus);
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

        public string CreateToken(string email, bool isAdmin)
        {
            Claim[] claims = new Claim[] {
                new Claim("email", email),
                 new Claim("isAdmin",isAdmin.ToString())

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

        public async Task<(bool, string)> ValidateUserForRegistrationAsync(UserForRegistrationDto userForRegistration)
        {
            var validationResult = ValidateObjectNotNullOrEmpty(userForRegistration);
            if (!validationResult.Item1)
            {
                return (false, validationResult.Item2);
            }

            bool isValidEmail = IsValidEmail(userForRegistration.Email);
            if (!isValidEmail)
            {
                var emailCheck = userForRegistration.Email.ToString();
                if (emailCheck.ToLower() != emailCheck)
                {
                    return (false, ResponseMessagesHelper.EnterLowerCaseEmail);
                }
                return (false, ResponseMessagesHelper.EnterValiedEmail);
            }

            bool isValidPhoneNum = IsValidPhoneNo(userForRegistration.PhoneNumber);
            if (!isValidPhoneNum)
            {
                return (false, ResponseMessagesHelper.EnterValiedPhoneNumber);
            }

            bool isValidPass = IsValidPassword(userForRegistration.Password);
            if (!isValidPass)
            {
                return (false, ResponseMessagesHelper.EnterValiedPassword);
            }

            if (userForRegistration.Password != userForRegistration.PasswordConfirm)
            {
                return (false, ResponseMessagesHelper.PasswordNotMatch);
            }

            User existingUser = await _userRepository.GetUserByEmailAsync(userForRegistration.Email);
            if (existingUser != null)
            {
                return (false, ResponseMessagesHelper.EmailAlreadyExists);
            }

            return (true, null);
        }

        public async Task<(string, string)> ValidateUserForLoginAsync(UserForLoginDto userForLogin)
        {
            var validationResult = ValidateObjectNotNullOrEmpty(userForLogin);
            if (!validationResult.Item1)
            {
                return ("badRequest", validationResult.Item2);
            }

            bool isValidEmail = IsValidEmail(userForLogin.Email);
            if (!isValidEmail)
            {
                var emailCheck = userForLogin.Email.ToString();

                if (emailCheck.ToLower() != emailCheck)
                {
                    return ("badRequest", ResponseMessagesHelper.EnterLowerCaseEmail); // Email contains uppercase letters
                }

                return ("badRequest", ResponseMessagesHelper.EnterValiedEmail);
            }

            User userDetails = await _userRepository.GetUserByEmailAsync(userForLogin.Email);
            Auth userForConfirmation = await _userRepository.GetAuthByEmailAsync(userForLogin.Email);

            if (userForConfirmation == null)
            {
                return ("unauthorized", ResponseMessagesHelper.EmailNotRegistered);
            }

            byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                {
                    return ("unauthorized", ResponseMessagesHelper.IncorrectPassword);
                }
            }


            return (null, null);
        }


        public (bool, string) ValidateUserForEdit(UserForEdit userForEdit)
        {
            var validationResult = ValidateObjectNotNullOrEmpty(userForEdit);
            if (!validationResult.Item1)
            {
                return (validationResult.Item1, validationResult.Item2);
            }

            bool isValidPhoneNum = IsValidPhoneNo(userForEdit.PhoneNumber);
            if (!isValidPhoneNum)
            {
                return (false, ResponseMessagesHelper.EnterValiedPhoneNumber);
            }

            return (true, null);
        }


        public async Task<(bool, string)> ValidateUserForDeleteAsync(int id, HttpContext httpContext)
        {
            var userStatus = await GetUserStatusFromTokenAsync(httpContext);
            var user = await _userRepository.GetUserByIdAsync(id);
            var emailFromToken = await GetEmailFromAccessTokenAsync(httpContext);

            if (user == null)
            {
                return (false, ResponseMessagesHelper.InvalidUserId);
            }

            if (emailFromToken == user.Email)
            {
                return (false, ResponseMessagesHelper.UnableToDeleteOwnAccount);
            }

            if (user.IsAdmin)
            {
                return (false, ResponseMessagesHelper.UnableToDeleteAdminAccount);
            }

            return (true, null);
        }

        private (bool, string) ValidateObjectNotNullOrEmpty(object model)
        {
            if (model == null)
            {
                return (false, ResponseMessagesHelper.RequestBodyMissing);
            }

            var properties = model.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(model);
                var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
                var displayName = displayNameAttribute != null ? displayNameAttribute.DisplayName : property.Name;

                if (value == null)
                {
                    return (false, $"The field '{displayName}' must not be null.");
                }

                if (property.PropertyType == typeof(string) && string.IsNullOrWhiteSpace((string)value))
                {
                    return (false, $"The field '{displayName}' must not be empty or whitespace.");
                }
            }

            return (true, null);
        }

        public (bool, object) ValidateUserForSearch(string? criteria)
        {
            // Deserialize the JSON string into UserSearchCriteria
            var searchCriteria = JsonConvert.DeserializeObject<User>(criteria);

            if (searchCriteria == null)
            {
                return (false, ResponseMessagesHelper.InvalidSearchCriteria);
            }

            if (searchCriteria.FirstName.Length >= 3 || searchCriteria.LastName.Length >= 3 || searchCriteria.Email.Length >= 3
                            || searchCriteria.PhoneNumber.Length >= 3 || searchCriteria.Gender.Length >= 3)
            {
                return (true, searchCriteria);

            }
            return (false, ResponseMessagesHelper.SearchCharacters);

        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                //string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

                string emailPattern = RegexPatterHelper.emailPattern;

                Regex regex = new Regex(emailPattern);
                return regex.IsMatch(email);
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }

        }

        private bool IsValidPhoneNo(string phoneNo)
        {
            if (string.IsNullOrWhiteSpace(phoneNo))
                return false;

            try
            {
                string phoneNoPattern = RegexPatterHelper.phoneNoPattern;

                Regex regex = new Regex(phoneNoPattern);
                return regex.IsMatch(phoneNo);
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }

        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            try
            {
                string passwordPattern = RegexPatterHelper.passwordPattern;

                Regex regex = new Regex(passwordPattern);
                return regex.IsMatch(password);

            }
            catch (RegexMatchTimeoutException e)
            {

                return false;
            }
        }
    }
}
