using System.Collections.Generic;
using System.Threading.Tasks;
using SamsungAccountUI.Models.Authentication;
using SamsungAccountUI.Models.User;

namespace SamsungAccountUI.Services.API
{
    public interface ISamsungAccountService
    {
        // Authentication APIs
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<bool> LogoutAsync(string userId);
        
        // Profile Management APIs  
        Task<SamsungAccount> FetchProfileInfoAsync(string userId);
        Task<List<SamsungAccount>> GetAllAccountListAsync();
        Task<List<SamsungAccount>> GetAllProfileInfoAsync();
        
        // Default User Management APIs
        Task<bool> SetDefaultUserAsync(string userId);
        Task<SamsungAccount> GetDefaultUserAsync();
        
        // Password Management APIs
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> VerifyPasswordAsync(string userId, string password);
        
        // Session Management APIs
        Task<bool> IsUserLoggedInAsync(string userId);
        Task<string> GetSessionTokenAsync(string userId);
        Task<bool> RefreshSessionAsync(string userId);
        
        // Account Validation APIs
        Task<bool> ValidateAccountAsync(string email);
        Task<bool> IsAccountLockedAsync(string userId);
    }
}