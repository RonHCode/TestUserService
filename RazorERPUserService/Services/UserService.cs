using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RazorERPUserService.Models;
using RazorERPUserService.Repositories;

namespace RazorERPUserService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public UserService(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }
        public async Task<User> GetUserByIdAsync(int id)
        {
            // Retrieve user by ID
            return await _userRepository.GetUserByIdAsync(id);
        }

        // Retrieve a list of users based on the company and role
        public async Task<IEnumerable<User>> GetUsersAsync(int CompanyID, bool includeAdmins)
        {
            return await _userRepository.GetUsersAsync(CompanyID, includeAdmins);
        }

        // Create a new user (admins only)
        public async Task<User> CreateUserAsync(User newUser)
        {
            var hashedPassword = _authService.HashPassword(newUser, newUser.Password_Hash);
            newUser.Password_Hash = hashedPassword;

            await _userRepository.CreateUserAsync(newUser);
            return newUser;
        }

        // Update an existing user (admins only)
        public async Task<bool> UpdateUserAsync(User updatedUser)
        {
            return await _userRepository.UpdateUserAsync(updatedUser);
        }

        // Delete a user (admins only)
        public async Task<bool> DeleteUserAsync(int userId)
        {
            return await _userRepository.DeleteUserAsync(userId);
        }

    }
}
