using System;
using System.Threading.Tasks;
using SamsungAccountUI.Controllers.Base;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Services.Device;
using SamsungAccountUI.Utils;
using SamsungAccountUI.Views.Navigation;

namespace SamsungAccountUI.Controllers.Navigation
{
    public class NavigationController : BaseController
    {
        private readonly IDeviceDetectionService _deviceDetectionService;
        private readonly ViewFactory _viewFactory;
        private readonly ControllerFactory _controllerFactory;
        private DeviceInfo _currentDevice;
        
        public NavigationController(
            INavigationService navigationService,
            ISamsungAccountService accountService,
            IGlobalConfigService configService,
            IDeviceDetectionService deviceDetectionService,
            ViewFactory viewFactory,
            ControllerFactory controllerFactory)
            : base(navigationService, accountService, configService)
        {
            _deviceDetectionService = deviceDetectionService;
            _viewFactory = viewFactory;
            _controllerFactory = controllerFactory;
        }
        
        public override async Task LoadAsync()
        {
            await InitializeApplication();
        }
        
        public override async Task HandleInputAsync(object input)
        {
            if (input is NavigationRequest request)
            {
                await ProcessNavigationRequest(request);
            }
            else if (input is string action)
            {
                await HandleNavigationAction(action);
            }
        }
        
        public async Task InitializeApplication()
        {
            try
            {
                await ShowLoading("Initializing application...");
                
                // Detect current device
                _currentDevice = _deviceDetectionService.GetCurrentDeviceInfo();
                SetDeviceType(_currentDevice.Type);
                
                // Check if device is supported
                if (!_deviceDetectionService.IsDeviceSupported())
                {
                    await HideLoading();
                    await ShowError("This device is not supported by Samsung Account UI");
                    return;
                }
                
                // Determine initial navigation based on existing accounts
                await DetermineInitialNavigation();
                
                await HideLoading();
            }
            catch (Exception ex)
            {
                await HideLoading();
                await ShowError("Failed to initialize application. Please restart the app.");
                System.Diagnostics.Debug.WriteLine($"App initialization error: {ex.Message}");
            }
        }
        
        private async Task DetermineInitialNavigation()
        {
            try
            {
                // Check for existing accounts
                var existingAccounts = await AccountService.GetAllAccountListAsync();
                
                if (existingAccounts.Count > 0)
                {
                    // Get default user and navigate to account info
                    var defaultUser = await AccountService.GetDefaultUserAsync();
                    if (defaultUser != null)
                    {
                        await NavigateToScreen("AccountInfo");
                    }
                    else
                    {
                        // Accounts exist but no default user, navigate to user switch
                        await NavigateToScreen("UserSwitch");
                    }
                }
                else
                {
                    // No accounts exist, navigate to login
                    await NavigateToScreen("QRLogin");
                }
            }
            catch (Exception ex)
            {
                // If we can't determine accounts, default to login
                await NavigateToScreen("QRLogin");
                System.Diagnostics.Debug.WriteLine($"Initial navigation determination error: {ex.Message}");
            }
        }
        
        public async Task HandleDeepLink(string deepLink)
        {
            try
            {
                if (string.IsNullOrEmpty(deepLink))
                {
                    return;
                }
                
                // Parse deep link and navigate accordingly
                var segments = deepLink.Split('/');
                if (segments.Length > 0)
                {
                    var screen = segments[0].ToLower();
                    object parameters = null;
                    
                    // Extract parameters if available
                    if (segments.Length > 1)
                    {
                        parameters = ParseDeepLinkParameters(segments);
                    }
                    
                    // Navigate to the requested screen
                    await NavigateToScreen(screen, parameters);
                }
            }
            catch (Exception ex)
            {
                await ShowError("Invalid deep link. Navigating to home screen.");
                await NavigateToScreen("AccountInfo");
                System.Diagnostics.Debug.WriteLine($"Deep link error: {ex.Message}");
            }
        }
        
        public async Task HandleBackNavigation()
        {
            try
            {
                if (NavigationService.CanNavigateBack)
                {
                    await NavigationService.NavigateBackAsync();
                }
                else
                {
                    // If we can't go back, navigate to appropriate home screen
                    var accounts = await AccountService.GetAllAccountListAsync();
                    if (accounts.Count > 0)
                    {
                        await NavigateToScreen("AccountInfo");
                    }
                    else
                    {
                        await NavigateToScreen("QRLogin");
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowError("Navigation error occurred");
                System.Diagnostics.Debug.WriteLine($"Back navigation error: {ex.Message}");
            }
        }
        
        public async Task HandleHomeNavigation()
        {
            try
            {
                // Navigate to appropriate home screen based on account state
                var accounts = await AccountService.GetAllAccountListAsync();
                if (accounts.Count > 0)
                {
                    await NavigationService.ReplaceCurrentAsync("AccountInfo");
                }
                else
                {
                    await NavigationService.ReplaceCurrentAsync("QRLogin");
                }
            }
            catch (Exception ex)
            {
                await ShowError("Failed to navigate home");
                System.Diagnostics.Debug.WriteLine($"Home navigation error: {ex.Message}");
            }
        }
        
        private async Task ProcessNavigationRequest(NavigationRequest request)
        {
            try
            {
                // Validate navigation request
                if (!ViewFactory.IsValidScreenName(request.TargetScreen))
                {
                    await ShowError($"Invalid screen: {request.TargetScreen}");
                    return;
                }
                
                // Check if navigation is allowed
                if (!await IsNavigationAllowed(request))
                {
                    await ShowError("Navigation not allowed");
                    return;
                }
                
                // Perform navigation
                switch (request.Type)
                {
                    case NavigationType.Push:
                        await NavigationService.NavigateToAsync(request.TargetScreen, request.Parameters);
                        break;
                    case NavigationType.Replace:
                        await NavigationService.ReplaceCurrentAsync(request.TargetScreen, request.Parameters);
                        break;
                    case NavigationType.Back:
                        await NavigationService.NavigateBackAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                await ShowError("Navigation failed. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Navigation request error: {ex.Message}");
            }
        }
        
        private async Task<bool> IsNavigationAllowed(NavigationRequest request)
        {
            // Check device-specific navigation restrictions
            if (_currentDevice.Type == DeviceType.AIHome)
            {
                // AIHome devices might have limited navigation options
                var restrictedScreens = new[] { "ProfileDetail", "UserSwitch" };
                if (Array.Exists(restrictedScreens, screen => 
                    string.Equals(screen, request.TargetScreen, StringComparison.OrdinalIgnoreCase)))
                {
                    // Check if these features are enabled for AIHome
                    if (!ConfigService.GetPreferenceValue("samsung.aihome.advanced.features", false))
                    {
                        return false;
                    }
                }
            }
            
            // Check authentication requirements
            var protectedScreens = new[] { "AccountInfo", "ChangePassword", "UserSwitch" };
            if (Array.Exists(protectedScreens, screen => 
                string.Equals(screen, request.TargetScreen, StringComparison.OrdinalIgnoreCase)))
            {
                var accounts = await AccountService.GetAllAccountListAsync();
                return accounts.Count > 0;
            }
            
            return true;
        }
        
        private async Task HandleNavigationAction(string action)
        {
            switch (action.ToLower())
            {
                case "back":
                    await HandleBackNavigation();
                    break;
                case "home":
                    await HandleHomeNavigation();
                    break;
                case "refresh":
                    await RefreshCurrentScreen();
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown navigation action: {action}");
                    break;
            }
        }
        
        private async Task RefreshCurrentScreen()
        {
            try
            {
                var currentScreen = NavigationService.CurrentScreen;
                if (!string.IsNullOrEmpty(currentScreen))
                {
                    // Get controller for current screen and reload
                    var controller = _controllerFactory.GetControllerForScreen(currentScreen, _currentDevice.Type);
                    await controller.LoadAsync();
                }
            }
            catch (Exception ex)
            {
                await ShowError("Failed to refresh screen");
                System.Diagnostics.Debug.WriteLine($"Screen refresh error: {ex.Message}");
            }
        }
        
        private object ParseDeepLinkParameters(string[] segments)
        {
            // Parse deep link parameters (implementation depends on specific deep link format)
            // For now, return a simple parameter object
            return new { Segments = segments };
        }
        
        // Device-specific navigation handling
        public override async Task OnDeviceSpecificAction(string action, object data)
        {
            switch (_currentDevice.Type)
            {
                case DeviceType.AIHome:
                    await HandleAIHomeNavigation(action, data);
                    break;
                case DeviceType.FamilyHub:
                    await HandleFamilyHubNavigation(action, data);
                    break;
            }
        }
        
        private async Task HandleAIHomeNavigation(string action, object data)
        {
            switch (action.ToLower())
            {
                case "quick_back":
                    // Quick back navigation for AIHome
                    await HandleBackNavigation();
                    break;
                case "minimal_navigation":
                    // Enable minimal navigation mode
                    break;
            }
        }
        
        private async Task HandleFamilyHubNavigation(string action, object data)
        {
            switch (action.ToLower())
            {
                case "advanced_navigation":
                    // Enable advanced navigation features
                    break;
                case "sidebar_navigation":
                    // Enable sidebar navigation
                    break;
            }
        }
    }
    
    // Helper classes for navigation
    public class NavigationRequest
    {
        public string TargetScreen { get; set; }
        public NavigationType Type { get; set; }
        public object Parameters { get; set; }
        
        public NavigationRequest(string targetScreen, NavigationType type = NavigationType.Push, object parameters = null)
        {
            TargetScreen = targetScreen;
            Type = type;
            Parameters = parameters;
        }
    }
    
    public enum NavigationType
    {
        Push,
        Replace,
        Back
    }
}