# View Implementation Guide

## 📋 Overview

This guide demonstrates how to create device-specific views that communicate with controllers, handle user input, and manage navigation in the Samsung Account UI application. Use the sample views as templates for implementing your remaining screens.

## 🏗️ Architecture Pattern

All views follow this consistent pattern:

```
BaseView (Common functionality)
    ↓
Device-Specific Views (FamilyHub/AIHome)
    ↓
Controller Communication (Events/Actions)
    ↓
Data Binding and UI Updates
```

## 🎯 Sample Views Created

### 1. Enhanced BaseView (`src/Views/Common/BaseView.cs`)

**Purpose**: Provides common functionality for all views including device-specific layouts, loading states, and controller communication.

**Key Features**:
- Inherits from Tizen NUI `View` class
- Built-in loading and error overlays with animations
- Device-specific sizing methods
- Event-based controller communication
- Proper lifecycle and disposal management

**Usage Example**:
```csharp
public class MyCustomView : BaseView
{
    public MyCustomView(IController controller, IGlobalConfigService config, DeviceInfo deviceInfo) 
        : base(controller, config, deviceInfo)
    {
        // View-specific initialization
    }

    protected override string GetViewTitle() => "My Custom View";

    public override async Task LoadContentAsync()
    {
        // Create view-specific content
        CreateMyContent();
        
        // Notify that loading is complete
        NotifyViewLoaded();
    }

    public override async Task<bool> OnBackPressedAsync()
    {
        // Handle back navigation
        RequestNavigation("PreviousScreen");
        return true;
    }
}
```

### 2. QR Login Views (Device-Specific)

#### FamilyHub Version (`src/Views/FamilyHub/QRLoginView.cs`)

**Layout**: Large vertical layout optimized for 21"/32" displays
**QR Code**: 320x320px with prominent display
**Buttons**: Horizontal layout with large touch targets

**Key Implementation Details**:
```csharp
private void CreateFamilyHubQRLayout()
{
    // Large QR code section
    _qrContainer = new View
    {
        WidthSpecification = LayoutParamPolicies.MatchParent,
        HeightSpecification = 400, // Large space for FamilyHub
        Layout = new LinearLayout
        {
            LinearOrientation = LinearLayout.Orientation.Vertical,
            LinearAlignment = LinearLayout.Alignment.Center,
            CellPadding = new Size2D(0, 20)
        }
    };

    // 320x320 QR code with rounded corners
    var qrBackground = new View
    {
        Size = new Size(320, 320),
        BackgroundColor = Color.White,
        CornerRadius = 16.0f,
        BoxShadow = new Shadow(5.0f, new Color(0, 0, 0, 0.2f), new Vector2(0, 2))
    };
}
```

#### AIHome Version (`src/Views/AIHome/QRLoginView.cs`)

**Layout**: Compact horizontal layout for 7"/9" displays
**QR Code**: 160x160px with status indicator
**Buttons**: Vertical stack with smaller touch targets

**Key Implementation Details**:
```csharp
private void CreateAIHomeCompactLayout()
{
    // Horizontal layout for compact display
    ContentContainer.Layout = new LinearLayout
    {
        LinearOrientation = LinearLayout.Orientation.Horizontal,
        LinearAlignment = LinearLayout.Alignment.Center,
        CellPadding = new Size2D(20, 0)
    };

    // Left panel: Compact QR (160x160)
    _leftPanel = new View
    {
        Size = new Size(200, 200),
        Layout = new LinearLayout
        {
            LinearOrientation = LinearLayout.Orientation.Vertical,
            LinearAlignment = LinearLayout.Alignment.Center,
            CellPadding = new Size2D(0, 10)
        }
    };

    // Right panel: Info and buttons
    _rightPanel = new View
    {
        WidthSpecification = LayoutParamPolicies.FillToParent,
        HeightSpecification = 200,
        Layout = new LinearLayout
        {
            LinearOrientation = LinearLayout.Orientation.Vertical,
            LinearAlignment = LinearLayout.Alignment.Center,
            CellPadding = new Size2D(0, 15)
        }
    };
}
```

### 3. Password Login View (`src/Views/FamilyHub/PasswordLoginView.cs`)

**Purpose**: Demonstrates form handling, validation, and user input management.

**Key Features**:
- Real-time email and password validation
- Visual feedback for form states
- Focus management between fields
- Error display and handling
- Secure password input

**Form Validation Example**:
```csharp
private void OnEmailTextChanged(object sender, TextField.TextChangedEventArgs e)
{
    var email = e.TextField.Text?.Trim() ?? "";
    ValidateEmail(email);
    UpdateFormState();
}

private void ValidateEmail(string email)
{
    var validation = ValidationHelper.ValidateEmail(email);
    _isEmailValid = validation.IsValid;
    
    if (!validation.IsValid && !string.IsNullOrEmpty(email))
    {
        ShowEmailError(validation.ErrorMessage);
    }
    else
    {
        HideEmailError();
    }
}

private void UpdateFormState()
{
    // Enable sign in button only if both fields are valid
    _signInButton.IsEnabled = _isEmailValid && _isPasswordValid && !_isFormSubmitting;
    
    // Update button appearance based on state
    if (_signInButton.IsEnabled)
    {
        _signInButton.BackgroundColor = GetPrimaryButtonColor();
        _signInButton.Opacity = 1.0f;
    }
    else
    {
        _signInButton.BackgroundColor = GetDisabledButtonColor();
        _signInButton.Opacity = 0.6f;
    }
}
```

**Focus Management Example**:
```csharp
private void OnEmailFocusGained(object sender, EventArgs e)
{
    // Highlight email field
    _emailField.BorderlineColor = GetFocusColor();
    _emailField.BorderlineWidth = 3.0f;
}

private void OnEmailFocusLost(object sender, EventArgs e)
{
    // Restore normal border
    _emailField.BorderlineWidth = 2.0f;
    if (_isEmailValid || string.IsNullOrEmpty(_emailField.Text))
    {
        _emailField.BorderlineColor = GetInputBorderColor();
    }
}
```

### 4. Account Info View (`src/Views/FamilyHub/AccountInfoView.cs`)

**Purpose**: Shows multi-user management with dynamic content updates and complex interactions.

**Key Features**:
- Active user display with avatar and status
- Scrollable list of other users
- User switching with confirmation
- Logout confirmation dialogs
- Real-time data updates from controller

**Dynamic User Cards Example**:
```csharp
private void UpdateUserCards()
{
    // Clear existing user cards
    ClearUserCards();
    
    if (_accountState?.AllAccounts == null)
        return;
    
    // Create cards for non-active users
    var otherUsers = _accountState.AllAccounts
        .Where(u => !u.IsActiveUser)
        .ToList();
    
    foreach (var user in otherUsers)
    {
        var userCard = CreateUserCard(user);
        _userCards.Add(userCard);
        _usersContainer.Add(userCard);
    }
    
    // Show/hide section based on other users
    _otherUsersSection.Visibility = otherUsers.Any();
}

private View CreateUserCard(SamsungAccount user)
{
    var card = new View
    {
        Size = new Size(150, 180),
        BackgroundColor = GetCardBackgroundColor(),
        CornerRadius = 12.0f,
        BoxShadow = new Shadow(4.0f, new Color(0, 0, 0, 0.1f), new Vector2(0, 2)),
        Layout = new LinearLayout
        {
            LinearOrientation = LinearLayout.Orientation.Vertical,
            LinearAlignment = LinearLayout.Alignment.Center,
            CellPadding = new Size2D(0, 10)
        }
    };

    // User avatar
    var avatar = new ImageView
    {
        Size = new Size(60, 60),
        ResourceUrl = user.ProfilePictureUrl ?? "images/default_avatar.png",
        CornerRadius = 30.0f
    };
    card.Add(avatar);

    // Switch button with event handling
    var switchButton = CreateStyledButton("Switch", new Size(100, 30), true);
    switchButton.Clicked += async (sender, e) => await OnUserSwitchClicked(user.UserId);
    card.Add(switchButton);

    return card;
}
```

## 🔄 Controller Communication Patterns

### 1. Event-Based Communication

Views communicate with controllers using events to maintain loose coupling:

```csharp
public class MyView : BaseView
{
    private MyController _controller;

    public MyView(MyController controller, /* other deps */) : base(/* base deps */)
    {
        _controller = controller;
        
        // Subscribe to controller events
        _controller.DataChanged += OnDataChanged;
        _controller.OperationCompleted += OnOperationCompleted;
        _controller.ErrorOccurred += OnErrorOccurred;
    }

    // Send actions to controller
    private async void OnButtonClicked(object sender, ClickedEventArgs e)
    {
        var actionData = new { UserId = "123", Action = "Switch" };
        SendActionToController(actionData);
        
        // Or call controller method directly
        await _controller.HandleUserActionAsync(actionData);
    }

    // Handle controller events
    private async void OnDataChanged(object sender, DataChangedEventArgs e)
    {
        // Update UI based on new data
        UpdateUserInterface(e.NewData);
    }

    private async void OnOperationCompleted(object sender, OperationResult result)
    {
        await HideLoadingAsync();
        
        if (result.IsSuccess)
        {
            // Handle success
            RequestNavigation("NextScreen");
        }
        else
        {
            // Handle failure
            await ShowErrorAsync(result.ErrorMessage);
        }
    }
}
```

### 2. Navigation Requests

Views request navigation through the base view event system:

```csharp
// In view implementation
private void OnBackButtonClicked(object sender, ClickedEventArgs e)
{
    RequestNavigation("QRLogin");
}

private void OnSettingsClicked(object sender, ClickedEventArgs e)
{
    RequestNavigation("Settings");
}

// Base view handles the navigation event
protected void RequestNavigation(string screenName)
{
    NavigationRequested?.Invoke(this, screenName);
}
```

### 3. Data Binding

Controllers provide data updates that views consume:

```csharp
// Controller provides data updates
public event EventHandler<AccountState> AccountStateChanged;

private void NotifyAccountStateChanged(AccountState newState)
{
    AccountStateChanged?.Invoke(this, newState);
}

// View responds to data changes
private async void OnAccountStateChanged(object sender, AccountState accountState)
{
    _accountState = accountState;
    
    // Update UI components
    UpdateActiveUserDisplay();
    UpdateUserCards();
    
    Console.WriteLine($"Account state updated: {accountState.AllAccounts.Count} total users");
}
```

## 🎨 Device-Specific Design Guidelines

### FamilyHub Devices (Large Vertical Displays)

**Screen Sizes**: 21", 32" portrait displays
**Design Principles**:
- Large touch targets (minimum 60px height)
- Generous spacing and padding
- Rich visual elements and animations
- Multiple information panels
- Horizontal button layouts

**Sizing Guidelines**:
```csharp
// FamilyHub-specific sizing
private int GetTitleHeight() => 120;
private int GetButtonAreaHeight() => 140;
private float GetTitleFontSize() => 24f;
private float GetButtonFontSize() => 16f;
private Size2D GetButtonSpacing() => new Size2D(30, 0);
private Extents GetContentPadding() => new Extents(40, 40, 20, 20);
```

### AIHome Devices (Compact Horizontal Displays)

**Screen Sizes**: 7", 9" landscape displays
**Design Principles**:
- Compact touch targets (minimum 30px height)
- Minimal spacing to fit content
- Simple visual elements
- Single information panels
- Vertical button stacks

**Sizing Guidelines**:
```csharp
// AIHome-specific sizing
private int GetTitleHeight() => 80;
private int GetButtonAreaHeight() => 100;
private float GetTitleFontSize() => 16f;
private float GetButtonFontSize() => 12f;
private Size2D GetButtonSpacing() => new Size2D(0, 15);
private Extents GetContentPadding() => new Extents(20, 20, 10, 10);
```

## 🛠️ Implementation Best Practices

### 1. View Lifecycle Management

Always implement proper lifecycle methods:

```csharp
public override async Task OnAppearingAsync()
{
    await base.OnAppearingAsync();
    
    // View-specific initialization
    await LoadDataAsync();
    StartTimersOrSubscriptions();
}

public override async Task OnDisappearingAsync()
{
    await base.OnDisappearingAsync();
    
    // Cleanup view-specific resources
    StopTimersOrSubscriptions();
    ClearSensitiveData();
}

protected override void Dispose(DisposeTypes type)
{
    if (type == DisposeTypes.Explicit)
    {
        // Unsubscribe from controller events
        if (_controller != null)
        {
            _controller.SomeEvent -= OnSomeEvent;
        }
        
        // Dispose UI elements
        _customElement?.Dispose();
    }
    
    base.Dispose(type);
}
```

### 2. Error Handling

Implement comprehensive error handling:

```csharp
public override async Task LoadContentAsync()
{
    try
    {
        await ShowLoadingAsync("Loading content...");
        
        // Load data from controller
        await _controller.LoadDataAsync();
        
        // Create UI elements
        CreateUserInterface();
        
        await HideLoadingAsync();
    }
    catch (NetworkException ex)
    {
        await HideLoadingAsync();
        await ShowErrorAsync("Network connection failed. Please check your connection.", "Retry");
    }
    catch (AuthenticationException ex)
    {
        await HideLoadingAsync();
        await ShowErrorAsync("Authentication failed. Please sign in again.", "Sign In");
        RequestNavigation("QRLogin");
    }
    catch (Exception ex)
    {
        await HideLoadingAsync();
        await ShowErrorAsync($"An unexpected error occurred: {ex.Message}", "OK");
    }
}
```

### 3. Loading States

Provide clear loading feedback:

```csharp
private async void OnSubmitClicked(object sender, ClickedEventArgs e)
{
    try
    {
        // Disable form during submission
        SetFormEnabled(false);
        await ShowLoadingAsync("Processing...");
        
        // Perform operation
        var result = await _controller.SubmitFormAsync(GetFormData());
        
        await HideLoadingAsync();
        
        if (result.IsSuccess)
        {
            RequestNavigation("Success");
        }
        else
        {
            await ShowErrorAsync(result.ErrorMessage);
        }
    }
    finally
    {
        SetFormEnabled(true);
    }
}
```

### 4. Configuration-Driven UI

Use configuration service for dynamic behavior:

```csharp
private void CreateLoginOptions()
{
    // Check configuration before creating UI elements
    if (ConfigService.IsPasswordLoginEnabled)
    {
        CreatePasswordLoginButton();
    }
    
    if (ConfigService.IsGoogleLoginEnabled)
    {
        CreateGoogleLoginButton();
    }
    
    if (ConfigService.IsBiometricLoginEnabled)
    {
        CreateBiometricLoginButton();
    }
    
    // Apply theme
    ApplyTheme(ConfigService.DefaultUITheme);
    
    // Apply accessibility settings
    if (ConfigService.EnableLargeText)
    {
        ApplyLargeTextSettings();
    }
}
```

## 📱 Complete Usage Example

The `SampleViewUsage.cs` file demonstrates how to wire everything together:

```csharp
// Initialize services and controllers
var configService = new GlobalConfigService();
var deviceService = new DeviceDetectionService();
var accountService = new MockSamsungAccountService();
var loginController = new LoginController(null, accountService, configService, deviceInfo);

// Create device-specific view
BaseView view;
switch (deviceInfo.Type)
{
    case DeviceType.FamilyHub:
        view = new FamilyHub.QRLoginView(loginController, configService, deviceInfo);
        break;
    case DeviceType.AIHome:
        view = new AIHome.QRLoginView(loginController, configService, deviceInfo);
        break;
}

// Wire up navigation
view.NavigationRequested += async (sender, screenName) =>
{
    await HandleNavigationRequest(screenName);
};

// Load and show view
await view.LoadContentAsync();
ShowView(view);
```

## 🧪 Testing Your Views

Test views using the provided patterns:

```csharp
public async Task TestMyView()
{
    // Create mock dependencies
    var mockController = new Mock<IMyController>();
    var mockConfig = new Mock<IGlobalConfigService>();
    var deviceInfo = new DeviceInfo { Type = DeviceType.FamilyHub };
    
    // Create view
    var view = new MyView(mockController.Object, mockConfig.Object, deviceInfo);
    
    // Test lifecycle
    await view.LoadContentAsync();
    Assert.IsNotNull(view.ContentContainer);
    
    // Test navigation
    var navigationRequested = false;
    view.NavigationRequested += (s, e) => navigationRequested = true;
    
    // Simulate user action that should trigger navigation
    // ... trigger action ...
    
    Assert.IsTrue(navigationRequested);
    
    // Cleanup
    view.Dispose();
}
```

This comprehensive guide provides everything you need to implement additional views following the established patterns. Each view should inherit from `BaseView`, implement device-specific layouts, communicate with controllers via events, and provide proper error handling and lifecycle management.