using System;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Views.AIHome;
using SamsungAccountUI.Views.Common;
using SamsungAccountUI.Views.FamilyHub;
using SamsungAccountUI.Views.Navigation;

namespace SamsungAccountUI.Utils
{
    public class ViewFactory
    {
        private readonly INavigationService _navigationService;
        private readonly IGlobalConfigService _configService;
        
        public ViewFactory(INavigationService navigationService, IGlobalConfigService configService)
        {
            _navigationService = navigationService;
            _configService = configService;
        }
        
        public BaseView CreateView(string screenName, DeviceType deviceType)
        {
            return deviceType switch
            {
                DeviceType.AIHome => CreateAIHomeView(screenName),
                DeviceType.FamilyHub => CreateFamilyHubView(screenName),
                _ => throw new NotSupportedException($"Device type {deviceType} not supported")
            };
        }
        
        public static T CreateView<T>(DeviceType deviceType, INavigationService navigationService, IGlobalConfigService configService) where T : BaseView
        {
            return deviceType switch
            {
                DeviceType.AIHome => CreateAIHomeView<T>(navigationService, configService),
                DeviceType.FamilyHub => CreateFamilyHubView<T>(navigationService, configService),
                _ => throw new NotSupportedException($"Device type {deviceType} not supported")
            };
        }
        
        public BaseView CreateLoadingView(DeviceType deviceType)
        {
            return deviceType switch
            {
                DeviceType.AIHome => new LoadingView(_navigationService, _configService),
                DeviceType.FamilyHub => new LoadingView(_navigationService, _configService),
                _ => new LoadingView(_navigationService, _configService)
            };
        }
        
        public BaseView CreateErrorView(DeviceType deviceType)
        {
            return deviceType switch
            {
                DeviceType.AIHome => new ErrorView(_navigationService, _configService),
                DeviceType.FamilyHub => new ErrorView(_navigationService, _configService),
                _ => new ErrorView(_navigationService, _configService)
            };
        }
        
        private BaseView CreateAIHomeView(string screenName)
        {
            return screenName.ToLower() switch
            {
                "qrlogin" => new SamsungAccountUI.Views.AIHome.QRLoginView(_navigationService, _configService),
                "passwordlogin" => new SamsungAccountUI.Views.AIHome.PasswordLoginView(_navigationService, _configService),
                "googlelogin" => new SamsungAccountUI.Views.AIHome.GoogleLoginView(_navigationService, _configService),
                "accountinfo" => new SamsungAccountUI.Views.AIHome.AccountInfoView(_navigationService, _configService),
                "logoutconfirm" => new SamsungAccountUI.Views.AIHome.LogoutConfirmView(_navigationService, _configService),
                "changepassword" => new SamsungAccountUI.Views.AIHome.ChangePasswordView(_navigationService, _configService),
                "loading" => new LoadingView(_navigationService, _configService),
                "error" => new ErrorView(_navigationService, _configService),
                _ => throw new ArgumentException($"Unknown screen name for AIHome: {screenName}")
            };
        }
        
        private BaseView CreateFamilyHubView(string screenName)
        {
            return screenName.ToLower() switch
            {
                "qrlogin" => new SamsungAccountUI.Views.FamilyHub.QRLoginView(_navigationService, _configService),
                "passwordlogin" => new SamsungAccountUI.Views.FamilyHub.PasswordLoginView(_navigationService, _configService),
                "googlelogin" => new SamsungAccountUI.Views.FamilyHub.GoogleLoginView(_navigationService, _configService),
                "accountinfo" => new SamsungAccountUI.Views.FamilyHub.AccountInfoView(_navigationService, _configService),
                "logoutconfirm" => new SamsungAccountUI.Views.FamilyHub.LogoutConfirmView(_navigationService, _configService),
                "changepassword" => new SamsungAccountUI.Views.FamilyHub.ChangePasswordView(_navigationService, _configService),
                "loading" => new LoadingView(_navigationService, _configService),
                "error" => new ErrorView(_navigationService, _configService),
                _ => throw new ArgumentException($"Unknown screen name for FamilyHub: {screenName}")
            };
        }
        
        private static T CreateAIHomeView<T>(INavigationService navigationService, IGlobalConfigService configService) where T : BaseView
        {
            var typeName = typeof(T).Name;
            var aiHomeTypeName = $"SamsungAccountUI.Views.AIHome.{typeName}";
            var type = Type.GetType(aiHomeTypeName);
            
            if (type != null)
            {
                return (T)Activator.CreateInstance(type, navigationService, configService);
            }
            
            throw new NotSupportedException($"AIHome view type {typeName} not supported");
        }
        
        private static T CreateFamilyHubView<T>(INavigationService navigationService, IGlobalConfigService configService) where T : BaseView
        {
            var typeName = typeof(T).Name;
            var familyHubTypeName = $"SamsungAccountUI.Views.FamilyHub.{typeName}";
            var type = Type.GetType(familyHubTypeName);
            
            if (type != null)
            {
                return (T)Activator.CreateInstance(type, navigationService, configService);
            }
            
            throw new NotSupportedException($"FamilyHub view type {typeName} not supported");
        }
        
        // Helper method to get all available screen names for a device type
        public static string[] GetAvailableScreens(DeviceType deviceType)
        {
            return new string[]
            {
                "QRLogin",
                "PasswordLogin", 
                "GoogleLogin",
                "AccountInfo",
                "LogoutConfirm",
                "ChangePassword",
                "Loading",
                "Error"
            };
        }
        
        // Helper method to validate screen name
        public static bool IsValidScreenName(string screenName)
        {
            var availableScreens = GetAvailableScreens(DeviceType.FamilyHub); // Same for both device types
            return Array.Exists(availableScreens, screen => 
                string.Equals(screen, screenName, StringComparison.OrdinalIgnoreCase));
        }
    }
}