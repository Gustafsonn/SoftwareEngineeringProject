using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SET09102.Models;

namespace SET09102.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed data
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = UserRole.Admin,
                    LastLogin = DateTime.Now.AddDays(-1)
                },
                new User 
                { 
                    Id = 2,
                    Username = "scientist",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("science123"),
                    Role = UserRole.Scientist,
                    LastLogin = DateTime.Now.AddDays(-2)
                },
                new User 
                { 
                    Id = 3,
                    Username = "operations",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("ops123"),
                    Role = UserRole.OperationsManager,
                    LastLogin = DateTime.Now.AddDays(-3)
                }
            );
        }
    }
    
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task<bool> UpdateRoleAsync(string username, UserRole newRole);
        Task<bool> UsernameExistsAsync(string username);
    }
    
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        
        public UserService(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogError("Username or password is empty");
                return null;
            }
            
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                _logger.LogError($"User not found: {username}");
                return null;
            }
            
            bool verified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            
            if (!verified)
            {
                _logger.LogError($"Invalid password for user: {username}");
                return null;
            }
            
            // Update last login time
            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"User authenticated successfully: {username}");
            return user;
        }
        
        public async Task<bool> UpdateRoleAsync(string username, UserRole newRole)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                _logger.LogError($"User not found: {username}");
                return false;
            }
            
            user.Role = newRole;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Role updated for user {username}: {newRole}");
            return true;
        }
        
        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }
    }
}