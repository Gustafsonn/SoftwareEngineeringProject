namespace SET09102.Services;

/// <summary>
/// Defines the contract for authentication services.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Attempts to log in a user with the provided credentials.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if login is successful, false otherwise.</returns>
    Task<bool> LoginAsync(string username, string password);

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the user is authenticated, false otherwise.</returns>
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task LogoutAsync();
}

/// <summary>
/// Provides basic authentication services using preferences for state management.
/// </summary>
/// <remarks>
/// This implementation uses hardcoded credentials and BCrypt for password hashing.
/// Authentication state is stored using MAUI Preferences.
/// </remarks>
public class AuthService : IAuthService
{
    private readonly IPreferences _preferences;
    private const string AuthKey = "is_authenticated";
    private const string DefaultUsername = "admin";
    // Hashed password for "admin123" using BCrypt
    private const string DefaultPasswordHash = "$2y$10$WMCQya8Tj33YCSrfK0JPi.hjIuu3q3nNiT1EylCy9tqSpskZGmoni"; 

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="preferences">The preferences service for storing authentication state.</param>
    public AuthService(IPreferences preferences)
    {
        _preferences = preferences;
    }

    /// <summary>
    /// Attempts to log in a user by comparing the provided username and password against hardcoded credentials.
    /// </summary>
    /// <param name="username">The username entered by the user.</param>
    /// <param name="password">The password entered by the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the credentials are valid, false otherwise.</returns>
    public async Task<bool> LoginAsync(string username, string password)
    {
        // Check if the username matches the hardcoded username.
        if (username != DefaultUsername)
            return false;

        // Verify the provided password against the stored hash using BCrypt.
        bool isValid = BCrypt.Net.BCrypt.Verify(password, DefaultPasswordHash);
        
        // If the password is valid, store the authenticated state.
        if (isValid)
        {
            _preferences.Set(AuthKey, true);
        }
        
        // Return the validation result.
        return isValid;
    }

    /// <summary>
    /// Checks the stored preferences to determine if the user is currently marked as authenticated.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the authentication key is set to true in preferences, false otherwise.</returns>
    public async Task<bool> IsAuthenticatedAsync()
    {
        // Retrieve the authentication state from preferences, defaulting to false if not found.
        return _preferences.Get(AuthKey, false);
    }

    /// <summary>
    /// Logs out the current user by setting the authentication state in preferences to false.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LogoutAsync()
    {
        // Set the authentication state to false in preferences.
        _preferences.Set(AuthKey, false);
    }
}
