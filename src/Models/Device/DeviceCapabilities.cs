using System.Collections.Generic;

namespace SamsungAccountUI.Models.Device
{
    public class DeviceCapabilities
    {
        public bool HasTouchScreen { get; set; }
        public bool HasCamera { get; set; }
        public bool HasWiFi { get; set; }
        public bool HasBluetooth { get; set; }
        public bool SupportsQRScanning { get; set; }
        public bool SupportsGoogleAuth { get; set; }
        public bool SupportsPasswordAuth { get; set; }
        public int MaxConcurrentUsers { get; set; }
        public List<string> SupportedAuthMethods { get; set; }
        
        public DeviceCapabilities()
        {
            HasTouchScreen = true;
            HasCamera = false;
            HasWiFi = true;
            HasBluetooth = false;
            SupportsQRScanning = true;
            SupportsGoogleAuth = true;
            SupportsPasswordAuth = true;
            MaxConcurrentUsers = 6;
            SupportedAuthMethods = new List<string> { "QR", "Password", "Google" };
        }
        
        public bool SupportsAuthMethod(string method)
        {
            return SupportedAuthMethods.Contains(method);
        }
        
        public static DeviceCapabilities GetAIHomeCapabilities()
        {
            return new DeviceCapabilities
            {
                HasTouchScreen = true,
                HasCamera = false,
                HasWiFi = true,
                HasBluetooth = false,
                SupportsQRScanning = true,
                SupportsGoogleAuth = true,
                SupportsPasswordAuth = true,
                MaxConcurrentUsers = 4,
                SupportedAuthMethods = new List<string> { "QR", "Password", "Google" }
            };
        }
        
        public static DeviceCapabilities GetFamilyHubCapabilities()
        {
            return new DeviceCapabilities
            {
                HasTouchScreen = true,
                HasCamera = true,
                HasWiFi = true,
                HasBluetooth = true,
                SupportsQRScanning = true,
                SupportsGoogleAuth = true,
                SupportsPasswordAuth = true,
                MaxConcurrentUsers = 6,
                SupportedAuthMethods = new List<string> { "QR", "Password", "Google" }
            };
        }
    }
}