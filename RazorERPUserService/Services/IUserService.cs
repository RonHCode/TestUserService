using RazorERPUserService.Models;

namespace RazorERPUserService.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUsersAsync(int CompanyID, bool includeAdmins);
        Task<User> CreateUserAsync(User newUser);
        Task<bool> UpdateUserAsync(User updatedUser);
        Task<bool> DeleteUserAsync(int userId);
        Task<User> GetUserByIdAsync(int id);

    }
}
