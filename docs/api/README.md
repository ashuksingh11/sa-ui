# API Documentation

## 📋 Overview

This document provides comprehensive API documentation for all services, interfaces, and data contracts in the Samsung Account UI application. It includes method signatures, parameters, return values, and usage examples.

## 🔧 Service APIs

### Samsung Account Service API

#### Interface: `ISamsungAccountService`

The primary service interface for all Samsung Account operations.

```csharp
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
```

#### Authentication Methods

##### `LoginAsync(LoginRequest request)`

Authenticates a user using various authentication methods.

**Parameters:**
- `request` (LoginRequest): Authentication request containing credentials

**Returns:**
- `Task<LoginResult>`: Authentication result with user information or error details

**Usage Examples:**

```csharp
// QR Login
var qrRequest = new LoginRequest(AuthenticationType.QR)
{
    QRToken = "QR_abc123def456",
    DeviceId = "device123"
};
var result = await accountService.LoginAsync(qrRequest);

// Password Login
var passwordRequest = new LoginRequest(AuthenticationType.Password)
{
    Email = "user@samsung.com",
    Password = "securePassword123",
    DeviceId = "device123"
};
var result = await accountService.LoginAsync(passwordRequest);

// Google OAuth Login
var googleRequest = new LoginRequest(AuthenticationType.Google)
{
    GoogleToken = "GOOGLE_oauth_token_xyz",
    DeviceId = "device123"
};
var result = await accountService.LoginAsync(googleRequest);
```

**Error Handling:**
```csharp
if (result.IsSuccess)
{
    var user = result.User;
    var sessionToken = result.SessionToken;
    // Handle successful login
}
else
{
    switch (result.ErrorType)
    {
        case AuthenticationError.InvalidCredentials:
            // Show invalid credentials error
            break;
        case AuthenticationError.NetworkError:
            // Show network error with retry option
            break;
        case AuthenticationError.ServiceUnavailable:
            // Show service unavailable error
            break;
    }
}
```

##### `LogoutAsync(string userId)`

Logs out a specific user and cleans up their session.

**Parameters:**
- `userId` (string): Unique identifier of the user to logout

**Returns:**
- `Task<bool>`: True if logout successful, false otherwise

**Usage Example:**
```csharp
try
{
    var success = await accountService.LogoutAsync("user123");
    if (success)
    {
        // Handle successful logout
        // Check if any users remain
        var remainingUsers = await accountService.GetAllAccountListAsync();
        if (remainingUsers.Count == 0)
        {
            // Navigate to login screen
        }
        else
        {
            // Navigate to account info with remaining users
        }
    }
}
catch (Exception ex)
{
    // Handle logout error
}
```

#### Profile Management Methods

##### `GetAllAccountListAsync()`

Retrieves all logged-in user accounts on the device.

**Returns:**
- `Task<List<SamsungAccount>>`: List of all user accounts

**Usage Example:**
```csharp
var accounts = await accountService.GetAllAccountListAsync();
foreach (var account in accounts)
{
    Console.WriteLine($"User: {account.DisplayName}, Active: {account.IsActiveUser}");
}
```

##### `GetDefaultUserAsync()`

Gets the currently active/default user.

**Returns:**
- `Task<SamsungAccount>`: The active user account, null if none

**Usage Example:**
```csharp
var activeUser = await accountService.GetDefaultUserAsync();
if (activeUser != null)
{
    Console.WriteLine($"Active user: {activeUser.DisplayName}");
}
```

##### `SetDefaultUserAsync(string userId)`

Sets a user as the active/default user.

**Parameters:**
- `userId` (string): User ID to set as active

**Returns:**
- `Task<bool>`: True if successful, false otherwise

**Usage Example:**
```csharp
var success = await accountService.SetDefaultUserAsync("user456");
if (success)
{
    // Refresh UI to show new active user
    await LoadAccountInfo();
}
```

#### Password Management Methods

##### `ChangePasswordAsync(string userId, string currentPassword, string newPassword)`

Changes a user's password after verifying the current password.

**Parameters:**
- `userId` (string): Target user ID
- `currentPassword` (string): Current password for verification
- `newPassword` (string): New password to set

**Returns:**
- `Task<bool>`: True if password changed successfully

**Usage Example:**
```csharp
var success = await accountService.ChangePasswordAsync(
    "user123", 
    "oldPassword", 
    "newSecurePassword123!"
);

if (success)
{
    // Show success message
    await ShowSuccess("Password changed successfully");
}
else
{
    // Show error - likely current password incorrect
    await ShowError("Failed to change password. Please verify current password.");
}
```

##### `VerifyPasswordAsync(string userId, string password)`

Verifies a user's password without changing it.

**Parameters:**
- `userId` (string): User ID to verify
- `password` (string): Password to verify

**Returns:**
- `Task<bool>`: True if password is correct

**Usage Example:**
```csharp
// For logout confirmation
var isValid = await accountService.VerifyPasswordAsync("user123", enteredPassword);
if (isValid)
{
    // Proceed with logout
    await ProcessLogout();
}
else
{
    await ShowError("Incorrect password");
}
```

### Configuration Service API

#### Interface: `IGlobalConfigService`

Manages application configuration and Tizen preferences.

```csharp
namespace SamsungAccountUI.Services.Config
{
    public interface IGlobalConfigService
    {
        // Generic preference methods
        T GetPreferenceValue<T>(string key, T defaultValue = default(T));
        bool SetPreferenceValue<T>(string key, T value);
        bool HasPreferenceKey(string key);
        
        // Samsung Account UI preferences
        bool IsMultiUserEnabled { get; }
        bool IsQRLoginEnabled { get; }
        bool IsPasswordLoginEnabled { get; }
        bool IsGoogleLoginEnabled { get; }
        string DefaultUITheme { get; }
        int MaxUserAccounts { get; }
        bool RequirePasswordForLogout { get; }
        bool EnableLargeText { get; }
        
        // Device-specific preferences
        bool IsAIHomeDevice { get; }
        bool IsFamilyHubDevice { get; }
        int ScreenWidth { get; }
        int ScreenHeight { get; }
        
        // Security preferences
        int LoginTimeoutMinutes { get; }
        int MaxLoginAttempts { get; }
        bool EnableSessionTimeout { get; }
        
        // UI preferences
        string PreferredLanguage { get; }
        bool ShowUserProfiles { get; }
        bool EnableAnimations { get; }
    }
}
```

#### Configuration Methods

##### `GetPreferenceValue<T>(string key, T defaultValue)`

Gets a typed preference value with fallback to default.

**Parameters:**
- `key` (string): Preference key
- `defaultValue` (T): Default value if key not found

**Returns:**
- `T`: The preference value or default

**Usage Examples:**
```csharp
// Get boolean preference
bool multiUserEnabled = configService.GetPreferenceValue("samsung.account.multiuser.enabled", true);

// Get integer preference
int maxUsers = configService.GetPreferenceValue("samsung.account.max.users", 6);

// Get string preference
string theme = configService.GetPreferenceValue("samsung.ui.theme", "dark");
```

##### Common Configuration Properties

```csharp
// Check if features are enabled
if (configService.IsMultiUserEnabled)
{
    // Show multi-user interface
}

if (configService.IsQRLoginEnabled)
{
    // Show QR login option
}

// Device-specific behavior
if (configService.IsAIHomeDevice)
{
    // Apply compact layout
    ApplyCompactLayout();
}
else if (configService.IsFamilyHubDevice)
{
    // Apply expanded layout
    ApplyExpandedLayout();
}

// Security settings
var timeoutMinutes = configService.LoginTimeoutMinutes;
var maxAttempts = configService.MaxLoginAttempts;
```

### Device Detection Service API

#### Interface: `IDeviceDetectionService`

Detects device capabilities and optimizes behavior accordingly.

```csharp
namespace SamsungAccountUI.Services.Device
{
    public interface IDeviceDetectionService
    {
        DeviceInfo GetCurrentDeviceInfo();
        DeviceType DetectDeviceType();
        DeviceCapabilities GetDeviceCapabilities();
        ScreenDimensions GetScreenDimensions();
        bool IsDeviceSupported();
        string GetDeviceModel();
        string GetOSVersion();
        bool HasFeature(string feature);
        
        // Device-specific detection
        bool IsAIHomeDevice();
        bool IsFamilyHubDevice();
        bool IsWashingMachine();
        bool IsDryer();
        bool IsRefrigerator();
        
        // Screen and orientation detection
        bool IsHorizontalOrientation();
        bool IsVerticalOrientation();
        bool IsSmallScreen();
        bool IsLargeScreen();
        
        // Capability detection
        bool SupportsTouchInput();
        bool SupportsMultiUser();
        bool HasNetworkConnection();
        bool HasCamera();
    }
}
```

#### Device Detection Methods

##### `GetCurrentDeviceInfo()`

Gets comprehensive device information.

**Returns:**
- `DeviceInfo`: Complete device information object

**Usage Example:**
```csharp
var deviceInfo = deviceDetectionService.GetCurrentDeviceInfo();
Console.WriteLine($"Device: {deviceInfo.ModelName}");
Console.WriteLine($"Type: {deviceInfo.Type}");
Console.WriteLine($"Screen: {deviceInfo.Dimensions.Width}x{deviceInfo.Dimensions.Height}");
Console.WriteLine($"Multi-user: {deviceInfo.SupportsMultiUser}");
```

##### Device Type Detection

```csharp
// Check device type
if (deviceDetectionService.IsAIHomeDevice())
{
    // Configure for washing machine/dryer
    ConfigureForCompactDevice();
}
else if (deviceDetectionService.IsFamilyHubDevice())
{
    // Configure for refrigerator
    ConfigureForLargeDevice();
}

// Check specific appliance types
if (deviceDetectionService.IsWashingMachine())
{
    // Washing machine specific features
}
else if (deviceDetectionService.IsRefrigerator())
{
    // Refrigerator specific features
}
```

##### Screen and Capability Detection

```csharp
// Screen optimization
if (deviceDetectionService.IsSmallScreen())
{
    // Use compact UI elements
    ApplyCompactLayout();
}

if (deviceDetectionService.IsHorizontalOrientation())
{
    // Optimize for landscape
    ApplyHorizontalLayout();
}

// Feature detection
if (deviceDetectionService.HasCamera())
{
    // Enable QR scanning
    EnableQRScanning();
}

if (deviceDetectionService.SupportsMultiUser())
{
    // Enable multi-user features
    EnableUserSwitching();
}
```

### Navigation Service API

#### Interface: `INavigationService`

Manages screen navigation and application flow.

```csharp
namespace SamsungAccountUI.Views.Navigation
{
    public interface INavigationService
    {
        Task NavigateToAsync(string screenName, object parameters = null);
        Task NavigateBackAsync();
        Task ReplaceCurrentAsync(string screenName, object parameters = null);
        Task ShowLoadingAsync(string message = "Loading...");
        Task HideLoadingAsync();
        Task ShowErrorAsync(string message, string title = "Error");
        
        bool CanNavigateBack { get; }
        string CurrentScreen { get; }
        
        event EventHandler<NavigationEventArgs> NavigationChanged;
    }
}
```

#### Navigation Methods

##### `NavigateToAsync(string screenName, object parameters)`

Navigates to a new screen with optional parameters.

**Parameters:**
- `screenName` (string): Target screen name
- `parameters` (object): Optional navigation parameters

**Usage Examples:**
```csharp
// Simple navigation
await navigationService.NavigateToAsync("AccountInfo");

// Navigation with parameters
await navigationService.NavigateToAsync("LogoutConfirm", currentUser);

// Navigation with complex parameters
await navigationService.NavigateToAsync("UserSwitch", new { 
    CurrentUserId = "user123",
    AllowQuickSwitch = true 
});
```

##### Loading and Error Management

```csharp
// Show loading during operations
await navigationService.ShowLoadingAsync("Signing in...");
try
{
    var result = await accountService.LoginAsync(request);
    await navigationService.HideLoadingAsync();
    
    if (result.IsSuccess)
    {
        await navigationService.NavigateToAsync("AccountInfo");
    }
    else
    {
        await navigationService.ShowErrorAsync("Login failed", "Authentication Error");
    }
}
catch (Exception ex)
{
    await navigationService.HideLoadingAsync();
    await navigationService.ShowErrorAsync("An unexpected error occurred");
}
```

## 📊 Data Transfer Objects

### Core Models

#### `SamsungAccount`

Represents a Samsung user account.

```csharp
public class SamsungAccount
{
    public string UserId { get; set; }              // Unique user identifier
    public string Email { get; set; }               // User's email address
    public string DisplayName { get; set; }         // Display name
    public string ProfilePictureUrl { get; set; }   // Profile picture URL
    public bool IsActiveUser { get; set; }          // Currently active user flag
    public DateTime LastLoginTime { get; set; }     // Last login timestamp
}
```

#### `LoginRequest`

Authentication request data.

```csharp
public class LoginRequest
{
    public AuthenticationType Type { get; set; }    // QR, Password, Google
    public string Email { get; set; }               // For password auth
    public string Password { get; set; }            // For password auth
    public string QRToken { get; set; }             // For QR auth
    public string GoogleToken { get; set; }         // For Google auth
    public string DeviceId { get; set; }            // Device identifier
    public DateTime RequestTime { get; set; }       // Request timestamp
    
    public bool IsValid() { /* validation logic */ }
}
```

#### `LoginResult`

Authentication response data.

```csharp
public class LoginResult
{
    public bool IsSuccess { get; set; }             // Success flag
    public SamsungAccount User { get; set; }        // User account (if successful)
    public string ErrorMessage { get; set; }        // Error description
    public AuthenticationError ErrorType { get; set; } // Error type enum
    public string SessionToken { get; set; }        // Session token
    public LoginStatus Status { get; set; }         // Current status
    
    public static LoginResult Success(SamsungAccount user, string sessionToken = "");
    public static LoginResult Failure(AuthenticationError errorType, string errorMessage);
}
```

#### `DeviceInfo`

Device information and capabilities.

```csharp
public class DeviceInfo
{
    public DeviceType Type { get; set; }            // AIHome, FamilyHub
    public string DeviceId { get; set; }            // Unique device ID
    public ScreenDimensions Dimensions { get; set; } // Screen size
    public bool SupportsMultiUser { get; set; }     // Multi-user capability
    public string ModelName { get; set; }           // Device model
    public string OSVersion { get; set; }           // OS version
    public string AppVersion { get; set; }          // App version
    
    public bool IsAIHomeDevice => Type == DeviceType.AIHome;
    public bool IsFamilyHubDevice => Type == DeviceType.FamilyHub;
}
```

## 🔧 Utility APIs

### ValidationHelper

Static class for input validation.

```csharp
public static class ValidationHelper
{
    // Email validation
    public static ValidationResult ValidateEmail(string email);
    
    // Password validation
    public static ValidationResult ValidatePassword(string password);
    public static ValidationResult ValidatePasswordStrength(string password);
    
    // Authentication validation
    public static ValidationResult ValidateQRToken(string qrToken);
    public static ValidationResult ValidateGoogleToken(string googleToken);
    public static ValidationResult ValidateLoginRequest(LoginRequest request);
    
    // User data validation
    public static ValidationResult ValidateUserId(string userId);
    public static ValidationResult ValidateDisplayName(string displayName);
    public static ValidationResult ValidateUrl(string url);
    public static ValidationResult ValidatePhoneNumber(string phoneNumber);
    
    // Utility methods
    public static string SanitizeInput(string input);
    public static bool IsSafeForDisplay(string input);
}
```

#### Usage Examples

```csharp
// Validate email
var emailResult = ValidationHelper.ValidateEmail("user@samsung.com");
if (!emailResult.IsValid)
{
    await ShowError(emailResult.ErrorMessage);
    return;
}

// Validate password strength
var passwordResult = ValidationHelper.ValidatePasswordStrength("MySecurePass123!");
if (!passwordResult.IsValid)
{
    await ShowError(passwordResult.ErrorMessage);
    return;
}

// Validate complete login request
var requestResult = ValidationHelper.ValidateLoginRequest(loginRequest);
if (!requestResult.IsValid)
{
    await ShowError(requestResult.ErrorMessage);
    return;
}
```

### ViewFactory and ControllerFactory

Factory classes for creating device-specific instances.

```csharp
// Create device-specific view
var view = ViewFactory.CreateView<QRLoginView>(deviceType, navigationService, configService);

// Create device-specific controller
var controller = ControllerFactory.CreateController<LoginController>(deviceType);

// Get controller for screen
var controller = controllerFactory.GetControllerForScreen("AccountInfo", deviceType);
```

## 🚨 Error Handling

### Error Types and Codes

```csharp
public enum AuthenticationError
{
    None,                    // No error
    InvalidCredentials,      // Wrong email/password
    NetworkError,           // Network connectivity issue
    ServiceUnavailable,     // Samsung Account service down
    InvalidQRCode,          // Invalid or expired QR code
    GoogleAuthFailed,       // Google OAuth failure
    PasswordExpired,        // Password needs reset
    AccountLocked,          // Account temporarily locked
    TooManyAttempts,        // Rate limiting
    UnknownError           // Unexpected error
}
```

### Error Response Pattern

```csharp
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Error Handling Examples

```csharp
try
{
    var result = await accountService.LoginAsync(request);
    if (result.IsSuccess)
    {
        // Handle success
    }
    else
    {
        // Handle specific errors
        await HandleAuthenticationError(result.ErrorType, result.ErrorMessage);
    }
}
catch (NetworkException ex)
{
    await ShowNetworkError(ex);
}
catch (ServiceUnavailableException ex)
{
    await ShowServiceError(ex);
}
catch (Exception ex)
{
    await ShowGenericError(ex);
}
```

## 📈 Performance Considerations

### Async/Await Best Practices

```csharp
// ✅ Good: Non-blocking with proper error handling
public async Task<LoginResult> LoginAsync(LoginRequest request)
{
    try
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        var response = await httpClient.PostAsync(loginUrl, content);
        var result = await response.Content.ReadAsStringAsync();
        
        return ProcessLoginResponse(result);
    }
    catch (TaskCanceledException)
    {
        return LoginResult.Failure(AuthenticationError.NetworkError, "Request timed out");
    }
    catch (HttpRequestException ex)
    {
        return LoginResult.Failure(AuthenticationError.NetworkError, ex.Message);
    }
}

// ❌ Bad: Blocking call
public LoginResult LoginSync(LoginRequest request)
{
    return LoginAsync(request).Result; // Don't do this!
}
```

### Caching Strategy

```csharp
public class CachedSamsungAccountService : ISamsungAccountService
{
    private readonly ISamsungAccountService _baseService;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);
    
    public async Task<List<SamsungAccount>> GetAllAccountListAsync()
    {
        const string cacheKey = "all_accounts";
        
        if (_cache.TryGetValue(cacheKey, out List<SamsungAccount> cachedAccounts))
        {
            return cachedAccounts;
        }
        
        var accounts = await _baseService.GetAllAccountListAsync();
        _cache.Set(cacheKey, accounts, _cacheTimeout);
        
        return accounts;
    }
}
```

---

**Next**: [Integration Guide](../integration/README.md) for Samsung Account SES API integration.