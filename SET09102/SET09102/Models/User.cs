using System;

namespace SET09102.Models
{
    public enum UserRole
    {
        Admin,
        Scientist,
        OperationsManager
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public DateTime LastLogin { get; set; }
    }
}