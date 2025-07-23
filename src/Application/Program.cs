using System;
using System.Threading.Tasks;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using SamsungAccountUI.Services.Container;
using SamsungAccountUI.Services.Core;
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Mock;
using SamsungAccountUI.Services.Navigation;
using SamsungAccountUI.Controllers;
using SamsungAccountUI.Views.Base;
using SamsungAccountUI.Views.FamilyHub;
using SamsungAccountUI.Views.AIHome;
using SamsungAccountUI.Utils;
using SamsungAccountUI.Models.Device;

namespace SamsungAccountUI.Application
{
    /// <summary>
    /// Main Samsung Account NUI Application
    /// Uses manual dependency injection for Tizen compatibility
    /// Integrates with Tizen NUI application lifecycle and navigation
    /// </summary>
    public class SamsungAccountApp : NUIApplication
    {
        #region Private Fields

        private TizenServiceContainer _services;
        private Window _mainWindow;
        private ITizenNavigationService _navigationService;
        
        // Controllers
        private AuthController _authController;
        private AccountController _accountController;
        private AppController _appController;

        #endregion

        #region Application Lifecycle

        protected override void OnCreate()
        {
            base.OnCreate();

            try
            {
                // Initialize manual dependency injection
                SetupServices();

                // Initialize controllers
                InitializeControllers();

                // Setup main window
                SetupMainWindow();

                // Initialize navigation service
                InitializeNavigationService();

                // Initialize application
                _ = Task.Run(InitializeApplicationAsync);
            }
            catch (Exception ex)
            {
                // Handle startup errors
                HandleStartupError(ex);
                Exit();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            try
            {
                // Pause current view
                _navigationService?.CurrentView?.OnDisappearingAsync();

                // Save application state
                _appController?.SaveAppState();
            }
            catch (Exception ex)
            {
                _appController?.HandleAppError(ex, "OnPause");
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            try
            {
                // Resume current view
                _navigationService?.CurrentView?.OnAppearingAsync();

                // Restore application state
                _appController?.RestoreAppState();
            }
            catch (Exception ex)
            {
                _appController?.HandleAppError(ex, "OnResume");
            }
        }

        protected override void OnTerminate()
        {
            try
            {
                // Save state before terminating
                _appController?.SaveAppState();

                // Cleanup resources
                CleanupResources();
            }
            catch (Exception ex)
            {
                // Log error but don't prevent termination
            }

            base.OnTerminate();
        }

        protected override void OnAppControlReceived(AppControlReceivedEventArgs e)
        {
            base.OnAppControlReceived(e);

            try
            {
                // Handle deep links or external launches
                var operation = e.ReceivedAppControl.Operation;
                var extraData = e.ReceivedAppControl.ExtraData;

                // Handle specific operations
                if (extraData.ContainsKey("screen"))
                {
                    var targetScreen = extraData.Get("screen");
                    _ = Task.Run(() => HandleDeepLinkAsync(targetScreen));
                }
                else if (operation == "http://tizen.org/appcontrol/operation/login")
                {
                    _ = Task.Run(() => NavigateToLoginAsync());
                }
            }
            catch (Exception ex)
            {
                _appController?.HandleAppError(ex, "OnAppControlReceived");
            }
        }

        #endregion

        #region Service Setup

        private void SetupServices()
        {
            _services = new TizenServiceContainer();

            // Register core services (Singleton - shared across app)
            _services.RegisterSingleton<IConfigService, TizenConfigService>();

            // Register Samsung Account service (mock for development)
            #if DEBUG
            _services.RegisterSingleton<ISamsungAccountService, MockSamsungAccountService>();
            #else
            // TODO: Replace with actual Samsung Account service when available
            _services.RegisterSingleton<ISamsungAccountService, MockSamsungAccountService>();
            #endif
        }

        private void InitializeControllers()
        {
            // Get services
            var configService = _services.GetService<IConfigService>();
            var accountService = _services.GetService<ISamsungAccountService>();

            // Create controllers manually (no factory needed for this complexity)
            _authController = new AuthController(accountService);
            _accountController = new AccountController(accountService);
            _appController = new AppController(configService);
        }

        private void InitializeNavigationService()
        {
            // Create navigation service with window and controllers
            _navigationService = new TizenNavigationService(
                _mainWindow,
                _authController,
                _accountController,
                _appController,
                _services.GetService<IConfigService>()
            );

            // Set navigation service reference in controllers for controller-based navigation
            _authController.SetNavigationService(_navigationService);
        }

        #endregion

        #region Window Setup

        private void SetupMainWindow()
        {
            _mainWindow = GetDefaultWindow();
            _mainWindow.Title = "Samsung Account";
            _mainWindow.BackgroundColor = Color.Black;

            // Configure window based on device type
            ConfigureWindowForDevice();

            // Handle window events
            _mainWindow.KeyEvent += OnWindowKeyEvent;
            _mainWindow.TouchEvent += OnWindowTouchEvent;
            _mainWindow.FocusChanged += OnWindowFocusChanged;
        }

        private void ConfigureWindowForDevice()
        {
            var deviceInfo = DeviceHelper.GetCurrentDeviceInfo();

            switch (deviceInfo.Type)
            {
                case DeviceType.FamilyHub:
                    // Configure for large vertical display (refrigerator)
                    _mainWindow.WindowSize = new Size2D(1080, 1920);
                    _mainWindow.BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
                    break;

                case DeviceType.AIHome:
                    // Configure for compact horizontal display (washing machine, etc.)
                    _mainWindow.WindowSize = new Size2D(800, 480);
                    _mainWindow.BackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1.0f);
                    break;

                default:
                    // Default to FamilyHub configuration
                    _mainWindow.WindowSize = new Size2D(1080, 1920);
                    _mainWindow.BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
                    break;
            }
        }

        #endregion

        #region Application Initialization

        private async Task InitializeApplicationAsync()
        {
            try
            {
                // Initialize app controller
                var initSuccess = await _appController.InitializeAppAsync();
                if (!initSuccess)
                {
                    throw new InvalidOperationException("Failed to initialize application");
                }

                // Determine initial screen
                var initialScreen = await _appController.GetInitialScreenAsync(_accountController);

                // Set initial root view using navigation service
                await _navigationService.SetRootAsync(initialScreen, animated: false);
            }
            catch (Exception ex)
            {
                _appController?.HandleAppError(ex, "InitializeApplication");
                
                // Fallback to QR login on any error
                await _navigationService.SetRootAsync("QRLogin", animated: false);
            }
        }

        #endregion

        #region Navigation (Using TizenNavigationService)

        private async Task HandleDeepLinkAsync(string targetScreen)
        {
            try
            {
                await _navigationService.PushAsync(targetScreen);
            }
            catch (Exception ex)
            {
                _appController?.HandleAppError(ex, $"HandleDeepLink: {targetScreen}");
            }
        }

        private async Task NavigateToLoginAsync()
        {
            try
            {
                var configService = _services.GetService<IConfigService>();
                
                if (configService.IsQRLoginEnabled)
                {
                    await _navigationService.SetRootAsync("QRLogin");
                }
                else if (configService.IsPasswordLoginEnabled)
                {
                    await _navigationService.SetRootAsync("PasswordLogin");
                }
                else
                {
                    await _navigationService.SetRootAsync("QRLogin"); // Default fallback
                }
            }
            catch (Exception ex)
            {
                _appController?.HandleAppError(ex, "NavigateToLogin");
            }
        }

        #endregion

        #region Input Handling

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
                            // Handle back button using navigation service
                            if (_navigationService?.CurrentView != null)
                            {
                                _ = Task.Run(async () =>
                                {
                                    var handled = await _navigationService.CurrentView.OnBackPressedAsync();
                                    if (!handled)
                                    {
                                        // Try to pop navigation stack
                                        if (_navigationService.CanGoBack)
                                        {
                                            await _navigationService.PopAsync();
                                        }
                                        else
                                        {
                                            // No more views in stack - minimize app
                                            Lower();
                                        }
                                    }
                                });
                                return true;
                            }
                            break;

                        case "XF86Home":
                            // Handle home button - minimize app
                            _appController?.HandleHomeNavigation();
                            Lower();
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _appController?.HandleAppError(ex, "OnWindowKeyEvent");
            }

            return false;
        }

        private bool OnWindowTouchEvent(object source, Window.TouchEventArgs e)
        {
            // Touch events are handled by individual views
            // This is mainly for global touch tracking if needed
            return false;
        }

        private void OnWindowFocusChanged(object source, Window.FocusChangedEventArgs e)
        {
            try
            {
                if (e.FocusGained)
                {
                    // Window gained focus - resume if needed
                    _navigationService?.CurrentView?.OnAppearingAsync();
                }
                else
                {
                    // Window lost focus - pause if needed
                    _navigationService?.CurrentView?.OnDisappearingAsync();
                }
            }
            catch (Exception ex)
            {
                _appController?.HandleAppError(ex, "OnWindowFocusChanged");
            }
        }

        #endregion

        #region Error Handling

        private void HandleStartupError(Exception ex)
        {
            try
            {
                // TODO: Log to Tizen logging system
                // Log.Error("SamsungAccountUI", $"Startup error: {ex.Message}");
                
                // For now, just output to console
                Console.WriteLine($"Samsung Account UI startup error: {ex.Message}");
            }
            catch (Exception)
            {
                // Avoid recursive errors during error handling
            }
        }

        #endregion

        #region Cleanup

        private void CleanupResources()
        {
            try
            {
                // Cleanup navigation service (disposes all views)
                _navigationService?.Dispose();

                // Clear singleton instances
                _services?.ClearSingletons();

                // Dispose window events
                if (_mainWindow != null)
                {
                    _mainWindow.KeyEvent -= OnWindowKeyEvent;
                    _mainWindow.TouchEvent -= OnWindowTouchEvent;
                    _mainWindow.FocusChanged -= OnWindowFocusChanged;
                }
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }

        #endregion
    }

    /// <summary>
    /// Application entry point
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Enable NUI debugging in debug builds
                #if DEBUG
                Environment.SetEnvironmentVariable("DALI_LOG_LEVEL", "INFO");
                Environment.SetEnvironmentVariable("DALI_BACKTRACE", "1");
                #endif

                // Create and run the Samsung Account application
                var app = new SamsungAccountApp();
                app.Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start Samsung Account UI: {ex.Message}");
            }
        }
    }
}