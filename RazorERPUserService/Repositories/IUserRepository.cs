using Microsoft.Data.SqlClient;
using RazorERPUserService.Models;
using Dapper;

namespace RazorERPUserService.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetUsersAsync(int CompanyID, bool includeAdmins = false);
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }

}
