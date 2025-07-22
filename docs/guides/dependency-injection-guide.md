# Dependency Injection (DI) Guide for Tizen NUI Application

## 📋 Overview

This guide explains how to use Dependency Injection (DI) in the Samsung Account UI application, how to add new services, and how to access them from views and controllers. We use Microsoft.Extensions.DependencyInjection for a clean, maintainable architecture.

## 🏗️ DI Architecture Overview

### What is Dependency Injection?

Dependency Injection is a design pattern that helps:
- **Decouple** components from their dependencies
- **Test** components in isolation
- **Configure** different implementations based on environment
- **Manage** object lifecycles efficiently

### Service Lifetimes

Understanding service lifetimes is crucial:

| Lifetime | Description | Use Case |
|----------|-------------|----------|
| **Singleton** | One instance for entire app lifetime | Configuration, Device Detection, Logging |
| **Scoped** | One instance per request/scope | API Services, Controllers |
| **Transient** | New instance every time | Lightweight stateless services |

## 🚀 Current DI Configuration

### Program.cs - Service Registration

```csharp
private void ConfigureServices()
{
    var services = new ServiceCollection();
    
    // Logging - Singleton (one logger factory for entire app)
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    });
    
    // Core Services - Singleton (shared across entire app)
    services.AddSingleton<IGlobalConfigService, GlobalConfigService>();
    services.AddSingleton<IDeviceDetectionService, DeviceDetectionService>();
    
    // API Services - Scoped (new instance per navigation)
    #if DEBUG
    services.AddScoped<ISamsungAccountService, MockSamsungAccountService>();
    #else
    services.AddScoped<ISamsungAccountService, RealSamsungAccountService>();
    #endif
    
    // Navigation - Scoped
    services.AddScoped<INavigationService, NavigationService>();
    
    // Controllers - Scoped (new instance per view)
    services.AddScoped<LoginController>();
    services.AddScoped<AccountInfoController>();
    services.AddScoped<UserSwitchController>();
    services.AddScoped<LogoutController>();
    services.AddScoped<PasswordController>();
    
    // Utilities - Scoped
    services.AddScoped<ViewFactory>();
    services.AddScoped<ControllerFactory>();
    
    _serviceProvider = services.BuildServiceProvider();
}
```

## 🎯 Adding New Services - Step by Step

### Step 1: Define Service Interface

Always start with an interface for testability and flexibility:

```csharp
// File: src/Services/Analytics/IAnalyticsService.cs
namespace SamsungAccountUI.Services.Analytics
{
    public interface IAnalyticsService
    {
        Task TrackEventAsync(string eventName, Dictionary<string, object> properties = null);
        Task TrackScreenViewAsync(string screenName);
        Task TrackUserActionAsync(string action, string target);
        Task TrackErrorAsync(string errorMessage, Exception exception = null);
        void SetUserId(string userId);
    }
}
```

### Step 2: Implement the Service

Create the concrete implementation:

```csharp
// File: src/Services/Analytics/TizenAnalyticsService.cs
using Microsoft.Extensions.Logging;

namespace SamsungAccountUI.Services.Analytics
{
    public class TizenAnalyticsService : IAnalyticsService
    {
        private readonly ILogger<TizenAnalyticsService> _logger;
        private readonly IGlobalConfigService _configService;
        private string _userId;
        
        public TizenAnalyticsService(
            ILogger<TizenAnalyticsService> logger,
            IGlobalConfigService configService)
        {
            _logger = logger;
            _configService = configService;
        }
        
        public async Task TrackEventAsync(string eventName, Dictionary<string, object> properties = null)
        {
            if (!_configService.IsAnalyticsEnabled)
                return;
                
            try
            {
                _logger.LogInformation($"Analytics Event: {eventName}");
                
                // Send to analytics backend
                // Example: Tizen.System.Information APIs or custom backend
                var analyticsData = new
                {
                    Event = eventName,
                    Properties = properties,
                    UserId = _userId,
                    Timestamp = DateTime.UtcNow,
                    DeviceId = GetDeviceId()
                };
                
                // await SendToAnalyticsBackendAsync(analyticsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track analytics event");
            }
        }
        
        public async Task TrackScreenViewAsync(string screenName)
        {
            await TrackEventAsync("ScreenView", new Dictionary<string, object>
            {
                ["ScreenName"] = screenName,
                ["PreviousScreen"] = GetPreviousScreen()
            });
        }
        
        public async Task TrackUserActionAsync(string action, string target)
        {
            await TrackEventAsync("UserAction", new Dictionary<string, object>
            {
                ["Action"] = action,
                ["Target"] = target
            });
        }
        
        public async Task TrackErrorAsync(string errorMessage, Exception exception = null)
        {
            await TrackEventAsync("Error", new Dictionary<string, object>
            {
                ["ErrorMessage"] = errorMessage,
                ["ExceptionType"] = exception?.GetType().Name,
                ["StackTrace"] = exception?.StackTrace
            });
        }
        
        public void SetUserId(string userId)
        {
            _userId = userId;
        }
        
        private string GetDeviceId()
        {
            // Get unique device ID using Tizen APIs
            // Example: Tizen.System.Information.GetValue<string>("device_id");
            return "device_unique_id";
        }
        
        private string GetPreviousScreen()
        {
            // Track previous screen for navigation analytics
            return "previous_screen";
        }
    }
}
```

### Step 3: Register Service in DI Container

Add to the ConfigureServices method:

```csharp
private void ConfigureServices()
{
    var services = new ServiceCollection();
    
    // ... existing services ...
    
    // Add Analytics Service - Singleton (shared across app)
    services.AddSingleton<IAnalyticsService, TizenAnalyticsService>();
    
    // Or if you want different implementations for debug/release
    #if DEBUG
    services.AddSingleton<IAnalyticsService, MockAnalyticsService>();
    #else
    services.AddSingleton<IAnalyticsService, TizenAnalyticsService>();
    #endif
    
    _serviceProvider = services.BuildServiceProvider();
}
```

## 🎨 Using Services in Views

### Method 1: Through Controller (Recommended)

Controllers should handle business logic and pass data to views:

```csharp
// In Controller
public class LoginController : BaseController
{
    private readonly ISamsungAccountService _accountService;
    private readonly IAnalyticsService _analyticsService;
    
    public LoginController(
        INavigationService navigationService,
        ISamsungAccountService accountService,
        IGlobalConfigService configService,
        IAnalyticsService analyticsService) // New service injected
        : base(navigationService, configService)
    {
        _accountService = accountService;
        _analyticsService = analyticsService;
    }
    
    public async Task HandlePasswordLoginAsync(string email, string password)
    {
        try
        {
            // Track login attempt
            await _analyticsService.TrackUserActionAsync("LoginAttempt", "Password");
            
            var result = await _accountService.LoginAsync(new LoginRequest
            {
                Type = AuthenticationType.Password,
                Email = email,
                Password = password
            });
            
            if (result.IsSuccess)
            {
                // Track successful login
                await _analyticsService.TrackEventAsync("LoginSuccess", new Dictionary<string, object>
                {
                    ["Method"] = "Password",
                    ["UserId"] = result.User.UserId
                });
                
                _analyticsService.SetUserId(result.User.UserId);
            }
            else
            {
                // Track failed login
                await _analyticsService.TrackEventAsync("LoginFailed", new Dictionary<string, object>
                {
                    ["Method"] = "Password",
                    ["Error"] = result.ErrorMessage
                });
            }
        }
        catch (Exception ex)
        {
            await _analyticsService.TrackErrorAsync("Login failed", ex);
            throw;
        }
    }
}
```

### Method 2: Direct Service Access in Views

When views need direct access to services:

```csharp
// Enhanced BaseView with service access
public abstract class BaseView : View
{
    protected IController Controller { get; private set; }
    protected IGlobalConfigService ConfigService { get; private set; }
    protected IServiceProvider ServiceProvider { get; private set; }
    protected DeviceInfo DeviceInfo { get; private set; }
    
    public BaseView(
        IController controller, 
        IGlobalConfigService configService, 
        DeviceInfo deviceInfo,
        IServiceProvider serviceProvider) // Add service provider
    {
        Controller = controller;
        ConfigService = configService;
        DeviceInfo = deviceInfo;
        ServiceProvider = serviceProvider;
        
        InitializeBaseLayout();
        ApplyDeviceSpecificSettings();
    }
    
    // Helper method to get services
    protected T GetService<T>() where T : class
    {
        return ServiceProvider.GetService<T>();
    }
    
    protected T GetRequiredService<T>() where T : class
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}
```

Using services in specific views:

```csharp
public class PasswordLoginView : BaseView
{
    private IAnalyticsService _analyticsService;
    
    public PasswordLoginView(
        LoginController loginController, 
        IGlobalConfigService configService, 
        DeviceInfo deviceInfo,
        IServiceProvider serviceProvider)
        : base(loginController, configService, deviceInfo, serviceProvider)
    {
        _loginController = loginController;
        
        // Get analytics service
        _analyticsService = GetRequiredService<IAnalyticsService>();
    }
    
    public override async Task LoadContentAsync()
    {
        try
        {
            // Track screen view
            await _analyticsService.TrackScreenViewAsync("PasswordLogin");
            
            CreatePasswordLoginForm();
        }
        catch (Exception ex)
        {
            await _analyticsService.TrackErrorAsync("Failed to load login form", ex);
            await ShowErrorAsync($"Failed to load login form: {ex.Message}");
        }
    }
    
    private async void OnSignInClicked(object sender, ClickedEventArgs e)
    {
        // Track button click
        await _analyticsService.TrackUserActionAsync("Click", "SignInButton");
        
        // Continue with login...
    }
}
```

### Method 3: View Factory with Service Injection

Enhance ViewFactory to inject services:

```csharp
public class ViewFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public ViewFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public BaseView CreateView(string screenName, DeviceInfo deviceInfo)
    {
        var configService = _serviceProvider.GetRequiredService<IGlobalConfigService>();
        
        return screenName.ToLower() switch
        {
            "qrlogin" => CreateQRLoginView(deviceInfo, configService),
            "passwordlogin" => CreatePasswordLoginView(deviceInfo, configService),
            "accountinfo" => CreateAccountInfoView(deviceInfo, configService),
            _ => throw new NotSupportedException($"Screen '{screenName}' is not supported")
        };
    }
    
    private BaseView CreatePasswordLoginView(DeviceInfo deviceInfo, IGlobalConfigService configService)
    {
        var loginController = _serviceProvider.GetRequiredService<LoginController>();
        
        return new PasswordLoginView(
            loginController, 
            configService, 
            deviceInfo,
            _serviceProvider); // Pass service provider to view
    }
}
```

## 🔧 Advanced DI Patterns

### 1. Service Collections and Factories

For related services, use a service collection:

```csharp
// Service collection interface
public interface INotificationServices
{
    IPushNotificationService PushService { get; }
    ILocalNotificationService LocalService { get; }
    IToastService ToastService { get; }
}

// Implementation
public class NotificationServices : INotificationServices
{
    public IPushNotificationService PushService { get; }
    public ILocalNotificationService LocalService { get; }
    public IToastService ToastService { get; }
    
    public NotificationServices(
        IPushNotificationService pushService,
        ILocalNotificationService localService,
        IToastService toastService)
    {
        PushService = pushService;
        LocalService = localService;
        ToastService = toastService;
    }
}

// Registration
services.AddSingleton<IPushNotificationService, TizenPushService>();
services.AddSingleton<ILocalNotificationService, TizenLocalNotificationService>();
services.AddSingleton<IToastService, TizenToastService>();
services.AddSingleton<INotificationServices, NotificationServices>();
```

### 2. Options Pattern for Configuration

Use options pattern for service configuration:

```csharp
// Configuration class
public class AnalyticsOptions
{
    public bool Enabled { get; set; } = true;
    public string ApiKey { get; set; }
    public string Endpoint { get; set; }
    public int BatchSize { get; set; } = 10;
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromMinutes(1);
}

// Service using options
public class TizenAnalyticsService : IAnalyticsService
{
    private readonly AnalyticsOptions _options;
    
    public TizenAnalyticsService(IOptions<AnalyticsOptions> options)
    {
        _options = options.Value;
    }
}

// Registration
services.Configure<AnalyticsOptions>(configuration.GetSection("Analytics"));
services.AddSingleton<IAnalyticsService, TizenAnalyticsService>();
```

### 3. Factory Pattern for Dynamic Service Creation

For services that need runtime parameters:

```csharp
// Factory interface
public interface IViewModelFactory
{
    T CreateViewModel<T>(params object[] parameters) where T : class;
}

// Implementation
public class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public ViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public T CreateViewModel<T>(params object[] parameters) where T : class
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, parameters);
    }
}

// Usage
var loginViewModel = _viewModelFactory.CreateViewModel<LoginViewModel>(userId);
```

### 4. Scoped Service Context

For request-scoped data:

```csharp
// Scoped context service
public interface IUserContext
{
    SamsungAccount CurrentUser { get; set; }
    string SessionToken { get; set; }
    DateTime LoginTime { get; set; }
}

public class UserContext : IUserContext
{
    public SamsungAccount CurrentUser { get; set; }
    public string SessionToken { get; set; }
    public DateTime LoginTime { get; set; }
}

// Registration (Scoped - shared within navigation)
services.AddScoped<IUserContext, UserContext>();

// Usage in services
public class SecureApiService
{
    private readonly IUserContext _userContext;
    
    public SecureApiService(IUserContext userContext)
    {
        _userContext = userContext;
    }
    
    public async Task<ApiResponse> MakeAuthenticatedRequestAsync()
    {
        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {_userContext.SessionToken}",
            ["UserId"] = _userContext.CurrentUser?.UserId
        };
        
        // Make API call with auth headers
    }
}
```

## 🎯 Real-World Service Examples

### 1. Biometric Service

```csharp
public interface IBiometricService
{
    Task<bool> IsAvailableAsync();
    Task<bool> AuthenticateAsync(string reason);
    Task<BiometricType> GetBiometricTypeAsync();
}

public class TizenBiometricService : IBiometricService
{
    private readonly ILogger<TizenBiometricService> _logger;
    
    public async Task<bool> IsAvailableAsync()
    {
        // Check Tizen biometric availability
        // return Tizen.Security.BiometricAuthentication.IsSupported();
        return true;
    }
    
    public async Task<bool> AuthenticateAsync(string reason)
    {
        try
        {
            // Perform biometric authentication
            // var result = await Tizen.Security.BiometricAuthentication.AuthenticateAsync(reason);
            // return result.Success;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Biometric authentication failed");
            return false;
        }
    }
}

// Registration
services.AddSingleton<IBiometricService, TizenBiometricService>();
```

### 2. Secure Storage Service

```csharp
public interface ISecureStorageService
{
    Task<bool> SetSecureValueAsync(string key, string value);
    Task<string> GetSecureValueAsync(string key);
    Task<bool> RemoveSecureValueAsync(string key);
    Task<bool> ContainsKeyAsync(string key);
}

public class TizenSecureStorageService : ISecureStorageService
{
    public async Task<bool> SetSecureValueAsync(string key, string value)
    {
        try
        {
            // Use Tizen secure storage
            // Tizen.Security.SecureRepository.Set(key, value);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<string> GetSecureValueAsync(string key)
    {
        // Tizen.Security.SecureRepository.Get(key);
        return "secure_value";
    }
}

// Registration
services.AddSingleton<ISecureStorageService, TizenSecureStorageService>();
```

### 3. Network Connectivity Service

```csharp
public interface IConnectivityService
{
    bool IsConnected { get; }
    NetworkType CurrentNetworkType { get; }
    event EventHandler<NetworkStatusChangedEventArgs> NetworkStatusChanged;
}

public class TizenConnectivityService : IConnectivityService
{
    public bool IsConnected => GetConnectionStatus();
    public NetworkType CurrentNetworkType => GetNetworkType();
    
    public event EventHandler<NetworkStatusChangedEventArgs> NetworkStatusChanged;
    
    public TizenConnectivityService()
    {
        // Subscribe to Tizen network events
        // Tizen.Network.Connection.ConnectionTypeChanged += OnConnectionTypeChanged;
    }
    
    private bool GetConnectionStatus()
    {
        // return Tizen.Network.Connection.CurrentConnectionType != ConnectionType.Disconnected;
        return true;
    }
}

// Registration
services.AddSingleton<IConnectivityService, TizenConnectivityService>();
```

## 🧪 Testing with DI

### Creating Test Services

```csharp
// Test implementation
public class MockAnalyticsService : IAnalyticsService
{
    public List<string> TrackedEvents { get; } = new List<string>();
    
    public async Task TrackEventAsync(string eventName, Dictionary<string, object> properties = null)
    {
        TrackedEvents.Add(eventName);
        await Task.CompletedTask;
    }
}

// In tests
[TestClass]
public class LoginControllerTests
{
    private IServiceProvider CreateTestServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Register test services
        services.AddSingleton<IAnalyticsService, MockAnalyticsService>();
        services.AddSingleton<ISamsungAccountService, MockSamsungAccountService>();
        services.AddSingleton<IGlobalConfigService, MockConfigService>();
        
        return services.BuildServiceProvider();
    }
    
    [TestMethod]
    public async Task Login_Should_Track_Analytics()
    {
        // Arrange
        var serviceProvider = CreateTestServiceProvider();
        var analyticsService = serviceProvider.GetRequiredService<IAnalyticsService>() as MockAnalyticsService;
        var controller = serviceProvider.GetRequiredService<LoginController>();
        
        // Act
        await controller.HandlePasswordLoginAsync("test@samsung.com", "password");
        
        // Assert
        Assert.IsTrue(analyticsService.TrackedEvents.Contains("LoginAttempt"));
    }
}
```

## 📊 Service Lifetime Best Practices

### When to Use Each Lifetime

| Service Type | Lifetime | Example |
|--------------|----------|---------|
| Configuration | Singleton | GlobalConfigService, DeviceDetectionService |
| Logging | Singleton | ILogger, ILoggerFactory |
| Caching | Singleton | IMemoryCache, ICacheService |
| API Clients | Scoped | ISamsungAccountService, IApiClient |
| Controllers | Scoped | LoginController, AccountController |
| View Models | Transient | LoginViewModel, UserViewModel |
| Utilities | Transient | IValidator, IMapper |

### Common Pitfalls to Avoid

1. **Don't inject Scoped into Singleton**
   ```csharp
   // ❌ Wrong - Scoped service in Singleton
   services.AddSingleton<IMyService>(provider => 
       new MyService(provider.GetRequiredService<IScopedService>()));
   
   // ✅ Correct - Use factory pattern
   services.AddSingleton<IMyService, MyService>();
   services.AddScoped<IScopedService, ScopedService>();
   ```

2. **Don't hold references to Transient services**
   ```csharp
   // ❌ Wrong - Storing transient service
   public class MyView
   {
       private readonly ITransientService _service; // Memory leak risk
   }
   
   // ✅ Correct - Get when needed
   public class MyView
   {
       private readonly IServiceProvider _serviceProvider;
       
       private ITransientService GetService() => 
           _serviceProvider.GetRequiredService<ITransientService>();
   }
   ```

## 🚀 Adding Services Checklist

When adding a new service to your application:

- [ ] Define interface in appropriate namespace
- [ ] Implement concrete service
- [ ] Determine appropriate lifetime (Singleton/Scoped/Transient)
- [ ] Register in ConfigureServices()
- [ ] Create mock implementation for testing
- [ ] Update ViewFactory if views need direct access
- [ ] Document service usage
- [ ] Add unit tests
- [ ] Update any dependent services

## 📖 Summary

The DI system in our Tizen NUI application provides:
- **Clean architecture** with proper separation of concerns
- **Testability** through interface-based design
- **Flexibility** to swap implementations
- **Lifecycle management** for efficient resource usage

To add new services:
1. Create interface
2. Implement service
3. Register in DI container
4. Inject where needed (controllers/views)
5. Test with mock implementations

This approach ensures your Samsung Account UI application remains maintainable, testable, and scalable as you add new features and services.