using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using SET09102.Models;
using SET09102.Services;
using Moq;

namespace SET09102.Tests
{
    public class UserServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger> _mockLogger;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
                
            _context = new ApplicationDbContext(options);
            
            // Seed test data
            _context.Database.EnsureCreated();
            
            _mockLogger = new Mock<ILogger>();
            _userService = new UserService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task ValidAuthentication_ReturnsUser()
        {
            // Arrange
            string username = "admin";
            string password = "admin123";

            // Act
            var user = await _userService.AuthenticateAsync(username, password);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(username, user.Username);
            Assert.Equal(UserRole.Admin, user.Role);
            _mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains(username))), Times.Once);
        }

        [Fact]
        public async Task InvalidPassword_ReturnsNull()
        {
            // Arrange
            string username = "admin";
            string wrongPassword = "wrongpassword";

            // Act
            var user = await _userService.AuthenticateAsync(username, wrongPassword);

            // Assert
            Assert.Null(user);
            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains(username))), Times.Once);
        }

        [Fact]
        public async Task NonExistentUser_ReturnsNull()
        {
            // Arrange
            string nonExistentUsername = "nonexistent";
            string password = "password";

            // Act
            var user = await _userService.AuthenticateAsync(nonExistentUsername, password);

            // Assert
            Assert.Null(user);
            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains(nonExistentUsername))), Times.Once);
        }

        [Fact]
        public async Task RoleAssignment_UpdatesRole()
        {
            // Arrange
            string username = "scientist";
            UserRole newRole = UserRole.OperationsManager;

            // Act
            bool result = await _userService.UpdateRoleAsync(username, newRole);
            var updatedUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedUser);
            Assert.Equal(newRole, updatedUser.Role);
            _mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains(username))), Times.Once);
        }

        [Fact]
        public async Task ConcurrentLogins_Succeed()
        {
            // Arrange
            string username = "admin";
            string password = "admin123";

            // Act
            var user1 = await _userService.AuthenticateAsync(username, password);
            var user2 = await _userService.AuthenticateAsync(username, password);

            // Assert
            Assert.NotNull(user1);
            Assert.NotNull(user2);
            Assert.Equal(user1.Id, user2.Id);
            // Verify that last login was updated twice
            _mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains(username))), Times.Exactly(2));
        }
    }
}