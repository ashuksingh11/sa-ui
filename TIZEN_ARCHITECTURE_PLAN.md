# Tizen-Specific Architecture Plan

## 🎯 **Current Issues to Address**

1. **Microsoft DI not available on Tizen** - Need manual service registration
2. **SA UI is mid-complexity app** - Current architecture may be over-engineered
3. **Navigation handled by Tizen base class** - Custom NavigationService conflicts
4. **NavigationController and factories unused** - Remove unnecessary abstractions
5. **Need Tizen-compatible dummy code** - Create expandable foundation

## 📋 **Simplified Architecture Plan**

### **New Architecture Goals:**
- ✅ **Tizen-compatible** service registration (no Microsoft.Extensions.DI)
- ✅ **Mid-complexity** appropriate (3-4 controllers instead of 8)
- ✅ **Tizen navigation** integration (use built-in patterns)
- ✅ **Simplified services** (essential interfaces only)
- ✅ **Dummy implementations** ready for Tizen expansion

## 🏗️ **Revised Directory Structure**

```
SamsungAccountUI/
├── src/
│   ├── Models/                    # Keep existing models
│   │   ├── User/
│   │   ├── Authentication/
│   │   └── Device/
│   ├── Views/                     # Simplified view structure
│   │   ├── Base/
│   │   │   └── BaseView.cs        # Tizen NUI base view
│   │   ├── FamilyHub/             # Device-specific views only
│   │   └── AIHome/
│   ├── Controllers/               # Simplified to 3-4 controllers
│   │   ├── AuthController.cs      # Login/Logout/Password
│   │   ├── AccountController.cs   # Account info/User switching
│   │   └── AppController.cs       # App lifecycle/navigation
│   ├── Services/
│   │   ├── Core/
│   │   │   ├── ISamsungAccountService.cs
│   │   │   ├── MockSamsungAccountService.cs
│   │   │   ├── IConfigService.cs
│   │   │   └── TizenConfigService.cs
│   │   └── Container/
│   │       └── TizenServiceContainer.cs   # Manual DI replacement
│   ├── Utils/
│   │   └── DeviceHelper.cs        # Device detection utility
│   └── Application/
│       └── Program.cs             # Tizen NUI app entry point
```

## 🔄 **Key Changes Planned**

### **1. Replace Microsoft DI with Manual Service Container**

```csharp
// New: src/Services/Container/TizenServiceContainer.cs
public class TizenServiceContainer
{
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly Dictionary<Type, Func<object>> _factories = new();
    
    public void RegisterSingleton<TInterface, TImplementation>()
        where TImplementation : class, TInterface, new()
    {
        _factories[typeof(TInterface)] = () => 
        {
            if (!_singletons.ContainsKey(typeof(TInterface)))
            {
                _singletons[typeof(TInterface)] = new TImplementation();
            }
            return _singletons[typeof(TInterface)];
        };
    }
    
    public T GetService<T>()
    {
        if (_factories.ContainsKey(typeof(T)))
        {
            return (T)_factories[typeof(T)]();
        }
        throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
    }
}
```

### **2. Simplify Controller Architecture**

```csharp
// Combine: LoginController + LogoutController + PasswordController
public class AuthController
{
    private readonly ISamsungAccountService _accountService;
    
    public AuthController(ISamsungAccountService accountService)
    {
        _accountService = accountService;
    }
    
    public async Task<bool> LoginAsync(AuthType type, string email = null, string password = null, string token = null)
    {
        // Handle QR, Password, Google login
    }
    
    public async Task<bool> LogoutAsync(string userId, string password = null)
    {
        // Handle logout with optional password verification
    }
    
    public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        // Handle password change
    }
}

// Combine: AccountInfoController + UserSwitchController
public class AccountController
{
    public async Task<List<SamsungAccount>> GetAllAccountsAsync() { }
    public async Task<bool> SwitchToUserAsync(string userId) { }
    public async Task<SamsungAccount> GetActiveUserAsync() { }
}

// New: App lifecycle and navigation
public class AppController
{
    public async Task InitializeAppAsync() { }
    public DeviceInfo GetDeviceInfo() { }
    public void SaveAppState() { }
    public void RestoreAppState() { }
}
```

### **3. Tizen Navigation Integration**

```csharp
// Updated: src/Views/Base/BaseView.cs
public abstract class BaseView : Tizen.NUI.BaseComponents.View
{
    protected AuthController AuthController { get; }
    protected AccountController AccountController { get; }
    protected DeviceInfo DeviceInfo { get; }
    
    // Remove custom navigation - use Tizen patterns
    protected void NavigateToView(BaseView targetView)
    {
        // Use Tizen's built-in navigation
        // GetParent().Remove(this);
        // GetParent().Add(targetView);
    }
    
    protected async Task<bool> ShowConfirmDialog(string message)
    {
        // Use Tizen dialog APIs
        // return await Tizen.NUI.Components.AlertDialog.ShowAsync(message);
        return true; // Dummy implementation
    }
}
```

### **4. Updated Program.cs for Tizen**

```csharp
// Updated: src/Application/Program.cs
public class SamsungAccountApp : Tizen.NUI.NUIApplication
{
    private TizenServiceContainer _services;
    private BaseView _currentView;
    private AuthController _authController;
    private AccountController _accountController;
    private AppController _appController;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        // Manual service registration (no Microsoft DI)
        SetupServices();
        
        // Initialize controllers manually
        InitializeControllers();
        
        // Setup main window
        SetupMainWindow();
        
        // Start app flow
        _ = StartAppAsync();
    }
    
    private void SetupServices()
    {
        _services = new TizenServiceContainer();
        
        // Register core services
        _services.RegisterSingleton<ISamsungAccountService, MockSamsungAccountService>();
        _services.RegisterSingleton<IConfigService, TizenConfigService>();
    }
    
    private void InitializeControllers()
    {
        var accountService = _services.GetService<ISamsungAccountService>();
        var configService = _services.GetService<IConfigService>();
        
        _authController = new AuthController(accountService);
        _accountController = new AccountController(accountService);
        _appController = new AppController(configService);
    }
}
```

## 📱 **Simplified Service Layer**

### **Essential Services Only:**

```csharp
// Keep: ISamsungAccountService (core functionality)
public interface ISamsungAccountService
{
    Task<LoginResult> LoginAsync(LoginRequest request);
    Task<bool> LogoutAsync(string userId);
    Task<List<SamsungAccount>> GetAllAccountsAsync();
    Task<bool> SetActiveUserAsync(string userId);
    Task<SamsungAccount> GetActiveUserAsync();
}

// Keep: IConfigService (simplified)
public interface IConfigService
{
    T GetValue<T>(string key, T defaultValue);
    bool SetValue<T>(string key, T value);
    bool IsMultiUserEnabled { get; }
    bool IsQRLoginEnabled { get; }
    bool IsGoogleLoginEnabled { get; }
}

// Remove: INavigationService (use Tizen navigation)
// Remove: IDeviceDetectionService (use utility class)
// Remove: Complex factory interfaces
```

## 🎨 **Device Detection Utility**

```csharp
// New: src/Utils/DeviceHelper.cs
public static class DeviceHelper
{
    public static DeviceInfo GetCurrentDeviceInfo()
    {
        // Use Tizen system APIs for device detection
        // var deviceType = Tizen.System.Information.GetValue<string>("device_type");
        
        // Dummy implementation for now
        return new DeviceInfo
        {
            Type = DeviceType.FamilyHub, // or AIHome
            ScreenSize = new Size(1080, 1920),
            SupportsMultiUser = true
        };
    }
    
    public static bool IsFamilyHub()
    {
        // return GetCurrentDeviceInfo().Type == DeviceType.FamilyHub;
        return true; // Dummy
    }
    
    public static bool IsAIHome()
    {
        // return GetCurrentDeviceInfo().Type == DeviceType.AIHome;
        return false; // Dummy
    }
}
```

## 🔧 **View Implementation Pattern**

```csharp
// Example: FamilyHub/QRLoginView.cs
public class QRLoginView : BaseView
{
    private TextLabel _titleLabel;
    private View _qrContainer;
    private Button _passwordButton;
    private Button _googleButton;
    
    public QRLoginView(AuthController authController, AccountController accountController)
        : base(authController, accountController, DeviceHelper.GetCurrentDeviceInfo())
    {
        CreateLayout();
        WireEvents();
    }
    
    private void CreateLayout()
    {
        // Use Tizen NUI components directly
        _titleLabel = new TextLabel("Samsung Account Login");
        _qrContainer = CreateQRSection();
        _passwordButton = new Button { Text = "Use Password" };
        _googleButton = new Button { Text = "Use Google" };
        
        // Add to view
        Add(_titleLabel);
        Add(_qrContainer);
        Add(_passwordButton);
        Add(_googleButton);
    }
    
    private void WireEvents()
    {
        _passwordButton.Clicked += async (s, e) => 
        {
            var passwordView = new PasswordLoginView(AuthController, AccountController);
            NavigateToView(passwordView);
        };
    }
}
```

## 📊 **Comparison: Before vs After**

| Aspect | Before (Full MVC) | After (Tizen-Optimized) |
|--------|------------------|-------------------------|
| **Controllers** | 8 separate controllers | 3 controllers |
| **Services** | 6+ interfaces | 2 core interfaces |
| **DI Framework** | Microsoft.Extensions.DI | Manual TizenServiceContainer |
| **Navigation** | Custom NavigationService | Tizen built-in patterns |
| **Factories** | ViewFactory, ControllerFactory | Direct instantiation |
| **Files** | ~40 files | ~25 files |
| **Complexity** | Enterprise-grade | Mid-complexity |
| **Tizen Compatibility** | Questionable | Full compatibility |

## 🚀 **Implementation Steps**

1. **Replace DI system** with TizenServiceContainer
2. **Consolidate controllers** (8 → 3)
3. **Remove NavigationService** and use Tizen patterns
4. **Simplify service interfaces** (keep only essential)
5. **Update Program.cs** for manual service setup
6. **Create device helper utility**
7. **Update views** to use new controller structure
8. **Add Tizen-specific dummy implementations**

## 🎯 **Benefits of New Architecture**

✅ **Tizen Compatible**: No external DI dependencies  
✅ **Right-sized**: Appropriate for mid-complexity app  
✅ **Native Navigation**: Uses Tizen built-in patterns  
✅ **Maintainable**: Simpler structure, easier to understand  
✅ **Expandable**: Dummy code ready for Tizen-specific implementation  
✅ **Performance**: Less abstraction overhead  

## 📝 **Migration Strategy**

### Phase 1: Service Container (Priority: High)
- Create TizenServiceContainer
- Replace Microsoft DI registration
- Test service resolution

### Phase 2: Controller Consolidation (Priority: High)  
- Merge related controllers
- Update view dependencies
- Remove unused controller files

### Phase 3: Navigation Integration (Priority: High)
- Remove NavigationService
- Update views to use Tizen navigation
- Test view transitions

### Phase 4: Cleanup (Priority: Medium)
- Remove factory classes
- Update documentation
- Add Tizen-specific comments

This plan transforms the architecture into a Tizen-compatible, mid-complexity application while maintaining the core MVC benefits and providing a solid foundation for Tizen-specific expansion.