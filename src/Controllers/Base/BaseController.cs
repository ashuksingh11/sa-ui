using System.Threading.Tasks;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Services.API;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Views.Navigation;

namespace SamsungAccountUI.Controllers.Base
{
    public abstract class BaseController : IController
    {
        protected INavigationService NavigationService { get; set; }
        protected ISamsungAccountService AccountService { get; set; }
        protected IGlobalConfigService ConfigService { get; set; }
        protected DeviceType DeviceType { get; set; }
        
        protected BaseController(
            INavigationService navigationService,
            ISamsungAccountService accountService,
            IGlobalConfigService configService)
        {
            NavigationService = navigationService;
            AccountService = accountService;
            ConfigService = configService;
        }
        
        public abstract Task LoadAsync();
        public abstract Task HandleInputAsync(object input);
        
        public virtual Task OnDeviceSpecificAction(string action, object data)
        {
            return Task.CompletedTask;
        }
        
        protected virtual async Task ShowLoading(string message = "Loading...")
        {
            await NavigationService.ShowLoadingAsync(message);
        }
        
        protected virtual async Task HideLoading()
        {
            await NavigationService.HideLoadingAsync();
        }
        
        protected virtual async Task ShowError(string message, string title = "Error")
        {
            await NavigationService.ShowErrorAsync(message, title);
        }
        
        protected virtual async Task NavigateToScreen(string screenName, object parameters = null)
        {
            await NavigationService.NavigateToAsync(screenName, parameters);
        }
        
        protected virtual async Task NavigateBack()
        {
            await NavigationService.NavigateBackAsync();
        }
        
        protected virtual void SetDeviceType(DeviceType deviceType)
        {
            DeviceType = deviceType;
        }
    }
}