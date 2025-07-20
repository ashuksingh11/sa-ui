using System;
using System.Linq;
using System.Threading.Tasks;
using SamsungAccountUI.Controllers.Base;
using SamsungAccountUI.Models.User;
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Views.Navigation;

namespace SamsungAccountUI.Controllers.Account
{
    public class AccountInfoController : BaseController
    {
        private AccountState _currentAccountState;
        
        public AccountInfoController(
            INavigationService navigationService,
            ISamsungAccountService accountService,
            IGlobalConfigService configService)
            : base(navigationService, accountService, configService)
        {
        }
        
        public override async Task LoadAsync()
        {
            await LoadAccountInfo();
        }
        
        public override async Task HandleInputAsync(object input)
        {
            if (input is string action)
            {
                await HandleAction(action);
            }
            else if (input is AccountAction accountAction)
            {
                await HandleAccountAction(accountAction);
            }
        }
        
        public async Task LoadAccountInfo()
        {
            try
            {
                await ShowLoading("Loading account information...");
                
                // Get all accounts using Samsung Account API
                var allAccounts = await AccountService.GetAllAccountListAsync();
                var defaultUser = await AccountService.GetDefaultUserAsync();
                
                _currentAccountState = new AccountState
                {
                    AllAccounts = allAccounts,
                    ActiveUser = defaultUser
                };
                
                await HideLoading();
                
                // Update view with account information (this would be handled by the view)
                await UpdateView(_currentAccountState);
                
                // If no accounts found, navigate to login
                if (!_currentAccountState.HasAccounts)
                {
                    await NavigationService.ReplaceCurrentAsync("QRLogin");
                }
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Failed to load account information. Please try again.");
                System.Diagnostics.Debug.WriteLine($"LoadAccountInfo error: {ex.Message}");
            }
        }
        
        public async Task HandleUserSwitch(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                await ShowError("Invalid user selection");
                return;
            }
            
            try
            {
                await ShowLoading("Switching user...");
                
                var success = await AccountService.SetDefaultUserAsync(userId);
                
                await HideLoading();
                
                if (success)
                {
                    // Reload account info to reflect the change
                    await LoadAccountInfo();
                }
                else
                {
                    await ShowError("Failed to switch user. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Failed to switch user. Please try again.");
                System.Diagnostics.Debug.WriteLine($"User switch error: {ex.Message}");
            }
        }
        
        public async Task HandleUserProfileView(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                await ShowError("Invalid user selection");
                return;
            }
            
            try
            {
                await ShowLoading("Loading user profile...");
                
                var userProfile = await AccountService.FetchProfileInfoAsync(userId);
                
                await HideLoading();
                
                if (userProfile != null)
                {
                    // Navigate to profile detail view
                    await NavigateToScreen("ProfileDetail", userProfile);
                }
                else
                {
                    await ShowError("Failed to load user profile");
                }
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Failed to load user profile. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Profile view error: {ex.Message}");
            }
        }
        
        public async Task HandleLogoutUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                await ShowError("Invalid user selection");
                return;
            }
            
            try
            {
                var user = _currentAccountState.AllAccounts.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    // Navigate to logout confirmation with user info
                    await NavigateToScreen("LogoutConfirm", user);
                }
                else
                {
                    await ShowError("User not found");
                }
            }
            catch (Exception ex)
            {
                await ShowError("Failed to prepare logout. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Logout preparation error: {ex.Message}");
            }
        }
        
        public async Task HandleAddNewUser()
        {
            try
            {
                // Check if multi-user is enabled
                if (!ConfigService.IsMultiUserEnabled)
                {
                    await ShowError("Multi-user support is not enabled");
                    return;
                }
                
                // Check if we've reached the maximum number of users
                int maxUsers = ConfigService.MaxUserAccounts;
                if (_currentAccountState.AccountCount >= maxUsers)
                {
                    await ShowError($"Maximum number of users ({maxUsers}) reached");
                    return;
                }
                
                // Navigate to login screen to add new user
                await NavigateToScreen("QRLogin");
            }
            catch (Exception ex)
            {
                await ShowError("Failed to add new user. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Add user error: {ex.Message}");
            }
        }
        
        public async Task HandleChangePassword()
        {
            try
            {
                var activeUser = _currentAccountState.GetActiveUser();
                if (activeUser == null)
                {
                    await ShowError("No active user found");
                    return;
                }
                
                // Navigate to change password screen
                await NavigateToScreen("ChangePassword", activeUser);
            }
            catch (Exception ex)
            {
                await ShowError("Failed to open password change. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Change password error: {ex.Message}");
            }
        }
        
        private async Task HandleAction(string action)
        {
            switch (action.ToLower())
            {
                case "refresh_accounts":
                    await LoadAccountInfo();
                    break;
                    
                case "add_new_user":
                    await HandleAddNewUser();
                    break;
                    
                case "change_password":
                    await HandleChangePassword();
                    break;
                    
                case "logout_all":
                    await HandleLogoutAll();
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown account info action: {action}");
                    break;
            }
        }
        
        private async Task HandleAccountAction(AccountAction action)
        {
            switch (action.Type)
            {
                case AccountActionType.SwitchUser:
                    await HandleUserSwitch(action.UserId);
                    break;
                    
                case AccountActionType.ViewProfile:
                    await HandleUserProfileView(action.UserId);
                    break;
                    
                case AccountActionType.LogoutUser:
                    await HandleLogoutUser(action.UserId);
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown account action type: {action.Type}");
                    break;
            }
        }
        
        private async Task HandleLogoutAll()
        {
            try
            {
                if (_currentAccountState.AccountCount == 0)
                {
                    await ShowError("No users to logout");
                    return;
                }
                
                // Navigate to logout all confirmation
                await NavigateToScreen("LogoutConfirm", new { LogoutAll = true, Accounts = _currentAccountState.AllAccounts });
            }
            catch (Exception ex)
            {
                await ShowError("Failed to prepare logout all. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Logout all preparation error: {ex.Message}");
            }
        }
        
        private async Task UpdateView(AccountState accountState)
        {
            // This method would update the view with the current account state
            // Implementation would depend on the specific view framework
            // For now, we'll just log the state
            System.Diagnostics.Debug.WriteLine($"Account state updated: {accountState.AccountCount} accounts, Active: {accountState.ActiveUser?.DisplayName}");
            
            await Task.CompletedTask;
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
            // AIHome-specific account actions
            switch (action.ToLower())
            {
                case "compact_user_list":
                    // Handle compact user list display for AIHome
                    break;
                case "quick_switch":
                    // Handle quick user switching for AIHome
                    if (data is string userId)
                    {
                        await HandleUserSwitch(userId);
                    }
                    break;
            }
        }
        
        private async Task HandleFamilyHubAction(string action, object data)
        {
            // FamilyHub-specific account actions
            switch (action.ToLower())
            {
                case "expanded_user_cards":
                    // Handle expanded user cards display for FamilyHub
                    break;
                case "detailed_profiles":
                    // Handle detailed profile view for FamilyHub
                    break;
            }
        }
    }
    
    // Helper classes
    public class AccountAction
    {
        public AccountActionType Type { get; set; }
        public string UserId { get; set; }
        public object Data { get; set; }
        
        public AccountAction(AccountActionType type, string userId, object data = null)
        {
            Type = type;
            UserId = userId;
            Data = data;
        }
    }
    
    public enum AccountActionType
    {
        SwitchUser,
        ViewProfile,
        LogoutUser,
        ChangePassword
    }
}