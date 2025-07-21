using System;
using System.Threading.Tasks;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Mock;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Services.Device;
using SamsungAccountUI.Views.Navigation;
using SamsungAccountUI.Controllers.Authentication;
using SamsungAccountUI.Controllers.Account;
using SamsungAccountUI.Utils;
using SamsungAccountUI.Views.Common;

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
        private IDeviceDetectionService _deviceService;
        private ISamsungAccountService _accountService;
        private ILogger<SamsungAccountNUIApp> _logger;
        
        // Current view management
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
                
                _logger?.LogInformation("Samsung Account NUI Application created successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create Samsung Account NUI Application");
                Exit();
            }
        }

        protected override void OnAppControlReceived(AppControlReceivedEventArgs e)
        {
            base.OnAppControlReceived(e);
            
            try
            {
                _logger?.LogInformation("App control received");
                
                // Handle deep links or external launches
                var operation = e.ReceivedAppControl.Operation;
                var extraData = e.ReceivedAppControl.ExtraData;
                
                // Example: Handle launch with specific screen
                if (extraData.ContainsKey("screen"))
                {
                    var targetScreen = extraData.Get("screen");
                    _ = Task.Run(() => HandleDeepLinkAsync(targetScreen));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error handling app control");
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            
            try
            {
                _logger?.LogInformation("Application paused");
                
                // Pause current view
                _currentView?.OnDisappearingAsync();
                
                // Save application state if needed
                SaveApplicationState();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during application pause");
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            
            try
            {
                _logger?.LogInformation("Application resumed");
                
                // Resume current view
                _currentView?.OnAppearingAsync();
                
                // Restore application state if needed
                RestoreApplicationState();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during application resume");
            }
        }

        protected override void OnTerminate()
        {
            try
            {
                _logger?.LogInformation("Application terminating");
                
                // Cleanup current view
                CleanupCurrentView();
                
                // Dispose services
                DisposeServices();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during application termination");
            }
            finally
            {
                base.OnTerminate();
            }
        }

        #region Service Configuration

        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            
            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            // Core services
            services.AddSingleton<IGlobalConfigService, GlobalConfigService>();
            services.AddSingleton<IDeviceDetectionService, DeviceDetectionService>();
            
            // Account service - use mock in debug, real in release
            #if DEBUG
            services.AddScoped<ISamsungAccountService, MockSamsungAccountService>();
            #else
            services.AddScoped<ISamsungAccountService, RealSamsungAccountService>();
            #endif
            
            // Navigation service
            services.AddScoped<INavigationService, NavigationService>();
            
            // Controllers
            services.AddScoped<LoginController>();
            services.AddScoped<AccountInfoController>();
            services.AddScoped<UserSwitchController>();
            services.AddScoped<LogoutController>();
            services.AddScoped<PasswordController>();
            
            // Utilities
            services.AddScoped<ViewFactory>();
            services.AddScoped<ControllerFactory>();
            
            _serviceProvider = services.BuildServiceProvider();
        }

        private void InitializeServices()
        {
            _logger = _serviceProvider.GetRequiredService<ILogger<SamsungAccountNUIApp>>();
            _deviceService = _serviceProvider.GetRequiredService<IDeviceDetectionService>();
            _accountService = _serviceProvider.GetRequiredService<ISamsungAccountService>();
            _navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            
            // Configure navigation service
            ConfigureNavigationService();
            
            _logger.LogInformation($"Services initialized for device type: {_deviceService.DetectDeviceType()}");
        }

        private void ConfigureNavigationService()
        {
            // Set the navigation service's window reference
            if (_navigationService is NavigationService navService)
            {
                navService.SetWindow(_mainWindow);
                navService.SetServiceProvider(_serviceProvider);
            }
            
            // Wire up navigation events
            _navigationService.NavigationRequested += OnNavigationRequested;
            _navigationService.ViewChanged += OnViewChanged;
        }

        #endregion

        #region Window and View Management

        private void SetupMainWindow()
        {
            _mainWindow = GetDefaultWindow();
            _mainWindow.Title = "Samsung Account";
            
            // Set window properties based on device type
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

        private void ConfigureWindowForDevice(Models.Device.DeviceInfo deviceInfo)
        {
            switch (deviceInfo.Type)
            {
                case Models.Device.DeviceType.FamilyHub:
                    // Configure for large vertical display
                    _mainWindow.WindowSize = new Size2D(1080, 1920); // Portrait
                    _mainWindow.BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f); // Dark theme
                    break;
                    
                case Models.Device.DeviceType.AIHome:
                    // Configure for compact horizontal display
                    _mainWindow.WindowSize = new Size2D(800, 480); // Landscape
                    _mainWindow.BackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1.0f); // Slightly lighter
                    break;
            }
        }

        private async Task StartApplicationFlowAsync()
        {
            try
            {
                _logger?.LogInformation("Starting application flow");
                
                // Check for existing user accounts
                var existingAccounts = await _accountService.GetAllAccountListAsync();
                
                // Navigate to appropriate initial screen
                if (existingAccounts.Count > 0)
                {
                    await NavigateToScreenAsync("AccountInfo");
                }
                else
                {
                    await NavigateToScreenAsync("QRLogin");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in application flow");
                await NavigateToScreenAsync("QRLogin"); // Fallback to QR login
            }
        }

        private async Task NavigateToScreenAsync(string screenName)
        {
            try
            {
                await Application.Current.Invoke(async () =>
                {
                    await _navigationService.NavigateToAsync(screenName);
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Navigation error to {screenName}");
            }
        }

        #endregion

        #region Event Handlers

        private async void OnNavigationRequested(object sender, string screenName)
        {
            try
            {
                _logger?.LogInformation($"Navigation requested to: {screenName}");
                await NavigateToScreenAsync(screenName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Navigation error to {screenName}");
            }
        }

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
                    // Add to main window
                    _mainWindow.Add(_currentView);
                    
                    // Wire up view events
                    WireViewEvents(_currentView);
                    
                    // Trigger view appearance
                    _ = Task.Run(async () => await _currentView.OnAppearingAsync());
                }
                
                _logger?.LogInformation($"View changed to: {newView?.GetType().Name}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error changing view");
            }
        }

        private void WireViewEvents(BaseView view)
        {
            if (view == null) return;
            
            // Wire navigation events
            view.NavigationRequested += (sender, screenName) =>
            {
                _ = Task.Run(() => NavigateToScreenAsync(screenName));
            };
            
            // Wire action events
            view.ActionRequested += (sender, actionData) =>
            {
                _logger?.LogInformation($"Action requested: {actionData?.GetType().Name}");
            };
            
            // Wire view loaded events
            view.ViewLoaded += (sender, e) =>
            {
                _logger?.LogInformation($"View loaded: {view.GetType().Name}");
            };
        }

        private bool OnWindowKeyEvent(object source, Window.KeyEventArgs e)
        {
            try
            {
                // Handle key events
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
                                        // Default back behavior - minimize or exit
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
                _logger?.LogError(ex, "Error handling key event");
            }
            
            return false;
        }

        private bool OnWindowTouchEvent(object source, Window.TouchEventArgs e)
        {
            // Touch events are handled by individual views
            // This is mainly for global touch tracking if needed
            return false;
        }

        private void OnWindowFocusChanged(object sender, Window.FocusChangedEventArgs e)
        {
            try
            {
                if (e.FocusGained)
                {
                    _logger?.LogInformation("Window gained focus");
                    _currentView?.OnAppearingAsync();
                }
                else
                {
                    _logger?.LogInformation("Window lost focus");
                    _currentView?.OnDisappearingAsync();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error handling focus change");
            }
        }

        private async Task HandleDeepLinkAsync(string targetScreen)
        {
            try
            {
                _logger?.LogInformation($"Handling deep link to: {targetScreen}");
                await NavigateToScreenAsync(targetScreen);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error handling deep link to {targetScreen}");
            }
        }

        #endregion

        #region State Management

        private void SaveApplicationState()
        {
            try
            {
                // Save current screen and user state
                var currentScreen = _currentView?.GetType().Name ?? "Unknown";
                _logger?.LogInformation($"Saving application state: {currentScreen}");
                
                // Use Tizen Preference to save state
                // This could include current user, screen, form data, etc.
                // Preference.Set("current_screen", currentScreen);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving application state");
            }
        }

        private void RestoreApplicationState()
        {
            try
            {
                _logger?.LogInformation("Restoring application state");
                
                // Restore from Tizen Preference
                // var savedScreen = Preference.Get("current_screen", "QRLogin");
                // NavigateToScreenAsync(savedScreen);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error restoring application state");
            }
        }

        #endregion

        #region Cleanup

        private void CleanupCurrentView()
        {
            if (_currentView != null)
            {
                try
                {
                    // Trigger view disappearance
                    _currentView.OnDisappearingAsync().Wait(1000); // Wait max 1 second
                    
                    // Remove from window
                    _mainWindow?.Remove(_currentView);
                    
                    // Dispose view
                    _currentView.Dispose();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error cleaning up current view");
                }
                finally
                {
                    _currentView = null;
                }
            }
        }

        private void DisposeServices()
        {
            try
            {
                // Cleanup navigation service
                if (_navigationService != null)
                {
                    _navigationService.NavigationRequested -= OnNavigationRequested;
                    _navigationService.ViewChanged -= OnViewChanged;
                }
                
                // Dispose service provider
                if (_serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disposing services");
            }
        }

        #endregion
    }

    /// <summary>
    /// Program entry point
    /// </summary>
    class Program : NUIApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            GetDefaultWindow().BackgroundColor = Color.White;
        }

        static void Main(string[] args)
        {
            var app = new SamsungAccountNUIApp();
            app.Run(args);
        }
    }
}