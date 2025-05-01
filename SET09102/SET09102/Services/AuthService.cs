using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;
using BCrypt.Net;

namespace SET09102.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<bool> IsAuthenticatedAsync();
        Task LogoutAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly IPreferences _preferences;
        private const string AuthKey = "is_authenticated";
        private const string DefaultUsername = "admin";
        private const string DefaultPasswordHash = "$2y$10$WMCQya8Tj33YCSrfK0JPi.hjIuu3q3nNiT1EylCy9tqSpskZGmoni"; // "admin123"

        public AuthService(IPreferences preferences)
        {
            _preferences = preferences;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            if (username != DefaultUsername)
                return false;

            // In a real application, you would verify against a database
            // For this example, we're using a hardcoded hash
            bool isValid = BCrypt.Net.BCrypt.Verify(password, DefaultPasswordHash);
            
            if (isValid)
            {
                _preferences.Set(AuthKey, true);
            }
            
            return isValid;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            return _preferences.Get(AuthKey, false);
        }

        public async Task LogoutAsync()
        {
            _preferences.Set(AuthKey, false);
        }
    }
} 