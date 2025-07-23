using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using SamsungAccountUI.Views.Base;
using SamsungAccountUI.Controllers;
using SamsungAccountUI.Services.Core;
using SamsungAccountUI.Utils;

namespace SamsungAccountUI.Services.Navigation
{
    /// <summary>
    /// Tizen-compatible navigation service using stack-based navigation
    /// Mimics Tizen Navigator class behavior with push/pop methods
    /// Integrates with Tizen NUI window management
    /// </summary>
    public class TizenNavigationService : ITizenNavigationService
    {
        #region Private Fields

        private readonly Stack<BaseView> _navigationStack = new Stack<BaseView>();
        private readonly Window _window;
        private readonly AuthController _authController;
        private readonly AccountController _accountController;
        private readonly AppController _appController;
        private readonly IConfigService _configService;

        #endregion

        #region Properties

        public BaseView CurrentView => _navigationStack.Count > 0 ? _navigationStack.Peek() : null;

        public bool CanGoBack => _navigationStack.Count > 1;

        public int StackDepth => _navigationStack.Count;

        #endregion

        #region Constructor

        public TizenNavigationService(
            Window window,
            AuthController authController,
            AccountController accountController,
            AppController appController,
            IConfigService configService)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _authController = authController ?? throw new ArgumentNullException(nameof(authController));
            _accountController = accountController ?? throw new ArgumentNullException(nameof(accountController));
            _appController = appController ?? throw new ArgumentNullException(nameof(appController));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        #endregion

        #region Push Navigation

        public async Task PushAsync(BaseView view, bool animated = true)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            try
            {
                // Raise navigating event
                var navigatingArgs = new NavigatingEventArgs
                {
                    FromView = CurrentView,
                    ToView = view,
                    NavigationType = NavigationType.Push,
                    Animated = animated
                };
                OnNavigating(navigatingArgs);

                if (navigatingArgs.Cancel)
                    return;

                // Hide current view (but keep in stack)
                if (CurrentView != null)
                {
                    await CurrentView.OnDisappearingAsync();
                    if (animated)
                    {
                        await AnimateViewOutAsync(CurrentView);
                    }
                    CurrentView.Hide();
                }

                // Add new view to stack and window
                _navigationStack.Push(view);
                _window.Add(view);

                // Show new view
                if (animated)
                {
                    await AnimateViewInAsync(view);
                }
                else
                {
                    view.Show();
                }

                await view.OnAppearingAsync();

                // Raise navigated event
                OnNavigated(new NavigationEventArgs
                {
                    FromView = navigatingArgs.FromView,
                    ToView = view,
                    NavigationType = NavigationType.Push,
                    Animated = animated
                });
            }
            catch (Exception ex)
            {
                _appController.HandleAppError(ex, "PushAsync");
                throw;
            }
        }

        public async Task PushAsync(string screenName, object parameters = null, bool animated = true)
        {
            var view = CreateViewForScreen(screenName, parameters);
            await PushAsync(view, animated);
        }

        #endregion

        #region Pop Navigation

        public async Task<BaseView> PopAsync(bool animated = true)
        {
            if (!CanGoBack)
                return null;

            try
            {
                var currentView = CurrentView;
                var previousView = _navigationStack.ElementAt(1); // Second item in stack

                // Raise navigating event
                var navigatingArgs = new NavigatingEventArgs
                {
                    FromView = currentView,
                    ToView = previousView,
                    NavigationType = NavigationType.Pop,
                    Animated = animated
                };
                OnNavigating(navigatingArgs);

                if (navigatingArgs.Cancel)
                    return null;

                // Hide and remove current view
                await currentView.OnDisappearingAsync();
                if (animated)
                {
                    await AnimateViewOutAsync(currentView);
                }

                _window.Remove(currentView);
                _navigationStack.Pop();

                // Show previous view
                if (animated)
                {
                    await AnimateViewInAsync(previousView);
                }
                else
                {
                    previousView.Show();
                }

                await previousView.OnAppearingAsync();

                // Dispose the popped view
                currentView.Dispose();

                // Raise navigated event
                OnNavigated(new NavigationEventArgs
                {
                    FromView = currentView,
                    ToView = previousView,
                    NavigationType = NavigationType.Pop,
                    Animated = animated
                });

                return currentView;
            }
            catch (Exception ex)
            {
                _appController.HandleAppError(ex, "PopAsync");
                throw;
            }
        }

        public async Task PopToAsync<T>(bool animated = true) where T : BaseView
        {
            try
            {
                var targetView = _navigationStack.FirstOrDefault(v => v is T);
                if (targetView == null)
                    return;

                // Pop views until we reach the target view
                while (CurrentView != targetView && CanGoBack)
                {
                    await PopAsync(animated);
                }
            }
            catch (Exception ex)
            {
                _appController.HandleAppError(ex, "PopToAsync");
                throw;
            }
        }

        public async Task PopToRootAsync(bool animated = true)
        {
            try
            {
                // Pop all views except the root (first view)
                while (_navigationStack.Count > 1)
                {
                    await PopAsync(animated);
                }
            }
            catch (Exception ex)
            {
                _appController.HandleAppError(ex, "PopToRootAsync");
                throw;
            }
        }

        #endregion

        #region Replace Navigation

        public async Task ReplaceAsync(BaseView view, bool animated = true)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            try
            {
                var currentView = CurrentView;

                // Raise navigating event
                var navigatingArgs = new NavigatingEventArgs
                {
                    FromView = currentView,
                    ToView = view,
                    NavigationType = NavigationType.Replace,
                    Animated = animated
                };
                OnNavigating(navigatingArgs);

                if (navigatingArgs.Cancel)
                    return;

                // Remove current view
                if (currentView != null)
                {
                    await currentView.OnDisappearingAsync();
                    if (animated)
                    {
                        await AnimateViewOutAsync(currentView);
                    }

                    _window.Remove(currentView);
                    _navigationStack.Pop();
                    currentView.Dispose();
                }

                // Add new view
                _navigationStack.Push(view);
                _window.Add(view);

                // Show new view
                if (animated)
                {
                    await AnimateViewInAsync(view);
                }
                else
                {
                    view.Show();
                }

                await view.OnAppearingAsync();

                // Raise navigated event
                OnNavigated(new NavigationEventArgs
                {
                    FromView = currentView,
                    ToView = view,
                    NavigationType = NavigationType.Replace,
                    Animated = animated
                });
            }
            catch (Exception ex)
            {
                _appController.HandleAppError(ex, "ReplaceAsync");
                throw;
            }
        }

        public async Task ReplaceAsync(string screenName, object parameters = null, bool animated = true)
        {
            var view = CreateViewForScreen(screenName, parameters);
            await ReplaceAsync(view, animated);
        }

        #endregion

        #region Root Navigation

        public async Task SetRootAsync(BaseView view, bool animated = true)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            try
            {
                var currentView = CurrentView;

                // Raise navigating event
                var navigatingArgs = new NavigatingEventArgs
                {
                    FromView = currentView,
                    ToView = view,
                    NavigationType = NavigationType.SetRoot,
                    Animated = animated
                };
                OnNavigating(navigatingArgs);

                if (navigatingArgs.Cancel)
                    return;

                // Clear entire navigation stack
                await ClearNavigationStackAsync(animated);

                // Set new root view
                _navigationStack.Push(view);
                _window.Add(view);

                // Show new view
                if (animated)
                {
                    await AnimateViewInAsync(view);
                }
                else
                {
                    view.Show();
                }

                await view.OnAppearingAsync();

                // Raise navigated event
                OnNavigated(new NavigationEventArgs
                {
                    FromView = currentView,
                    ToView = view,
                    NavigationType = NavigationType.SetRoot,
                    Animated = animated
                });
            }
            catch (Exception ex)
            {
                _appController.HandleAppError(ex, "SetRootAsync");
                throw;
            }
        }

        public async Task SetRootAsync(string screenName, object parameters = null, bool animated = true)
        {
            var view = CreateViewForScreen(screenName, parameters);
            await SetRootAsync(view, animated);
        }

        #endregion

        #region Stack Management

        public async Task InsertBeforeAsync(BaseView viewToInsert, BaseView existingView)
        {
            if (viewToInsert == null || existingView == null)
                return;

            try
            {
                var stackArray = _navigationStack.ToArray();
                var existingIndex = Array.IndexOf(stackArray, existingView);

                if (existingIndex >= 0)
                {
                    // Reconstruct stack with inserted view
                    _navigationStack.Clear();
                    
                    for (int i = stackArray.Length - 1; i >= 0; i--)
                    {
                        if (i == existingIndex)
                        {
                            _navigationStack.Push(viewToInsert);
                        }
                        _navigationStack.Push(stackArray[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                _appController.HandleAppError(ex, "InsertBeforeAsync");
            }
        }

        public async Task RemoveAsync(BaseView view)
        {
            if (view == null || !_navigationStack.Contains(view))
                return;

            try
            {
                if (view == CurrentView)
                {
                    // If removing current view, pop it
                    await PopAsync();
                }
                else
                {
                    // Remove from stack without affecting current view
                    var stackArray = _navigationStack.ToArray();
                    _navigationStack.Clear();

                    foreach (var stackView in stackArray.Reverse())
                    {
                        if (stackView != view)
                        {
                            _navigationStack.Push(stackView);
                        }
                        else
                        {
                            // Remove from window and dispose
                            _window.Remove(stackView);
                            stackView.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _appController.HandleAppError(ex, "RemoveAsync");
            }
        }

        public BaseView[] GetNavigationStack()
        {
            return _navigationStack.ToArray();
        }

        #endregion

        #region View Creation

        private BaseView CreateViewForScreen(string screenName, object parameters = null)
        {
            var deviceInfo = DeviceHelper.GetCurrentDeviceInfo();

            // Create device-specific views
            return deviceInfo.Type switch
            {
                DeviceType.FamilyHub => CreateFamilyHubView(screenName, parameters),
                DeviceType.AIHome => CreateAIHomeView(screenName, parameters),
                _ => CreateFamilyHubView(screenName, parameters) // Default to FamilyHub
            };
        }

        private BaseView CreateFamilyHubView(string screenName, object parameters)
        {
            return screenName.ToLower() switch
            {
                "qrlogin" => new Views.FamilyHub.QRLoginView(_authController, _accountController, _appController, _configService),
                "passwordlogin" => new Views.FamilyHub.PasswordLoginView(_authController, _accountController, _appController, _configService),
                "googlelogin" => new Views.FamilyHub.GoogleLoginView(_authController, _accountController, _appController, _configService),
                "accountinfo" => new Views.FamilyHub.AccountInfoView(_authController, _accountController, _appController, _configService),
                "logoutconfirm" => new Views.FamilyHub.LogoutConfirmView(_authController, _accountController, _appController, _configService),
                "changepassword" => new Views.FamilyHub.ChangePasswordView(_authController, _accountController, _appController, _configService),
                _ => throw new NotSupportedException($"Screen '{screenName}' is not supported for FamilyHub")
            };
        }

        private BaseView CreateAIHomeView(string screenName, object parameters)
        {
            return screenName.ToLower() switch
            {
                "qrlogin" => new Views.AIHome.QRLoginView(_authController, _accountController, _appController, _configService),
                "passwordlogin" => new Views.AIHome.PasswordLoginView(_authController, _accountController, _appController, _configService),
                "googlelogin" => new Views.AIHome.GoogleLoginView(_authController, _accountController, _appController, _configService),
                "accountinfo" => new Views.AIHome.AccountInfoView(_authController, _accountController, _appController, _configService),
                "logoutconfirm" => new Views.AIHome.LogoutConfirmView(_authController, _accountController, _appController, _configService),
                "changepassword" => new Views.AIHome.ChangePasswordView(_authController, _accountController, _appController, _configService),
                _ => throw new NotSupportedException($"Screen '{screenName}' is not supported for AIHome")
            };
        }

        #endregion

        #region Animation Helpers

        private async Task AnimateViewInAsync(BaseView view)
        {
            try
            {
                if (!_configService.AnimationsEnabled)
                {
                    view.Show();
                    return;
                }

                // TODO: Replace with actual Tizen animation APIs
                // Example using Tizen.NUI.Animation:
                // var animation = new Animation(300);
                // animation.AnimateTo(view, "Position", new Position(0, 0));
                // animation.AnimateTo(view, "Opacity", 1.0f);
                // animation.Play();

                // Dummy implementation
                view.Opacity = 0.0f;
                view.Show();
                await Task.Delay(50);
                view.Opacity = 1.0f;
            }
            catch (Exception)
            {
                // Fallback to simple show
                view.Show();
            }
        }

        private async Task AnimateViewOutAsync(BaseView view)
        {
            try
            {
                if (!_configService.AnimationsEnabled)
                {
                    return;
                }

                // TODO: Replace with actual Tizen animation APIs
                // var animation = new Animation(300);
                // animation.AnimateTo(view, "Opacity", 0.0f);
                // animation.Play();

                // Dummy implementation
                view.Opacity = 0.0f;
                await Task.Delay(50);
            }
            catch (Exception)
            {
                // Ignore animation errors
            }
        }

        #endregion

        #region Helper Methods

        private async Task ClearNavigationStackAsync(bool animated)
        {
            while (_navigationStack.Count > 0)
            {
                var view = _navigationStack.Pop();
                
                await view.OnDisappearingAsync();
                
                if (animated)
                {
                    await AnimateViewOutAsync(view);
                }

                _window.Remove(view);
                view.Dispose();
            }
        }

        #endregion

        #region Events

        public event EventHandler<NavigationEventArgs> Navigated;
        public event EventHandler<NavigatingEventArgs> Navigating;

        protected virtual void OnNavigated(NavigationEventArgs args)
        {
            Navigated?.Invoke(this, args);
        }

        protected virtual void OnNavigating(NavigatingEventArgs args)
        {
            Navigating?.Invoke(this, args);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            // Clear navigation stack and dispose all views
            while (_navigationStack.Count > 0)
            {
                var view = _navigationStack.Pop();
                _window.Remove(view);
                view.Dispose();
            }
        }

        #endregion
    }
}