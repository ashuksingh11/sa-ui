using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SamsungAccountUI.Models.Authentication;
using SamsungAccountUI.Models.User;
using SamsungAccountUI.Services.API;

namespace SamsungAccountUI.Services.Mock
{
    public class MockSamsungAccountService : ISamsungAccountService
    {
        private readonly List<SamsungAccount> _mockAccounts;
        private readonly Dictionary<string, string> _mockPasswords;
        private readonly Dictionary<string, string> _mockSessions;
        
        public MockSamsungAccountService()
        {
            _mockAccounts = new List<SamsungAccount>
            {
                new SamsungAccount 
                { 
                    UserId = "user1", 
                    Email = "john@samsung.com", 
                    DisplayName = "John Doe", 
                    IsActiveUser = true,
                    ProfilePictureUrl = "https://example.com/profile/user1.jpg",
                    LastLoginTime = DateTime.Now.AddHours(-2)
                },
                new SamsungAccount 
                { 
                    UserId = "user2", 
                    Email = "jane@samsung.com", 
                    DisplayName = "Jane Smith", 
                    IsActiveUser = false,
                    ProfilePictureUrl = "https://example.com/profile/user2.jpg",
                    LastLoginTime = DateTime.Now.AddDays(-1)
                },
                new SamsungAccount 
                { 
                    UserId = "user3", 
                    Email = "bob@samsung.com", 
                    DisplayName = "Bob Johnson", 
                    IsActiveUser = false,
                    ProfilePictureUrl = "https://example.com/profile/user3.jpg",
                    LastLoginTime = DateTime.Now.AddDays(-3)
                }
            };
            
            _mockPasswords = new Dictionary<string, string>
            {
                { "user1", "password123" },
                { "user2", "securepass456" },
                { "user3", "mypassword789" },
                { "test@samsung.com", "testpass123" }
            };
            
            _mockSessions = new Dictionary<string, string>();
        }
        
        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            await Task.Delay(1000); // Simulate network delay
            
            try
            {
                switch (request.Type)
                {
                    case AuthenticationType.Password:
                        return await HandlePasswordLogin(request);
                    case AuthenticationType.QR:
                        return await HandleQRLogin(request);
                    case AuthenticationType.Google:
                        return await HandleGoogleLogin(request);
                    default:
                        return LoginResult.Failure(AuthenticationError.UnknownError, "Unsupported authentication type");
                }
            }
            catch (Exception ex)
            {
                return LoginResult.Failure(AuthenticationError.ServiceUnavailable, $"Login service error: {ex.Message}");
            }
        }
        
        private async Task<LoginResult> HandlePasswordLogin(LoginRequest request)
        {
            await Task.Delay(500);
            
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return LoginResult.Failure(AuthenticationError.InvalidCredentials, "Email and password are required");
            }
            
            // Check if password is correct
            if (_mockPasswords.ContainsKey(request.Email) && _mockPasswords[request.Email] == request.Password)
            {
                // Find existing user or create new one
                var user = _mockAccounts.FirstOrDefault(u => u.Email == request.Email);
                if (user == null)
                {
                    user = new SamsungAccount
                    {
                        UserId = Guid.NewGuid().ToString(),
                        Email = request.Email,
                        DisplayName = request.Email.Split('@')[0],
                        IsActiveUser = true,
                        LastLoginTime = DateTime.Now
                    };
                    _mockAccounts.Add(user);
                }
                else
                {
                    user.LastLoginTime = DateTime.Now;
                    SetActiveUser(user.UserId);
                }
                
                var sessionToken = GenerateSessionToken(user.UserId);
                return LoginResult.Success(user, sessionToken);
            }
            
            return LoginResult.Failure(AuthenticationError.InvalidCredentials, "Invalid email or password");
        }
        
        private async Task<LoginResult> HandleQRLogin(LoginRequest request)
        {
            await Task.Delay(800);
            
            if (string.IsNullOrEmpty(request.QRToken))
            {
                return LoginResult.Failure(AuthenticationError.InvalidQRCode, "QR token is required");
            }
            
            // Simulate QR token validation (in real implementation, this would validate with server)
            if (request.QRToken.StartsWith("QR_") && request.QRToken.Length > 10)
            {
                // Create a mock user for QR login
                var user = new SamsungAccount
                {
                    UserId = Guid.NewGuid().ToString(),
                    Email = "qruser@samsung.com",
                    DisplayName = "QR User",
                    IsActiveUser = true,
                    LastLoginTime = DateTime.Now
                };
                
                _mockAccounts.Add(user);
                var sessionToken = GenerateSessionToken(user.UserId);
                return LoginResult.Success(user, sessionToken);
            }
            
            return LoginResult.Failure(AuthenticationError.InvalidQRCode, "Invalid QR code");
        }
        
        private async Task<LoginResult> HandleGoogleLogin(LoginRequest request)
        {
            await Task.Delay(1200);
            
            if (string.IsNullOrEmpty(request.GoogleToken))
            {
                return LoginResult.Failure(AuthenticationError.GoogleAuthFailed, "Google token is required");
            }
            
            // Simulate Google OAuth validation
            if (request.GoogleToken.StartsWith("GOOGLE_") && request.GoogleToken.Length > 15)
            {
                var user = new SamsungAccount
                {
                    UserId = Guid.NewGuid().ToString(),
                    Email = "googleuser@gmail.com",
                    DisplayName = "Google User",
                    IsActiveUser = true,
                    LastLoginTime = DateTime.Now
                };
                
                _mockAccounts.Add(user);
                var sessionToken = GenerateSessionToken(user.UserId);
                return LoginResult.Success(user, sessionToken);
            }
            
            return LoginResult.Failure(AuthenticationError.GoogleAuthFailed, "Google authentication failed");
        }
        
        public async Task<bool> LogoutAsync(string userId)
        {
            await Task.Delay(400);
            
            var user = _mockAccounts.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                _mockAccounts.Remove(user);
                _mockSessions.Remove(userId);
                
                // If this was the active user, set another user as active
                if (user.IsActiveUser && _mockAccounts.Any())
                {
                    _mockAccounts.First().IsActiveUser = true;
                }
                
                return true;
            }
            
            return false;
        }
        
        public async Task<List<SamsungAccount>> GetAllAccountListAsync()
        {
            await Task.Delay(500);
            return _mockAccounts.ToList();
        }
        
        public async Task<SamsungAccount> GetDefaultUserAsync()
        {
            await Task.Delay(300);
            return _mockAccounts.FirstOrDefault(u => u.IsActiveUser);
        }
        
        public async Task<bool> SetDefaultUserAsync(string userId)
        {
            await Task.Delay(400);
            
            // Reset all users to non-active
            _mockAccounts.ForEach(u => u.IsActiveUser = false);
            
            // Set specified user as active
            var user = _mockAccounts.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                user.IsActiveUser = true;
                return true;
            }
            
            return false;
        }
        
        public async Task<SamsungAccount> FetchProfileInfoAsync(string userId)
        {
            await Task.Delay(600);
            return _mockAccounts.FirstOrDefault(u => u.UserId == userId);
        }
        
        public async Task<List<SamsungAccount>> GetAllProfileInfoAsync()
        {
            await Task.Delay(800);
            return _mockAccounts.Select(account => new SamsungAccount
            {
                UserId = account.UserId,
                Email = account.Email,
                DisplayName = account.DisplayName,
                ProfilePictureUrl = account.ProfilePictureUrl ?? $"https://example.com/profile/{account.UserId}.jpg",
                IsActiveUser = account.IsActiveUser,
                LastLoginTime = account.LastLoginTime
            }).ToList();
        }
        
        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            await Task.Delay(600);
            
            var user = _mockAccounts.FirstOrDefault(u => u.UserId == userId);
            if (user != null && _mockPasswords.ContainsKey(user.Email))
            {
                if (_mockPasswords[user.Email] == currentPassword)
                {
                    _mockPasswords[user.Email] = newPassword;
                    return true;
                }
            }
            
            return false;
        }
        
        public async Task<bool> VerifyPasswordAsync(string userId, string password)
        {
            await Task.Delay(300);
            
            var user = _mockAccounts.FirstOrDefault(u => u.UserId == userId);
            if (user != null && _mockPasswords.ContainsKey(user.Email))
            {
                return _mockPasswords[user.Email] == password;
            }
            
            return false;
        }
        
        public async Task<bool> IsUserLoggedInAsync(string userId)
        {
            await Task.Delay(200);
            return _mockAccounts.Any(u => u.UserId == userId) && _mockSessions.ContainsKey(userId);
        }
        
        public async Task<string> GetSessionTokenAsync(string userId)
        {
            await Task.Delay(200);
            return _mockSessions.GetValueOrDefault(userId, string.Empty);
        }
        
        public async Task<bool> RefreshSessionAsync(string userId)
        {
            await Task.Delay(400);
            
            if (_mockSessions.ContainsKey(userId))
            {
                _mockSessions[userId] = GenerateSessionToken(userId);
                return true;
            }
            
            return false;
        }
        
        public async Task<bool> ValidateAccountAsync(string email)
        {
            await Task.Delay(500);
            // Simulate account validation (check if email exists in system)
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        }
        
        public async Task<bool> IsAccountLockedAsync(string userId)
        {
            await Task.Delay(300);
            // Mock implementation - no accounts are locked in this simulation
            return false;
        }
        
        private void SetActiveUser(string userId)
        {
            _mockAccounts.ForEach(u => u.IsActiveUser = false);
            var user = _mockAccounts.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                user.IsActiveUser = true;
            }
        }
        
        private string GenerateSessionToken(string userId)
        {
            var token = $"SESSION_{userId}_{DateTime.Now.Ticks}";
            _mockSessions[userId] = token;
            return token;
        }
    }
}