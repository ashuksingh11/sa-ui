using System;
using System.Threading.Tasks;
using Tizen.NUI;
using SamsungAccountUI.Views.Common;
using SamsungAccountUI.Views.FamilyHub;
using SamsungAccountUI.Views.AIHome;
using SamsungAccountUI.Controllers.Authentication;
using SamsungAccountUI.Controllers.Account;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Services.Device;
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Mock;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Models.Authentication;
using SamsungAccountUI.Models.User;

namespace SamsungAccountUI.Examples
{
    /// <summary>
    /// Comprehensive example showing how to create, wire, and use the sample views
    /// Demonstrates complete navigation flow, controller communication, and event handling
    /// </summary>
    public class SampleViewUsage
    {
        private IGlobalConfigService _configService;
        private IDeviceDetectionService _deviceService;
        private ISamsungAccountService _accountService;
        private DeviceInfo _deviceInfo;
        
        // Controllers
        private LoginController _loginController;
        private AccountInfoController _accountController;
        
        // Current view
        private BaseView _currentView;
        private Window _window;
        
        public SampleViewUsage()
        {
            InitializeServices();
            InitializeControllers();
        }
        
        #region Initialization
        
        private void InitializeServices()
        {
            // Initialize configuration service
            _configService = new GlobalConfigService();
            
            // Initialize device detection
            _deviceService = new DeviceDetectionService();
            _deviceInfo = _deviceService.GetCurrentDeviceInfo();
            
            // Initialize account service (using mock for demo)
            _accountService = new MockSamsungAccountService();
            
            Console.WriteLine($"Initialized for device type: {_deviceInfo.Type}");
            Console.WriteLine($"Device dimensions: {_deviceInfo.Dimensions.Width}x{_deviceInfo.Dimensions.Height}");
        }
        
        private void InitializeControllers()
        {
            // Create controllers with dependencies
            _loginController = new LoginController(
                null, // Navigation service will be set later
                _accountService,
                _configService,
                _deviceInfo
            );
            
            _accountController = new AccountInfoController(
                null, // Navigation service will be set later
                _accountService,
                _configService,
                _deviceInfo
            );
            
            // Wire up controller events
            WireControllerEvents();
        }
        
        private void WireControllerEvents()
        {
            // Login controller events
            _loginController.LoginCompleted += OnLoginCompleted;
            _loginController.LoginFailed += OnLoginFailed;
            
            // Account controller events
            _accountController.AccountStateChanged += OnAccountStateChanged;
            _accountController.UserSwitched += OnUserSwitched;
            _accountController.UserLoggedOut += OnUserLoggedOut;
        }
        
        #endregion
        
        #region View Creation Examples
        
        /// <summary>
        /// Example: Create device-specific QR Login view
        /// </summary>
        public async Task<BaseView> CreateQRLoginViewExample()
        {
            BaseView qrView;
            
            switch (_deviceInfo.Type)
            {
                case DeviceType.FamilyHub:
                    Console.WriteLine("Creating FamilyHub QR Login view...");
                    qrView = new FamilyHub.QRLoginView(_loginController, _configService, _deviceInfo);
                    break;
                    
                case DeviceType.AIHome:
                    Console.WriteLine("Creating AIHome QR Login view...");
                    qrView = new AIHome.QRLoginView(_loginController, _configService, _deviceInfo);
                    break;
                    
                default:
                    throw new NotSupportedException($"Device type {_deviceInfo.Type} not supported");
            }
            
            // Wire up view events
            WireViewEvents(qrView);
            
            // Load content
            await qrView.LoadContentAsync();
            
            return qrView;
        }
        
        /// <summary>
        /// Example: Create Password Login view (FamilyHub version)
        /// </summary>
        public async Task<BaseView> CreatePasswordLoginViewExample()
        {
            Console.WriteLine("Creating Password Login view...");
            
            var passwordView = new FamilyHub.PasswordLoginView(_loginController, _configService, _deviceInfo);
            
            // Wire up view events
            WireViewEvents(passwordView);
            
            // Load content
            await passwordView.LoadContentAsync();
            
            return passwordView;
        }
        
        /// <summary>
        /// Example: Create Account Info view
        /// </summary>
        public async Task<BaseView> CreateAccountInfoViewExample()
        {
            Console.WriteLine("Creating Account Info view...");
            
            var accountView = new FamilyHub.AccountInfoView(_accountController, _configService, _deviceInfo);
            
            // Wire up view events
            WireViewEvents(accountView);
            
            // Load content
            await accountView.LoadContentAsync();
            
            return accountView;
        }
        
        #endregion
        
        #region Navigation Examples
        
        /// <summary>
        /// Example: Complete navigation flow from QR login to account info
        /// </summary>
        public async Task DemonstrateCompleteNavigationFlow()
        {
            Console.WriteLine("=== Starting Complete Navigation Flow Demo ===");
            
            try
            {
                // Step 1: Start with QR Login
                Console.WriteLine("Step 1: Loading QR Login view...");
                _currentView = await CreateQRLoginViewExample();
                ShowView(_currentView);
                
                // Step 2: Simulate user choosing password login
                Console.WriteLine("Step 2: User chooses password login...");
                await Task.Delay(2000); // Simulate user interaction
                await NavigateToPasswordLogin();
                
                // Step 3: Simulate successful login
                Console.WriteLine("Step 3: Simulating successful login...");
                await Task.Delay(2000);
                await SimulateSuccessfulLogin();
                
                // Step 4: Navigate to account info
                Console.WriteLine("Step 4: Navigating to account info...");
                await NavigateToAccountInfo();
                
                Console.WriteLine("=== Navigation Flow Demo Completed ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation flow error: {ex.Message}");
            }
        }
        
        private async Task NavigateToPasswordLogin()
        {
            await DisposeCurrentView();
            _currentView = await CreatePasswordLoginViewExample();
            ShowView(_currentView);
        }
        
        private async Task NavigateToAccountInfo()
        {
            await DisposeCurrentView();
            _currentView = await CreateAccountInfoViewExample();
            ShowView(_currentView);
        }
        
        private async Task NavigateBackToQRLogin()
        {
            await DisposeCurrentView();
            _currentView = await CreateQRLoginViewExample();
            ShowView(_currentView);
        }
        
        #endregion
        
        #region Controller Communication Examples
        
        /// <summary>
        /// Example: Simulate successful password login
        /// </summary>
        private async Task SimulateSuccessfulLogin()
        {
            Console.WriteLine("Simulating password login...");
            
            // This would normally be triggered by the view
            await _loginController.HandlePasswordLoginAsync("john@samsung.com", "password123");
        }
        
        /// <summary>
        /// Example: Simulate QR code scan
        /// </summary>
        public async Task SimulateQRScan()
        {
            Console.WriteLine("Simulating QR code scan...");
            
            // Generate mock QR token
            var qrToken = $"QR_{Guid.NewGuid().ToString("N")[..8]}";
            
            // Trigger QR login
            await _loginController.HandleQRLoginAsync(qrToken);
        }
        
        /// <summary>
        /// Example: Simulate user switching
        /// </summary>
        public async Task SimulateUserSwitch()
        {
            Console.WriteLine("Simulating user switch...");
            
            // Get all accounts first
            var allAccounts = await _accountService.GetAllAccountListAsync();
            if (allAccounts.Count > 1)
            {
                // Switch to first non-active user
                var targetUser = allAccounts.Find(u => !u.IsActiveUser);
                if (targetUser != null)
                {
                    await _accountController.HandleUserSwitchAsync(targetUser.UserId);
                }
            }
        }
        
        /// <summary>
        /// Example: Simulate logout
        /// </summary>
        public async Task SimulateLogout()
        {
            Console.WriteLine("Simulating logout...");
            
            var activeUser = await _accountService.GetDefaultUserAsync();
            if (activeUser != null)
            {
                await _accountController.HandleLogoutAsync(activeUser.UserId);
            }
        }
        
        #endregion
        
        #region Event Handling Examples
        
        /// <summary>
        /// Example: Wire up view events for navigation
        /// </summary>
        private void WireViewEvents(BaseView view)
        {
            // Handle navigation requests from views
            view.NavigationRequested += async (sender, screenName) =>
            {
                Console.WriteLine($"Navigation requested to: {screenName}");
                await HandleNavigationRequest(screenName);
            };
            
            // Handle action requests from views
            view.ActionRequested += (sender, actionData) =>
            {
                Console.WriteLine($"Action requested: {actionData}");
                HandleActionRequest(actionData);
            };
            
            // Handle view loaded events
            view.ViewLoaded += (sender, e) =>
            {
                Console.WriteLine($"View loaded: {view.GetType().Name}");
            };
        }
        
        /// <summary>
        /// Example: Handle navigation requests from views
        /// </summary>
        private async Task HandleNavigationRequest(string screenName)
        {
            switch (screenName.ToLower())
            {
                case "qrlogin":
                    await NavigateBackToQRLogin();
                    break;
                    
                case "passwordlogin":
                    await NavigateToPasswordLogin();
                    break;
                    
                case "accountinfo":
                    await NavigateToAccountInfo();
                    break;
                    
                case "googlelogin":
                    Console.WriteLine("Google login not implemented in this demo");
                    break;
                    
                case "settings":
                    Console.WriteLine("Settings screen not implemented in this demo");
                    break;
                    
                default:
                    Console.WriteLine($"Unknown navigation target: {screenName}");
                    break;
            }
        }
        
        /// <summary>
        /// Example: Handle action requests from views
        /// </summary>
        private void HandleActionRequest(object actionData)
        {
            // Views can send custom action data to be processed
            Console.WriteLine($"Processing action: {actionData?.GetType().Name}");
            
            // Example: Handle login data
            if (actionData is LoginRequest loginRequest)
            {
                Console.WriteLine($"Login request for: {loginRequest.Email}");
            }
        }
        
        #endregion
        
        #region Controller Event Handlers
        
        private async void OnLoginCompleted(object sender, LoginResult result)
        {
            Console.WriteLine($"Login completed: Success={result.IsSuccess}");
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"User logged in: {result.User.DisplayName}");
                await NavigateToAccountInfo();
            }
            else
            {
                Console.WriteLine($"Login failed: {result.ErrorMessage}");
            }
        }
        
        private void OnLoginFailed(object sender, string errorMessage)
        {
            Console.WriteLine($"Login failed: {errorMessage}");
        }
        
        private void OnAccountStateChanged(object sender, AccountState accountState)
        {
            Console.WriteLine($"Account state changed: {accountState.AllAccounts.Count} users");
            
            if (accountState.ActiveUser != null)
            {
                Console.WriteLine($"Active user: {accountState.ActiveUser.DisplayName}");
            }
        }
        
        private void OnUserSwitched(object sender, SamsungAccount newActiveUser)
        {
            Console.WriteLine($"User switched to: {newActiveUser.DisplayName}");
        }
        
        private async void OnUserLoggedOut(object sender, string userId)
        {
            Console.WriteLine($"User logged out: {userId}");
            
            // Check if any users remain
            var remainingUsers = await _accountService.GetAllAccountListAsync();
            if (remainingUsers.Count == 0)
            {
                Console.WriteLine("No users remaining, returning to QR login");
                await NavigateBackToQRLogin();
            }
        }
        
        #endregion
        
        #region View Management
        
        private void ShowView(BaseView view)
        {
            if (_window == null)
            {
                _window = NUIApplication.GetDefaultWindow();
            }
            
            // Add view to window
            _window.Add(view);
            
            // Trigger view appearance
            _ = Task.Run(async () => await view.OnAppearingAsync());
            
            Console.WriteLine($"Showing view: {view.GetType().Name}");
        }
        
        private async Task DisposeCurrentView()
        {
            if (_currentView != null)
            {
                // Trigger view disappearance
                await _currentView.OnDisappearingAsync();
                
                // Remove from window
                if (_window != null)
                {
                    _window.Remove(_currentView);
                }
                
                // Dispose view
                _currentView.Dispose();
                _currentView = null;
            }
        }
        
        #endregion
        
        #region Testing and Demo Methods
        
        /// <summary>
        /// Example: Test all view types
        /// </summary>
        public async Task TestAllViews()
        {
            Console.WriteLine("=== Testing All View Types ===");
            
            try
            {
                // Test QR Login
                Console.WriteLine("Testing QR Login view...");
                var qrView = await CreateQRLoginViewExample();
                await Task.Delay(1000);
                qrView.Dispose();
                
                // Test Password Login
                Console.WriteLine("Testing Password Login view...");
                var passwordView = await CreatePasswordLoginViewExample();
                await Task.Delay(1000);
                passwordView.Dispose();
                
                // Test Account Info
                Console.WriteLine("Testing Account Info view...");
                var accountView = await CreateAccountInfoViewExample();
                await Task.Delay(1000);
                accountView.Dispose();
                
                Console.WriteLine("=== All Views Tested Successfully ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"View testing error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Example: Test device-specific differences
        /// </summary>
        public async Task TestDeviceSpecificViews()
        {
            Console.WriteLine("=== Testing Device-Specific Views ===");
            
            // Test FamilyHub QR view
            _deviceInfo = new DeviceInfo
            {
                Type = DeviceType.FamilyHub,
                Dimensions = new ScreenDimensions { Width = 1920, Height = 1080 }
            };
            
            Console.WriteLine("Testing FamilyHub QR view...");
            var familyHubView = new FamilyHub.QRLoginView(_loginController, _configService, _deviceInfo);
            await familyHubView.LoadContentAsync();
            await Task.Delay(1000);
            familyHubView.Dispose();
            
            // Test AIHome QR view
            _deviceInfo = new DeviceInfo
            {
                Type = DeviceType.AIHome,
                Dimensions = new ScreenDimensions { Width = 800, Height = 480 }
            };
            
            Console.WriteLine("Testing AIHome QR view...");
            var aiHomeView = new AIHome.QRLoginView(_loginController, _configService, _deviceInfo);
            await aiHomeView.LoadContentAsync();
            await Task.Delay(1000);
            aiHomeView.Dispose();
            
            Console.WriteLine("=== Device-Specific Views Tested ===");
        }
        
        /// <summary>
        /// Example: Performance test with multiple views
        /// </summary>
        public async Task PerformanceTest()
        {
            Console.WriteLine("=== Running Performance Test ===");
            
            var startTime = DateTime.Now;
            
            // Create and dispose multiple views quickly
            for (int i = 0; i < 10; i++)
            {
                var view = await CreateQRLoginViewExample();
                await Task.Delay(100); // Simulate brief usage
                view.Dispose();
                
                Console.WriteLine($"Iteration {i + 1} completed");
            }
            
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            
            Console.WriteLine($"=== Performance Test Completed in {duration.TotalMilliseconds}ms ===");
        }
        
        #endregion
        
        #region Cleanup
        
        public void Dispose()
        {
            // Dispose current view
            _currentView?.Dispose();
            
            // Clean up controllers
            _loginController?.Dispose();
            _accountController?.Dispose();
            
            Console.WriteLine("SampleViewUsage disposed");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Entry point example for running the sample view usage
    /// </summary>
    public static class SampleViewUsageProgram
    {
        public static async Task RunSampleAsync()
        {
            var sample = new SampleViewUsage();
            
            try
            {
                // Run different demo scenarios
                await sample.TestAllViews();
                await sample.TestDeviceSpecificViews();
                await sample.DemonstrateCompleteNavigationFlow();
                await sample.PerformanceTest();
                
                // Interactive demo
                Console.WriteLine("Starting interactive demo...");
                await sample.CreateQRLoginViewExample();
                
                // Simulate some user interactions
                await Task.Delay(2000);
                await sample.SimulateQRScan();
                
                await Task.Delay(2000);
                await sample.SimulateUserSwitch();
                
                await Task.Delay(2000);
                await sample.SimulateLogout();
            }
            finally
            {
                sample.Dispose();
            }
        }
    }
}