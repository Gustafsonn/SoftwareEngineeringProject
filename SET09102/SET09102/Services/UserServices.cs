using Microsoft.Data.Sqlite;
using SET09102.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SET09102.Services
{
    public class UserService
    {
        private readonly DatabaseService _databaseService;

        public UserService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task InitializeUsersTableAsync()
        {
            using var connection = _databaseService.GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT NOT NULL UNIQUE,
                    password_hash TEXT NOT NULL,
                    role TEXT NOT NULL,
                    full_name TEXT NOT NULL,
                    email TEXT,
                    is_active INTEGER NOT NULL DEFAULT 1,
                    created_at TEXT NOT NULL,
                    last_login TEXT
                );";
            await command.ExecuteNonQueryAsync();

            // Check if default admin user exists
            command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM users WHERE username = 'admin'";
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());

            if (count == 0)
            {
                // Create default admin user
                string passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO users (username, password_hash, role, full_name, email, created_at)
                    VALUES (@Username, @PasswordHash, @Role, @FullName, @Email, @CreatedAt)";
                command.Parameters.AddWithValue("@Username", "admin");
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@Role", "Administrator");
                command.Parameters.AddWithValue("@FullName", "System Administrator");
                command.Parameters.AddWithValue("@Email", "admin@example.com");
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));
                await command.ExecuteNonQueryAsync();

                // Create default operation manager user
                passwordHash = BCrypt.Net.BCrypt.HashPassword("operations123");
                command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO users (username, password_hash, role, full_name, email, created_at)
                    VALUES (@Username, @PasswordHash, @Role, @FullName, @Email, @CreatedAt)";
                command.Parameters.AddWithValue("@Username", "operations");
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@Role", "Operations Manager");
                command.Parameters.AddWithValue("@FullName", "Operations Manager");
                command.Parameters.AddWithValue("@Email", "operations@example.com");
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));
                await command.ExecuteNonQueryAsync();

                // Create default environmental scientist user
                passwordHash = BCrypt.Net.BCrypt.HashPassword("scientist123");
                command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO users (username, password_hash, role, full_name, email, created_at)
                    VALUES (@Username, @PasswordHash, @Role, @FullName, @Email, @CreatedAt)";
                command.Parameters.AddWithValue("@Username", "scientist");
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@Role", "Environmental Scientist");
                command.Parameters.AddWithValue("@FullName", "Environmental Scientist");
                command.Parameters.AddWithValue("@Email", "scientist@example.com");
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using var connection = _databaseService.GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM users ORDER BY username";
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Role = reader.GetString(3),
                    FullName = reader.GetString(4),
                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsActive = reader.GetInt32(6) == 1,
                    CreatedAt = DateTime.Parse(reader.GetString(7)),
                    LastLogin = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8))
                });
            }
            
            return users;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = _databaseService.GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM users WHERE id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Role = reader.GetString(3),
                    FullName = reader.GetString(4),
                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsActive = reader.GetInt32(6) == 1,
                    CreatedAt = DateTime.Parse(reader.GetString(7)),
                    LastLogin = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8))
                };
            }
            
            return null;
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.CreatedAt = DateTime.Now;
            
            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO users (username, password_hash, role, full_name, email, is_active, created_at)
                    VALUES (@Username, @PasswordHash, @Role, @FullName, @Email, @IsActive, @CreatedAt)";
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@Role", user.Role);
                command.Parameters.AddWithValue("@FullName", user.FullName);
                command.Parameters.AddWithValue("@Email", user.Email ?? string.Empty);
                command.Parameters.AddWithValue("@IsActive", user.IsActive ? 1 : 0);
                command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt.ToString("o"));
                
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE users
                    SET role = @Role, 
                        full_name = @FullName, 
                        email = @Email, 
                        is_active = @IsActive
                    WHERE id = @Id";
                command.Parameters.AddWithValue("@Id", user.Id);
                command.Parameters.AddWithValue("@Role", user.Role);
                command.Parameters.AddWithValue("@FullName", user.FullName);
                command.Parameters.AddWithValue("@Email", user.Email ?? string.Empty);
                command.Parameters.AddWithValue("@IsActive", user.IsActive ? 1 : 0);
                
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            try
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE users SET password_hash = @PasswordHash WHERE id = @Id";
                command.Parameters.AddWithValue("@Id", userId);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM users WHERE id = @Id";
                command.Parameters.AddWithValue("@Id", userId);
                
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}