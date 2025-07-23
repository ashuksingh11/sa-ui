# Navigation Usage Guide - TizenNavigationService

## 📋 Overview

This guide shows how to use the stack-based navigation system in the Samsung Account UI application. The TizenNavigationService mimics Tizen's Navigator class patterns with Push, Pop, Replace, and SetRoot methods.

## 🎯 Navigation Methods Available

### **Core Navigation Methods**

| Method | Description | Use Case |
|--------|-------------|----------|
| `PushAsync(screenName)` | Push new view onto stack | Navigate to new screen |
| `PopAsync()` | Pop current view from stack | Go back to previous screen |
| `ReplaceAsync(screenName)` | Replace current view | Change current screen without stacking |
| `SetRootAsync(screenName)` | Clear stack and set new root | Reset navigation (login/logout) |
| `PopToRootAsync()` | Pop all views except root | Return to home screen |

### **Stack Information**
- `CanNavigateBack` - Check if stack has more than one view
- `StackDepth` - Get current number of views in stack
- `CurrentView` - Get the top view on the stack

## 🔄 Navigation from Views

### **Basic Navigation Examples**

```csharp
public class QRLoginView : BaseView
{
    private async void OnPasswordButtonClicked(object sender, ClickedEventArgs e)
    {
        // Push password login view onto navigation stack
        await PushAsync("PasswordLogin");
    }
    
    private async void OnGoogleButtonClicked(object sender, ClickedEventArgs e)
    {
        // Push Google login view onto navigation stack
        await PushAsync("GoogleLogin");
    }
    
    public override async Task<bool> OnBackPressedAsync()
    {
        // Can't go back from root login view
        return false; // Let Tizen handle (minimize app)
    }
}
```

### **Navigation with Parameters**

```csharp
public class AccountInfoView : BaseView
{
    private async void OnChangePasswordClicked(object sender, ClickedEventArgs e)
    {
        // Push change password view with user context
        await PushAsync("ChangePassword", new { UserId = GetCurrentUserId() });
    }
    
    private async void OnUserCardClicked(object sender, ClickedEventArgs e)
    {
        // Replace with user detail view
        await ReplaceAsync("UserDetail", new { User = selectedUser });
    }
}
```

### **Back Navigation Handling**

```csharp
public class PasswordLoginView : BaseView
{
    private async void OnBackButtonClicked(object sender, ClickedEventArgs e)
    {
        // Go back to previous view (QRLogin)
        if (CanNavigateBack)
        {
            await PopAsync();
        }
    }
    
    public override async Task<bool> OnBackPressedAsync()
    {
        // Handle hardware back button
        if (CanNavigateBack)
        {
            await PopAsync();
            return true; // Handled
        }
        
        return false; // Let Tizen handle
    }
}
```

### **Root Navigation (Reset Stack)**

```csharp
public class LogoutConfirmView : BaseView
{
    private async void OnConfirmLogoutClicked(object sender, ClickedEventArgs e)
    {
        // Perform logout
        var success = await AuthController.LogoutAsync(userId, password);
        
        if (success)
        {
            // Reset navigation to login (clear entire stack)
            await SetRootAsync("QRLogin");
        }
    }
    
    private async void OnCancelClicked(object sender, ClickedEventArgs e)
    {
        // Return to account info (pop logout confirm)
        await PopAsync();
    }
}
```

## 🎮 Navigation from Controllers

Controllers can trigger navigation automatically after operations:

### **AuthController Navigation**

```csharp
public class AuthController
{
    public async Task<LoginResult> LoginWithPasswordAsync(string email, string password)
    {
        var result = await _accountService.LoginAsync(request);
        
        // Automatic navigation after successful login
        if (result.IsSuccess && _navigationService != null)
        {
            await _navigationService.SetRootAsync("AccountInfo");
        }
        
        return result;
    }
    
    public async Task<bool> LogoutAsync(string userId, string password = null)
    {
        var success = await _accountService.LogoutAsync(userId);

        // Navigate based on remaining users
        if (success && _navigationService != null)
        {
            var remainingAccounts = await _accountService.GetAllAccountListAsync();
            if (remainingAccounts.Count > 0)
            {
                await _navigationService.SetRootAsync("AccountInfo");
            }
            else
            {
                await _navigationService.SetRootAsync("QRLogin");
            }
        }

        return success;
    }
}
```

## 📱 Navigation Flow Examples

### **1. Login Flow**
```
Initial: [QRLogin] (root)
User clicks "Password": [QRLogin, PasswordLogin] (push)
User enters credentials: → AuthController.LoginAsync() → SetRoot → [AccountInfo] (new root)
```

### **2. Account Management Flow**
```
Start: [AccountInfo] (root after login)
User clicks "Change Password": [AccountInfo, ChangePassword] (push)
User clicks "Back": [AccountInfo] (pop)
```

### **3. Logout Flow**
```
Start: [AccountInfo, ChangePassword] (multiple views)
User clicks "Logout": [AccountInfo, ChangePassword, LogoutConfirm] (push)
User confirms logout: → AuthController.LogoutAsync() → SetRoot → [QRLogin] (new root)
```

### **4. Multi-User Switching**
```
Start: [AccountInfo] (root)
User clicks different user: → AccountController.SwitchUserAsync() → Replace → [AccountInfo] (same screen, different user)
```

## 🔧 Advanced Navigation Patterns

### **Conditional Navigation**

```csharp
public class SettingsView : BaseView
{
    private async void OnAdvancedSettingsClicked(object sender, ClickedEventArgs e)
    {
        // Check user permissions before navigating
        if (await AuthController.IsUserAdminAsync(currentUserId))
        {
            await PushAsync("AdminSettings");
        }
        else
        {
            await ShowToastAsync("Admin access required");
        }
    }
}
```

### **Navigation with Animation Control**

```csharp
public class SplashView : BaseView
{
    private async void OnSplashComplete()
    {
        // Navigate without animation for faster transition
        await SetRootAsync("QRLogin", animated: false);
    }
}

public class MenuView : BaseView
{
    private async void OnSubMenuClicked(object sender, ClickedEventArgs e)
    {
        // Navigate with animation for smooth UX
        await PushAsync("SubMenu", animated: true);
    }
}
```

### **Stack Inspection and Management**

```csharp
public class NavigationDebugView : BaseView
{
    private void ShowNavigationInfo()
    {
        var currentDepth = NavigationStackDepth;
        var canGoBack = CanNavigateBack;
        
        Console.WriteLine($"Stack depth: {currentDepth}");
        Console.WriteLine($"Can navigate back: {canGoBack}");
        
        // Get all views in stack (for debugging)
        var allViews = NavigationService.GetNavigationStack();
        foreach (var view in allViews)
        {
            Console.WriteLine($"Stack contains: {view.GetType().Name}");
        }
    }
}
```

## 🎨 Device-Specific Navigation Patterns

### **FamilyHub (Large Vertical Display)**

```csharp
public class FamilyHubHomeView : BaseView
{
    private async void OnQuickActionClicked(object sender, ClickedEventArgs e)
    {
        // FamilyHub can handle deeper navigation stacks
        await PushAsync("QuickActions");
    }
    
    private void CreateNavigationTabs()
    {
        // Large screen allows tab-based navigation
        var tabsContainer = new View();
        // Add multiple tabs that push different views
    }
}
```

### **AIHome (Compact Horizontal Display)**

```csharp
public class AIHomeView : BaseView
{
    private async void OnSettingsClicked(object sender, ClickedEventArgs e)
    {
        // AIHome prefers replace over push for limited screen space
        await ReplaceAsync("CompactSettings");
    }
    
    private async void OnBackToMainClicked(object sender, ClickedEventArgs e)
    {
        // Quick return to root for simple navigation
        await PopToRootAsync();
    }
}
```

## 🛠️ Error Handling in Navigation

### **Navigation Error Handling**

```csharp
public class ErrorHandlingView : BaseView
{
    private async void SafeNavigationExample()
    {
        try
        {
            await PushAsync("NonExistentView");
        }
        catch (NotSupportedException ex)
        {
            // Handle unknown screen error
            await ShowErrorAsync($"Screen not available: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle general navigation errors
            AppController.HandleAppError(ex, "Navigation");
            await ShowErrorAsync("Navigation failed. Please try again.");
        }
    }
}
```

### **Graceful Navigation Fallbacks**

```csharp
public class RobustNavigationView : BaseView
{
    private async void NavigateWithFallback(string primaryScreen, string fallbackScreen)
    {
        try
        {
            await PushAsync(primaryScreen);
        }
        catch (Exception)
        {
            // Fallback to known screen
            await PushAsync(fallbackScreen);
        }
    }
}
```

## 📊 Navigation Best Practices

### **✅ Do This**

```csharp
// Use appropriate navigation method
await PushAsync("DetailView");        // For drill-down navigation
await ReplaceAsync("UpdatedView");    // For same-level updates
await SetRootAsync("NewFlow");        // For flow changes (login/logout)

// Handle back navigation properly
public override async Task<bool> OnBackPressedAsync()
{
    if (hasUnsavedChanges)
    {
        var confirm = await ShowConfirmDialogAsync("Discard changes?");
        if (!confirm) return true; // Block navigation
    }
    
    return false; // Allow default navigation
}

// Check navigation state
if (CanNavigateBack)
{
    await PopAsync();
}
```

### **❌ Avoid This**

```csharp
// Don't ignore navigation errors
await PushAsync("SomeView"); // Missing error handling

// Don't break navigation stack
await SetRootAsync("Detail"); // Don't set details as root

// Don't create deep stacks unnecessarily
// [Root, List, Detail, SubDetail, SubSubDetail] // Too deep
```

## 🎯 Summary

The TizenNavigationService provides:

- **Stack-based navigation** similar to Tizen Navigator
- **Easy view navigation** with PushAsync, PopAsync, ReplaceAsync
- **Automatic navigation** from controllers after auth operations
- **Device-specific optimization** for FamilyHub vs AIHome
- **Proper lifecycle management** with view disposal
- **Error handling** and graceful fallbacks

Use these patterns to create smooth, intuitive navigation flows in your Samsung Account UI application!