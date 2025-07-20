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
    public class UserSwitchController : BaseController
    {
        private AccountState _accountState;
        private string _targetUserId;
        
        public UserSwitchController(
            INavigationService navigationService,
            ISamsungAccountService accountService,
            IGlobalConfigService configService)
            : base(navigationService, accountService, configService)
        {
        }
        
        public override async Task LoadAsync()
        {
            await LoadUserSwitchView();
        }
        
        public override async Task HandleInputAsync(object input)
        {
            if (input is UserSwitchRequest request)
            {
                await ProcessUserSwitch(request);
            }
            else if (input is string action)
            {
                await HandleAction(action);
            }
        }
        
        public async Task LoadUserSwitchView()
        {
            try
            {
                await ShowLoading("Loading users...");
                
                // Get all accounts
                var allAccounts = await AccountService.GetAllAccountListAsync();
                var activeUser = await AccountService.GetDefaultUserAsync();
                
                _accountState = new AccountState
                {
                    AllAccounts = allAccounts,
                    ActiveUser = activeUser
                };
                
                await HideLoading();
                
                // Check if multi-user is supported
                if (!ConfigService.IsMultiUserEnabled)
                {
                    await ShowError("Multi-user switching is not enabled");
                    await NavigateBack();
                    return;
                }
                
                // Check if there are users to switch to
                if (_accountState.AccountCount <= 1)
                {
                    await ShowError("No other users available to switch to");
                    await NavigateBack();
                    return;
                }
                
                // Update view with user list
                await UpdateUserSwitchView(_accountState);
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Failed to load user switch view. Please try again.");
                System.Diagnostics.Debug.WriteLine($"LoadUserSwitchView error: {ex.Message}");
            }
        }
        
        public async Task HandleUserSelection(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                await ShowError("Invalid user selection");
                return;
            }
            
            var user = _accountState.AllAccounts.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                await ShowError("Selected user not found");
                return;
            }
            
            if (user.IsActiveUser)
            {
                await ShowError("This user is already active");
                return;
            }
            
            _targetUserId = userId;
            
            // Check if password verification is required for user switching
            bool requirePassword = ConfigService.GetPreferenceValue("samsung.account.switch.require.password", false);
            
            if (requirePassword)
            {
                // Show password verification dialog
                await ShowPasswordVerification(user);
            }
            else
            {
                // Proceed with user switch directly
                var request = new UserSwitchRequest
                {
                    FromUserId = _accountState.ActiveUser?.UserId,
                    ToUserId = userId,
                    RequirePassword = false
                };
                
                await ProcessUserSwitch(request);
            }
        }
        
        public async Task HandleQuickSwitch(string userId)
        {
            // Quick switch without confirmation (for AIHome devices)
            if (string.IsNullOrEmpty(userId))
            {
                await ShowError("Invalid user selection");
                return;
            }
            
            var request = new UserSwitchRequest
            {
                FromUserId = _accountState.ActiveUser?.UserId,
                ToUserId = userId,
                RequirePassword = false,
                IsQuickSwitch = true
            };
            
            await ProcessUserSwitch(request);
        }
        
        private async Task ProcessUserSwitch(UserSwitchRequest request)
        {
            try
            {
                await ShowLoading("Switching user...");
                
                // Validate request
                if (string.IsNullOrEmpty(request.ToUserId))
                {
                    await HideLoading();
                    await ShowError("Invalid user switch request");
                    return;
                }
                
                // Verify password if required
                if (request.RequirePassword && !string.IsNullOrEmpty(request.Password))
                {
                    var isPasswordValid = await AccountService.VerifyPasswordAsync(request.ToUserId, request.Password);
                    if (!isPasswordValid)
                    {
                        await HideLoading();
                        await ShowError("Invalid password. Please try again.");
                        return;
                    }
                }
                
                // Perform user switch
                var success = await AccountService.SetDefaultUserAsync(request.ToUserId);
                
                await HideLoading();
                
                if (success)
                {
                    // User switch successful
                    if (request.IsQuickSwitch)
                    {
                        // For quick switch, go back to previous screen
                        await NavigateBack();
                    }
                    else
                    {
                        // Navigate to account info to show the switched user
                        await NavigationService.ReplaceCurrentAsync("AccountInfo");
                    }
                }
                else
                {
                    await ShowError("Failed to switch user. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("User switch failed. Please try again.");
                System.Diagnostics.Debug.WriteLine($"User switch error: {ex.Message}");
            }
        }
        
        private async Task ShowPasswordVerification(SamsungAccount user)
        {
            // This would show a password input dialog for the target user
            // For now, we'll simulate this
            await Task.CompletedTask;
            System.Diagnostics.Debug.WriteLine($"Showing password verification for user: {user.DisplayName}");
        }
        
        private async Task HandleAction(string action)
        {
            switch (action.ToLower())
            {
                case "cancel_switch":
                    await NavigateBack();
                    break;
                    
                case "refresh_users":
                    await LoadUserSwitchView();
                    break;
                    
                case "add_new_user":
                    await HandleAddNewUser();
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown user switch action: {action}");
                    break;
            }
        }
        
        private async Task HandleAddNewUser()
        {
            try
            {
                // Check if we can add more users
                int maxUsers = ConfigService.MaxUserAccounts;
                if (_accountState.AccountCount >= maxUsers)
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
                System.Diagnostics.Debug.WriteLine($"Add new user error: {ex.Message}");
            }
        }
        
        private async Task UpdateUserSwitchView(AccountState accountState)
        {
            // This method would update the view with the available users
            // Implementation would depend on the specific view framework
            System.Diagnostics.Debug.WriteLine($"User switch view updated: {accountState.AccountCount} users available");
            
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
            // AIHome-specific user switch actions
            switch (action.ToLower())
            {
                case "compact_user_grid":
                    // Handle compact user grid for AIHome
                    break;
                case "single_tap_switch":
                    // Handle single tap user switching for AIHome
                    if (data is string userId)
                    {
                        await HandleQuickSwitch(userId);
                    }
                    break;
            }
        }
        
        private async Task HandleFamilyHubAction(string action, object data)
        {
            // FamilyHub-specific user switch actions
            switch (action.ToLower())
            {
                case "expanded_user_cards":
                    // Handle expanded user cards for FamilyHub
                    break;
                case "profile_preview":
                    // Handle profile preview on selection for FamilyHub
                    if (data is string userId)
                    {
                        var user = _accountState.AllAccounts.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Showing profile preview for: {user.DisplayName}");
                        }
                    }
                    break;
            }
        }
    }
    
    // Helper class for user switch requests
    public class UserSwitchRequest
    {
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string Password { get; set; }
        public bool RequirePassword { get; set; }
        public bool IsQuickSwitch { get; set; }
        public DateTime RequestTime { get; set; }
        
        public UserSwitchRequest()
        {
            FromUserId = string.Empty;
            ToUserId = string.Empty;
            Password = string.Empty;
            RequirePassword = false;
            IsQuickSwitch = false;
            RequestTime = DateTime.Now;
        }
    }
}