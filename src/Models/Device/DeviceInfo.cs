using System;

namespace SamsungAccountUI.Models.Device
{
    public enum DeviceType
    {
        AIHome,
        FamilyHub,
        Unknown
    }
    
    public class ScreenDimensions
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsHorizontal => Width > Height;
        public bool IsVertical => Height > Width;
        
        public ScreenDimensions(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
    
    public class DeviceInfo
    {
        public DeviceType Type { get; set; }
        public string DeviceId { get; set; }
        public ScreenDimensions Dimensions { get; set; }
        public bool SupportsMultiUser { get; set; }
        public string ModelName { get; set; }
        public string OSVersion { get; set; }
        public string AppVersion { get; set; }
        
        public DeviceInfo()
        {
            Type = DeviceType.Unknown;
            DeviceId = string.Empty;
            Dimensions = new ScreenDimensions(800, 600);
            SupportsMultiUser = true;
            ModelName = string.Empty;
            OSVersion = string.Empty;
            AppVersion = "1.0.0";
        }
        
        public bool IsAIHomeDevice => Type == DeviceType.AIHome;
        public bool IsFamilyHubDevice => Type == DeviceType.FamilyHub;
        public bool IsHorizontalLayout => Dimensions.IsHorizontal;
        public bool IsVerticalLayout => Dimensions.IsVertical;
    }
}