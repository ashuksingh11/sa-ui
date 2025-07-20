using System;
using System.Threading.Tasks;
using SamsungAccountUI.Controllers.Base;
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Views.Navigation;

namespace SamsungAccountUI.Controllers.Authentication
{
    public class LogoutController : BaseController
    {
        public LogoutController(
            INavigationService navigationService,
            ISamsungAccountService accountService,
            IGlobalConfigService configService)
            : base(navigationService, accountService, configService)
        {
        }
        
        public override async Task LoadAsync()
        {
            // Load current user information for logout confirmation
            try
            {
                var currentUser = await AccountService.GetDefaultUserAsync();
                if (currentUser == null)
                {
                    // No user to logout, navigate back to login
                    await NavigateToScreen("QRLogin");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Failed to load user information: {ex.Message}");
            }
        }
        
        public override async Task HandleInputAsync(object input)
        {
            if (input is LogoutRequest request)
            {
                await ProcessLogout(request);
            }
            else if (input is string action)
            {
                await HandleAction(action);
            }
        }
        
        public async Task HandleLogout(string userId, string password = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                await ShowError("User ID is required for logout");
                return;
            }
            
            var request = new LogoutRequest
            {
                UserId = userId,
                Password = password,
                RequirePasswordConfirmation = ConfigService.RequirePasswordForLogout
            };
            
            await ProcessLogout(request);
        }
        
        public async Task HandleLogoutWithConfirmation(string userId)
        {
            try
            {
                var user = await AccountService.FetchProfileInfoAsync(userId);
                if (user == null)
                {
                    await ShowError("User not found");
                    return;
                }
                
                // Navigate to logout confirmation screen
                await NavigateToScreen("LogoutConfirm", user);
            }
            catch (Exception ex)
            {
                await ShowError($"Failed to prepare logout: {ex.Message}");
            }
        }
        
        public async Task HandleLogoutAll()
        {
            try
            {
                await ShowLoading("Signing out all users...");
                
                var allAccounts = await AccountService.GetAllAccountListAsync();
                bool hasErrors = false;
                
                foreach (var account in allAccounts)
                {
                    try
                    {
                        await AccountService.LogoutAsync(account.UserId);
                    }
                    catch (Exception ex)
                    {
                        hasErrors = true;
                        System.Diagnostics.Debug.WriteLine($"Failed to logout user {account.UserId}: {ex.Message}");
                    }
                }
                
                await HideLoading();
                
                if (hasErrors)
                {
                    await ShowError("Some users could not be logged out. Please try again.");
                }
                else
                {
                    // All users logged out successfully, navigate to login
                    await NavigationService.ReplaceCurrentAsync("QRLogin");
                }
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Failed to logout all users. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Logout all error: {ex.Message}");
            }
        }
        
        private async Task ProcessLogout(LogoutRequest request)
        {
            try
            {
                await ShowLoading("Signing out...");
                
                // Validate request
                if (string.IsNullOrEmpty(request.UserId))
                {
                    await HideLoading();
                    await ShowError("Invalid logout request");
                    return;
                }
                
                // Verify password if required
                if (request.RequirePasswordConfirmation && !string.IsNullOrEmpty(request.Password))
                {
                    var isPasswordValid = await AccountService.VerifyPasswordAsync(request.UserId, request.Password);
                    if (!isPasswordValid)
                    {
                        await HideLoading();
                        await ShowError("Invalid password. Please try again.");
                        return;
                    }
                }
                
                // Perform logout
                var success = await AccountService.LogoutAsync(request.UserId);
                
                await HideLoading();
                
                if (success)
                {
                    // Check if any accounts remain
                    var remainingAccounts = await AccountService.GetAllAccountListAsync();
                    if (remainingAccounts.Count > 0)
                    {
                        // Still have accounts, navigate to account info
                        await NavigationService.ReplaceCurrentAsync("AccountInfo");
                    }
                    else
                    {
                        // No accounts remain, navigate to login
                        await NavigationService.ReplaceCurrentAsync("QRLogin");
                    }
                }
                else
                {
                    await ShowError("Failed to logout. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Logout failed. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
            }
        }
        
        private async Task HandleAction(string action)
        {
            switch (action.ToLower())
            {
                case "confirm_logout":
                    // This would be called from the logout confirmation view
                    break;
                    
                case "cancel_logout":
                    await NavigateBack();
                    break;
                    
                case "logout_all_users":
                    await HandleLogoutAll();
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown logout action: {action}");
                    break;
            }
        }
        
        // Device-specific actions
        public override async Task OnDeviceSpecificAction(string action, object data)
        {
            switch (DeviceType)
            {
                case Models.Device.DeviceType.AIHome:
                    await HandleAIHomeAction(action, data);
                    break;
                case Models.Device.DeviceType.FamilyHub:
                    await HandleFamilyHubAction(action, data);
                    break;
            }
        }
        
        private async Task HandleAIHomeAction(string action, object data)
        {
            // AIHome-specific logout actions
            switch (action.ToLower())
            {
                case "quick_logout":
                    // Handle quick logout for AIHome devices (may skip password confirmation)
                    if (data is string userId)
                    {
                        var request = new LogoutRequest
                        {
                            UserId = userId,
                            RequirePasswordConfirmation = false
                        };
                        await ProcessLogout(request);
                    }
                    break;
            }
        }
        
        private async Task HandleFamilyHubAction(string action, object data)
        {
            // FamilyHub-specific logout actions
            switch (action.ToLower())
            {
                case "logout_with_animation":
                    // Handle logout with animation for FamilyHub
                    break;
            }
        }
    }
    
    // Helper class for logout requests
    public class LogoutRequest
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool RequirePasswordConfirmation { get; set; }
        public DateTime RequestTime { get; set; }
        
        public LogoutRequest()
        {
            UserId = string.Empty;
            Password = string.Empty;
            RequirePasswordConfirmation = true;
            RequestTime = DateTime.Now;
        }
    }
}