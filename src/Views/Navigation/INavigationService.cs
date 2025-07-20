using System.Threading.Tasks;

namespace SamsungAccountUI.Views.Navigation
{
    public interface INavigationService
    {
        Task NavigateToAsync(string screenName, object parameters = null);
        Task NavigateBackAsync();
        Task ReplaceCurrentAsync(string screenName, object parameters = null);
        Task ShowLoadingAsync(string message = "Loading...");
        Task HideLoadingAsync();
        Task ShowErrorAsync(string message, string title = "Error");
        
        bool CanNavigateBack { get; }
        string CurrentScreen { get; }
        
        event System.EventHandler<NavigationEventArgs> NavigationChanged;
    }
    
    public class NavigationEventArgs : System.EventArgs
    {
        public string FromScreen { get; set; }
        public string ToScreen { get; set; }
        public object Parameters { get; set; }
        
        public NavigationEventArgs(string fromScreen, string toScreen, object parameters = null)
        {
            FromScreen = fromScreen;
            ToScreen = toScreen;
            Parameters = parameters;
        }
    }
}