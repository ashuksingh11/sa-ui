using System;
using System.Threading.Tasks;
using SamsungAccountUI.Views.Base;

namespace SamsungAccountUI.Services.Navigation
{
    /// <summary>
    /// Navigation service interface using Tizen Navigator patterns
    /// Provides stack-based navigation with push/pop methods
    /// </summary>
    public interface ITizenNavigationService
    {
        /// <summary>
        /// Current view on top of the navigation stack
        /// </summary>
        BaseView CurrentView { get; }

        /// <summary>
        /// Check if navigation can go back (stack has more than one view)
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Get current navigation stack depth
        /// </summary>
        int StackDepth { get; }

        /// <summary>
        /// Push a new view onto the navigation stack
        /// Similar to Tizen Navigator.Push()
        /// </summary>
        Task PushAsync(BaseView view, bool animated = true);

        /// <summary>
        /// Push a new view by screen name onto the navigation stack
        /// </summary>
        Task PushAsync(string screenName, object parameters = null, bool animated = true);

        /// <summary>
        /// Pop the current view from the navigation stack
        /// Similar to Tizen Navigator.Pop()
        /// </summary>
        Task<BaseView> PopAsync(bool animated = true);

        /// <summary>
        /// Pop to a specific view type in the stack
        /// Similar to Tizen Navigator.PopTo()
        /// </summary>
        Task PopToAsync<T>(bool animated = true) where T : BaseView;

        /// <summary>
        /// Pop to root view (clear all except first view)
        /// Similar to Tizen Navigator.PopToRoot()
        /// </summary>
        Task PopToRootAsync(bool animated = true);

        /// <summary>
        /// Replace current view with a new view (pop current, push new)
        /// Similar to Tizen Navigator.Replace()
        /// </summary>
        Task ReplaceAsync(BaseView view, bool animated = true);

        /// <summary>
        /// Replace current view by screen name
        /// </summary>
        Task ReplaceAsync(string screenName, object parameters = null, bool animated = true);

        /// <summary>
        /// Clear entire navigation stack and set new root view
        /// Similar to Tizen Navigator.SetRootPage()
        /// </summary>
        Task SetRootAsync(BaseView view, bool animated = true);

        /// <summary>
        /// Clear entire navigation stack and set new root view by screen name
        /// </summary>
        Task SetRootAsync(string screenName, object parameters = null, bool animated = true);

        /// <summary>
        /// Insert a view at specific position in the stack
        /// Similar to Tizen Navigator.InsertPageBefore()
        /// </summary>
        Task InsertBeforeAsync(BaseView viewToInsert, BaseView existingView);

        /// <summary>
        /// Remove a specific view from the navigation stack
        /// Similar to Tizen Navigator.RemovePage()
        /// </summary>
        Task RemoveAsync(BaseView view);

        /// <summary>
        /// Get all views in the navigation stack
        /// </summary>
        BaseView[] GetNavigationStack();

        /// <summary>
        /// Event raised when navigation occurs
        /// </summary>
        event EventHandler<NavigationEventArgs> Navigated;

        /// <summary>
        /// Event raised when navigation is about to occur
        /// </summary>
        event EventHandler<NavigatingEventArgs> Navigating;
    }

    /// <summary>
    /// Navigation event arguments
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        public BaseView FromView { get; set; }
        public BaseView ToView { get; set; }
        public NavigationType NavigationType { get; set; }
        public bool Animated { get; set; }
    }

    /// <summary>
    /// Navigating event arguments (cancellable)
    /// </summary>
    public class NavigatingEventArgs : NavigationEventArgs
    {
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Types of navigation operations
    /// </summary>
    public enum NavigationType
    {
        Push,
        Pop,
        Replace,
        SetRoot,
        PopToRoot,
        PopTo
    }
}