using System;
using SamsungAccountUI.Controllers.Account;
using SamsungAccountUI.Controllers.Authentication;
using SamsungAccountUI.Controllers.Base;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Views.Navigation;

namespace SamsungAccountUI.Utils
{
    public class ControllerFactory
    {
        private readonly INavigationService _navigationService;
        private readonly ISamsungAccountService _accountService;
        private readonly IGlobalConfigService _configService;
        
        public ControllerFactory(
            INavigationService navigationService,
            ISamsungAccountService accountService,
            IGlobalConfigService configService)
        {
            _navigationService = navigationService;
            _accountService = accountService;
            _configService = configService;
        }
        
        public BaseController CreateController(string controllerName, DeviceType deviceType)
        {
            var controller = controllerName.ToLower() switch
            {
                "login" => new LoginController(_navigationService, _accountService, _configService),
                "logout" => new LogoutController(_navigationService, _accountService, _configService),
                "password" => new PasswordController(_navigationService, _accountService, _configService),
                "accountinfo" => new AccountInfoController(_navigationService, _accountService, _configService),
                "userswitch" => new UserSwitchController(_navigationService, _accountService, _configService),
                _ => throw new ArgumentException($"Unknown controller name: {controllerName}")
            };
            
            // Set device type for device-specific behavior
            controller.SetDeviceType(deviceType);
            
            return controller;
        }
        
        public T CreateController<T>(DeviceType deviceType) where T : BaseController
        {
            var controllerType = typeof(T);
            
            if (controllerType == typeof(LoginController))
            {
                var controller = new LoginController(_navigationService, _accountService, _configService);
                controller.SetDeviceType(deviceType);
                return controller as T;
            }
            else if (controllerType == typeof(LogoutController))
            {
                var controller = new LogoutController(_navigationService, _accountService, _configService);
                controller.SetDeviceType(deviceType);
                return controller as T;
            }
            else if (controllerType == typeof(PasswordController))
            {
                var controller = new PasswordController(_navigationService, _accountService, _configService);
                controller.SetDeviceType(deviceType);
                return controller as T;
            }
            else if (controllerType == typeof(AccountInfoController))
            {
                var controller = new AccountInfoController(_navigationService, _accountService, _configService);
                controller.SetDeviceType(deviceType);
                return controller as T;
            }
            else if (controllerType == typeof(UserSwitchController))
            {
                var controller = new UserSwitchController(_navigationService, _accountService, _configService);
                controller.SetDeviceType(deviceType);
                return controller as T;
            }
            
            throw new NotSupportedException($"Controller type {controllerType.Name} not supported");
        }
        
        // Helper method to get controller for screen
        public BaseController GetControllerForScreen(string screenName, DeviceType deviceType)
        {
            return screenName.ToLower() switch
            {
                "qrlogin" or "passwordlogin" or "googlelogin" => CreateController("login", deviceType),
                "accountinfo" => CreateController("accountinfo", deviceType),
                "logoutconfirm" => CreateController("logout", deviceType),
                "changepassword" => CreateController("password", deviceType),
                "userswitch" => CreateController("userswitch", deviceType),
                _ => throw new ArgumentException($"No controller mapped for screen: {screenName}")
            };
        }
        
        // Helper method to validate controller name
        public static bool IsValidControllerName(string controllerName)
        {
            var validControllers = new[] { "login", "logout", "password", "accountinfo", "userswitch" };
            return Array.Exists(validControllers, name => 
                string.Equals(name, controllerName, StringComparison.OrdinalIgnoreCase));
        }
        
        // Helper method to get all available controller names
        public static string[] GetAvailableControllers()
        {
            return new string[] { "Login", "Logout", "Password", "AccountInfo", "UserSwitch" };
        }
    }
}