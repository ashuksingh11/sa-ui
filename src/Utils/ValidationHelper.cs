using System;
using System.Linq;
using System.Text.RegularExpressions;
using SamsungAccountUI.Models.Authentication;
using SamsungAccountUI.Controllers.Authentication;

namespace SamsungAccountUI.Utils
{
    public static class ValidationHelper
    {
        // Email validation
        public static ValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ValidationResult.Invalid("Email is required");
            }
            
            if (email.Length > 254)
            {
                return ValidationResult.Invalid("Email is too long");
            }
            
            // Basic email regex pattern
            const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                return ValidationResult.Invalid("Please enter a valid email address");
            }
            
            return ValidationResult.Valid();
        }
        
        // Password validation
        public static ValidationResult ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return ValidationResult.Invalid("Password is required");
            }
            
            if (password.Length < 8)
            {
                return ValidationResult.Invalid("Password must be at least 8 characters long");
            }
            
            if (password.Length > 128)
            {
                return ValidationResult.Invalid("Password must be less than 128 characters long");
            }
            
            return ValidationResult.Valid();
        }
        
        // Strong password validation
        public static ValidationResult ValidatePasswordStrength(string password)
        {
            var basicValidation = ValidatePassword(password);
            if (!basicValidation.IsValid)
            {
                return basicValidation;
            }
            
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
            
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
        
        // QR token validation
        public static ValidationResult ValidateQRToken(string qrToken)
        {
            if (string.IsNullOrWhiteSpace(qrToken))
            {
                return ValidationResult.Invalid("QR token is required");
            }
            
            if (qrToken.Length < 10)
            {
                return ValidationResult.Invalid("Invalid QR token format");
            }
            
            // Check if QR token has valid format (starts with QR_ for mock implementation)
            if (!qrToken.StartsWith("QR_"))
            {
                return ValidationResult.Invalid("Invalid QR token format");
            }
            
            return ValidationResult.Valid();
        }
        
        // Google token validation
        public static ValidationResult ValidateGoogleToken(string googleToken)
        {
            if (string.IsNullOrWhiteSpace(googleToken))
            {
                return ValidationResult.Invalid("Google authentication token is required");
            }
            
            if (googleToken.Length < 15)
            {
                return ValidationResult.Invalid("Invalid Google token format");
            }
            
            // Check if Google token has valid format (starts with GOOGLE_ for mock implementation)
            if (!googleToken.StartsWith("GOOGLE_"))
            {
                return ValidationResult.Invalid("Invalid Google token format");
            }
            
            return ValidationResult.Valid();
        }
        
        // User ID validation
        public static ValidationResult ValidateUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ValidationResult.Invalid("User ID is required");
            }
            
            if (userId.Length < 3)
            {
                return ValidationResult.Invalid("User ID is too short");
            }
            
            if (userId.Length > 50)
            {
                return ValidationResult.Invalid("User ID is too long");
            }
            
            return ValidationResult.Valid();
        }
        
        // Display name validation
        public static ValidationResult ValidateDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return ValidationResult.Invalid("Display name is required");
            }
            
            if (displayName.Length < 1)
            {
                return ValidationResult.Invalid("Display name cannot be empty");
            }
            
            if (displayName.Length > 50)
            {
                return ValidationResult.Invalid("Display name is too long");
            }
            
            // Check for invalid characters
            const string invalidChars = "<>\"'&";
            if (displayName.Any(c => invalidChars.Contains(c)))
            {
                return ValidationResult.Invalid("Display name contains invalid characters");
            }
            
            return ValidationResult.Valid();
        }
        
        // Login request validation
        public static ValidationResult ValidateLoginRequest(LoginRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Invalid("Login request is required");
            }
            
            switch (request.Type)
            {
                case AuthenticationType.Password:
                    var emailValidation = ValidateEmail(request.Email);
                    if (!emailValidation.IsValid)
                    {
                        return emailValidation;
                    }
                    
                    var passwordValidation = ValidatePassword(request.Password);
                    if (!passwordValidation.IsValid)
                    {
                        return passwordValidation;
                    }
                    break;
                    
                case AuthenticationType.QR:
                    var qrValidation = ValidateQRToken(request.QRToken);
                    if (!qrValidation.IsValid)
                    {
                        return qrValidation;
                    }
                    break;
                    
                case AuthenticationType.Google:
                    var googleValidation = ValidateGoogleToken(request.GoogleToken);
                    if (!googleValidation.IsValid)
                    {
                        return googleValidation;
                    }
                    break;
                    
                default:
                    return ValidationResult.Invalid("Invalid authentication type");
            }
            
            return ValidationResult.Valid();
        }
        
        // URL validation (for profile pictures, etc.)
        public static ValidationResult ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return ValidationResult.Valid(); // URL is optional
            }
            
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri result))
            {
                return ValidationResult.Invalid("Invalid URL format");
            }
            
            if (result.Scheme != Uri.UriSchemeHttp && result.Scheme != Uri.UriSchemeHttps)
            {
                return ValidationResult.Invalid("URL must use HTTP or HTTPS");
            }
            
            return ValidationResult.Valid();
        }
        
        // Phone number validation (basic)
        public static ValidationResult ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return ValidationResult.Valid(); // Phone number is optional
            }
            
            // Remove common formatting characters
            var cleanNumber = Regex.Replace(phoneNumber, @"[\s\-\(\)\+]", "");
            
            if (cleanNumber.Length < 10 || cleanNumber.Length > 15)
            {
                return ValidationResult.Invalid("Phone number must be between 10 and 15 digits");
            }
            
            if (!cleanNumber.All(char.IsDigit))
            {
                return ValidationResult.Invalid("Phone number can only contain digits");
            }
            
            return ValidationResult.Valid();
        }
        
        // Date validation
        public static ValidationResult ValidateDateOfBirth(DateTime dateOfBirth)
        {
            var minDate = DateTime.Now.AddYears(-120);
            var maxDate = DateTime.Now.AddYears(-13); // Minimum age 13
            
            if (dateOfBirth < minDate)
            {
                return ValidationResult.Invalid("Invalid date of birth");
            }
            
            if (dateOfBirth > maxDate)
            {
                return ValidationResult.Invalid("Must be at least 13 years old");
            }
            
            return ValidationResult.Valid();
        }
        
        // Helper method to sanitize input strings
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }
            
            // Trim whitespace
            input = input.Trim();
            
            // Remove control characters
            input = Regex.Replace(input, @"[\x00-\x1F\x7F]", "");
            
            return input;
        }
        
        // Helper method to check if string is safe for UI display
        public static bool IsSafeForDisplay(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }
            
            // Check for potentially dangerous characters
            const string dangerousChars = "<>\"'&";
            return !input.Any(c => dangerousChars.Contains(c));
        }
    }
    
    // Note: ValidationResult class is defined in PasswordController.cs
}