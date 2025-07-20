using System.Threading.Tasks;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Views.Navigation;

namespace SamsungAccountUI.Views.Common
{
    public abstract class BaseView
    {
        protected DeviceType DeviceType { get; set; }
        protected INavigationService NavigationService { get; set; }
        protected IGlobalConfigService ConfigService { get; set; }
        
        protected BaseView(INavigationService navigationService, IGlobalConfigService configService)
        {
            NavigationService = navigationService;
            ConfigService = configService;
        }
        
        public abstract void LoadContent();
        public abstract void UpdateForDevice(DeviceType deviceType);
        
        protected virtual void ApplyConfigSettings()
        {
            // Load Tizen preference key values and apply to view elements
            // This will be overridden by specific views to apply their config
        }
        
        public virtual Task ShowLoadingAsync(string message = "Loading...")
        {
            // Show loading indicator
            return Task.CompletedTask;
        }
        
        public virtual Task HideLoadingAsync()
        {
            // Hide loading indicator
            return Task.CompletedTask;
        }
        
        public virtual Task ShowErrorAsync(string message, string title = "Error")
        {
            // Show error dialog or overlay
            return Task.CompletedTask;
        }
        
        public virtual void Dispose()
        {
            // Clean up resources
        }
        
        protected virtual void SetDeviceType(DeviceType deviceType)
        {
            DeviceType = deviceType;
            UpdateForDevice(deviceType);
        }
        
        protected virtual void ApplyTheme(string theme)
        {
            // Apply theme based on preference
            // Will be implemented based on actual UI framework
        }
        
        protected virtual void ConfigureForMaxUsers(int maxUsers)
        {
            // Configure UI elements based on max users setting
        }
        
        protected virtual void ApplyLargeTextSettings()
        {
            // Apply large text settings for accessibility
        }
    }
}