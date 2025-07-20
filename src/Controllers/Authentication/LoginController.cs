using System;
using System.Threading.Tasks;
using SamsungAccountUI.Controllers.Base;
using SamsungAccountUI.Models.Authentication;
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Views.Navigation;

namespace SamsungAccountUI.Controllers.Authentication
{
    public class LoginController : BaseController
    {
        public LoginController(
            INavigationService navigationService,
            ISamsungAccountService accountService,
            IGlobalConfigService configService)
            : base(navigationService, accountService, configService)
        {
        }
        
        public override async Task LoadAsync()
        {
            try
            {
                // Check if any accounts already exist
                var existingAccounts = await AccountService.GetAllAccountListAsync();
                if (existingAccounts.Count > 0)
                {
                    // If accounts exist, get default user and navigate to account info
                    var defaultUser = await AccountService.GetDefaultUserAsync();
                    if (defaultUser != null)
                    {
                        await NavigateToScreen("AccountInfo");
                        return;
                    }
                }
                
                // No accounts exist, stay on login screen
                // The view should already be loaded by navigation service
            }
            catch (Exception ex)
            {
                await ShowError($"Failed to load account information: {ex.Message}");
            }
        }
        
        public override async Task HandleInputAsync(object input)
        {
            if (input is LoginRequest request)
            {
                await ProcessLogin(request);
            }
            else if (input is string action)
            {
                await HandleAction(action);
            }
        }
        
        public async Task HandleQRLogin(string qrToken)
        {
            if (string.IsNullOrEmpty(qrToken))
            {
                await ShowError("QR code is required for login");
                return;
            }
            
            var request = new LoginRequest(AuthenticationType.QR)
            {
                QRToken = qrToken
            };
            
            await ProcessLogin(request);
        }
        
        public async Task HandlePasswordLogin(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await ShowError("Email and password are required");
                return;
            }
            
            var request = new LoginRequest(AuthenticationType.Password)
            {
                Email = email,
                Password = password
            };
            
            await ProcessLogin(request);
        }
        
        public async Task HandleGoogleLogin(string googleToken)
        {
            if (string.IsNullOrEmpty(googleToken))
            {
                await ShowError("Google authentication token is required");
                return;
            }
            
            var request = new LoginRequest(AuthenticationType.Google)
            {
                GoogleToken = googleToken
            };
            
            await ProcessLogin(request);
        }
        
        private async Task ProcessLogin(LoginRequest request)
        {
            try
            {
                await ShowLoading("Signing in...");
                
                // Validate request
                if (!request.IsValid())
                {
                    await HideLoading();
                    await ShowError("Invalid login request. Please check your input.");
                    return;
                }
                
                // Perform login
                var result = await AccountService.LoginAsync(request);
                
                await HideLoading();
                
                if (result.IsSuccess && result.User != null)
                {
                    // Login successful, navigate to account info
                    await NavigationService.ReplaceCurrentAsync("AccountInfo", result.User);
                }
                else
                {
                    // Login failed, show error
                    var errorMessage = GetUserFriendlyErrorMessage(result.ErrorType, result.ErrorMessage);
                    await ShowError(errorMessage);
                }
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Login failed. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
        }
        
        private async Task HandleAction(string action)
        {
            switch (action.ToLower())
            {
                case "navigate_to_password":
                    if (ConfigService.IsPasswordLoginEnabled)
                    {
                        await NavigateToScreen("PasswordLogin");
                    }
                    else
                    {
                        await ShowError("Password login is not enabled");
                    }
                    break;
                    
                case "navigate_to_google":
                    if (ConfigService.IsGoogleLoginEnabled)
                    {
                        await NavigateToScreen("GoogleLogin");
                    }
                    else
                    {
                        await ShowError("Google login is not enabled");
                    }
                    break;
                    
                case "navigate_to_qr":
                    if (ConfigService.IsQRLoginEnabled)
                    {
                        await NavigateToScreen("QRLogin");
                    }
                    else
                    {
                        await ShowError("QR login is not enabled");
                    }
                    break;
                    
                case "refresh_qr":
                    await RefreshQRCode();
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown action: {action}");
                    break;
            }
        }
        
        private async Task RefreshQRCode()
        {
            try
            {
                await ShowLoading("Refreshing QR code...");
                
                // Simulate QR code refresh
                await Task.Delay(1000);
                
                await HideLoading();
                
                // Notify view to update QR code (this would be implemented in the view)
                // For now, we'll just log it
                System.Diagnostics.Debug.WriteLine("QR code refreshed");
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Failed to refresh QR code");
                System.Diagnostics.Debug.WriteLine($"QR refresh error: {ex.Message}");
            }
        }
        
        private string GetUserFriendlyErrorMessage(AuthenticationError errorType, string originalMessage)
        {
            return errorType switch
            {
                AuthenticationError.InvalidCredentials => "Invalid email or password. Please try again.",
                AuthenticationError.NetworkError => "Network connection failed. Please check your internet connection.",
                AuthenticationError.ServiceUnavailable => "Samsung Account service is temporarily unavailable. Please try again later.",
                AuthenticationError.InvalidQRCode => "Invalid or expired QR code. Please refresh and try again.",
                AuthenticationError.GoogleAuthFailed => "Google authentication failed. Please try again.",
                AuthenticationError.PasswordExpired => "Your password has expired. Please reset your password.",
                AuthenticationError.AccountLocked => "Your account is temporarily locked. Please try again later.",
                AuthenticationError.TooManyAttempts => "Too many login attempts. Please wait before trying again.",
                AuthenticationError.UnknownError => "An unexpected error occurred. Please try again.",
                _ => !string.IsNullOrEmpty(originalMessage) ? originalMessage : "Login failed. Please try again."
            };
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
            // AIHome-specific login actions
            switch (action.ToLower())
            {
                case "compact_view":
                    // Handle compact view requirements for AIHome
                    break;
                case "quick_login":
                    // Handle quick login for AIHome devices
                    if (data is string[] credentials && credentials.Length >= 2)
                    {
                        await HandlePasswordLogin(credentials[0], credentials[1]);
                    }
                    break;
            }
        }
        
        private async Task HandleFamilyHubAction(string action, object data)
        {
            // FamilyHub-specific login actions
            switch (action.ToLower())
            {
                case "expanded_view":
                    // Handle expanded view requirements for FamilyHub
                    break;
                case "multi_user_hint":
                    // Show multi-user capabilities hint
                    break;
            }
        }
    }
}