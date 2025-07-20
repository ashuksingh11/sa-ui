using System;
using System.Collections.Generic;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Services.Config;

namespace SamsungAccountUI.Services.Device
{
    public class DeviceDetectionService : IDeviceDetectionService
    {
        private readonly IGlobalConfigService _configService;
        private DeviceInfo _cachedDeviceInfo;
        private DeviceCapabilities _cachedCapabilities;
        
        // Mock device models for testing
        private static readonly Dictionary<string, DeviceType> DeviceModelMapping = new Dictionary<string, DeviceType>
        {
            { "Samsung-WW22N6850QX", DeviceType.AIHome }, // Washing Machine
            { "Samsung-DV22N6800HX", DeviceType.AIHome }, // Dryer
            { "Samsung-RF23M8570DT", DeviceType.FamilyHub }, // 32" FamilyHub
            { "Samsung-RF23M8590SR", DeviceType.FamilyHub }, // 21" FamilyHub
            { "Samsung-AIHome-7", DeviceType.AIHome }, // 7" AIHome
            { "Samsung-AIHome-9", DeviceType.AIHome }, // 9" AIHome
        };
        
        public DeviceDetectionService(IGlobalConfigService configService)
        {
            _configService = configService;
        }
        
        public DeviceInfo GetCurrentDeviceInfo()
        {
            if (_cachedDeviceInfo == null)
            {
                _cachedDeviceInfo = DetectDeviceInfo();
            }
            
            return _cachedDeviceInfo;
        }
        
        public DeviceType DetectDeviceType()
        {
            // In real implementation, this would use Tizen device APIs
            // For now, use configuration service
            var deviceTypeString = _configService.GetPreferenceValue("samsung.device.type", "FamilyHub");
            
            return Enum.TryParse<DeviceType>(deviceTypeString, out var deviceType) 
                ? deviceType 
                : DeviceType.FamilyHub;
        }
        
        public DeviceCapabilities GetDeviceCapabilities()
        {
            if (_cachedCapabilities == null)
            {
                _cachedCapabilities = DetectDeviceCapabilities();
            }
            
            return _cachedCapabilities;
        }
        
        public ScreenDimensions GetScreenDimensions()
        {
            // Get from configuration or detect from system
            var width = _configService.GetPreferenceValue("samsung.device.screen.width", 1920);
            var height = _configService.GetPreferenceValue("samsung.device.screen.height", 1080);
            
            return new ScreenDimensions(width, height);
        }
        
        public bool IsDeviceSupported()
        {
            var deviceType = DetectDeviceType();
            return deviceType == DeviceType.AIHome || deviceType == DeviceType.FamilyHub;
        }
        
        public string GetDeviceModel()
        {
            // In real implementation, this would use Tizen.System.Information
            // For mock, return based on device type
            var deviceType = DetectDeviceType();
            var dimensions = GetScreenDimensions();
            
            return deviceType switch
            {
                DeviceType.AIHome when dimensions.Width <= 800 => "Samsung-AIHome-7",
                DeviceType.AIHome when dimensions.Width <= 1200 => "Samsung-AIHome-9", 
                DeviceType.FamilyHub when dimensions.Height >= 1800 => "Samsung-RF23M8570DT", // 32"
                DeviceType.FamilyHub => "Samsung-RF23M8590SR", // 21"
                _ => "Samsung-Unknown"
            };
        }
        
        public string GetOSVersion()
        {
            // In real implementation, this would use Tizen.System.Information
            return "Tizen 6.5";
        }
        
        public bool HasFeature(string feature)
        {
            // In real implementation, this would use Tizen.System.Information.TryGetValue
            var capabilities = GetDeviceCapabilities();
            
            return feature.ToLower() switch
            {
                "touchscreen" => capabilities.HasTouchScreen,
                "camera" => capabilities.HasCamera,
                "wifi" => capabilities.HasWiFi,
                "bluetooth" => capabilities.HasBluetooth,
                "qr" => capabilities.SupportsQRScanning,
                "google" => capabilities.SupportsGoogleAuth,
                "password" => capabilities.SupportsPasswordAuth,
                _ => false
            };
        }
        
        // Device-specific detection methods
        public bool IsAIHomeDevice()
        {
            return DetectDeviceType() == DeviceType.AIHome;
        }
        
        public bool IsFamilyHubDevice()
        {
            return DetectDeviceType() == DeviceType.FamilyHub;
        }
        
        public bool IsWashingMachine()
        {
            var model = GetDeviceModel();
            return model.Contains("WW") || model.Contains("WashingMachine");
        }
        
        public bool IsDryer()
        {
            var model = GetDeviceModel();
            return model.Contains("DV") || model.Contains("Dryer");
        }
        
        public bool IsRefrigerator()
        {
            var model = GetDeviceModel();
            return model.Contains("RF") || model.Contains("FamilyHub");
        }
        
        // Screen and orientation detection
        public bool IsHorizontalOrientation()
        {
            var dimensions = GetScreenDimensions();
            return dimensions.IsHorizontal;
        }
        
        public bool IsVerticalOrientation()
        {
            var dimensions = GetScreenDimensions();
            return dimensions.IsVertical;
        }
        
        public bool IsSmallScreen()
        {
            var dimensions = GetScreenDimensions();
            // Consider screens 7" or 9" as small (typically <= 1200px width)
            return dimensions.Width <= 1200;
        }
        
        public bool IsLargeScreen()
        {
            var dimensions = GetScreenDimensions();
            // Consider screens 21" or 32" as large (typically >= 1400px)
            return dimensions.Width >= 1400 || dimensions.Height >= 1400;
        }
        
        // Capability detection
        public bool SupportsTouchInput()
        {
            return GetDeviceCapabilities().HasTouchScreen;
        }
        
        public bool SupportsMultiUser()
        {
            var capabilities = GetDeviceCapabilities();
            return capabilities.MaxConcurrentUsers > 1;
        }
        
        public bool HasNetworkConnection()
        {
            // In real implementation, this would check actual network status
            return GetDeviceCapabilities().HasWiFi;
        }
        
        public bool HasCamera()
        {
            return GetDeviceCapabilities().HasCamera;
        }
        
        private DeviceInfo DetectDeviceInfo()
        {
            var deviceType = DetectDeviceType();
            var dimensions = GetScreenDimensions();
            var model = GetDeviceModel();
            
            return new DeviceInfo
            {
                Type = deviceType,
                DeviceId = GenerateDeviceId(),
                Dimensions = dimensions,
                SupportsMultiUser = SupportsMultiUser(),
                ModelName = model,
                OSVersion = GetOSVersion(),
                AppVersion = "1.0.0"
            };
        }
        
        private DeviceCapabilities DetectDeviceCapabilities()
        {
            var deviceType = DetectDeviceType();
            
            return deviceType switch
            {
                DeviceType.AIHome => DeviceCapabilities.GetAIHomeCapabilities(),
                DeviceType.FamilyHub => DeviceCapabilities.GetFamilyHubCapabilities(),
                _ => new DeviceCapabilities()
            };
        }
        
        private string GenerateDeviceId()
        {
            // In real implementation, this would get actual device ID from system
            var model = GetDeviceModel();
            var hash = Math.Abs(model.GetHashCode());
            return $"DEVICE_{hash:X8}";
        }
        
        // Helper method to refresh cached information
        public void RefreshDeviceInfo()
        {
            _cachedDeviceInfo = null;
            _cachedCapabilities = null;
        }
        
        // Debug method to simulate different device types (for testing)
        public void SimulateDevice(DeviceType deviceType, int width, int height)
        {
            _configService.SetPreferenceValue("samsung.device.type", deviceType.ToString());
            _configService.SetPreferenceValue("samsung.device.screen.width", width);
            _configService.SetPreferenceValue("samsung.device.screen.height", height);
            
            RefreshDeviceInfo();
        }
    }
}