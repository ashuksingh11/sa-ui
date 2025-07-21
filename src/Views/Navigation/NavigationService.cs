using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Utils;
using SamsungAccountUI.Views.Common;
using SamsungAccountUI.Views.FamilyHub;
using SamsungAccountUI.Views.AIHome;
using SamsungAccountUI.Controllers.Authentication;
using SamsungAccountUI.Controllers.Account;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Services.API;

namespace SamsungAccountUI.Views.Navigation
{
    /// <summary>
    /// Tizen NUI-integrated navigation service that manages view lifecycle within NUI application
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly Stack<string> _navigationStack = new Stack<string>();
        private readonly ILogger<NavigationService> _logger;
        private Window _window;
        private IServiceProvider _serviceProvider;
        private DeviceInfo _deviceInfo;
        
        // Current view management
        private BaseView _currentView;
        private View _loadingOverlay;
        private View _errorOverlay;
        
        // Events for NUI application integration
        public event EventHandler<string> NavigationRequested;
        public event EventHandler<BaseView> ViewChanged;
        
        public bool CanNavigateBack => _navigationStack.Count > 1;
        public string CurrentScreen => _navigationStack.Count > 0 ? _navigationStack.Peek() : string.Empty;
        
        public NavigationService(ILogger<NavigationService> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Set the NUI window reference - called by the main application
        /// </summary>
        public void SetWindow(Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            CreateGlobalOverlays();
        }
        
        /// <summary>
        /// Set the service provider for dependency injection - called by the main application
        /// </summary>
        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            
            // Get device info
            var deviceService = _serviceProvider.GetService<Services.Device.IDeviceDetectionService>();
            _deviceInfo = deviceService?.GetCurrentDeviceInfo();
        }
        
        #region Navigation Methods
        
        public async Task NavigateToAsync(string screenName, object parameters = null)
        {
            try
            {
                _logger?.LogInformation($"Navigating to screen: {screenName}");
                
                if (_window == null || _serviceProvider == null)
                {
                    throw new InvalidOperationException("NavigationService not properly initialized. Call SetWindow and SetServiceProvider first.");
                }
                
                var fromScreen = CurrentScreen;
                
                // Create the view for the target screen
                var newView = await CreateViewForScreenAsync(screenName);
                if (newView == null)
                {
                    throw new InvalidOperationException($"Cannot create view for screen: {screenName}");
                }
                
                // Cleanup current view
                await CleanupCurrentViewAsync();
                
                // Set the new view as current
                _currentView = newView;
                
                // Add to navigation stack
                _navigationStack.Push(screenName);
                
                // Load content for the new view
                await _currentView.LoadContentAsync();
                
                // Initialize view with parameters if provided
                if (parameters != null)
                {
                    await InitializeViewWithParameters(_currentView, parameters);
                }
                
                // Notify the main application about view change
                ViewChanged?.Invoke(this, _currentView);
                
                _logger?.LogInformation($"Successfully navigated from {fromScreen} to {screenName}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to navigate to {screenName}");
                throw new NavigationException($"Failed to navigate to {screenName}: {ex.Message}", ex);
            }
        }
        
        public async Task NavigateBackAsync()
        {
            if (!CanNavigateBack)
            {
                throw new InvalidOperationException("Cannot navigate back - no previous screen in stack");
            }
            
            try
            {
                var fromScreen = _navigationStack.Pop(); // Remove current screen
                var toScreen = _navigationStack.Peek(); // Get previous screen
                
                _logger?.LogInformation($"Navigating back from {fromScreen} to {toScreen}");
                
                // Create view for previous screen
                var previousView = await CreateViewForScreenAsync(toScreen);
                
                // Cleanup current view
                await CleanupCurrentViewAsync();
                
                // Set previous view as current
                _currentView = previousView;
                await _currentView.LoadContentAsync();
                
                // Notify the main application about view change
                ViewChanged?.Invoke(this, _currentView);
                
                _logger?.LogInformation($"Successfully navigated back to {toScreen}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to navigate back");
                throw new NavigationException($"Failed to navigate back: {ex.Message}", ex);
            }
        }
        
        public async Task ReplaceCurrentAsync(string screenName, object parameters = null)
        {
            try
            {
                _logger?.LogInformation($"Replacing current screen with: {screenName}");
                
                var fromScreen = CurrentScreen;
                
                // Remove current screen from stack if exists
                if (_navigationStack.Count > 0)
                {
                    _navigationStack.Pop();
                }
                
                // Navigate to new screen (which will add it to stack)
                await NavigateToAsync(screenName, parameters);
                
                _logger?.LogInformation($"Successfully replaced {fromScreen} with {screenName}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to replace current screen with {screenName}");
                throw new NavigationException($"Failed to replace current screen with {screenName}: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region View Creation
        
        private async Task<BaseView> CreateViewForScreenAsync(string screenName)
        {
            try
            {
                _logger?.LogDebug($"Creating view for screen: {screenName}");
                
                // Get required services
                var configService = _serviceProvider.GetRequiredService<IGlobalConfigService>();
                
                BaseView view = screenName.ToLower() switch
                {
                    "qrlogin" => CreateQRLoginView(configService),
                    "passwordlogin" => CreatePasswordLoginView(configService),
                    "googlelogin" => CreateGoogleLoginView(configService),
                    "accountinfo" => CreateAccountInfoView(configService),
                    "logout" => CreateLogoutView(configService),
                    "changepassword" => CreateChangePasswordView(configService),
                    "settings" => CreateSettingsView(configService),
                    "forgotpassword" => CreateForgotPasswordView(configService),
                    _ => throw new NotSupportedException($"Screen '{screenName}' is not supported")
                };
                
                _logger?.LogDebug($"Successfully created view for screen: {screenName}");
                return view;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating view for screen: {screenName}");
                throw;
            }
        }
        
        private BaseView CreateQRLoginView(IGlobalConfigService configService)
        {
            var loginController = _serviceProvider.GetRequiredService<LoginController>();
            
            return _deviceInfo.Type switch
            {
                DeviceType.FamilyHub => new Views.FamilyHub.QRLoginView(loginController, configService, _deviceInfo),
                DeviceType.AIHome => new Views.AIHome.QRLoginView(loginController, configService, _deviceInfo),
                _ => throw new NotSupportedException($"Device type {_deviceInfo.Type} not supported for QR login")
            };
        }
        
        private BaseView CreatePasswordLoginView(IGlobalConfigService configService)
        {
            var loginController = _serviceProvider.GetRequiredService<LoginController>();
            
            // Currently only FamilyHub version implemented
            return new Views.FamilyHub.PasswordLoginView(loginController, configService, _deviceInfo);
        }
        
        private BaseView CreateGoogleLoginView(IGlobalConfigService configService)
        {
            var loginController = _serviceProvider.GetRequiredService<LoginController>();
            
            // Placeholder - implement Google login view
            throw new NotImplementedException("Google login view not yet implemented");
        }
        
        private BaseView CreateAccountInfoView(IGlobalConfigService configService)
        {
            var accountController = _serviceProvider.GetRequiredService<AccountInfoController>();
            
            // Currently only FamilyHub version implemented
            return new Views.FamilyHub.AccountInfoView(accountController, configService, _deviceInfo);
        }
        
        private BaseView CreateLogoutView(IGlobalConfigService configService)
        {
            var logoutController = _serviceProvider.GetRequiredService<LogoutController>();
            
            // Placeholder - implement logout confirmation view
            throw new NotImplementedException("Logout view not yet implemented");
        }
        
        private BaseView CreateChangePasswordView(IGlobalConfigService configService)
        {
            var passwordController = _serviceProvider.GetRequiredService<PasswordController>();
            
            // Placeholder - implement change password view
            throw new NotImplementedException("Change password view not yet implemented");
        }
        
        private BaseView CreateSettingsView(IGlobalConfigService configService)
        {
            // Placeholder - implement settings view
            throw new NotImplementedException("Settings view not yet implemented");
        }
        
        private BaseView CreateForgotPasswordView(IGlobalConfigService configService)
        {
            // Placeholder - implement forgot password view
            throw new NotImplementedException("Forgot password view not yet implemented");
        }
        
        #endregion
        
        #region Overlay Management
        
        private void CreateGlobalOverlays()
        {
            try
            {
                // Create loading overlay
                _loadingOverlay = new View
                {
                    WidthSpecification = LayoutParamPolicies.MatchParent,
                    HeightSpecification = LayoutParamPolicies.MatchParent,
                    BackgroundColor = new Color(0, 0, 0, 0.7f),
                    Layout = new LinearLayout
                    {
                        LinearOrientation = LinearLayout.Orientation.Vertical,
                        LinearAlignment = LinearLayout.Alignment.Center,
                        CellPadding = new Size2D(0, 20)
                    },
                    Visibility = false
                };
                
                // Create error overlay
                _errorOverlay = new View
                {
                    WidthSpecification = LayoutParamPolicies.MatchParent,
                    HeightSpecification = LayoutParamPolicies.MatchParent,
                    BackgroundColor = new Color(0, 0, 0, 0.8f),
                    Layout = new LinearLayout
                    {
                        LinearOrientation = LinearLayout.Orientation.Vertical,
                        LinearAlignment = LinearLayout.Alignment.Center
                    },
                    Visibility = false
                };
                
                _logger?.LogDebug("Global overlays created");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating global overlays");
            }
        }
        
        public async Task ShowLoadingAsync(string message = "Loading...")
        {
            try
            {
                if (_loadingOverlay != null && _window != null)
                {
                    // Clear previous content
                    _loadingOverlay.Children.Clear();
                    
                    // Loading spinner
                    var spinner = new ImageView
                    {
                        ResourceUrl = "images/loading_spinner.png",
                        Size = new Size(60, 60)
                    };
                    
                    // Loading text
                    var loadingText = new TextLabel
                    {
                        Text = message,
                        TextColor = Color.White,
                        PointSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    
                    _loadingOverlay.Add(spinner);
                    _loadingOverlay.Add(loadingText);
                    
                    // Add to window and show
                    _window.Add(_loadingOverlay);
                    _loadingOverlay.Visibility = true;
                    
                    // Animate appearance
                    var animation = new Animation(300);
                    animation.AnimateTo(_loadingOverlay, "Opacity", 1.0f);
                    animation.Play();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing loading overlay");
            }
        }
        
        public async Task HideLoadingAsync()
        {
            try
            {
                if (_loadingOverlay != null && _window != null && _loadingOverlay.Visibility)
                {
                    // Animate disappearance
                    var animation = new Animation(200);
                    animation.AnimateTo(_loadingOverlay, "Opacity", 0.0f);
                    animation.Finished += (sender, e) =>
                    {
                        _loadingOverlay.Visibility = false;
                        _window.Remove(_loadingOverlay);
                    };
                    animation.Play();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error hiding loading overlay");
            }
        }
        
        public async Task ShowErrorAsync(string message, string title = "Error")
        {
            try
            {
                if (_errorOverlay != null && _window != null)
                {
                    // Clear previous content
                    _errorOverlay.Children.Clear();
                    
                    // Error container
                    var errorContainer = new View
                    {
                        WidthSpecification = 400,
                        HeightSpecification = LayoutParamPolicies.WrapContent,
                        BackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1.0f),
                        CornerRadius = 16.0f,
                        Layout = new LinearLayout
                        {
                            LinearOrientation = LinearLayout.Orientation.Vertical,
                            LinearAlignment = LinearLayout.Alignment.Center,
                            CellPadding = new Size2D(0, 20)
                        },
                        Padding = new Extents(30, 30, 30, 30)
                    };
                    
                    // Error title
                    var titleLabel = new TextLabel
                    {
                        Text = title,
                        TextColor = new Color(0.9f, 0.3f, 0.3f, 1.0f),
                        PointSize = 18,
                        FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold")),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    
                    // Error message
                    var messageLabel = new TextLabel
                    {
                        Text = message,
                        TextColor = Color.White,
                        PointSize = 14,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        MultiLine = true,
                        WidthSpecification = 340
                    };
                    
                    // OK button
                    var okButton = new Button
                    {
                        Text = "OK",
                        Size = new Size(100, 40),
                        PointSize = 14,
                        CornerRadius = 8.0f,
                        BackgroundColor = new Color(0.2f, 0.6f, 1.0f, 1.0f),
                        TextColor = Color.White
                    };
                    
                    okButton.Clicked += (sender, e) => _ = Task.Run(HideErrorAsync);
                    
                    errorContainer.Add(titleLabel);
                    errorContainer.Add(messageLabel);
                    errorContainer.Add(okButton);
                    
                    _errorOverlay.Add(errorContainer);
                    
                    // Add to window and show
                    _window.Add(_errorOverlay);
                    _errorOverlay.Visibility = true;
                    
                    // Animate appearance
                    var animation = new Animation(300);
                    animation.AnimateTo(_errorOverlay, "Opacity", 1.0f);
                    animation.Play();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing error overlay");
            }
        }
        
        private async Task HideErrorAsync()
        {
            try
            {
                if (_errorOverlay != null && _window != null && _errorOverlay.Visibility)
                {
                    // Animate disappearance
                    var animation = new Animation(200);
                    animation.AnimateTo(_errorOverlay, "Opacity", 0.0f);
                    animation.Finished += (sender, e) =>
                    {
                        _errorOverlay.Visibility = false;
                        _window.Remove(_errorOverlay);
                    };
                    animation.Play();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error hiding error overlay");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private async Task CleanupCurrentViewAsync()
        {
            if (_currentView != null)
            {
                try
                {
                    // Trigger view disappearance
                    await _currentView.OnDisappearingAsync();
                    
                    // Remove from window
                    _window?.Remove(_currentView);
                    
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
        
        private async Task InitializeViewWithParameters(BaseView view, object parameters)
        {
            try
            {
                if (view is IParameterizedView paramView)
                {
                    await paramView.InitializeAsync(parameters);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing view with parameters");
            }
        }
        
        #endregion
        
        #region Cleanup
        
        public void Dispose()
        {
            try
            {
                // Cleanup current view
                _currentView?.OnDisappearingAsync().Wait(1000);
                _currentView?.Dispose();
                
                // Dispose overlays
                _loadingOverlay?.Dispose();
                _errorOverlay?.Dispose();
                
                // Clear navigation stack
                _navigationStack.Clear();
                
                _logger?.LogInformation("NavigationService disposed");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disposing NavigationService");
            }
        }
        
        #endregion
    }
    
    // Interface for views that can accept parameters
    public interface IParameterizedView
    {
        Task InitializeAsync(object parameters);
    }
    
    // Custom exception for navigation errors
    public class NavigationException : Exception
    {
        public NavigationException(string message) : base(message) { }
        public NavigationException(string message, Exception innerException) : base(message, innerException) { }
    }
}