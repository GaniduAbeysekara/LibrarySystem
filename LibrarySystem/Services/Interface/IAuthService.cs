using LibrarySystem.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Web.API.Services.Interface
{
    public interface IAuthService
    {
        public void RevokeToken(string token);
        public bool IsTokenRevoked(string token);
        public string GetUserFromToken(string? jwt);
        public bool GetStatusFromToken(string accessToken);
        public byte[] GetPasswordHash(string password, byte[] passwordSalt);
        public string CreateToken(string email, bool isAdmin);
        public  bool IsValidEmail(string email);
        public  bool IsValidPhoneNo(string phoneNo);
        public bool isValidPassword(string password);
        public IActionResult ValidateObjectNotNullOrEmpty(object model);
    }
}
