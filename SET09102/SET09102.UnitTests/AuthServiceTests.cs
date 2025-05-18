using Moq;
using SET09102.Services;
using System.Threading.Tasks;
using Xunit;

namespace SET09102.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IPreferences> _mockPreferences;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockPreferences = new Mock<IPreferences>();
            _authService = new AuthService(_mockPreferences.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsTrue()
        {
            string validUsername = "admin";
            string validPassword = "admin123";

            bool result = await _authService.LoginAsync(validUsername, validPassword);

            Assert.True(result);
            _mockPreferences.Verify(p => p.Set(It.Is<string>(k => k == "is_authenticated"), true, null), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidUsername_ReturnsFalse()
        {
            string invalidUsername = "wronguser";
            string password = "admin123";

            bool result = await _authService.LoginAsync(invalidUsername, password);

            Assert.False(result);
            _mockPreferences.Verify(p => p.Set(It.IsAny<string>(), It.IsAny<bool>(), null), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ReturnsFalse()
        {
            string validUsername = "admin";
            string invalidPassword = "wrongpassword";

            bool result = await _authService.LoginAsync(validUsername, invalidPassword);

            Assert.False(result);
            _mockPreferences.Verify(p => p.Set(It.IsAny<string>(), It.IsAny<bool>(), null), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IsAuthenticatedAsync_ReturnsPreferenceValue(bool isAuthenticated)
        {
            _mockPreferences.Setup(p => p.Get(It.Is<string>(k => k == "is_authenticated"), It.IsAny<bool>(), null))
                .Returns(isAuthenticated);

            bool result = await _authService.IsAuthenticatedAsync();

            Assert.Equal(isAuthenticated, result);
            _mockPreferences.Verify(p => p.Get(It.Is<string>(k => k == "is_authenticated"), false, null), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_SetsAuthenticationToFalse()
        {
            await _authService.LogoutAsync();

            _mockPreferences.Verify(p => p.Set(It.Is<string>(k => k == "is_authenticated"), false, null), Times.Once);
        }

        [Fact]
        public async Task IsAuthenticatedAsync_DefaultsToFalseWhenNotSet()
        {
            _mockPreferences.Setup(p => p.Get(It.IsAny<string>(), It.IsAny<bool>(), null))
                .Returns((string key, bool defaultValue) => defaultValue);

            bool result = await _authService.IsAuthenticatedAsync();

            Assert.False(result);
        }
    }
}