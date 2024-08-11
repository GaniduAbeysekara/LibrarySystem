using LibrarySystem.Data.Entities;
using LibrarySystem.Web.API.Model;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Web.API.Services.Interface
{
    public interface IAuthService
    {
        public Task RevokeTokenAsync(string token);
        public bool IsTokenRevoked(string token);
        public Task<string> GetEmailFromAccessTokenAsync(HttpContext httpContext);
        public Task<bool> GetUserStatusFromTokenAsync(HttpContext httpContext);
        public byte[] GetPasswordHash(string password, byte[] passwordSalt);
        public string CreateToken(string email, bool isAdmin);
        public Task<(bool, string)> ValidateUserForRegistrationAsync(UserForRegistrationDto userForRegistration);
        public Task<(string, string)> ValidateUserForLoginAsync(UserForLoginDto userForLogin);
        public (bool, string) ValidateUserForEdit(UserForEdit userForEdit);
        public (bool, object) ValidateUserForSearch(string? criteria);
        public Task<(bool, string)> ValidateUserForDeleteAsync(int id, HttpContext httpContext);

    }
}
