using Microsoft.Maui.Controls;
using SET09102.Models;
using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Administrator.Pages
{
    public partial class UserManagementPage : ContentPage, INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private readonly IAuthService _authService;
        private ObservableCollection<Models.User> _allUsers;
        private ObservableCollection<Models.User> _filteredUsers;
        private Models.User? _selectedUser;
        private bool _hasSelectedUser;
        private int _currentUserId;

        public UserManagementPage(UserService userService, IAuthService authService)
        {
            InitializeComponent();
            
            _userService = userService;
            _authService = authService;
            _allUsers = new ObservableCollection<Models.User>();
            _filteredUsers = new ObservableCollection<Models.User>();
            
            BindingContext = this;
            
            RoleFilterPicker.SelectedIndex = 0;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _currentUserId = await _authService.GetCurrentUserIdAsync();
            await LoadUsersAsync();
        }

        public ObservableCollection<Models.User> Users
        {
            get => _filteredUsers;
            set
            {
                _filteredUsers = value;
                OnPropertyChanged();
            }
        }

        public Models.User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                HasSelectedUser = (_selectedUser != null);
                OnPropertyChanged();
            }
        }

        public bool HasSelectedUser
        {
            get => _hasSelectedUser;
            set
            {
                _hasSelectedUser = value;
                OnPropertyChanged();
            }
        }

        private void OnRoleFilterChanged(object sender, EventArgs e)
        {
            FilterUsers();
        }

        private void OnUserSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                SelectedUser = e.CurrentSelection[0] as Models.User;
            }
            else
            {
                SelectedUser = null;
            }
        }

        private async void OnAddUserClicked(object sender, EventArgs e)
        {
            // Get username
            string username = await DisplayPromptAsync("New User", "Enter username:", initialValue: "");
            if (string.IsNullOrWhiteSpace(username))
                return;

            // Validate username (check if exists)
            var existingUsers = _allUsers.Where(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
            if (existingUsers.Any())
            {
                await DisplayAlert("Error", "Username already exists.", "OK");
                return;
            }

            // Get full name
            string fullName = await DisplayPromptAsync("New User", "Enter full name:", initialValue: "");
            if (string.IsNullOrWhiteSpace(fullName))
                return;

            // Get email (optional)
            string email = await DisplayPromptAsync("New User", "Enter email (optional):", initialValue: "");

            // Get role
            string[] roles = new[] { "Administrator", "Operations Manager", "Environmental Scientist" };
            string role = await DisplayActionSheet("Select role:", "Cancel", null, roles);
            if (role == "Cancel" || string.IsNullOrWhiteSpace(role))
                return;

            // Get password
            // NOTE: DisplayPromptAsync doesn't have isPassword param, use a different approach
            string password = await DisplayPromptAsync("New User", "Enter password (min 6 characters):", 
                initialValue: "", maxLength: 20, keyboard: Keyboard.Default);
            
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                await DisplayAlert("Error", "Password must be at least 6 characters.", "OK");
                return;
            }
            
            // Create user
            var newUser = new Models.User
            {
                Username = username,
                FullName = fullName,
                Email = email,
                Role = role,
                IsActive = true
            };
            
            bool success = await _userService.CreateUserAsync(newUser, password);
            
            if (success)
            {
                await DisplayAlert("Success", "User created successfully.", "OK");
                await LoadUsersAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to create user.", "OK");
            }
        }

        private async void OnEditUserClicked(object sender, EventArgs e)
        {
            if (SelectedUser == null)
                return;

            // Get updated full name
            string fullName = await DisplayPromptAsync("Edit User", "Enter full name:", initialValue: SelectedUser.FullName);
            if (string.IsNullOrWhiteSpace(fullName))
                return;

            // Get updated email (optional)
            string email = await DisplayPromptAsync("Edit User", "Enter email (optional):", initialValue: SelectedUser.Email ?? "");

            // Get updated role (only if not editing self)
            string updatedRole = SelectedUser.Role;
            if (SelectedUser.Id != _currentUserId)
            {
                string[] roles = new[] { "Administrator", "Operations Manager", "Environmental Scientist" };
                string role = await DisplayActionSheet("Select role:", "Cancel", null, roles);
                if (role != "Cancel" && !string.IsNullOrWhiteSpace(role))
                {
                    updatedRole = role;
                }
            }
            else if (updatedRole != "Administrator")
            {
                await DisplayAlert("Warning", "You cannot change your own role as you are currently logged in.", "OK");
            }

            // Update user
            var updatedUser = new Models.User
            {
                Id = SelectedUser.Id,
                Username = SelectedUser.Username,
                PasswordHash = SelectedUser.PasswordHash,
                FullName = fullName,
                Email = email,
                Role = updatedRole,
                IsActive = SelectedUser.IsActive,
                CreatedAt = SelectedUser.CreatedAt,
                LastLogin = SelectedUser.LastLogin
            };

            bool success = await _userService.UpdateUserAsync(updatedUser);

            if (success)
            {
                await DisplayAlert("Success", "User updated successfully.", "OK");
                await LoadUsersAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to update user.", "OK");
            }
        }

        private async void OnDeleteUserClicked(object sender, EventArgs e)
        {
            if (SelectedUser == null)
                return;

            // Prevent deletion of current user
            if (SelectedUser.Id == _currentUserId)
            {
                await DisplayAlert("Error", "You cannot delete your own account while logged in.", "OK");
                return;
            }

            // Confirmation
            bool confirm = await DisplayAlert("Confirm Delete", 
                $"Are you sure you want to delete user '{SelectedUser.Username}'? This cannot be undone.", 
                "Delete", "Cancel");

            if (!confirm)
                return;

            bool success = await _userService.DeleteUserAsync(SelectedUser.Id);

            if (success)
            {
                await DisplayAlert("Success", "User deleted successfully.", "OK");
                SelectedUser = null;
                await LoadUsersAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete user.", "OK");
            }
        }

        private async void OnResetPasswordClicked(object sender, EventArgs e)
        {
            if (SelectedUser == null)
                return;

            // Get new password - removed isPassword parameter
            string password = await DisplayPromptAsync("Reset Password", 
                $"Enter new password for {SelectedUser.Username} (min 6 characters):", 
                initialValue: "", maxLength: 20, keyboard: Keyboard.Default);

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                await DisplayAlert("Error", "Password must be at least 6 characters.", "OK");
                return;
            }

            // Confirmation
            bool confirm = await DisplayAlert("Confirm Password Reset", 
                $"Are you sure you want to reset the password for '{SelectedUser.Username}'?", 
                "Reset", "Cancel");

            if (!confirm)
                return;

            bool success = await _userService.UpdatePasswordAsync(SelectedUser.Id, password);

            if (success)
            {
                await DisplayAlert("Success", "Password reset successfully.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to reset password.", "OK");
            }
        }

        private async void OnToggleActiveStatusClicked(object sender, EventArgs e)
        {
            if (SelectedUser == null)
                return;

            // Prevent deactivating self
            if (SelectedUser.Id == _currentUserId)
            {
                await DisplayAlert("Error", "You cannot change your own active status while logged in.", "OK");
                return;
            }

            // Toggle active status
            var updatedUser = new Models.User
            {
                Id = SelectedUser.Id,
                Username = SelectedUser.Username,
                PasswordHash = SelectedUser.PasswordHash,
                FullName = SelectedUser.FullName,
                Email = SelectedUser.Email,
                Role = SelectedUser.Role,
                IsActive = !SelectedUser.IsActive,
                CreatedAt = SelectedUser.CreatedAt,
                LastLogin = SelectedUser.LastLogin
            };

            bool success = await _userService.UpdateUserAsync(updatedUser);

            if (success)
            {
                await DisplayAlert("Success", $"User is now {(updatedUser.IsActive ? "active" : "inactive")}.", "OK");
                await LoadUsersAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to update user status.", "OK");
            }
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                _allUsers = new ObservableCollection<Models.User>(users);
                FilterUsers();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
            }
        }

        private void FilterUsers()
        {
            string? selectedRole = RoleFilterPicker.SelectedItem?.ToString();
            
            if (string.IsNullOrEmpty(selectedRole) || selectedRole == "All Roles")
            {
                Users = new ObservableCollection<Models.User>(_allUsers);
            }
            else
            {
                Users = new ObservableCollection<Models.User>(_allUsers.Where(u => u.Role == selectedRole));
            }
            
            UserListView.ItemsSource = Users;
            
            // Preserve selection if possible
            if (SelectedUser != null)
            {
                var stillInList = Users.FirstOrDefault(u => u.Id == SelectedUser.Id);
                if (stillInList == null)
                {
                    SelectedUser = null;
                }
            }
        }

        // This addresses the conflict with Element.OnPropertyChanged
        public new event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}