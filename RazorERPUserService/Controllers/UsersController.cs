using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RazorERPUserService.Data;
using RazorERPUserService.DTOs;
using RazorERPUserService.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RazorERPUserService.Services;

namespace RazorERPUserService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var CompanyID = int.Parse(User.FindFirst("CompanyID")?.Value ?? "0");

            var users = await _userService.GetUsersAsync(CompanyID, includeAdmins: currentUserRole == "Admin");

            // Map the data from the User entity to the UserDTO before returning it.
            var userDtos = users.Select(user => new UserDTO
            {
                Id = user.UserID,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CompanyID = user.CompanyID,
                Password = "***"
            });

            Console.WriteLine("Fetched users count: " + users.Count());

            return Ok(userDtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(); ;
            }

            var userDto = new UserDTO
            {
                Id = user.UserID,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CompanyID = user.CompanyID,
                DateCreated = user.DateCreated,
                DateUpdated = user.DateUpdated,
                Password = "***"
            };

            return Ok(userDto);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO createUserDTO)
        {
            var x = User;

            var newUser = new User
            {
                Username = createUserDTO.Username,
                Role = createUserDTO.Role,
                Email = createUserDTO.Email,
                Password_Hash = createUserDTO.Password,
                CompanyID = createUserDTO.CompanyID
            };

            await _userService.CreateUserAsync(newUser);

            return CreatedAtAction(nameof(GetUsers), new { id = newUser.UserID }, new UserDTO
            {
                Id = newUser.UserID,
                Username = newUser.Username,
                Role = newUser.Role,
                Email = newUser.Email,
                Password = "***"
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO userDTO)
        {
            if (userDTO == null || userDTO.Id != id)
            {
                return BadRequest();
            }

            var updateUser = new User
            {
                UserID = userDTO.Id,
                Username = userDTO.Username,
                Role = userDTO.Role,
                Email = userDTO.Email,                
                CompanyID = userDTO.CompanyID,
                DateUpdated = DateTime.Now
                //Password_Hash = userDTO.Password, // Changing of password should be in a separate method
            };

            var result = await _userService.UpdateUserAsync(updateUser);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return Ok(); 
        }


    }

}
