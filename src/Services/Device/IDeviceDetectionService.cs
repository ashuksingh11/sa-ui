using SamsungAccountUI.Models.Device;

namespace SamsungAccountUI.Services.Device
{
    public interface IDeviceDetectionService
    {
        DeviceInfo GetCurrentDeviceInfo();
        DeviceType DetectDeviceType();
        DeviceCapabilities GetDeviceCapabilities();
        ScreenDimensions GetScreenDimensions();
        bool IsDeviceSupported();
        string GetDeviceModel();
        string GetOSVersion();
        bool HasFeature(string feature);
        
        // Device-specific detection methods
        bool IsAIHomeDevice();
        bool IsFamilyHubDevice();
        bool IsWashingMachine();
        bool IsDryer();
        bool IsRefrigerator();
        
        // Screen and orientation detection
        bool IsHorizontalOrientation();
        bool IsVerticalOrientation();
        bool IsSmallScreen(); // 7"/9"
        bool IsLargeScreen(); // 21"/32"
        
        // Capability detection
        bool SupportsTouchInput();
        bool SupportsMultiUser();
        bool HasNetworkConnection();
        bool HasCamera();
    }
}