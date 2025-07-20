using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Utils;
using SamsungAccountUI.Views.Common;

namespace SamsungAccountUI.Views.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly Stack<string> _navigationStack = new Stack<string>();
        private readonly ViewFactory _viewFactory;
        private readonly DeviceType _deviceType;
        private BaseView _currentView;
        private BaseView _loadingView;
        private BaseView _errorView;
        
        public event EventHandler<NavigationEventArgs> NavigationChanged;
        
        public bool CanNavigateBack => _navigationStack.Count > 1;
        public string CurrentScreen => _navigationStack.Count > 0 ? _navigationStack.Peek() : string.Empty;
        
        public NavigationService(ViewFactory viewFactory, DeviceType deviceType)
        {
            _viewFactory = viewFactory;
            _deviceType = deviceType;
        }
        
        public async Task NavigateToAsync(string screenName, object parameters = null)
        {
            try
            {
                var fromScreen = CurrentScreen;
                
                // Create the view for the target screen
                var newView = _viewFactory.CreateView(screenName, _deviceType);
                if (newView == null)
                {
                    throw new InvalidOperationException($"Cannot create view for screen: {screenName}");
                }
                
                // Dispose current view if exists
                _currentView?.Dispose();
                
                // Set the new view as current
                _currentView = newView;
                
                // Add to navigation stack
                _navigationStack.Push(screenName);
                
                // Load content for the new view
                _currentView.LoadContent();
                
                // Initialize view with parameters if provided
                if (parameters != null)
                {
                    await InitializeViewWithParameters(_currentView, parameters);
                }
                
                // Raise navigation event
                NavigationChanged?.Invoke(this, new NavigationEventArgs(fromScreen, screenName, parameters));
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new NavigationException($"Failed to navigate to {screenName}: {ex.Message}", ex);
            }
        }
        
        public async Task NavigateBackAsync()
        {
            if (!CanNavigateBack)
            {
                throw new InvalidOperationException("Cannot navigate back - no previous screen in stack");
            }
            
            try
            {
                var fromScreen = _navigationStack.Pop(); // Remove current screen
                var toScreen = _navigationStack.Peek(); // Get previous screen
                
                // Dispose current view
                _currentView?.Dispose();
                
                // Create view for previous screen
                _currentView = _viewFactory.CreateView(toScreen, _deviceType);
                _currentView.LoadContent();
                
                // Raise navigation event
                NavigationChanged?.Invoke(this, new NavigationEventArgs(fromScreen, toScreen));
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new NavigationException($"Failed to navigate back: {ex.Message}", ex);
            }
        }
        
        public async Task ReplaceCurrentAsync(string screenName, object parameters = null)
        {
            try
            {
                var fromScreen = CurrentScreen;
                
                // Remove current screen from stack if exists
                if (_navigationStack.Count > 0)
                {
                    _navigationStack.Pop();
                }
                
                // Navigate to new screen (which will add it to stack)
                await NavigateToAsync(screenName, parameters);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new NavigationException($"Failed to replace current screen with {screenName}: {ex.Message}", ex);
            }
        }
        
        public async Task ShowLoadingAsync(string message = "Loading...")
        {
            try
            {
                if (_loadingView == null)
                {
                    _loadingView = _viewFactory.CreateLoadingView(_deviceType);
                }
                
                await _loadingView.ShowLoadingAsync(message);
            }
            catch (Exception ex)
            {
                // Loading should not fail navigation, log error instead
                System.Diagnostics.Debug.WriteLine($"Failed to show loading: {ex.Message}");
            }
        }
        
        public async Task HideLoadingAsync()
        {
            try
            {
                if (_loadingView != null)
                {
                    await _loadingView.HideLoadingAsync();
                }
            }
            catch (Exception ex)
            {
                // Loading should not fail navigation, log error instead
                System.Diagnostics.Debug.WriteLine($"Failed to hide loading: {ex.Message}");
            }
        }
        
        public async Task ShowErrorAsync(string message, string title = "Error")
        {
            try
            {
                if (_errorView == null)
                {
                    _errorView = _viewFactory.CreateErrorView(_deviceType);
                }
                
                await _errorView.ShowErrorAsync(message, title);
            }
            catch (Exception ex)
            {
                // Error display should not fail navigation, log error instead
                System.Diagnostics.Debug.WriteLine($"Failed to show error: {ex.Message}");
            }
        }
        
        private async Task InitializeViewWithParameters(BaseView view, object parameters)
        {
            // This would be implemented based on the specific view initialization needs
            // For now, we'll just pass the parameters to the view if it supports them
            try
            {
                if (view is IParameterizedView paramView)
                {
                    await paramView.InitializeAsync(parameters);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize view with parameters: {ex.Message}");
            }
        }
        
        public void Dispose()
        {
            _currentView?.Dispose();
            _loadingView?.Dispose();
            _errorView?.Dispose();
            _navigationStack.Clear();
        }
    }
    
    // Interface for views that can accept parameters
    public interface IParameterizedView
    {
        Task InitializeAsync(object parameters);
    }
    
    // Custom exception for navigation errors
    public class NavigationException : Exception
    {
        public NavigationException(string message) : base(message) { }
        public NavigationException(string message, Exception innerException) : base(message, innerException) { }
    }
}