using System;

namespace SamsungAccountUI.Models.User
{
    public class SamsungAccount
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public bool IsActiveUser { get; set; }
        public DateTime LastLoginTime { get; set; }
        
        public SamsungAccount()
        {
            UserId = string.Empty;
            Email = string.Empty;
            DisplayName = string.Empty;
            ProfilePictureUrl = string.Empty;
            IsActiveUser = false;
            LastLoginTime = DateTime.Now;
        }
        
        public SamsungAccount(string userId, string email, string displayName)
        {
            UserId = userId ?? string.Empty;
            Email = email ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            ProfilePictureUrl = string.Empty;
            IsActiveUser = false;
            LastLoginTime = DateTime.Now;
        }
    }
}