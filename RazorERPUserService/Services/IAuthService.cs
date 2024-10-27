using RazorERPUserService.Models;

namespace RazorERPUserService.Services
{
    public interface IAuthService
    {
        Task<string> AuthenticateAsync(string username, string password);
        string HashPassword(User user, string password);
        bool VerifyPassword(User user, string hashedPassword, string providedPassword);

    }
}
