using System;

namespace SamsungAccountUI.Models.User
{
    public class UserProfile
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePictureUrl { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        public UserProfile()
        {
            UserId = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            ProfilePictureUrl = string.Empty;
            Country = string.Empty;
            Language = "en-US";
            CreatedDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
        }
    }
}