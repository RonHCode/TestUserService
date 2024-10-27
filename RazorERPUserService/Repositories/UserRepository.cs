using Dapper;
using Microsoft.EntityFrameworkCore;
using RazorERPUserService.Data;
using RazorERPUserService.Models;

namespace RazorERPUserService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DapperContext _context;

        public UserRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetUsersAsync(int CompanyID, bool includeAdmins = false)
        {
            using (var connection = _context.CreateConnection())
            {
                string query = includeAdmins ?
                    "SELECT * FROM Users WHERE CompanyID = @CompanyID" :
                    "SELECT * FROM Users WHERE CompanyID = @CompanyID AND Role <> 'Admin'";
                return await connection.QueryAsync<User>(query, new { CompanyID = CompanyID });
            }
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE UserID = @Id", new { Id = id });
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var query = "SELECT * FROM Users WHERE UserName = @Username";
            using (var connection = _context.CreateConnection())
            {
                var Users = await connection.QuerySingleOrDefaultAsync<User>(query, new { Username = username });
                return Users;
            }
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            var query = "INSERT INTO Users (Username, Password_Hash, Role, CompanyID) VALUES (@Username, @Password_Hash, @Role, @CompanyID)";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(query, new
                {
                    user.Username,
                    user.Password_Hash,
                    user.Role,
                    user.CompanyID
                });

                return result > 0;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var query = "UPDATE Users SET Username = @Username, Email = @Email, @Role = @Role, CompanyID = @CompanyID, DateUpdated = @DateUpdated WHERE UserID = @UserID";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(query, new
                {
                    user.Username,
                    user.Email,
                    user.Role,
                    user.CompanyID,
                    user.DateUpdated,
                    user.UserID
                });

                return result > 0;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync("DELETE FROM Users WHERE UserID = @Id", new { Id = id });

                return result > 0;
            }
        }
    }
}
