using Microsoft.AspNetCore.Mvc;
using RazorERPUserService.Controllers;
using RazorERPUserService.Data;
using RazorERPUserService.Models;
using Moq;
using Xunit;
using RazorERPUserService.Services;
using RazorERPUserService.DTOs;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using RazorERPUserService.Repositories;
using AutoMapper;
using RazorERPUserService.Mappings;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog;


namespace RazorERPUserServiceTest
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            //add the user mapping for use at unittest
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserMapping());
            });
            _mapper = config.CreateMapper();
            _userServiceMock = new Mock<IUserService>();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .CreateLogger());
            });

            _logger = loggerFactory.CreateLogger<UsersController>();

            _controller = new UsersController(_userServiceMock.Object, _mapper, _logger);

            // Mock a ClaimsPrincipal with a role claim
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Admin") 
            };

            var identity = new ClaimsIdentity(userClaims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Set the User property on the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

        }


        [Fact]
        public async Task GetUsers_ReturnsListOfUsers()
        {
            // Arrange
            var mockUsers = new List<User>
            {
                new User { UserID = 1, Username = "John Doe", Role = "Admin", CompanyID = 0 },
                new User { UserID = 2, Username = "Jane Doe", Role = "User", CompanyID = 1 }
            };

            _userServiceMock.Setup(s => s.GetUsersAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(mockUsers);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDTO>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
        }

        [Fact]
        public async Task GetUserById_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var user = new User { UserID = userId, Username = "testuser" };
            _userServiceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<UserDTO>(okResult.Value);
            Assert.Equal(userId, returnedUser.Id);
        }

        [Fact]
        public async Task GetUserById_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            _userServiceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateUser_WhenUserIsValid()
        {
            // Arrange
            var newUser = new CreateUserDTO { Username = "newuser" };
            _userServiceMock.Setup(s => s.CreateUserAsync(new User
            {
                Username = newUser.Username,
                Email = newUser.Email,
                Role = newUser.Role,
                CompanyID = newUser.CompanyID
            })).ReturnsAsync((User)null);

            // Act
            var result = await _controller.CreateUser(newUser);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetUsers", createdResult.ActionName);
            Assert.Equal(newUser.Username, ((UserDTO)createdResult.Value).Username);
        }

        [Fact]
        public async Task UpdateUser_WhenUpdateIsSuccessful()
        {
            // Arrange
            var userToUpdate = new UserDTO { Id = 1, Username = "updateduser", Email = "email", Role = "Admin", CompanyID = 1 };
            _userServiceMock.Setup(s => s.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUser(userToUpdate.Id, userToUpdate);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateUser_WhenUpdateIsNotSuccessful()
        {
            // Arrange
            var userToUpdate = new UserDTO { Id = 1, Username = "updateduser" };
            _userServiceMock.Setup(s => s.UpdateUserAsync(new User
            {
                Username = userToUpdate.Username,
                Email = userToUpdate.Email,
                Role = userToUpdate.Role,
                CompanyID = userToUpdate.CompanyID,
                DateUpdated = DateTime.Now,
                UserID = userToUpdate.Id
            })).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateUser(userToUpdate.Id, userToUpdate);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUser_WhenDeleteIsSuccessful()
        {
            // Arrange
            var userId = 1;
            _userServiceMock.Setup(s => s.DeleteUserAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteUser_WhenDeleteIsNotSuccessful()
        {
            // Arrange
            var userId = 1;
            _userServiceMock.Setup(s => s.DeleteUserAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

    }
}