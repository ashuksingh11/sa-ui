# Code Examples and Implementation Samples

## 📋 Overview

This section provides practical code examples and implementation samples for common scenarios in the Samsung Account UI application. These examples demonstrate best practices and real-world usage patterns.

## 🚀 Quick Start Examples

### Basic Application Setup

```csharp
// Program.cs - Main Application Entry Point
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Services.Device;
using SamsungAccountUI.Views.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace SamsungAccountUI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup dependency injection
            var serviceProvider = ConfigureServices();
            
            // Initialize application
            var app = new SamsungAccountApplication(serviceProvider);
            await app.RunAsync();
        }
        
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            
            // Register services
            services.AddSingleton<IGlobalConfigService, GlobalConfigService>();
            services.AddSingleton<IDeviceDetectionService, DeviceDetectionService>();
            
            #if DEBUG
            services.AddScoped<ISamsungAccountService, MockSamsungAccountService>();
            #else
            services.AddScoped<ISamsungAccountService, RealSamsungAccountService>();
            #endif
            
            // Register navigation
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<ViewFactory>();
            services.AddScoped<ControllerFactory>();
            
            return services.BuildServiceProvider();
        }
    }
    
    public class SamsungAccountApplication
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;
        
        public SamsungAccountApplication(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _navigationService = serviceProvider.GetRequiredService<INavigationService>();
        }
        
        public async Task RunAsync()
        {
            try
            {
                // Detect device type
                var deviceService = _serviceProvider.GetRequiredService<IDeviceDetectionService>();
                var deviceInfo = deviceService.GetCurrentDeviceInfo();
                
                // Check for existing accounts
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
                
                // Keep application running
                Console.WriteLine("Samsung Account UI started successfully");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application startup failed: {ex.Message}");
            }
        }
    }
}
```

## 🔐 Authentication Examples

### Complete Login Flow Implementation

```csharp
// Example: Complete login flow with error handling
public class LoginFlowExample
{
    private readonly INavigationService _navigationService;
    private readonly ISamsungAccountService _accountService;
    private readonly LoginController _loginController;
    
    public LoginFlowExample(
        INavigationService navigationService,
        ISamsungAccountService accountService)
    {
        _navigationService = navigationService;
        _accountService = accountService;
        _loginController = new LoginController(navigationService, accountService, /* config */);
    }
    
    // Password login example
    public async Task PerformPasswordLoginAsync()
    {
        try
        {
            Console.WriteLine("Starting password login...");
            
            // Show loading
            await _navigationService.ShowLoadingAsync("Signing in...");
            
            // Perform login
            await _loginController.HandlePasswordLogin("john@samsung.com", "password123");
            
            Console.WriteLine("Login completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
            await _navigationService.ShowErrorAsync("Login failed. Please try again.");
        }
        finally
        {
            await _navigationService.HideLoadingAsync();
        }
    }
    
    // QR login example
    public async Task PerformQRLoginAsync()
    {
        try
        {
            Console.WriteLine("Starting QR login...");
            
            // Simulate QR code scanning
            var qrToken = await ScanQRCodeAsync();
            
            if (!string.IsNullOrEmpty(qrToken))
            {
                await _loginController.HandleQRLogin(qrToken);
                Console.WriteLine("QR login completed successfully");
            }
            else
            {
                await _navigationService.ShowErrorAsync("Invalid QR code. Please try again.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"QR login failed: {ex.Message}");
        }
    }
    
    private async Task<string> ScanQRCodeAsync()
    {
        // Simulate QR scanning process
        await Task.Delay(2000);
        return "QR_valid_token_123456";
    }
    
    // Multi-step authentication example
    public async Task PerformMultiStepAuthAsync()
    {
        try
        {
            // Step 1: Try QR login
            Console.WriteLine("Attempting QR login...");
            var qrResult = await TryQRLoginAsync();
            
            if (qrResult.IsSuccess)
            {
                Console.WriteLine("QR login successful");
                return;
            }
            
            // Step 2: Fallback to password login
            Console.WriteLine("QR login failed, trying password login...");
            var passwordResult = await TryPasswordLoginAsync();
            
            if (passwordResult.IsSuccess)
            {
                Console.WriteLine("Password login successful");
                return;
            }
            
            // Step 3: Fallback to Google login
            Console.WriteLine("Password login failed, trying Google login...");
            await TryGoogleLoginAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"All authentication methods failed: {ex.Message}");
        }
    }
    
    private async Task<LoginResult> TryQRLoginAsync()
    {
        var qrToken = await ScanQRCodeAsync();
        var request = new LoginRequest(AuthenticationType.QR) { QRToken = qrToken };
        return await _accountService.LoginAsync(request);
    }
    
    private async Task<LoginResult> TryPasswordLoginAsync()
    {
        var request = new LoginRequest(AuthenticationType.Password)
        {
            Email = "john@samsung.com",
            Password = "password123"
        };
        return await _accountService.LoginAsync(request);
    }
    
    private async Task<LoginResult> TryGoogleLoginAsync()
    {
        var googleToken = await GetGoogleTokenAsync();
        var request = new LoginRequest(AuthenticationType.Google) { GoogleToken = googleToken };
        return await _accountService.LoginAsync(request);
    }
    
    private async Task<string> GetGoogleTokenAsync()
    {
        // Simulate Google OAuth flow
        await Task.Delay(1500);
        return "GOOGLE_oauth_token_xyz";
    }
}
```

### User Management Examples

```csharp
// Example: Complete user management scenarios
public class UserManagementExample
{
    private readonly ISamsungAccountService _accountService;
    private readonly AccountInfoController _accountController;
    
    public UserManagementExample(ISamsungAccountService accountService)
    {
        _accountService = accountService;
        _accountController = new AccountInfoController(/* dependencies */);
    }
    
    // Load and display all users
    public async Task DisplayAllUsersAsync()
    {
        try
        {
            var accounts = await _accountService.GetAllAccountListAsync();
            var activeUser = await _accountService.GetDefaultUserAsync();
            
            Console.WriteLine($"Total accounts: {accounts.Count}");
            Console.WriteLine($"Active user: {activeUser?.DisplayName ?? "None"}");
            
            foreach (var account in accounts)
            {
                var status = account.IsActiveUser ? "[ACTIVE]" : "[INACTIVE]";
                Console.WriteLine($"{status} {account.DisplayName} ({account.Email})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load users: {ex.Message}");
        }
    }
    
    // Switch active user
    public async Task SwitchUserAsync(string userId)
    {
        try
        {
            Console.WriteLine($"Switching to user: {userId}");
            
            var success = await _accountService.SetDefaultUserAsync(userId);
            if (success)
            {
                Console.WriteLine("User switch successful");
                await DisplayCurrentUserAsync();
            }
            else
            {
                Console.WriteLine("User switch failed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error switching user: {ex.Message}");
        }
    }
    
    // Add new user
    public async Task AddNewUserAsync(string email, string password)
    {
        try
        {
            Console.WriteLine($"Adding new user: {email}");
            
            var loginRequest = new LoginRequest(AuthenticationType.Password)
            {
                Email = email,
                Password = password
            };
            
            var result = await _accountService.LoginAsync(loginRequest);
            if (result.IsSuccess)
            {
                Console.WriteLine($"New user added successfully: {result.User.DisplayName}");
            }
            else
            {
                Console.WriteLine($"Failed to add user: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding user: {ex.Message}");
        }
    }
    
    // Remove user
    public async Task RemoveUserAsync(string userId)
    {
        try
        {
            Console.WriteLine($"Removing user: {userId}");
            
            var success = await _accountService.LogoutAsync(userId);
            if (success)
            {
                Console.WriteLine("User removed successfully");
                await DisplayAllUsersAsync();
            }
            else
            {
                Console.WriteLine("Failed to remove user");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing user: {ex.Message}");
        }
    }
    
    // Change user password
    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        try
        {
            Console.WriteLine($"Changing password for user: {userId}");
            
            var success = await _accountService.ChangePasswordAsync(userId, currentPassword, newPassword);
            if (success)
            {
                Console.WriteLine("Password changed successfully");
            }
            else
            {
                Console.WriteLine("Failed to change password");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing password: {ex.Message}");
        }
    }
    
    private async Task DisplayCurrentUserAsync()
    {
        var currentUser = await _accountService.GetDefaultUserAsync();
        if (currentUser != null)
        {
            Console.WriteLine($"Current active user: {currentUser.DisplayName} ({currentUser.Email})");
        }
        else
        {
            Console.WriteLine("No active user");
        }
    }
}
```

## 🎨 UI Implementation Examples

### Device-Specific View Creation

```csharp
// Example: Creating device-specific views
public class ViewCreationExample
{
    public static void CreateQRLoginViews()
    {
        var configService = new GlobalConfigService();
        var navigationService = new NavigationService(/* dependencies */);
        
        // Create FamilyHub QR Login View (Large, Vertical)
        var familyHubView = CreateFamilyHubQRLoginView(navigationService, configService);
        Console.WriteLine("Created FamilyHub QR Login View - Large vertical layout");
        
        // Create AIHome QR Login View (Compact, Horizontal)
        var aiHomeView = CreateAIHomeQRLoginView(navigationService, configService);
        Console.WriteLine("Created AIHome QR Login View - Compact horizontal layout");
    }
    
    private static View CreateFamilyHubQRLoginView(INavigationService nav, IGlobalConfigService config)
    {
        var view = new View
        {
            WidthSpecification = LayoutParamPolicies.MatchParent,
            HeightSpecification = LayoutParamPolicies.MatchParent,
            Layout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Vertical,
                LinearAlignment = LinearLayout.Alignment.Center,
                CellPadding = new Size2D(0, 40)
            },
            Padding = new Extents(60, 60, 80, 80)
        };
        
        // Large welcome text
        var welcomeText = new TextLabel
        {
            Text = "Sign in to Samsung Account",
            TextColor = Color.White,
            PointSize = 28,
            FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold")),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        view.Add(welcomeText);
        
        // Large QR code display
        var qrContainer = new View
        {
            Size = new Size(320, 320),
            BackgroundColor = Color.White,
            CornerRadius = 16.0f,
            Layout = new LinearLayout { LinearAlignment = LinearLayout.Alignment.Center }
        };
        
        var qrImage = new ImageView
        {
            ResourceUrl = "images/qr_large.png",
            Size = new Size(280, 280)
        };
        qrContainer.Add(qrImage);
        view.Add(qrContainer);
        
        // Large instruction text
        var instructionText = new TextLabel
        {
            Text = "Scan this QR code with your Samsung Account mobile app",
            TextColor = Color.White,
            PointSize = 18,
            HorizontalAlignment = HorizontalAlignment.Center,
            MultiLine = true,
            WidthSpecification = 400
        };
        view.Add(instructionText);
        
        // Large alternative buttons
        var buttonContainer = new View
        {
            Layout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Horizontal,
                CellPadding = new Size2D(30, 0)
            }
        };
        
        var passwordButton = new Button
        {
            Text = "Use Password",
            Size = new Size(200, 60),
            PointSize = 16,
            CornerRadius = 8.0f
        };
        buttonContainer.Add(passwordButton);
        
        var googleButton = new Button
        {
            Text = "Use Google",
            Size = new Size(200, 60),
            PointSize = 16,
            CornerRadius = 8.0f
        };
        buttonContainer.Add(googleButton);
        
        view.Add(buttonContainer);
        
        return view;
    }
    
    private static View CreateAIHomeQRLoginView(INavigationService nav, IGlobalConfigService config)
    {
        var view = new View
        {
            WidthSpecification = LayoutParamPolicies.MatchParent,
            HeightSpecification = LayoutParamPolicies.MatchParent,
            Layout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Horizontal,
                LinearAlignment = LinearLayout.Alignment.Center,
                CellPadding = new Size2D(30, 0)
            },
            Padding = new Extents(40, 40, 30, 30)
        };
        
        // Compact QR section
        var qrSection = new View
        {
            Size = new Size(180, 180),
            Layout = new LinearLayout { LinearAlignment = LinearLayout.Alignment.Center }
        };
        
        var qrBackground = new View
        {
            Size = new Size(160, 160),
            BackgroundColor = Color.White,
            CornerRadius = 8.0f,
            Layout = new LinearLayout { LinearAlignment = LinearLayout.Alignment.Center }
        };
        
        var qrImage = new ImageView
        {
            ResourceUrl = "images/qr_small.png",
            Size = new Size(140, 140)
        };
        qrBackground.Add(qrImage);
        qrSection.Add(qrBackground);
        view.Add(qrSection);
        
        // Compact info section
        var infoSection = new View
        {
            WidthSpecification = LayoutParamPolicies.FillToParent,
            Layout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Vertical,
                LinearAlignment = LinearLayout.Alignment.Center,
                CellPadding = new Size2D(0, 15)
            }
        };
        
        // Compact title
        var titleText = new TextLabel
        {
            Text = "Samsung Account",
            TextColor = Color.White,
            PointSize = 16,
            FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold")),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        infoSection.Add(titleText);
        
        // Compact instruction
        var instructionText = new TextLabel
        {
            Text = "Scan QR with mobile app",
            TextColor = Color.White,
            PointSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            MultiLine = true,
            WidthSpecification = 200
        };
        infoSection.Add(instructionText);
        
        // Compact buttons
        var buttonContainer = new View
        {
            Layout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Vertical,
                CellPadding = new Size2D(0, 10)
            }
        };
        
        var passwordButton = new Button
        {
            Text = "Password",
            Size = new Size(120, 35),
            PointSize = 12
        };
        buttonContainer.Add(passwordButton);
        
        var googleButton = new Button
        {
            Text = "Google",
            Size = new Size(120, 35),
            PointSize = 12
        };
        buttonContainer.Add(googleButton);
        
        infoSection.Add(buttonContainer);
        view.Add(infoSection);
        
        return view;
    }
}
```

## 🔧 Configuration Examples

### Dynamic Configuration Management

```csharp
// Example: Dynamic configuration based on device and preferences
public class ConfigurationExample
{
    private readonly IGlobalConfigService _configService;
    private readonly IDeviceDetectionService _deviceService;
    
    public ConfigurationExample(IGlobalConfigService configService, IDeviceDetectionService deviceService)
    {
        _configService = configService;
        _deviceService = deviceService;
    }
    
    public void ApplyDeviceSpecificConfiguration()
    {
        var deviceType = _deviceService.DetectDeviceType();
        
        Console.WriteLine($"Detected device type: {deviceType}");
        
        switch (deviceType)
        {
            case DeviceType.FamilyHub:
                ApplyFamilyHubConfiguration();
                break;
            case DeviceType.AIHome:
                ApplyAIHomeConfiguration();
                break;
        }
        
        ApplyUserPreferences();
    }
    
    private void ApplyFamilyHubConfiguration()
    {
        Console.WriteLine("Applying FamilyHub configuration:");
        Console.WriteLine($"- Multi-user enabled: {_configService.IsMultiUserEnabled}");
        Console.WriteLine($"- Max users: {_configService.MaxUserAccounts}");
        Console.WriteLine($"- QR login enabled: {_configService.IsQRLoginEnabled}");
        Console.WriteLine($"- Google login enabled: {_configService.IsGoogleLoginEnabled}");
        Console.WriteLine($"- Animations enabled: {_configService.EnableAnimations}");
        
        // Apply FamilyHub-specific settings
        if (_configService.IsMultiUserEnabled)
        {
            EnableRichUserProfiles();
        }
        
        if (_configService.EnableAnimations)
        {
            EnableRichAnimations();
        }
    }
    
    private void ApplyAIHomeConfiguration()
    {
        Console.WriteLine("Applying AIHome configuration:");
        
        // AIHome typically has more restrictions
        var maxUsers = Math.Min(_configService.MaxUserAccounts, 4); // Limit to 4 users
        Console.WriteLine($"- Max users (limited): {maxUsers}");
        Console.WriteLine($"- QR login enabled: {_configService.IsQRLoginEnabled}");
        Console.WriteLine($"- Compact layout enabled: true");
        
        // Disable animations for better performance
        Console.WriteLine("- Animations disabled for performance");
        DisableAnimations();
        
        // Enable compact mode
        EnableCompactMode();
    }
    
    private void ApplyUserPreferences()
    {
        Console.WriteLine("Applying user preferences:");
        
        // Theme preference
        var theme = _configService.DefaultUITheme;
        Console.WriteLine($"- Theme: {theme}");
        ApplyTheme(theme);
        
        // Large text preference
        if (_configService.EnableLargeText)
        {
            Console.WriteLine("- Large text enabled");
            EnableLargeText();
        }
        
        // Language preference
        var language = _configService.PreferredLanguage;
        Console.WriteLine($"- Language: {language}");
        SetLanguage(language);
    }
    
    private void EnableRichUserProfiles()
    {
        Console.WriteLine("  → Rich user profiles enabled");
    }
    
    private void EnableRichAnimations()
    {
        Console.WriteLine("  → Rich animations enabled");
    }
    
    private void DisableAnimations()
    {
        Console.WriteLine("  → Animations disabled");
    }
    
    private void EnableCompactMode()
    {
        Console.WriteLine("  → Compact mode enabled");
    }
    
    private void ApplyTheme(string theme)
    {
        Console.WriteLine($"  → {theme} theme applied");
    }
    
    private void EnableLargeText()
    {
        Console.WriteLine("  → Large text mode enabled");
    }
    
    private void SetLanguage(string language)
    {
        Console.WriteLine($"  → Language set to {language}");
    }
    
    // Example: Runtime configuration changes
    public void HandleConfigurationChange(string key, object value)
    {
        Console.WriteLine($"Configuration changed: {key} = {value}");
        
        switch (key.ToLower())
        {
            case "theme":
                ApplyTheme(value.ToString());
                break;
            case "large.text":
                if ((bool)value)
                    EnableLargeText();
                break;
            case "animations":
                if ((bool)value)
                    EnableRichAnimations();
                else
                    DisableAnimations();
                break;
        }
    }
}
```

## 🧪 Testing Examples

### Unit Test Examples

```csharp
// Example: Comprehensive unit tests
[TestClass]
public class RealWorldTestingExample
{
    private Mock<ISamsungAccountService> _mockAccountService;
    private Mock<INavigationService> _mockNavigationService;
    private Mock<IGlobalConfigService> _mockConfigService;
    private LoginController _loginController;
    
    [TestInitialize]
    public void Setup()
    {
        _mockAccountService = new Mock<ISamsungAccountService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockConfigService = new Mock<IGlobalConfigService>();
        
        _loginController = new LoginController(
            _mockNavigationService.Object,
            _mockAccountService.Object,
            _mockConfigService.Object);
    }
    
    [TestMethod]
    public async Task CompleteLoginFlow_Success_NavigatesToAccountInfo()
    {
        // Arrange
        var testUser = new SamsungAccount
        {
            UserId = "test_user",
            Email = "test@samsung.com",
            DisplayName = "Test User",
            IsActiveUser = true
        };
        
        var successResult = LoginResult.Success(testUser, "session_token");
        
        _mockAccountService
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(successResult);
        
        // Act
        await _loginController.HandlePasswordLogin("test@samsung.com", "password123");
        
        // Assert
        _mockAccountService.Verify(x => x.LoginAsync(It.Is<LoginRequest>(r => 
            r.Type == AuthenticationType.Password &&
            r.Email == "test@samsung.com" &&
            r.Password == "password123")), Times.Once);
        
        _mockNavigationService.Verify(x => x.ReplaceCurrentAsync("AccountInfo", testUser), Times.Once);
    }
    
    [TestMethod]
    public async Task LoginWithNetworkError_ShowsRetryOption()
    {
        // Arrange
        var networkError = LoginResult.Failure(AuthenticationError.NetworkError, "Network connection failed");
        
        _mockAccountService
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(networkError);
        
        // Act
        await _loginController.HandlePasswordLogin("test@samsung.com", "password123");
        
        // Assert
        _mockNavigationService.Verify(x => x.ShowErrorAsync(
            It.Is<string>(msg => msg.Contains("Network connection failed")),
            It.IsAny<string>()), Times.Once);
    }
    
    [TestMethod]
    public async Task MultipleUsers_UserSwitch_UpdatesActiveUser()
    {
        // Arrange
        var users = new List<SamsungAccount>
        {
            new SamsungAccount { UserId = "user1", DisplayName = "User 1", IsActiveUser = true },
            new SamsungAccount { UserId = "user2", DisplayName = "User 2", IsActiveUser = false },
            new SamsungAccount { UserId = "user3", DisplayName = "User 3", IsActiveUser = false }
        };
        
        _mockAccountService.Setup(x => x.GetAllAccountListAsync()).ReturnsAsync(users);
        _mockAccountService.Setup(x => x.SetDefaultUserAsync("user2")).ReturnsAsync(true);
        
        var accountController = new AccountInfoController(
            _mockNavigationService.Object,
            _mockAccountService.Object,
            _mockConfigService.Object);
        
        // Act
        await accountController.HandleUserSwitch("user2");
        
        // Assert
        _mockAccountService.Verify(x => x.SetDefaultUserAsync("user2"), Times.Once);
    }
    
    [TestMethod]
    public async Task DeviceSpecificBehavior_AIHome_UsesCompactLayout()
    {
        // Arrange
        _mockConfigService.Setup(x => x.IsAIHomeDevice).Returns(true);
        
        // Act
        await _loginController.OnDeviceSpecificAction("compact_view", null);
        
        // Assert - Would verify compact layout is applied
        // This would depend on the specific implementation
    }
}
```

### Integration Test Example

```csharp
// Example: End-to-end integration test
[TestClass]
public class IntegrationTestExample
{
    private ISamsungAccountService _accountService;
    private INavigationService _navigationService;
    private IGlobalConfigService _configService;
    
    [TestInitialize]
    public void Setup()
    {
        // Use real services for integration testing
        _configService = new GlobalConfigService();
        _accountService = new MockSamsungAccountService(); // Or real service in staging
        _navigationService = new NavigationService(/* dependencies */);
    }
    
    [TestMethod]
    public async Task FullUserJourney_LoginToLogout_CompletesSuccessfully()
    {
        // Step 1: Start with no users
        var initialAccounts = await _accountService.GetAllAccountListAsync();
        foreach (var account in initialAccounts)
        {
            await _accountService.LogoutAsync(account.UserId);
        }
        
        // Step 2: Perform login
        var loginController = new LoginController(_navigationService, _accountService, _configService);
        await loginController.HandlePasswordLogin("test@samsung.com", "password123");
        
        // Verify login successful
        var accounts = await _accountService.GetAllAccountListAsync();
        Assert.AreEqual(1, accounts.Count);
        
        var activeUser = await _accountService.GetDefaultUserAsync();
        Assert.IsNotNull(activeUser);
        Assert.AreEqual("test@samsung.com", activeUser.Email);
        
        // Step 3: Switch to account info
        var accountController = new AccountInfoController(_navigationService, _accountService, _configService);
        await accountController.LoadAccountInfo();
        
        // Step 4: Add another user
        await loginController.HandlePasswordLogin("user2@samsung.com", "password456");
        
        // Verify multiple users
        accounts = await _accountService.GetAllAccountListAsync();
        Assert.AreEqual(2, accounts.Count);
        
        // Step 5: Switch users
        var user1 = accounts.First(u => u.Email == "test@samsung.com");
        await accountController.HandleUserSwitch(user1.UserId);
        
        activeUser = await _accountService.GetDefaultUserAsync();
        Assert.AreEqual("test@samsung.com", activeUser.Email);
        
        // Step 6: Logout one user
        var logoutController = new LogoutController(_navigationService, _accountService, _configService);
        await logoutController.HandleLogout(user1.UserId);
        
        // Verify user removed
        accounts = await _accountService.GetAllAccountListAsync();
        Assert.AreEqual(1, accounts.Count);
        Assert.IsFalse(accounts.Any(u => u.Email == "test@samsung.com"));
        
        // Step 7: Logout last user
        var lastUser = accounts.First();
        await logoutController.HandleLogout(lastUser.UserId);
        
        // Verify all users removed
        accounts = await _accountService.GetAllAccountListAsync();
        Assert.AreEqual(0, accounts.Count);
        
        Console.WriteLine("Full user journey completed successfully");
    }
}
```

## 🎯 Performance Examples

### Performance Monitoring Implementation

```csharp
// Example: Performance monitoring in real scenarios
public class PerformanceExample
{
    private readonly PerformanceMonitor _monitor = new PerformanceMonitor();
    
    public async Task MonitoredLoginOperation()
    {
        // Start performance monitoring
        _monitor.StartOperation("password_login");
        
        try
        {
            var loginController = new LoginController(/* dependencies */);
            await loginController.HandlePasswordLogin("test@samsung.com", "password123");
            
            _monitor.EndOperation("password_login", success: true);
        }
        catch (Exception ex)
        {
            _monitor.EndOperation("password_login", success: false, error: ex.Message);
            throw;
        }
    }
    
    public async Task BenchmarkDifferentDevices()
    {
        var devices = new[] { DeviceType.FamilyHub, DeviceType.AIHome };
        
        foreach (var device in devices)
        {
            Console.WriteLine($"\nBenchmarking {device} device:");
            
            // Test login performance
            await BenchmarkOperation($"login_{device}", async () =>
            {
                await SimulateLogin(device);
            });
            
            // Test navigation performance
            await BenchmarkOperation($"navigation_{device}", async () =>
            {
                await SimulateNavigation(device);
            });
            
            // Test user switching performance
            await BenchmarkOperation($"user_switch_{device}", async () =>
            {
                await SimulateUserSwitch(device);
            });
        }
    }
    
    private async Task BenchmarkOperation(string operationName, Func<Task> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var startMemory = GC.GetTotalMemory(false);
        
        try
        {
            await operation();
            stopwatch.Stop();
            
            var endMemory = GC.GetTotalMemory(false);
            var memoryUsed = endMemory - startMemory;
            
            Console.WriteLine($"  {operationName}:");
            Console.WriteLine($"    Time: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"    Memory: {memoryUsed / 1024}KB");
            
            // Validate against targets
            ValidatePerformance(operationName, stopwatch.Elapsed, memoryUsed);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  {operationName} FAILED: {ex.Message}");
        }
    }
    
    private void ValidatePerformance(string operation, TimeSpan duration, long memoryUsed)
    {
        var targets = GetPerformanceTargets();
        
        if (targets.ContainsKey(operation))
        {
            var target = targets[operation];
            
            if (duration > target.MaxDuration)
            {
                Console.WriteLine($"    ⚠️  TIME WARNING: {duration.TotalMilliseconds}ms > {target.MaxDuration.TotalMilliseconds}ms");
            }
            else
            {
                Console.WriteLine($"    ✅ Time OK");
            }
            
            if (memoryUsed > target.MaxMemory)
            {
                Console.WriteLine($"    ⚠️  MEMORY WARNING: {memoryUsed / 1024}KB > {target.MaxMemory / 1024}KB");
            }
            else
            {
                Console.WriteLine($"    ✅ Memory OK");
            }
        }
    }
    
    private Dictionary<string, PerformanceTarget> GetPerformanceTargets()
    {
        return new Dictionary<string, PerformanceTarget>
        {
            ["login_FamilyHub"] = new PerformanceTarget { MaxDuration = TimeSpan.FromSeconds(2), MaxMemory = 2 * 1024 * 1024 },
            ["login_AIHome"] = new PerformanceTarget { MaxDuration = TimeSpan.FromMilliseconds(1500), MaxMemory = 1024 * 1024 },
            ["navigation_FamilyHub"] = new PerformanceTarget { MaxDuration = TimeSpan.FromMilliseconds(500), MaxMemory = 512 * 1024 },
            ["navigation_AIHome"] = new PerformanceTarget { MaxDuration = TimeSpan.FromMilliseconds(300), MaxMemory = 256 * 1024 },
            ["user_switch_FamilyHub"] = new PerformanceTarget { MaxDuration = TimeSpan.FromMilliseconds(800), MaxMemory = 512 * 1024 },
            ["user_switch_AIHome"] = new PerformanceTarget { MaxDuration = TimeSpan.FromMilliseconds(500), MaxMemory = 256 * 1024 }
        };
    }
    
    private async Task SimulateLogin(DeviceType device)
    {
        // Simulate device-specific login operations
        await Task.Delay(device == DeviceType.FamilyHub ? 800 : 500);
    }
    
    private async Task SimulateNavigation(DeviceType device)
    {
        // Simulate screen navigation
        await Task.Delay(device == DeviceType.FamilyHub ? 300 : 200);
    }
    
    private async Task SimulateUserSwitch(DeviceType device)
    {
        // Simulate user switching
        await Task.Delay(device == DeviceType.FamilyHub ? 600 : 400);
    }
}
```

These examples provide practical, real-world implementations that demonstrate the key concepts and patterns used throughout the Samsung Account UI application. They serve as both learning resources and starting points for implementing specific features.