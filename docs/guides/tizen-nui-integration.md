# Tizen NUI Application Integration Guide

## 📋 Overview

This guide demonstrates how to properly integrate the Samsung Account UI MVC architecture with the Tizen .NET NUI application lifecycle. Our implementation properly handles NUI application events, window management, and view lifecycle within the Tizen framework.

## 🏗️ Tizen NUI Application Lifecycle

### Understanding NUI Application Events

Tizen NUI applications follow a specific lifecycle pattern that our Samsung Account UI must integrate with:

```
Application Launch → OnCreate() → OnAppControlReceived() → OnResume()
                                     ↓
Application Running ← → OnPause() ← → OnResume() (focus changes)
                                     ↓
Application Terminate ← OnTerminate()
```

## 🚀 Main Application Implementation

### Program.cs - NUI Application Entry Point

```csharp
using System;
using System.Threading.Tasks;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SamsungAccountUI
{
    /// <summary>
    /// Main Samsung Account NUI Application
    /// Integrates our MVC architecture with Tizen NUI application lifecycle
    /// </summary>
    public class SamsungAccountNUIApp : NUIApplication
    {
        private IServiceProvider _serviceProvider;
        private INavigationService _navigationService;
        private BaseView _currentView;
        private Window _mainWindow;

        protected override void OnCreate()
        {
            base.OnCreate();
            
            try
            {
                // Initialize dependency injection
                ConfigureServices();
                
                // Setup main window
                SetupMainWindow();
                
                // Initialize services
                InitializeServices();
                
                // Start the application flow
                _ = Task.Run(StartApplicationFlowAsync);
            }
            catch (Exception ex)
            {
                // Handle startup errors
                Exit();
            }
        }
        
        // Additional lifecycle methods...
    }
}
```

## 🔧 Service Configuration Integration

### Dependency Injection Setup

Our MVC services integrate seamlessly with the NUI application:

```csharp
private void ConfigureServices()
{
    var services = new ServiceCollection();
    
    // Logging for Tizen
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    });
    
    // Core services
    services.AddSingleton<IGlobalConfigService, GlobalConfigService>();
    services.AddSingleton<IDeviceDetectionService, DeviceDetectionService>();
    
    // Account service - conditional based on build
    #if DEBUG
    services.AddScoped<ISamsungAccountService, MockSamsungAccountService>();
    #else
    services.AddScoped<ISamsungAccountService, RealSamsungAccountService>();
    #endif
    
    // Navigation service with NUI integration
    services.AddScoped<INavigationService, NavigationService>();
    
    // Controllers
    services.AddScoped<LoginController>();
    services.AddScoped<AccountInfoController>();
    // ... other controllers
    
    _serviceProvider = services.BuildServiceProvider();
}
```

## 🖼️ Window and View Management

### NUI Window Configuration

The main window is configured based on device type:

```csharp
private void SetupMainWindow()
{
    _mainWindow = GetDefaultWindow();
    _mainWindow.Title = "Samsung Account";
    
    // Configure based on device type
    var deviceInfo = _serviceProvider?.GetService<IDeviceDetectionService>()?.GetCurrentDeviceInfo();
    if (deviceInfo != null)
    {
        ConfigureWindowForDevice(deviceInfo);
    }
    
    // Handle window events
    _mainWindow.KeyEvent += OnWindowKeyEvent;
    _mainWindow.TouchEvent += OnWindowTouchEvent;
    _mainWindow.FocusChanged += OnWindowFocusChanged;
}

private void ConfigureWindowForDevice(DeviceInfo deviceInfo)
{
    switch (deviceInfo.Type)
    {
        case DeviceType.FamilyHub:
            // Configure for large vertical display (refrigerator)
            _mainWindow.WindowSize = new Size2D(1080, 1920); // Portrait
            _mainWindow.BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
            break;
            
        case DeviceType.AIHome:
            // Configure for compact horizontal display (washing machine, etc.)
            _mainWindow.WindowSize = new Size2D(800, 480); // Landscape
            _mainWindow.BackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1.0f);
            break;
    }
}
```

### View Integration with NUI Window

Views are properly integrated with the NUI window system:

```csharp
private void OnViewChanged(object sender, BaseView newView)
{
    try
    {
        // Cleanup previous view
        CleanupCurrentView();
        
        // Set new current view
        _currentView = newView;
        
        if (_currentView != null)
        {
            // Add to main window (this is the key integration point)
            _mainWindow.Add(_currentView);
            
            // Wire up view events
            WireViewEvents(_currentView);
            
            // Trigger view appearance
            _ = Task.Run(async () => await _currentView.OnAppearingAsync());
        }
    }
    catch (Exception ex)
    {
        // Handle view change errors
    }
}
```

## 🎯 Navigation Service NUI Integration

### Enhanced NavigationService with NUI Support

The NavigationService is enhanced to work directly with NUI:

```csharp
public class NavigationService : INavigationService
{
    private Window _window;
    private IServiceProvider _serviceProvider;
    private BaseView _currentView;
    
    // Events for NUI application integration
    public event EventHandler<BaseView> ViewChanged;
    
    /// <summary>
    /// Set the NUI window reference - called by the main application
    /// </summary>
    public void SetWindow(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        CreateGlobalOverlays();
    }
    
    public async Task NavigateToAsync(string screenName, object parameters = null)
    {
        // Create the view for the target screen
        var newView = await CreateViewForScreenAsync(screenName);
        
        // Cleanup current view
        await CleanupCurrentViewAsync();
        
        // Set the new view as current
        _currentView = newView;
        
        // Load content for the new view
        await _currentView.LoadContentAsync();
        
        // Notify the main application about view change
        // This triggers the NUI window integration
        ViewChanged?.Invoke(this, _currentView);
    }
    
    private async Task CleanupCurrentViewAsync()
    {
        if (_currentView != null)
        {
            // Trigger view disappearance
            await _currentView.OnDisappearingAsync();
            
            // Remove from NUI window
            _window?.Remove(_currentView);
            
            // Dispose view resources
            _currentView.Dispose();
            _currentView = null;
        }
    }
}
```

## 🎮 Input Handling Integration

### Hardware Key Events

Handle device-specific input (back button, home button, etc.):

```csharp
private bool OnWindowKeyEvent(object source, Window.KeyEventArgs e)
{
    try
    {
        if (e.Key.State == Key.StateType.Down)
        {
            switch (e.Key.KeyPressedName)
            {
                case "XF86Back":
                case "Escape":
                    // Handle back button
                    if (_currentView != null)
                    {
                        _ = Task.Run(async () =>
                        {
                            var handled = await _currentView.OnBackPressedAsync();
                            if (!handled)
                            {
                                // Default back behavior - minimize app
                                Lower();
                            }
                        });
                        return true;
                    }
                    break;
                    
                case "XF86Home":
                    // Handle home button - minimize app
                    Lower();
                    return true;
            }
        }
    }
    catch (Exception ex)
    {
        // Log error
    }
    
    return false;
}
```

### Touch Event Handling

Touch events are primarily handled by individual views, but global tracking can be added:

```csharp
private bool OnWindowTouchEvent(object source, Window.TouchEventArgs e)
{
    // Touch events are handled by individual views
    // This is mainly for global touch tracking if needed
    
    // Example: Track global touch for analytics
    if (e.Touch.GetState(0) == PointStateType.Down)
    {
        // Log touch event for analytics
    }
    
    return false; // Let views handle the actual touch
}
```

## 📱 Application State Management

### Handling Application Lifecycle Events

Properly handle Tizen application lifecycle events:

```csharp
protected override void OnPause()
{
    base.OnPause();
    
    try
    {
        // Pause current view
        _currentView?.OnDisappearingAsync();
        
        // Save application state
        SaveApplicationState();
    }
    catch (Exception ex)
    {
        // Handle pause errors
    }
}

protected override void OnResume()
{
    base.OnResume();
    
    try
    {
        // Resume current view
        _currentView?.OnAppearingAsync();
        
        // Restore application state
        RestoreApplicationState();
    }
    catch (Exception ex)
    {
        // Handle resume errors
    }
}

private void SaveApplicationState()
{
    try
    {
        // Save current screen and user state using Tizen Preference
        var currentScreen = _currentView?.GetType().Name ?? "Unknown";
        
        // Example using Tizen Preference API
        // Preference.Set("current_screen", currentScreen);
        // Preference.Set("active_user_id", GetActiveUserId());
    }
    catch (Exception ex)
    {
        // Handle save errors
    }
}
```

## 🔄 App Control Integration

### Deep Link Support

Handle app control events for deep linking:

```csharp
protected override void OnAppControlReceived(AppControlReceivedEventArgs e)
{
    base.OnAppControlReceived(e);
    
    try
    {
        // Handle deep links or external launches
        var operation = e.ReceivedAppControl.Operation;
        var extraData = e.ReceivedAppControl.ExtraData;
        
        // Example: Handle launch with specific screen
        if (extraData.ContainsKey("screen"))
        {
            var targetScreen = extraData.Get("screen");
            _ = Task.Run(() => HandleDeepLinkAsync(targetScreen));
        }
        
        // Example: Handle login request from another app
        if (operation == "http://tizen.org/appcontrol/operation/login")
        {
            _ = Task.Run(() => NavigateToLoginAsync());
        }
    }
    catch (Exception ex)
    {
        // Handle app control errors
    }
}

private async Task HandleDeepLinkAsync(string targetScreen)
{
    try
    {
        await _navigationService.NavigateToAsync(targetScreen);
    }
    catch (Exception ex)
    {
        // Handle deep link navigation errors
    }
}
```

## 🎨 View Lifecycle Integration

### BaseView NUI Integration

The BaseView class properly integrates with NUI lifecycle:

```csharp
public abstract class BaseView : View
{
    // BaseView inherits from Tizen.NUI.BaseComponents.View
    // This provides automatic integration with NUI layout and rendering
    
    public BaseView(IController controller, IGlobalConfigService configService, DeviceInfo deviceInfo)
    {
        Controller = controller;
        ConfigService = configService;
        DeviceInfo = deviceInfo;
        
        // Initialize NUI view properties
        InitializeBaseLayout();
        ApplyDeviceSpecificSettings();
    }
    
    private void InitializeBaseLayout()
    {
        // Set up main view properties for NUI
        WidthSpecification = LayoutParamPolicies.MatchParent;
        HeightSpecification = LayoutParamPolicies.MatchParent;
        BackgroundColor = GetThemeBackgroundColor();
        
        // Create NUI layout
        Layout = new LinearLayout
        {
            LinearOrientation = LinearLayout.Orientation.Vertical,
            LinearAlignment = LinearLayout.Alignment.Top
        };
        
        // Create child elements using NUI components
        CreateTitleArea();
        CreateContentContainer();
        CreateButtonContainer();
    }
}
```

## 🚀 Application Launch Flow

### Complete Launch Sequence

The complete application launch sequence integrating with Tizen:

```
1. Tizen Launcher starts app
2. NUIApplication.OnCreate() called
3. Configure dependency injection
4. Setup main window with device-specific settings
5. Initialize services (config, device detection, navigation)
6. Check for existing user accounts
7. Navigate to appropriate initial screen (QRLogin or AccountInfo)
8. Load and display first view
9. Ready for user interaction
```

### Implementation Example:

```csharp
private async Task StartApplicationFlowAsync()
{
    try
    {
        // Check for existing user accounts
        var accountService = _serviceProvider.GetRequiredService<ISamsungAccountService>();
        var existingAccounts = await accountService.GetAllAccountListAsync();
        
        // Navigate to appropriate initial screen
        if (existingAccounts.Count > 0)
        {
            await _navigationService.NavigateToAsync("AccountInfo");
        }
        else
        {
            await _navigationService.NavigateToAsync("QRLogin");
        }
    }
    catch (Exception ex)
    {
        // Fallback to QR login on any error
        await _navigationService.NavigateToAsync("QRLogin");
    }
}
```

## 🧪 Testing NUI Integration

### Testing with Tizen Emulator

To test the integration:

1. **Setup Tizen Studio**:
   ```bash
   # Install Tizen Studio and .NET support
   # Create new Tizen .NET project
   tizen create native-project -t "CSharp" -n "SamsungAccountUI"
   ```

2. **Configure Emulator**:
   ```bash
   # Launch appropriate emulator
   tizen run -t [emulator_name] -p SamsungAccountUI
   ```

3. **Test Device-Specific Behavior**:
   - FamilyHub emulator: Test large vertical layout
   - AIHome emulator: Test compact horizontal layout

### Debug NUI Integration

Enable NUI debugging:

```csharp
// In Program.cs Main method
static void Main(string[] args)
{
    // Enable NUI debugging
    Environment.SetEnvironmentVariable("DALI_LOG_LEVEL", "INFO");
    Environment.SetEnvironmentVariable("DALI_BACKTRACE", "1");
    
    var app = new SamsungAccountNUIApp();
    app.Run(args);
}
```

## 📊 Performance Considerations

### Memory Management in NUI

Proper memory management is critical for Tizen devices:

```csharp
protected override void Dispose(DisposeTypes type)
{
    if (type == DisposeTypes.Explicit)
    {
        // Dispose NUI resources properly
        LoadingOverlay?.Dispose();
        ErrorOverlay?.Dispose();
        TitleLabel?.Dispose();
        ContentContainer?.Dispose();
        ButtonContainer?.Dispose();
    }
    
    base.Dispose(type);
}
```

### Optimizing for Device Types

Different optimization strategies for different devices:

```csharp
private void OptimizeForDevice(DeviceType deviceType)
{
    switch (deviceType)
    {
        case DeviceType.FamilyHub:
            // Rich UI for powerful refrigerator
            EnableRichAnimations();
            EnableHighResolutionAssets();
            break;
            
        case DeviceType.AIHome:
            // Optimized UI for appliances
            DisableHeavyAnimations();
            UseLowResolutionAssets();
            EnablePowerSaveMode();
            break;
    }
}
```

## 🎯 Key Integration Points

1. **NUIApplication Lifecycle**: Proper handling of OnCreate, OnPause, OnResume, OnTerminate
2. **Window Management**: Device-specific window configuration and event handling
3. **View Hierarchy**: BaseView inheriting from NUI View for proper integration
4. **Navigation Service**: NUI-aware navigation with window add/remove operations
5. **Resource Management**: Proper disposal of NUI resources
6. **Input Handling**: Hardware key and touch event integration
7. **State Persistence**: Using Tizen Preference API for state management
8. **App Control**: Deep link and external launch handling

This integration ensures that our MVC architecture works seamlessly within the Tizen NUI application framework while maintaining clean separation of concerns and proper lifecycle management.