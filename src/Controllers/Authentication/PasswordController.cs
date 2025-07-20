using System;
using System.Threading.Tasks;
using SamsungAccountUI.Controllers.Base;
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Views.Navigation;

namespace SamsungAccountUI.Controllers.Authentication
{
    public class PasswordController : BaseController
    {
        public PasswordController(
            INavigationService navigationService,
            ISamsungAccountService accountService,
            IGlobalConfigService configService)
            : base(navigationService, accountService, configService)
        {
        }
        
        public override async Task LoadAsync()
        {
            // Initialize password change view
            try
            {
                var currentUser = await AccountService.GetDefaultUserAsync();
                if (currentUser == null)
                {
                    await ShowError("No active user found");
                    await NavigateBack();
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Failed to load user information: {ex.Message}");
            }
        }
        
        public override async Task HandleInputAsync(object input)
        {
            if (input is PasswordChangeRequest request)
            {
                await ProcessPasswordChange(request);
            }
            else if (input is string action)
            {
                await HandleAction(action);
            }
        }
        
        public async Task HandlePasswordChange(string userId, string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(userId))
            {
                await ShowError("User ID is required");
                return;
            }
            
            var request = new PasswordChangeRequest
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };
            
            await ProcessPasswordChange(request);
        }
        
        public async Task HandlePasswordVerification(string userId, string password)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                await ShowError("User ID and password are required");
                return;
            }
            
            try
            {
                await ShowLoading("Verifying password...");
                
                var isValid = await AccountService.VerifyPasswordAsync(userId, password);
                
                await HideLoading();
                
                if (isValid)
                {
                    // Password verified successfully
                    // This could be used for logout confirmation or other secure operations
                    System.Diagnostics.Debug.WriteLine("Password verification successful");
                }
                else
                {
                    await ShowError("Invalid password. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Password verification failed. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Password verification error: {ex.Message}");
            }
        }
        
        private async Task ProcessPasswordChange(PasswordChangeRequest request)
        {
            try
            {
                await ShowLoading("Changing password...");
                
                // Validate request
                var validationResult = ValidatePasswordChangeRequest(request);
                if (!validationResult.IsValid)
                {
                    await HideLoading();
                    await ShowError(validationResult.ErrorMessage);
                    return;
                }
                
                // Change password
                var success = await AccountService.ChangePasswordAsync(
                    request.UserId, 
                    request.CurrentPassword, 
                    request.NewPassword);
                
                await HideLoading();
                
                if (success)
                {
                    // Password changed successfully, navigate to success screen or back
                    await NavigationService.NavigateToAsync("Success", "Password changed successfully");
                }
                else
                {
                    await ShowError("Failed to change password. Please verify your current password and try again.");
                }
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Password change failed. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Password change error: {ex.Message}");
            }
        }
        
        private ValidationResult ValidatePasswordChangeRequest(PasswordChangeRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return ValidationResult.Invalid("User ID is required");
            }
            
            if (string.IsNullOrEmpty(request.CurrentPassword))
            {
                return ValidationResult.Invalid("Current password is required");
            }
            
            if (string.IsNullOrEmpty(request.NewPassword))
            {
                return ValidationResult.Invalid("New password is required");
            }
            
            if (string.IsNullOrEmpty(request.ConfirmPassword))
            {
                return ValidationResult.Invalid("Password confirmation is required");
            }
            
            if (request.NewPassword != request.ConfirmPassword)
            {
                return ValidationResult.Invalid("New password and confirmation do not match");
            }
            
            if (request.CurrentPassword == request.NewPassword)
            {
                return ValidationResult.Invalid("New password must be different from current password");
            }
            
            // Validate password strength
            var strengthValidation = ValidatePasswordStrength(request.NewPassword);
            if (!strengthValidation.IsValid)
            {
                return strengthValidation;
            }
            
            return ValidationResult.Valid();
        }
        
        private ValidationResult ValidatePasswordStrength(string password)
        {
            if (password.Length < 8)
            {
                return ValidationResult.Invalid("Password must be at least 8 characters long");
            }
            
            if (password.Length > 128)
            {
                return ValidationResult.Invalid("Password must be less than 128 characters long");
            }
            
            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSpecial = false;
            
            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsWhiteSpace(c)) hasSpecial = true;
            }
            
            if (!hasUpper)
            {
                return ValidationResult.Invalid("Password must contain at least one uppercase letter");
            }
            
            if (!hasLower)
            {
                return ValidationResult.Invalid("Password must contain at least one lowercase letter");
            }
            
            if (!hasDigit)
            {
                return ValidationResult.Invalid("Password must contain at least one number");
            }
            
            if (!hasSpecial)
            {
                return ValidationResult.Invalid("Password must contain at least one special character");
            }
            
            return ValidationResult.Valid();
        }
        
        private async Task HandleAction(string action)
        {
            switch (action.ToLower())
            {
                case "cancel_password_change":
                    await NavigateBack();
                    break;
                    
                case "show_password_requirements":
                    await ShowPasswordRequirements();
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown password action: {action}");
                    break;
            }
        }
        
        private async Task ShowPasswordRequirements()
        {
            var requirements = "Password requirements:\n" +
                             "• At least 8 characters long\n" +
                             "• Contains uppercase and lowercase letters\n" +
                             "• Contains at least one number\n" +
                             "• Contains at least one special character";
            
            await ShowError(requirements, "Password Requirements");
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
            // AIHome-specific password actions
            switch (action.ToLower())
            {
                case "simple_password_view":
                    // Handle simplified password change view for AIHome
                    break;
            }
        }
        
        private async Task HandleFamilyHubAction(string action, object data)
        {
            // FamilyHub-specific password actions
            switch (action.ToLower())
            {
                case "detailed_password_view":
                    // Handle detailed password change view for FamilyHub
                    break;
            }
        }
    }
    
    // Helper classes
    public class PasswordChangeRequest
    {
        public string UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public DateTime RequestTime { get; set; }
        
        public PasswordChangeRequest()
        {
            UserId = string.Empty;
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            RequestTime = DateTime.Now;
        }
    }
    
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        
        public static ValidationResult Valid()
        {
            return new ValidationResult { IsValid = true, ErrorMessage = string.Empty };
        }
        
        public static ValidationResult Invalid(string errorMessage)
        {
            return new ValidationResult { IsValid = false, ErrorMessage = errorMessage };
        }
    }
}