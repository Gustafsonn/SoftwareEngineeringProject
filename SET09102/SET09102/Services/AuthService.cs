using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;
using BCrypt.Net;
using Microsoft.Data.Sqlite;

namespace SET09102.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetCurrentUserRoleAsync();
        Task<int> GetCurrentUserIdAsync();
        Task LogoutAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly IPreferences _preferences;
        private readonly DatabaseService _databaseService;
        private const string AuthKey = "is_authenticated";
        private const string UserIdKey = "current_user_id";
        private const string UserRoleKey = "current_user_role";

        public AuthService(IPreferences preferences, DatabaseService databaseService)
        {
            _preferences = preferences;
            _databaseService = databaseService;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            using var connection = _databaseService.GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT id, password_hash, role FROM users WHERE username = @Username AND is_active = 1";
            command.Parameters.AddWithValue("@Username", username);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                int userId = reader.GetInt32(0);
                string storedHash = reader.GetString(1);
                string role = reader.GetString(2);
                
                bool isValid = BCrypt.Net.BCrypt.Verify(password, storedHash);
                
                if (isValid)
                {
                    _preferences.Set(AuthKey, true);
                    _preferences.Set(UserIdKey, userId);
                    _preferences.Set(UserRoleKey, role);
                    
                    // Update last login time
                    await UpdateLastLoginAsync(userId);
                    
                    return true;
                }
            }
            
            return false;
        }

        public Task<bool> IsAuthenticatedAsync()
        {
            return Task.FromResult(_preferences.Get(AuthKey, false));
        }

        public Task<string> GetCurrentUserRoleAsync()
        {
            return Task.FromResult(_preferences.Get(UserRoleKey, string.Empty));
        }

        public Task<int> GetCurrentUserIdAsync()
        {
            return Task.FromResult(_preferences.Get(UserIdKey, 0));
        }

        public Task LogoutAsync()
        {
            _preferences.Remove(AuthKey);
            _preferences.Remove(UserIdKey);
            _preferences.Remove(UserRoleKey);
            return Task.CompletedTask;
        }

        private async Task UpdateLastLoginAsync(int userId)
        {
            using var connection = _databaseService.GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE users SET last_login = @LastLogin WHERE id = @UserId";
            command.Parameters.AddWithValue("@LastLogin", DateTime.Now.ToString("o"));
            command.Parameters.AddWithValue("@UserId", userId);
            await command.ExecuteNonQueryAsync();
        }
    }
}