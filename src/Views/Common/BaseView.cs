using System;
using System.Threading.Tasks;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using SamsungAccountUI.Controllers.Base;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Services.Device;
using SamsungAccountUI.Models.Device;

namespace SamsungAccountUI.Views.Common
{
    /// <summary>
    /// Base view class that provides common functionality for all views
    /// Shows how to communicate with controllers and handle device-specific layouts
    /// </summary>
    public abstract class BaseView : View
    {
        protected IController Controller { get; private set; }
        protected IGlobalConfigService ConfigService { get; private set; }
        protected DeviceInfo DeviceInfo { get; private set; }
        
        // UI State management
        protected bool IsLoading { get; private set; }
        protected View LoadingOverlay { get; private set; }
        protected View ErrorOverlay { get; private set; }
        
        // Common UI elements
        protected TextLabel TitleLabel { get; private set; }
        protected View ContentContainer { get; private set; }
        protected View ButtonContainer { get; private set; }
        
        // Events for controller communication
        public event EventHandler<string> NavigationRequested;
        public event EventHandler<object> ActionRequested;
        public event EventHandler ViewLoaded;
        
        public BaseView(IController controller, IGlobalConfigService configService, DeviceInfo deviceInfo)
        {
            Controller = controller;
            ConfigService = configService;
            DeviceInfo = deviceInfo;
            
            InitializeBaseLayout();
            ApplyDeviceSpecificSettings();
        }
        
        /// <summary>
        /// Initialize the base layout structure that all views will use
        /// </summary>
        private void InitializeBaseLayout()
        {
            // Set up main view properties
            WidthSpecification = LayoutParamPolicies.MatchParent;
            HeightSpecification = LayoutParamPolicies.MatchParent;
            BackgroundColor = GetThemeBackgroundColor();
            
            // Create main layout
            Layout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Vertical,
                LinearAlignment = LinearLayout.Alignment.Top,
                CellPadding = new Size2D(0, 0)
            };
            
            // Create title area
            CreateTitleArea();
            
            // Create content container
            CreateContentContainer();
            
            // Create button area
            CreateButtonContainer();
            
            // Create overlay elements
            CreateLoadingOverlay();
            CreateErrorOverlay();
        }
        
        private void CreateTitleArea()
        {
            var titleContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = GetTitleHeight(),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    LinearAlignment = LinearLayout.Alignment.CenterVertical
                },
                Padding = GetTitlePadding()
            };
            
            TitleLabel = new TextLabel
            {
                Text = GetViewTitle(),
                TextColor = GetThemeTextColor(),
                PointSize = GetTitleFontSize(),
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold")),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                WidthSpecification = LayoutParamPolicies.MatchParent
            };
            
            titleContainer.Add(TitleLabel);
            Add(titleContainer);
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
                Padding = GetContentPadding()
            };
            
            Add(ContentContainer);
        }
        
        private void CreateButtonContainer()
        {
            ButtonContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = GetButtonAreaHeight(),
                Layout = new LinearLayout
                {
                    LinearOrientation = GetButtonOrientation(),
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = GetButtonSpacing()
                },
                Padding = GetButtonPadding()
            };
            
            Add(ButtonContainer);
        }
        
        private void CreateLoadingOverlay()
        {
            LoadingOverlay = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                BackgroundColor = new Color(0, 0, 0, 0.7f),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(0, 20)
                },
                Visibility = false
            };
            
            // Loading spinner (simplified)
            var loadingSpinner = new ImageView
            {
                ResourceUrl = "images/loading_spinner.png",
                Size = new Size(60, 60)
            };
            
            var loadingText = new TextLabel
            {
                Text = "Loading...",
                TextColor = Color.White,
                PointSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            
            LoadingOverlay.Add(loadingSpinner);
            LoadingOverlay.Add(loadingText);
            Add(LoadingOverlay);
        }
        
        private void CreateErrorOverlay()
        {
            ErrorOverlay = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                BackgroundColor = new Color(0, 0, 0, 0.8f),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(0, 20)
                },
                Visibility = false
            };
            
            Add(ErrorOverlay);
        }
        
        /// <summary>
        /// Apply device-specific settings based on device type
        /// </summary>
        private void ApplyDeviceSpecificSettings()
        {
            switch (DeviceInfo.Type)
            {
                case DeviceType.FamilyHub:
                    ApplyFamilyHubSettings();
                    break;
                case DeviceType.AIHome:
                    ApplyAIHomeSettings();
                    break;
            }
        }
        
        private void ApplyFamilyHubSettings()
        {
            // Large vertical layout optimizations
            Padding = new Extents(60, 60, 80, 80);
        }
        
        private void ApplyAIHomeSettings()
        {
            // Compact horizontal layout optimizations
            Padding = new Extents(30, 30, 40, 40);
        }
        
        #region Abstract Methods - Must be implemented by derived views
        
        /// <summary>
        /// Load the specific content for this view
        /// </summary>
        public abstract Task LoadContentAsync();
        
        /// <summary>
        /// Get the title for this view
        /// </summary>
        protected abstract string GetViewTitle();
        
        /// <summary>
        /// Handle back button press
        /// </summary>
        public abstract Task<bool> OnBackPressedAsync();
        
        #endregion
        
        #region Virtual Methods - Can be overridden by derived views
        
        /// <summary>
        /// Show loading state
        /// </summary>
        public virtual async Task ShowLoadingAsync(string message = "Loading...")
        {
            IsLoading = true;
            
            // Update loading text if overlay has text label
            var loadingText = LoadingOverlay.Children[1] as TextLabel;
            if (loadingText != null)
            {
                loadingText.Text = message;
            }
            
            LoadingOverlay.Visibility = true;
            
            // Animate the loading overlay
            var animation = new Animation(300);
            animation.AnimateTo(LoadingOverlay, "Opacity", 1.0f);
            animation.Play();
            
            await Task.Delay(300); // Wait for animation
        }
        
        /// <summary>
        /// Hide loading state
        /// </summary>
        public virtual async Task HideLoadingAsync()
        {
            if (!IsLoading) return;
            
            IsLoading = false;
            
            var animation = new Animation(200);
            animation.AnimateTo(LoadingOverlay, "Opacity", 0.0f);
            animation.Finished += (sender, e) => LoadingOverlay.Visibility = false;
            animation.Play();
            
            await Task.Delay(200);
        }
        
        /// <summary>
        /// Show error message
        /// </summary>
        public virtual async Task ShowErrorAsync(string message, string actionText = "OK")
        {
            // Clear previous error content
            ErrorOverlay.Children.Clear();
            
            // Error icon
            var errorIcon = new ImageView
            {
                ResourceUrl = "images/error_icon.png",
                Size = new Size(80, 80)
            };
            
            // Error message
            var errorText = new TextLabel
            {
                Text = message,
                TextColor = Color.White,
                PointSize = GetErrorMessageFontSize(),
                HorizontalAlignment = HorizontalAlignment.Center,
                MultiLine = true,
                WidthSpecification = GetErrorMessageWidth()
            };
            
            // OK button
            var okButton = CreateStyledButton(actionText, GetErrorButtonSize());
            okButton.Clicked += async (sender, e) => await HideErrorAsync();
            
            ErrorOverlay.Add(errorIcon);
            ErrorOverlay.Add(errorText);
            ErrorOverlay.Add(okButton);
            
            ErrorOverlay.Visibility = true;
            
            // Animate error overlay
            var animation = new Animation(300);
            animation.AnimateTo(ErrorOverlay, "Opacity", 1.0f);
            animation.Play();
            
            await Task.Delay(300);
        }
        
        /// <summary>
        /// Hide error overlay
        /// </summary>
        public virtual async Task HideErrorAsync()
        {
            var animation = new Animation(200);
            animation.AnimateTo(ErrorOverlay, "Opacity", 0.0f);
            animation.Finished += (sender, e) => ErrorOverlay.Visibility = false;
            animation.Play();
            
            await Task.Delay(200);
        }
        
        #endregion
        
        #region Helper Methods for Controller Communication
        
        /// <summary>
        /// Request navigation to another screen
        /// </summary>
        protected void RequestNavigation(string screenName)
        {
            NavigationRequested?.Invoke(this, screenName);
        }
        
        /// <summary>
        /// Send action to controller
        /// </summary>
        protected void SendActionToController(object actionData)
        {
            ActionRequested?.Invoke(this, actionData);
        }
        
        /// <summary>
        /// Notify that view has finished loading
        /// </summary>
        protected void NotifyViewLoaded()
        {
            ViewLoaded?.Invoke(this, EventArgs.Empty);
        }
        
        #endregion
        
        #region UI Helper Methods
        
        /// <summary>
        /// Create a styled button with consistent appearance
        /// </summary>
        protected Button CreateStyledButton(string text, Size size, bool isPrimary = true)
        {
            var button = new Button
            {
                Text = text,
                Size = size,
                PointSize = GetButtonFontSize(),
                CornerRadius = GetButtonCornerRadius(),
                BackgroundColor = isPrimary ? GetPrimaryButtonColor() : GetSecondaryButtonColor(),
                TextColor = isPrimary ? GetPrimaryButtonTextColor() : GetSecondaryButtonTextColor()
            };
            
            // Add hover/press effects
            button.TouchEvent += OnButtonTouchEvent;
            
            return button;
        }
        
        /// <summary>
        /// Handle button touch events for visual feedback
        /// </summary>
        private bool OnButtonTouchEvent(object source, TouchEventArgs e)
        {
            var button = source as Button;
            if (button == null) return false;
            
            if (e.Touch.GetState(0) == PointStateType.Down)
            {
                // Button pressed - darken
                var currentColor = button.BackgroundColor;
                button.BackgroundColor = new Color(
                    currentColor.R * 0.8f,
                    currentColor.G * 0.8f,
                    currentColor.B * 0.8f,
                    currentColor.A
                );
            }
            else if (e.Touch.GetState(0) == PointStateType.Up)
            {
                // Button released - restore color
                var isPrimary = button.TextColor == GetPrimaryButtonTextColor();
                button.BackgroundColor = isPrimary ? GetPrimaryButtonColor() : GetSecondaryButtonColor();
            }
            
            return false; // Allow event to continue
        }
        
        #endregion
        
        #region Device-Specific Sizing Methods
        
        private int GetTitleHeight()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 120 : 80;
        }
        
        private int GetButtonAreaHeight()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 140 : 100;
        }
        
        private float GetTitleFontSize()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 24f : 16f;
        }
        
        private float GetButtonFontSize()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 16f : 12f;
        }
        
        private float GetErrorMessageFontSize()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 16f : 12f;
        }
        
        private int GetErrorMessageWidth()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 500 : 300;
        }
        
        private Size GetErrorButtonSize()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? new Size(150, 50) : new Size(100, 35);
        }
        
        private float GetButtonCornerRadius()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 12f : 8f;
        }
        
        private LinearLayout.Orientation GetButtonOrientation()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 
                LinearLayout.Orientation.Horizontal : 
                LinearLayout.Orientation.Vertical;
        }
        
        private Size2D GetButtonSpacing()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? new Size2D(30, 0) : new Size2D(0, 15);
        }
        
        private Extents GetTitlePadding()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 
                new Extents(40, 40, 20, 20) : 
                new Extents(20, 20, 10, 10);
        }
        
        private Extents GetContentPadding()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 
                new Extents(40, 40, 20, 20) : 
                new Extents(20, 20, 10, 10);
        }
        
        private Extents GetButtonPadding()
        {
            return DeviceInfo.Type == DeviceType.FamilyHub ? 
                new Extents(40, 40, 20, 20) : 
                new Extents(20, 20, 10, 10);
        }
        
        #endregion
        
        #region Theme Color Methods
        
        private Color GetThemeBackgroundColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? new Color(0.1f, 0.1f, 0.1f, 1.0f) : new Color(0.95f, 0.95f, 0.95f, 1.0f);
        }
        
        private Color GetThemeTextColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? Color.White : Color.Black;
        }
        
        private Color GetPrimaryButtonColor()
        {
            return new Color(0.2f, 0.6f, 1.0f, 1.0f); // Samsung blue
        }
        
        private Color GetSecondaryButtonColor()
        {
            return new Color(0.4f, 0.4f, 0.4f, 1.0f); // Gray
        }
        
        private Color GetPrimaryButtonTextColor()
        {
            return Color.White;
        }
        
        private Color GetSecondaryButtonTextColor()
        {
            return Color.White;
        }
        
        #endregion
        
        #region Lifecycle Methods
        
        /// <summary>
        /// Called when view is about to be shown
        /// </summary>
        public virtual async Task OnAppearingAsync()
        {
            await LoadContentAsync();
            NotifyViewLoaded();
        }
        
        /// <summary>
        /// Called when view is about to be hidden
        /// </summary>
        public virtual async Task OnDisappearingAsync()
        {
            await HideLoadingAsync();
            await HideErrorAsync();
        }
        
        #endregion
        
        protected override void Dispose(DisposeTypes type)
        {
            if (type == DisposeTypes.Explicit)
            {
                LoadingOverlay?.Dispose();
                ErrorOverlay?.Dispose();
                TitleLabel?.Dispose();
                ContentContainer?.Dispose();
                ButtonContainer?.Dispose();
            }
            
            base.Dispose(type);
        }
    }
}