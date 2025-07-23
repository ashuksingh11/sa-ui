using System;
using System.Threading.Tasks;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using SamsungAccountUI.Controllers;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Services.Core;
using SamsungAccountUI.Services.Navigation;
using SamsungAccountUI.Utils;

namespace SamsungAccountUI.Views.Base
{
    /// <summary>
    /// Base view class for all Samsung Account UI views
    /// Integrates with Tizen NUI and provides common functionality
    /// Uses Tizen built-in navigation instead of custom NavigationService
    /// </summary>
    public abstract class BaseView : View
    {
        #region Protected Properties

        protected AuthController AuthController { get; private set; }
        protected AccountController AccountController { get; private set; }
        protected AppController AppController { get; private set; }
        protected DeviceInfo DeviceInfo { get; private set; }
        protected IConfigService ConfigService { get; private set; }
        protected ITizenNavigationService NavigationService { get; private set; }

        #endregion

        #region UI Components

        protected View ContentContainer { get; private set; }
        protected TextLabel TitleLabel { get; private set; }
        protected View ButtonContainer { get; private set; }
        protected View LoadingOverlay { get; private set; }
        protected View ErrorOverlay { get; private set; }

        #endregion

        #region Constructor

        public BaseView(
            AuthController authController, 
            AccountController accountController,
            AppController appController,
            IConfigService configService,
            ITizenNavigationService navigationService = null)
        {
            AuthController = authController ?? throw new ArgumentNullException(nameof(authController));
            AccountController = accountController ?? throw new ArgumentNullException(nameof(accountController));
            AppController = appController ?? throw new ArgumentNullException(nameof(appController));
            ConfigService = configService ?? throw new ArgumentNullException(nameof(configService));
            NavigationService = navigationService; // Can be null during initialization
            DeviceInfo = DeviceHelper.GetCurrentDeviceInfo();

            InitializeBaseLayout();
            ApplyDeviceSpecificSettings();
            WireControllerEvents();
        }

        /// <summary>
        /// Set navigation service after view construction (called by navigation service itself)
        /// </summary>
        public void SetNavigationService(ITizenNavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        #endregion

        #region Initialization

        private void InitializeBaseLayout()
        {
            // Set up main view properties for Tizen NUI
            WidthSpecification = LayoutParamPolicies.MatchParent;
            HeightSpecification = LayoutParamPolicies.MatchParent;
            BackgroundColor = GetThemeBackgroundColor();

            // Create main layout based on device type
            Layout = CreateMainLayout();

            // Create base UI components
            CreateTitleArea();
            CreateContentContainer();
            CreateButtonContainer();
            CreateOverlays();

            // Add components to view
            Add(TitleLabel);
            Add(ContentContainer);
            Add(ButtonContainer);
            Add(LoadingOverlay);
            Add(ErrorOverlay);
        }

        private LinearLayout CreateMainLayout()
        {
            return new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Vertical,
                LinearAlignment = LinearLayout.Alignment.Top,
                CellPadding = new Size2D(0, GetLayoutSpacing())
            };
        }

        private void CreateTitleArea()
        {
            TitleLabel = new TextLabel
            {
                Text = GetViewTitle(),
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                PointSize = GetTitleFontSize(),
                TextColor = GetThemeTextColor(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Extents(20, 20, 20, 10)
            };
        }

        private void CreateContentContainer()
        {
            ContentContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.FillToParent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center
                },
                Padding = new Extents(20, 20, 20, 20)
            };
        }

        private void CreateButtonContainer()
        {
            var isVertical = DeviceInfo.Type == DeviceType.FamilyHub;
            
            ButtonContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Layout = new LinearLayout
                {
                    LinearOrientation = isVertical ? 
                        LinearLayout.Orientation.Horizontal : 
                        LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(GetButtonSpacing(), GetButtonSpacing())
                },
                Margin = new Extents(20, 20, 10, 20)
            };
        }

        private void CreateOverlays()
        {
            // Loading overlay
            LoadingOverlay = CreateOverlay(GetThemeOverlayColor());
            LoadingOverlay.Add(new TextLabel
            {
                Text = "Loading...",
                WidthSpecification = LayoutParamPolicies.WrapContent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                PointSize = GetBodyFontSize(),
                TextColor = GetThemeTextColor(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
            LoadingOverlay.Hide();

            // Error overlay
            ErrorOverlay = CreateOverlay(GetThemeErrorBackgroundColor());
            ErrorOverlay.Hide();
        }

        private View CreateOverlay(Color backgroundColor)
        {
            return new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                BackgroundColor = backgroundColor,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center
                }
            };
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Get the title for this view
        /// </summary>
        protected abstract string GetViewTitle();

        /// <summary>
        /// Load the content for this view
        /// Called after the view is added to the window
        /// </summary>
        public abstract Task LoadContentAsync();

        #endregion

        #region Navigation (Tizen Stack-Based)

        /// <summary>
        /// Push a new view onto the navigation stack
        /// Uses Tizen Navigator-style push method
        /// </summary>
        protected async Task PushAsync(string screenName, object parameters = null, bool animated = true)
        {
            try
            {
                if (NavigationService != null)
                {
                    await NavigationService.PushAsync(screenName, parameters, animated);
                }
            }
            catch (Exception ex)
            {
                AppController.HandleAppError(ex, "PushAsync");
            }
        }

        /// <summary>
        /// Push a specific view onto the navigation stack
        /// </summary>
        protected async Task PushAsync(BaseView view, bool animated = true)
        {
            try
            {
                if (NavigationService != null)
                {
                    await NavigationService.PushAsync(view, animated);
                }
            }
            catch (Exception ex)
            {
                AppController.HandleAppError(ex, "PushAsync");
            }
        }

        /// <summary>
        /// Pop current view from the navigation stack
        /// Uses Tizen Navigator-style pop method
        /// </summary>
        protected async Task<bool> PopAsync(bool animated = true)
        {
            try
            {
                if (NavigationService != null && NavigationService.CanGoBack)
                {
                    await NavigationService.PopAsync(animated);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                AppController.HandleAppError(ex, "PopAsync");
                return false;
            }
        }

        /// <summary>
        /// Replace current view with a new view
        /// Uses Tizen Navigator-style replace method
        /// </summary>
        protected async Task ReplaceAsync(string screenName, object parameters = null, bool animated = true)
        {
            try
            {
                if (NavigationService != null)
                {
                    await NavigationService.ReplaceAsync(screenName, parameters, animated);
                }
            }
            catch (Exception ex)
            {
                AppController.HandleAppError(ex, "ReplaceAsync");
            }
        }

        /// <summary>
        /// Pop to root view (clear navigation stack except first view)
        /// Uses Tizen Navigator-style pop to root method
        /// </summary>
        protected async Task PopToRootAsync(bool animated = true)
        {
            try
            {
                if (NavigationService != null)
                {
                    await NavigationService.PopToRootAsync(animated);
                }
            }
            catch (Exception ex)
            {
                AppController.HandleAppError(ex, "PopToRootAsync");
            }
        }

        /// <summary>
        /// Set a new root view (clear entire navigation stack)
        /// Uses Tizen Navigator-style set root method
        /// </summary>
        protected async Task SetRootAsync(string screenName, object parameters = null, bool animated = true)
        {
            try
            {
                if (NavigationService != null)
                {
                    await NavigationService.SetRootAsync(screenName, parameters, animated);
                }
            }
            catch (Exception ex)
            {
                AppController.HandleAppError(ex, "SetRootAsync");
            }
        }

        /// <summary>
        /// Check if navigation can go back
        /// </summary>
        protected bool CanNavigateBack => NavigationService?.CanGoBack ?? false;

        /// <summary>
        /// Get current navigation stack depth
        /// </summary>
        protected int NavigationStackDepth => NavigationService?.StackDepth ?? 0;

        /// <summary>
        /// Show dialog using Tizen dialog APIs
        /// TODO: Replace with actual Tizen.NUI.Components.AlertDialog
        /// </summary>
        protected async Task<bool> ShowConfirmDialogAsync(string message, string title = "Confirm")
        {
            try
            {
                // TODO: Use actual Tizen dialog
                // var dialog = new Tizen.NUI.Components.AlertDialog();
                // dialog.Title = title;
                // dialog.Message = message;
                // dialog.PositiveButton = new Button { Text = "OK" };
                // dialog.NegativeButton = new Button { Text = "Cancel" };
                // return await dialog.ShowAsync();

                // Dummy implementation
                await Task.Delay(100);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Show toast message using Tizen toast APIs
        /// TODO: Replace with actual Tizen toast implementation
        /// </summary>
        protected async Task ShowToastAsync(string message)
        {
            try
            {
                // TODO: Use actual Tizen toast
                // Tizen.Applications.ToastMessage.Post(message);
                
                // Dummy implementation
                await Task.Delay(50);
            }
            catch (Exception)
            {
                // Ignore toast errors
            }
        }

        #endregion

        #region Lifecycle Events

        /// <summary>
        /// Called when view appears on screen
        /// </summary>
        public virtual async Task OnAppearingAsync()
        {
            try
            {
                ApplyDeviceSpecificSettings();
                await LoadContentAsync();
            }
            catch (Exception ex)
            {
                AppController.HandleAppError(ex, "OnAppearing");
                await ShowErrorAsync($"Failed to load view: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when view disappears from screen
        /// </summary>
        public virtual async Task OnDisappearingAsync()
        {
            try
            {
                await HideLoadingAsync();
                await HideErrorAsync();
            }
            catch (Exception ex)
            {
                AppController.HandleAppError(ex, "OnDisappearing");
            }
        }

        /// <summary>
        /// Handle back button press
        /// Return true if handled, false to let Tizen handle it
        /// </summary>
        public virtual async Task<bool> OnBackPressedAsync()
        {
            try
            {
                // Default behavior: let Tizen handle back navigation
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected virtual void OnViewNavigatedTo()
        {
            ViewNavigatedTo?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnViewNavigatedFrom()
        {
            ViewNavigatedFrom?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Loading and Error States

        /// <summary>
        /// Show loading overlay with message
        /// </summary>
        protected async Task ShowLoadingAsync(string message = "Loading...")
        {
            try
            {
                if (LoadingOverlay.Children.Count > 0 && LoadingOverlay.Children[0] is TextLabel loadingLabel)
                {
                    loadingLabel.Text = message;
                }
                LoadingOverlay.Show();
                await Task.Delay(50); // Allow UI to update
            }
            catch (Exception)
            {
                // Ignore loading display errors
            }
        }

        /// <summary>
        /// Hide loading overlay
        /// </summary>
        protected async Task HideLoadingAsync()
        {
            try
            {
                LoadingOverlay.Hide();
                await Task.Delay(50); // Allow UI to update
            }
            catch (Exception)
            {
                // Ignore loading hide errors
            }
        }

        /// <summary>
        /// Show error overlay with message
        /// </summary>
        protected async Task ShowErrorAsync(string errorMessage)
        {
            try
            {
                // Clear existing error content
                ErrorOverlay.RemoveAll();
                
                // Add error message
                var errorLabel = new TextLabel
                {
                    Text = errorMessage,
                    WidthSpecification = LayoutParamPolicies.MatchParent,
                    HeightSpecification = LayoutParamPolicies.WrapContent,
                    PointSize = GetBodyFontSize(),
                    TextColor = GetThemeErrorTextColor(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    MultiLine = true,
                    Padding = new Extents(20, 20, 20, 20)
                };
                
                // Add retry button
                var retryButton = CreateStyledButton("Retry", new Size(120, 40));
                retryButton.Clicked += async (s, e) => 
                {
                    await HideErrorAsync();
                    await OnAppearingAsync(); // Retry loading
                };
                
                ErrorOverlay.Add(errorLabel);
                ErrorOverlay.Add(retryButton);
                ErrorOverlay.Show();
                
                await Task.Delay(50); // Allow UI to update
            }
            catch (Exception)
            {
                // Ignore error display errors
            }
        }

        /// <summary>
        /// Hide error overlay
        /// </summary>
        protected async Task HideErrorAsync()
        {
            try
            {
                ErrorOverlay.Hide();
                await Task.Delay(50); // Allow UI to update
            }
            catch (Exception)
            {
                // Ignore error hide errors
            }
        }

        #endregion

        #region Helper Methods

        protected Button CreateStyledButton(string text, Size size, bool isPrimary = false)
        {
            var button = new Button
            {
                Text = text,
                Size = size,
                PointSize = GetButtonFontSize(),
                BackgroundColor = isPrimary ? GetThemePrimaryColor() : GetThemeButtonColor(),
                TextColor = isPrimary ? Color.White : GetThemeTextColor(),
                CornerRadius = 8.0f
            };

            return button;
        }

        private void WireControllerEvents()
        {
            // Wire up controller events if needed
            AuthController.UserLoggedIn += OnUserLoggedIn;
            AuthController.UserLoggedOut += OnUserLoggedOut;
            AccountController.UserSwitched += OnUserSwitched;
        }

        protected virtual void OnUserLoggedIn(object sender, Models.User.SamsungAccount user)
        {
            // Override in derived classes if needed
        }

        protected virtual void OnUserLoggedOut(object sender, string userId)
        {
            // Override in derived classes if needed
        }

        protected virtual void OnUserSwitched(object sender, Models.User.SamsungAccount user)
        {
            // Override in derived classes if needed
        }

        #endregion

        #region Device-Specific Settings

        private void ApplyDeviceSpecificSettings()
        {
            var layoutSettings = AppController.GetLayoutSettings();
            
            // Apply device-specific styling
            if (DeviceInfo.Type == DeviceType.AIHome)
            {
                // Compact layout for appliances
                ApplyCompactLayout(layoutSettings);
            }
            else
            {
                // Rich layout for FamilyHub
                ApplyRichLayout(layoutSettings);
            }
        }

        private void ApplyCompactLayout(LayoutSettings settings)
        {
            // Smaller fonts and spacing for AIHome
            if (TitleLabel != null)
            {
                TitleLabel.PointSize = GetTitleFontSize() * 0.9f;
            }
        }

        private void ApplyRichLayout(LayoutSettings settings)
        {
            // Larger fonts and spacing for FamilyHub
            if (TitleLabel != null)
            {
                TitleLabel.PointSize = GetTitleFontSize() * settings.FontSizeMultiplier;
            }
        }

        #endregion

        #region Theme and Styling

        protected virtual Color GetThemeBackgroundColor()
        {
            return ConfigService.UITheme == "dark" ? 
                new Color(0.1f, 0.1f, 0.1f, 1.0f) : 
                new Color(0.95f, 0.95f, 0.95f, 1.0f);
        }

        protected virtual Color GetThemeTextColor()
        {
            return ConfigService.UITheme == "dark" ? Color.White : Color.Black;
        }

        protected virtual Color GetThemePrimaryColor()
        {
            return new Color(0.2f, 0.6f, 1.0f, 1.0f); // Samsung blue
        }

        protected virtual Color GetThemeButtonColor()
        {
            return ConfigService.UITheme == "dark" ? 
                new Color(0.2f, 0.2f, 0.2f, 1.0f) : 
                new Color(0.9f, 0.9f, 0.9f, 1.0f);
        }

        protected virtual Color GetThemeOverlayColor()
        {
            return new Color(0.0f, 0.0f, 0.0f, 0.5f); // Semi-transparent black
        }

        protected virtual Color GetThemeErrorBackgroundColor()
        {
            return new Color(0.8f, 0.2f, 0.2f, 0.8f); // Semi-transparent red
        }

        protected virtual Color GetThemeErrorTextColor()
        {
            return Color.White;
        }

        protected virtual float GetTitleFontSize()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 24f : 20f;
        }

        protected virtual float GetBodyFontSize()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 16f : 14f;
        }

        protected virtual float GetButtonFontSize()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 14f : 12f;
        }

        protected virtual int GetLayoutSpacing()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 20 : 10;
        }

        protected virtual int GetButtonSpacing()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 15 : 8;
        }

        #endregion

        #region Events

        public event EventHandler ViewNavigatedTo;
        public event EventHandler ViewNavigatedFrom;

        #endregion

        #region Disposal

        protected override void Dispose(DisposeTypes type)
        {
            if (type == DisposeTypes.Explicit)
            {
                // Unwire controller events
                if (AuthController != null)
                {
                    AuthController.UserLoggedIn -= OnUserLoggedIn;
                    AuthController.UserLoggedOut -= OnUserLoggedOut;
                }
                
                if (AccountController != null)
                {
                    AccountController.UserSwitched -= OnUserSwitched;
                }

                // Dispose UI components
                LoadingOverlay?.Dispose();
                ErrorOverlay?.Dispose();
                TitleLabel?.Dispose();
                ContentContainer?.Dispose();
                ButtonContainer?.Dispose();
            }

            base.Dispose(type);
        }

        #endregion
    }
}