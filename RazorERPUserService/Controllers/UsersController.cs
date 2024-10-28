using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RazorERPUserService.Data;
using RazorERPUserService.DTOs;
using RazorERPUserService.Models;
using RazorERPUserService.Mappings;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RazorERPUserService.Services;
using AutoMapper;

namespace RazorERPUserService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IMapper mapper, ILogger<UsersController> logger)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Getting all users");

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var CompanyID = int.Parse(User.FindFirst("CompanyID")?.Value ?? "0");

            var users = await _userService.GetUsersAsync(CompanyID, includeAdmins: currentUserRole == "Admin");
            var userDtos = _mapper.Map<List<UserDTO>>(users);

            return Ok(userDtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            _logger.LogInformation($"Getting user by ID {id}");

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"NotFound user ID {id}");
                return NotFound();
            }

            var userDto = _mapper.Map<UserDTO>(user);

            return Ok(userDto);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO createUserDTO)
        {
            _logger.LogInformation($"Creating new user {createUserDTO.Username}");

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
            _logger.LogInformation($"Updating user {userDTO.Username}");

            if (userDTO == null || userDTO.Id != id)
            {
                _logger.LogError($"BadRequest - User does not match {id}");
                return BadRequest();
            }

            //var updateUser = new User
            //{
            //    UserID = userDTO.Id,
            //    Username = userDTO.Username,
            //    Role = userDTO.Role,
            //    Email = userDTO.Email,                
            //    CompanyID = userDTO.CompanyID,
            //    //Password_Hash = userDTO.Password, // Changing of password should be in a separate method
            //};

            var updateUser = _mapper.Map<User>(userDTO);


            var result = await _userService.UpdateUserAsync(updateUser);
            if (!result)
            {
                _logger.LogWarning($"NotFound user ID {id}");
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation($"Updating user id {id}");

            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                _logger.LogWarning($"NotFound user ID {id}");
                return NotFound();
            }

            return Ok(); 
        }


    }

}
