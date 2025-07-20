# Tizen NUI Framework Integration Guide

## 📋 Overview

This guide provides comprehensive instructions for integrating the Samsung Account UI application with the Tizen NUI framework, implementing actual user interface components and device-specific optimizations.

## 🎯 NUI Framework Overview

Tizen NUI (Natural User Interface) is a 3D scene graph based UI toolkit that provides:
- Hardware-accelerated graphics rendering
- Flexible layout management
- Touch and gesture handling
- Animation and transition effects
- Device-specific optimizations

## 🏗️ View Implementation Architecture

### Base View Implementation

Replace the abstract BaseView with concrete Tizen NUI implementation:

```csharp
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Views.Navigation;

namespace SamsungAccountUI.Views.Common
{
    public abstract class BaseView : View, IDisposable
    {
        protected DeviceType DeviceType { get; set; }
        protected INavigationService NavigationService { get; set; }
        protected IGlobalConfigService ConfigService { get; set; }
        
        // NUI-specific properties
        protected View ContentContainer { get; set; }
        protected View LoadingOverlay { get; set; }
        protected View ErrorOverlay { get; set; }
        
        // Layout managers
        protected LinearLayout MainLayout { get; set; }
        protected AbsoluteLayout OverlayLayout { get; set; }
        
        // Animation objects
        protected Animation FadeInAnimation { get; set; }
        protected Animation FadeOutAnimation { get; set; }
        
        protected BaseView(INavigationService navigationService, IGlobalConfigService configService)
        {
            NavigationService = navigationService;
            ConfigService = configService;
            
            InitializeNUIComponents();
            SetupAnimations();
        }
        
        private void InitializeNUIComponents()
        {
            // Set up base view properties
            WidthSpecification = LayoutParamPolicies.MatchParent;
            HeightSpecification = LayoutParamPolicies.MatchParent;
            BackgroundColor = Color.Black;
            
            // Create main layout
            MainLayout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Vertical,
                LinearAlignment = LinearLayout.Alignment.Center
            };
            Layout = MainLayout;
            
            // Create content container
            ContentContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                Layout = new AbsoluteLayout()
            };
            Add(ContentContainer);
            
            // Create overlay layout for loading and error states
            OverlayLayout = new AbsoluteLayout();
            
            // Create loading overlay
            LoadingOverlay = CreateLoadingOverlay();
            LoadingOverlay.Hide();
            Add(LoadingOverlay);
            
            // Create error overlay
            ErrorOverlay = CreateErrorOverlay();
            ErrorOverlay.Hide();
            Add(ErrorOverlay);
        }
        
        private void SetupAnimations()
        {
            // Fade in animation
            FadeInAnimation = new Animation(300);
            FadeOutAnimation = new Animation(300);
        }
        
        protected virtual View CreateLoadingOverlay()
        {
            var overlay = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                BackgroundColor = new Color(0, 0, 0, 0.7f), // Semi-transparent background
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(20, 20)
                }
            };
            
            // Loading spinner
            var spinner = new LoadingView
            {
                Size = new Size(60, 60)
            };
            overlay.Add(spinner);
            
            // Loading text
            var loadingText = new TextLabel
            {
                Text = "Loading...",
                TextColor = Color.White,
                PointSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                WidthSpecification = LayoutParamPolicies.WrapContent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Margin = new Extents(0, 0, 20, 0)
            };
            overlay.Add(loadingText);
            
            return overlay;
        }
        
        protected virtual View CreateErrorOverlay()
        {
            var overlay = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                BackgroundColor = new Color(0, 0, 0, 0.8f),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(30, 30)
                }
            };
            
            // Error icon
            var errorIcon = new ImageView
            {
                ResourceUrl = "images/error_icon.png",
                Size = new Size(80, 80)
            };
            overlay.Add(errorIcon);
            
            // Error title
            var errorTitle = new TextLabel
            {
                Text = "Error",
                TextColor = Color.Red,
                PointSize = 20,
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold")),
                HorizontalAlignment = HorizontalAlignment.Center,
                WidthSpecification = LayoutParamPolicies.WrapContent,
                HeightSpecification = LayoutParamPolicies.WrapContent
            };
            overlay.Add(errorTitle);
            
            // Error message
            var errorMessage = new TextLabel
            {
                Name = "ErrorMessage",
                TextColor = Color.White,
                PointSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MultiLine = true,
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Margin = new Extents(40, 40, 20, 20)
            };
            overlay.Add(errorMessage);
            
            // Action buttons container
            var buttonContainer = new View
            {
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(20, 0)
                },
                WidthSpecification = LayoutParamPolicies.WrapContent,
                HeightSpecification = LayoutParamPolicies.WrapContent
            };
            
            // Retry button
            var retryButton = new Button
            {
                Text = "Retry",
                Size = new Size(120, 50)
            };
            retryButton.Clicked += OnRetryClicked;
            buttonContainer.Add(retryButton);
            
            // Cancel button  
            var cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(120, 50)
            };
            cancelButton.Clicked += OnCancelClicked;
            buttonContainer.Add(cancelButton);
            
            overlay.Add(buttonContainer);
            
            return overlay;
        }
        
        public abstract void LoadContent();
        public abstract void UpdateForDevice(DeviceType deviceType);
        
        protected virtual void ApplyConfigSettings()
        {
            // Apply theme
            var theme = ConfigService.DefaultUITheme;
            ApplyTheme(theme);
            
            // Apply large text if enabled
            if (ConfigService.EnableLargeText)
            {
                ApplyLargeTextSettings();
            }
            
            // Apply animations setting
            if (!ConfigService.EnableAnimations)
            {
                DisableAnimations();
            }
        }
        
        protected virtual void ApplyTheme(string theme)
        {
            switch (theme.ToLower())
            {
                case "dark":
                    BackgroundColor = Color.Black;
                    break;
                case "light":
                    BackgroundColor = Color.White;
                    break;
                default:
                    BackgroundColor = Color.Black;
                    break;
            }
        }
        
        protected virtual void ApplyLargeTextSettings()
        {
            // Increase font sizes for accessibility
            ScaleText(1.2f);
        }
        
        protected virtual void DisableAnimations()
        {
            FadeInAnimation?.Stop();
            FadeOutAnimation?.Stop();
        }
        
        protected virtual void ScaleText(float scale)
        {
            // Apply text scaling to all TextLabel children
            foreach (var child in Children)
            {
                if (child is TextLabel textLabel)
                {
                    textLabel.PointSize *= scale;
                }
            }
        }
        
        public override async Task ShowLoadingAsync(string message = "Loading...")
        {
            if (LoadingOverlay.FindChildByName("LoadingText") is TextLabel loadingText)
            {
                loadingText.Text = message;
            }
            
            LoadingOverlay.Show();
            
            if (ConfigService.EnableAnimations)
            {
                FadeInAnimation.Clear();
                FadeInAnimation.AnimateTo(LoadingOverlay, "Opacity", 1.0f);
                FadeInAnimation.Play();
            }
            else
            {
                LoadingOverlay.Opacity = 1.0f;
            }
            
            await Task.CompletedTask;
        }
        
        public override async Task HideLoadingAsync()
        {
            if (ConfigService.EnableAnimations)
            {
                FadeOutAnimation.Clear();
                FadeOutAnimation.AnimateTo(LoadingOverlay, "Opacity", 0.0f);
                FadeOutAnimation.Finished += (sender, e) => LoadingOverlay.Hide();
                FadeOutAnimation.Play();
            }
            else
            {
                LoadingOverlay.Hide();
            }
            
            await Task.CompletedTask;
        }
        
        public override async Task ShowErrorAsync(string message, string title = "Error")
        {
            if (ErrorOverlay.FindChildByName("ErrorTitle") is TextLabel errorTitle)
            {
                errorTitle.Text = title;
            }
            
            if (ErrorOverlay.FindChildByName("ErrorMessage") is TextLabel errorMessage)
            {
                errorMessage.Text = message;
            }
            
            ErrorOverlay.Show();
            
            if (ConfigService.EnableAnimations)
            {
                FadeInAnimation.Clear();
                FadeInAnimation.AnimateTo(ErrorOverlay, "Opacity", 1.0f);
                FadeInAnimation.Play();
            }
            else
            {
                ErrorOverlay.Opacity = 1.0f;
            }
            
            await Task.CompletedTask;
        }
        
        protected virtual void OnRetryClicked(object sender, ClickedEventArgs e)
        {
            // Hide error overlay and trigger retry action
            ErrorOverlay.Hide();
            // Implement retry logic in derived classes
        }
        
        protected virtual void OnCancelClicked(object sender, ClickedEventArgs e)
        {
            ErrorOverlay.Hide();
            NavigationService.NavigateBackAsync();
        }
        
        public virtual new void Dispose()
        {
            // Clean up animations
            FadeInAnimation?.Dispose();
            FadeOutAnimation?.Dispose();
            
            // Clean up views
            ContentContainer?.Dispose();
            LoadingOverlay?.Dispose();
            ErrorOverlay?.Dispose();
            
            base.Dispose();
        }
    }
}
```

## 📱 Device-Specific View Implementations

### FamilyHub QR Login View (Large Vertical Screen)

```csharp
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;
using SamsungAccountUI.Controllers.Authentication;
using SamsungAccountUI.Models.Device;

namespace SamsungAccountUI.Views.FamilyHub
{
    public class QRLoginView : BaseView
    {
        private ImageView _qrCodeImage;
        private TextLabel _instructionText;
        private Button _passwordLoginButton;
        private Button _googleLoginButton;
        private Button _refreshQRButton;
        private View _qrContainer;
        private View _buttonContainer;
        private LoginController _controller;
        
        public QRLoginView(INavigationService navigationService, IGlobalConfigService configService)
            : base(navigationService, configService)
        {
            _controller = new LoginController(navigationService, /* inject dependencies */);
        }
        
        public override void LoadContent()
        {
            ApplyConfigSettings();
            CreateFamilyHubLayout();
            UpdateForDevice(DeviceType.FamilyHub);
        }
        
        public override void UpdateForDevice(DeviceType deviceType)
        {
            SetDeviceType(deviceType);
            
            if (deviceType == DeviceType.FamilyHub)
            {
                ApplyFamilyHubLayout();
            }
        }
        
        private void CreateFamilyHubLayout()
        {
            // Create main container with vertical layout
            var mainContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(0, 40)
                },
                Padding = new Extents(60, 60, 80, 80)
            };
            
            // Samsung Account logo
            var logoImage = new ImageView
            {
                ResourceUrl = "images/samsung_account_logo.png",
                Size = new Size(200, 60),
                Margin = new Extents(0, 0, 0, 40)
            };
            mainContainer.Add(logoImage);
            
            // Welcome text
            var welcomeText = new TextLabel
            {
                Text = "Sign in to Samsung Account",
                TextColor = Color.White,
                PointSize = 28,
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold")),
                HorizontalAlignment = HorizontalAlignment.Center,
                WidthSpecification = LayoutParamPolicies.WrapContent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Margin = new Extents(0, 0, 0, 30)
            };
            mainContainer.Add(welcomeText);
            
            // QR Code container
            _qrContainer = CreateQRContainer();
            mainContainer.Add(_qrContainer);
            
            // Alternative login options
            _buttonContainer = CreateButtonContainer();
            mainContainer.Add(_buttonContainer);
            
            ContentContainer.Add(mainContainer);
        }
        
        private View CreateQRContainer()
        {
            var container = new View
            {
                Size = new Size(400, 500),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(0, 20)
                }
            };
            
            // QR Code background
            var qrBackground = new View
            {
                Size = new Size(320, 320),
                BackgroundColor = Color.White,
                CornerRadius = 16.0f,
                Layout = new LinearLayout
                {
                    LinearAlignment = LinearLayout.Alignment.Center
                }
            };
            
            // QR Code image
            _qrCodeImage = new ImageView
            {
                ResourceUrl = "images/qr_code_placeholder.png",
                Size = new Size(280, 280)
            };
            qrBackground.Add(_qrCodeImage);
            container.Add(qrBackground);
            
            // Instruction text
            _instructionText = new TextLabel
            {
                Text = "Scan this QR code with your Samsung Account mobile app",
                TextColor = Color.White,
                PointSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MultiLine = true,
                WidthSpecification = 350,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Margin = new Extents(0, 0, 10, 0)
            };
            container.Add(_instructionText);
            
            // Refresh QR button
            _refreshQRButton = new Button
            {
                Text = "Refresh QR Code",
                Size = new Size(180, 45),
                PointSize = 14,
                Margin = new Extents(0, 0, 10, 0)
            };
            _refreshQRButton.Clicked += OnRefreshQRClicked;
            container.Add(_refreshQRButton);
            
            return container;
        }
        
        private View CreateButtonContainer()
        {
            var container = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(30, 0)
                },
                Margin = new Extents(0, 0, 40, 0)
            };
            
            // Password login button
            _passwordLoginButton = CreateAlternativeLoginButton("Use Password", "images/password_icon.png");
            _passwordLoginButton.Clicked += OnPasswordLoginClicked;
            container.Add(_passwordLoginButton);
            
            // Google login button
            _googleLoginButton = CreateAlternativeLoginButton("Use Google", "images/google_icon.png");
            _googleLoginButton.Clicked += OnGoogleLoginClicked;
            container.Add(_googleLoginButton);
            
            return container;
        }
        
        private Button CreateAlternativeLoginButton(string text, string iconPath)
        {
            var button = new Button
            {
                Size = new Size(200, 60),
                Text = text,
                PointSize = 16,
                CornerRadius = 8.0f,
                BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f)
            };
            
            // Add icon to button
            var icon = new ImageView
            {
                ResourceUrl = iconPath,
                Size = new Size(24, 24)
            };
            // Note: Button icon setup would depend on specific NUI Button implementation
            
            return button;
        }
        
        private void ApplyFamilyHubLayout()
        {
            // Optimize for large vertical screen (21"/32")
            // Use larger fonts, more spacing, rich visuals
            
            if (_instructionText != null)
            {
                _instructionText.PointSize = 18;
                _instructionText.Margin = new Extents(0, 0, 20, 20);
            }
            
            if (_qrCodeImage != null)
            {
                _qrCodeImage.Size = new Size(300, 300);
            }
            
            // Enable rich animations for FamilyHub
            if (ConfigService.EnableAnimations)
            {
                StartQRAnimation();
            }
        }
        
        private void StartQRAnimation()
        {
            // Create subtle breathing animation for QR container
            var breatheAnimation = new Animation(2000);
            breatheAnimation.AnimateTo(_qrContainer, "Scale", new Vector3(1.05f, 1.05f, 1.0f), 0, 1000);
            breatheAnimation.AnimateTo(_qrContainer, "Scale", new Vector3(1.0f, 1.0f, 1.0f), 1000, 2000);
            breatheAnimation.Looping = true;
            breatheAnimation.Play();
        }
        
        // Event handlers
        private async void OnRefreshQRClicked(object sender, ClickedEventArgs e)
        {
            await _controller.HandleInputAsync("refresh_qr");
        }
        
        private async void OnPasswordLoginClicked(object sender, ClickedEventArgs e)
        {
            await _controller.HandleInputAsync("navigate_to_password");
        }
        
        private async void OnGoogleLoginClicked(object sender, ClickedEventArgs e)
        {
            await _controller.HandleInputAsync("navigate_to_google");
        }
        
        // QR code simulation (replace with actual QR generation)
        private void UpdateQRCode(string qrData)
        {
            // Generate QR code image from data
            // _qrCodeImage.ResourceUrl = GenerateQRCode(qrData);
        }
    }
}
```

### AIHome QR Login View (Small Horizontal Screen)

```csharp
namespace SamsungAccountUI.Views.AIHome
{
    public class QRLoginView : BaseView
    {
        private ImageView _qrCodeImage;
        private TextLabel _instructionText;
        private View _buttonRow;
        private LoginController _controller;
        
        public QRLoginView(INavigationService navigationService, IGlobalConfigService configService)
            : base(navigationService, configService)
        {
            _controller = new LoginController(navigationService, /* inject dependencies */);
        }
        
        public override void LoadContent()
        {
            ApplyConfigSettings();
            CreateAIHomeLayout();
            UpdateForDevice(DeviceType.AIHome);
        }
        
        public override void UpdateForDevice(DeviceType deviceType)
        {
            SetDeviceType(deviceType);
            
            if (deviceType == DeviceType.AIHome)
            {
                ApplyAIHomeLayout();
            }
        }
        
        private void CreateAIHomeLayout()
        {
            // Create compact horizontal layout for small screens
            var mainContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(30, 0)
                },
                Padding = new Extents(40, 40, 30, 30)
            };
            
            // Left side - QR Code
            var qrSection = CreateCompactQRSection();
            mainContainer.Add(qrSection);
            
            // Right side - Instructions and buttons
            var infoSection = CreateCompactInfoSection();
            mainContainer.Add(infoSection);
            
            ContentContainer.Add(mainContainer);
        }
        
        private View CreateCompactQRSection()
        {
            var container = new View
            {
                Size = new Size(200, 200),
                Layout = new LinearLayout
                {
                    LinearAlignment = LinearLayout.Alignment.Center
                }
            };
            
            var qrBackground = new View
            {
                Size = new Size(180, 180),
                BackgroundColor = Color.White,
                CornerRadius = 8.0f,
                Layout = new LinearLayout
                {
                    LinearAlignment = LinearLayout.Alignment.Center
                }
            };
            
            _qrCodeImage = new ImageView
            {
                ResourceUrl = "images/qr_code_placeholder.png",
                Size = new Size(160, 160)
            };
            qrBackground.Add(_qrCodeImage);
            container.Add(qrBackground);
            
            return container;
        }
        
        private View CreateCompactInfoSection()
        {
            var container = new View
            {
                WidthSpecification = LayoutParamPolicies.FillToParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(0, 15)
                }
            };
            
            // Compact title
            var titleText = new TextLabel
            {
                Text = "Samsung Account",
                TextColor = Color.White,
                PointSize = 18,
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold")),
                HorizontalAlignment = HorizontalAlignment.Center,
                WidthSpecification = LayoutParamPolicies.WrapContent,
                HeightSpecification = LayoutParamPolicies.WrapContent
            };
            container.Add(titleText);
            
            // Compact instruction
            _instructionText = new TextLabel
            {
                Text = "Scan QR code with mobile app",
                TextColor = Color.White,
                PointSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MultiLine = true,
                WidthSpecification = 250,
                HeightSpecification = LayoutParamPolicies.WrapContent
            };
            container.Add(_instructionText);
            
            // Compact button row
            _buttonRow = CreateCompactButtonRow();
            container.Add(_buttonRow);
            
            return container;
        }
        
        private View CreateCompactButtonRow()
        {
            var container = new View
            {
                WidthSpecification = LayoutParamPolicies.WrapContent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    CellPadding = new Size2D(0, 10)
                }
            };
            
            // Compact buttons
            var passwordButton = new Button
            {
                Text = "Password",
                Size = new Size(120, 35),
                PointSize = 12
            };
            passwordButton.Clicked += OnPasswordLoginClicked;
            container.Add(passwordButton);
            
            var googleButton = new Button
            {
                Text = "Google",
                Size = new Size(120, 35),
                PointSize = 12
            };
            googleButton.Clicked += OnGoogleLoginClicked;
            container.Add(googleButton);
            
            return container;
        }
        
        private void ApplyAIHomeLayout()
        {
            // Optimize for small horizontal screen (7"/9")
            // Use smaller fonts, minimal spacing, essential features only
            
            if (_instructionText != null)
            {
                _instructionText.PointSize = 11;
            }
            
            // Disable animations for better performance on AIHome
            DisableAnimations();
        }
        
        private async void OnPasswordLoginClicked(object sender, ClickedEventArgs e)
        {
            await _controller.OnDeviceSpecificAction("quick_login", new[] { "email", "password" });
        }
        
        private async void OnGoogleLoginClicked(object sender, ClickedEventArgs e)
        {
            await _controller.HandleInputAsync("navigate_to_google");
        }
    }
}
```

## 🎨 Styling and Theming

### Theme System Implementation

```csharp
using Tizen.NUI;

namespace SamsungAccountUI.Themes
{
    public static class SamsungAccountTheme
    {
        public static class Colors
        {
            // Samsung brand colors
            public static readonly Color SamsungBlue = new Color(0.067f, 0.341f, 0.663f, 1.0f); // #1157AA
            public static readonly Color SamsungLightBlue = new Color(0.306f, 0.710f, 0.988f, 1.0f); // #4EB5FC
            
            // Dark theme colors
            public static readonly Color DarkBackground = new Color(0.067f, 0.067f, 0.067f, 1.0f); // #111111
            public static readonly Color DarkSurface = new Color(0.133f, 0.133f, 0.133f, 1.0f); // #222222
            public static readonly Color DarkPrimary = Color.White;
            public static readonly Color DarkSecondary = new Color(0.8f, 0.8f, 0.8f, 1.0f);
            
            // Light theme colors
            public static readonly Color LightBackground = Color.White;
            public static readonly Color LightSurface = new Color(0.95f, 0.95f, 0.95f, 1.0f);
            public static readonly Color LightPrimary = Color.Black;
            public static readonly Color LightSecondary = new Color(0.4f, 0.4f, 0.4f, 1.0f);
            
            // Status colors
            public static readonly Color Success = new Color(0.2f, 0.7f, 0.2f, 1.0f);
            public static readonly Color Error = new Color(0.9f, 0.2f, 0.2f, 1.0f);
            public static readonly Color Warning = new Color(0.9f, 0.7f, 0.2f, 1.0f);
        }
        
        public static class Fonts
        {
            public static readonly string PrimaryFont = "SamsungSharpSans";
            public static readonly string SecondaryFont = "SamsungSharpSans-Medium";
            
            // Font sizes for different device types
            public static class FamilyHub
            {
                public const float TitleSize = 28f;
                public const float HeaderSize = 20f;
                public const float BodySize = 16f;
                public const float CaptionSize = 14f;
            }
            
            public static class AIHome
            {
                public const float TitleSize = 18f;
                public const float HeaderSize = 16f;
                public const float BodySize = 12f;
                public const float CaptionSize = 10f;
            }
        }
        
        public static class Dimensions
        {
            // Spacing
            public static readonly Extents SmallPadding = new Extents(8, 8, 8, 8);
            public static readonly Extents MediumPadding = new Extents(16, 16, 16, 16);
            public static readonly Extents LargePadding = new Extents(32, 32, 32, 32);
            
            // Corner radius
            public const float SmallRadius = 4f;
            public const float MediumRadius = 8f;
            public const float LargeRadius = 16f;
            
            // Button sizes
            public static readonly Size FamilyHubButtonSize = new Size(200, 60);
            public static readonly Size AIHomeButtonSize = new Size(120, 40);
        }
        
        public static void ApplyTheme(View view, string themeName, DeviceType deviceType)
        {
            switch (themeName.ToLower())
            {
                case "dark":
                    ApplyDarkTheme(view, deviceType);
                    break;
                case "light":
                    ApplyLightTheme(view, deviceType);
                    break;
            }
        }
        
        private static void ApplyDarkTheme(View view, DeviceType deviceType)
        {
            view.BackgroundColor = Colors.DarkBackground;
            
            ApplyThemeToChildren(view, deviceType, true);
        }
        
        private static void ApplyLightTheme(View view, DeviceType deviceType)
        {
            view.BackgroundColor = Colors.LightBackground;
            
            ApplyThemeToChildren(view, deviceType, false);
        }
        
        private static void ApplyThemeToChildren(View parent, DeviceType deviceType, bool isDark)
        {
            foreach (var child in parent.Children)
            {
                if (child is TextLabel textLabel)
                {
                    textLabel.TextColor = isDark ? Colors.DarkPrimary : Colors.LightPrimary;
                    textLabel.FontFamily = Fonts.PrimaryFont;
                    
                    // Apply device-specific font sizes
                    if (deviceType == DeviceType.FamilyHub)
                    {
                        if (textLabel.Name?.Contains("Title") == true)
                            textLabel.PointSize = Fonts.FamilyHub.TitleSize;
                        else if (textLabel.Name?.Contains("Header") == true)
                            textLabel.PointSize = Fonts.FamilyHub.HeaderSize;
                        else
                            textLabel.PointSize = Fonts.FamilyHub.BodySize;
                    }
                    else if (deviceType == DeviceType.AIHome)
                    {
                        if (textLabel.Name?.Contains("Title") == true)
                            textLabel.PointSize = Fonts.AIHome.TitleSize;
                        else if (textLabel.Name?.Contains("Header") == true)
                            textLabel.PointSize = Fonts.AIHome.HeaderSize;
                        else
                            textLabel.PointSize = Fonts.AIHome.BodySize;
                    }
                }
                else if (child is Button button)
                {
                    button.BackgroundColor = Colors.SamsungBlue;
                    button.TextColor = Color.White;
                    button.CornerRadius = Dimensions.MediumRadius;
                    
                    // Apply device-specific button sizes
                    if (deviceType == DeviceType.FamilyHub)
                    {
                        button.Size = Dimensions.FamilyHubButtonSize;
                    }
                    else if (deviceType == DeviceType.AIHome)
                    {
                        button.Size = Dimensions.AIHomeButtonSize;
                    }
                }
                
                // Recursively apply to nested children
                if (child.ChildCount > 0)
                {
                    ApplyThemeToChildren(child, deviceType, isDark);
                }
            }
        }
    }
}
```

## 🎬 Animation System

### Animation Helper Class

```csharp
using Tizen.NUI;

namespace SamsungAccountUI.Animations
{
    public static class AnimationHelper
    {
        public static Animation CreateFadeIn(View target, uint duration = 300)
        {
            var animation = new Animation((int)duration);
            target.Opacity = 0.0f;
            animation.AnimateTo(target, "Opacity", 1.0f);
            return animation;
        }
        
        public static Animation CreateFadeOut(View target, uint duration = 300)
        {
            var animation = new Animation((int)duration);
            animation.AnimateTo(target, "Opacity", 0.0f);
            return animation;
        }
        
        public static Animation CreateSlideIn(View target, SlideDirection direction, uint duration = 400)
        {
            var animation = new Animation((int)duration);
            
            var startPosition = target.Position;
            Vector3 endPosition = startPosition;
            
            switch (direction)
            {
                case SlideDirection.FromLeft:
                    startPosition.X -= target.Size.Width;
                    break;
                case SlideDirection.FromRight:
                    startPosition.X += target.Size.Width;
                    break;
                case SlideDirection.FromTop:
                    startPosition.Y -= target.Size.Height;
                    break;
                case SlideDirection.FromBottom:
                    startPosition.Y += target.Size.Height;
                    break;
            }
            
            target.Position = startPosition;
            animation.AnimateTo(target, "Position", endPosition, 0, (int)duration, new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOutQuart));
            
            return animation;
        }
        
        public static Animation CreateScale(View target, Vector3 fromScale, Vector3 toScale, uint duration = 300)
        {
            var animation = new Animation((int)duration);
            target.Scale = fromScale;
            animation.AnimateTo(target, "Scale", toScale, 0, (int)duration, new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOutBack));
            return animation;
        }
        
        public static Animation CreateBounce(View target, float intensity = 0.1f, uint duration = 600)
        {
            var animation = new Animation((int)duration);
            var originalScale = target.Scale;
            var bounceScale = new Vector3(originalScale.X + intensity, originalScale.Y + intensity, originalScale.Z);
            
            animation.AnimateTo(target, "Scale", bounceScale, 0, (int)(duration * 0.3f));
            animation.AnimateTo(target, "Scale", originalScale, (int)(duration * 0.3f), (int)duration);
            
            return animation;
        }
        
        public static void AnimateScreenTransition(View fromScreen, View toScreen, TransitionType transition)
        {
            switch (transition)
            {
                case TransitionType.FadeInOut:
                    var fadeOut = CreateFadeOut(fromScreen);
                    var fadeIn = CreateFadeIn(toScreen);
                    fadeOut.Finished += (s, e) => fadeIn.Play();
                    fadeOut.Play();
                    break;
                    
                case TransitionType.SlideLeftToRight:
                    var slideOut = CreateSlideIn(fromScreen, SlideDirection.FromRight);
                    var slideIn = CreateSlideIn(toScreen, SlideDirection.FromLeft);
                    slideOut.Play();
                    slideIn.Play();
                    break;
            }
        }
    }
    
    public enum SlideDirection
    {
        FromLeft,
        FromRight,
        FromTop,
        FromBottom
    }
    
    public enum TransitionType
    {
        FadeInOut,
        SlideLeftToRight,
        SlideRightToLeft,
        SlideTopToBottom,
        SlideBottomToTop
    }
}
```

## 📱 Input Handling and Touch Events

### Touch Event Manager

```csharp
using Tizen.NUI;

namespace SamsungAccountUI.Input
{
    public class TouchEventManager
    {
        private readonly Dictionary<View, TouchInfo> _touchInfos = new();
        
        public void RegisterTouchEvents(View view)
        {
            view.TouchEvent += OnTouchEvent;
            _touchInfos[view] = new TouchInfo();
        }
        
        public void UnregisterTouchEvents(View view)
        {
            view.TouchEvent -= OnTouchEvent;
            _touchInfos.Remove(view);
        }
        
        private bool OnTouchEvent(object sender, TouchEventArgs e)
        {
            var view = sender as View;
            if (view == null || !_touchInfos.ContainsKey(view))
                return false;
            
            var touchInfo = _touchInfos[view];
            var touch = e.Touch;
            
            switch (touch.GetState(0))
            {
                case PointStateType.Down:
                    HandleTouchDown(view, touchInfo, touch);
                    break;
                    
                case PointStateType.Motion:
                    HandleTouchMove(view, touchInfo, touch);
                    break;
                    
                case PointStateType.Up:
                    HandleTouchUp(view, touchInfo, touch);
                    break;
            }
            
            return true;
        }
        
        private void HandleTouchDown(View view, TouchInfo touchInfo, Touch touch)
        {
            touchInfo.StartPosition = touch.GetLocalPosition(0);
            touchInfo.StartTime = DateTime.Now;
            touchInfo.IsPressed = true;
            
            // Visual feedback
            ApplyPressedState(view);
        }
        
        private void HandleTouchMove(View view, TouchInfo touchInfo, Touch touch)
        {
            if (!touchInfo.IsPressed) return;
            
            var currentPosition = touch.GetLocalPosition(0);
            var distance = Vector2.Distance(touchInfo.StartPosition, currentPosition);
            
            // Cancel press if moved too far
            if (distance > 20) // 20 pixel threshold
            {
                touchInfo.IsPressed = false;
                RemovePressedState(view);
            }
        }
        
        private void HandleTouchUp(View view, TouchInfo touchInfo, Touch touch)
        {
            if (touchInfo.IsPressed)
            {
                var endTime = DateTime.Now;
                var duration = endTime - touchInfo.StartTime;
                
                // Check for tap (quick press and release)
                if (duration.TotalMilliseconds < 500)
                {
                    TriggerTapEvent(view);
                }
            }
            
            touchInfo.IsPressed = false;
            RemovePressedState(view);
        }
        
        private void ApplyPressedState(View view)
        {
            // Apply visual pressed state
            var scaleAnimation = new Animation(100);
            scaleAnimation.AnimateTo(view, "Scale", new Vector3(0.95f, 0.95f, 1.0f));
            scaleAnimation.Play();
            
            // Darken background slightly
            if (view.BackgroundColor != null)
            {
                var darkerColor = new Color(
                    view.BackgroundColor.R * 0.8f,
                    view.BackgroundColor.G * 0.8f,
                    view.BackgroundColor.B * 0.8f,
                    view.BackgroundColor.A
                );
                view.BackgroundColor = darkerColor;
            }
        }
        
        private void RemovePressedState(View view)
        {
            // Remove visual pressed state
            var scaleAnimation = new Animation(100);
            scaleAnimation.AnimateTo(view, "Scale", new Vector3(1.0f, 1.0f, 1.0f));
            scaleAnimation.Play();
            
            // Restore original background color
            // (In practice, you'd store the original color)
        }
        
        private void TriggerTapEvent(View view)
        {
            // Create and trigger custom tap event
            var tapEventArgs = new TapEventArgs { View = view };
            TapEvent?.Invoke(this, tapEventArgs);
        }
        
        public event EventHandler<TapEventArgs> TapEvent;
    }
    
    public class TouchInfo
    {
        public Vector2 StartPosition { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsPressed { get; set; }
    }
    
    public class TapEventArgs : EventArgs
    {
        public View View { get; set; }
    }
}
```

## 🔄 Navigation Integration with NUI

### NUI Navigation Service Implementation

```csharp
using Tizen.NUI;
using Tizen.NUI.BaseComponents;

namespace SamsungAccountUI.Views.Navigation
{
    public class NUINavigationService : INavigationService
    {
        private readonly Stack<NavigationFrame> _navigationStack = new();
        private readonly ViewFactory _viewFactory;
        private readonly Window _mainWindow;
        private View _currentView;
        private View _navigationContainer;
        
        public bool CanNavigateBack => _navigationStack.Count > 1;
        public string CurrentScreen => _navigationStack.Count > 0 ? _navigationStack.Peek().ScreenName : string.Empty;
        
        public event EventHandler<NavigationEventArgs> NavigationChanged;
        
        public NUINavigationService(ViewFactory viewFactory, Window mainWindow)
        {
            _viewFactory = viewFactory;
            _mainWindow = mainWindow;
            
            InitializeNavigationContainer();
        }
        
        private void InitializeNavigationContainer()
        {
            _navigationContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                Layout = new AbsoluteLayout()
            };
            
            _mainWindow.Add(_navigationContainer);
        }
        
        public async Task NavigateToAsync(string screenName, object parameters = null)
        {
            try
            {
                var fromScreen = CurrentScreen;
                
                // Create new view
                var deviceType = DetectDeviceType();
                var newView = _viewFactory.CreateView(screenName, deviceType);
                
                if (newView == null)
                {
                    throw new InvalidOperationException($"Cannot create view for screen: {screenName}");
                }
                
                // Setup the new view
                newView.WidthSpecification = LayoutParamPolicies.MatchParent;
                newView.HeightSpecification = LayoutParamPolicies.MatchParent;
                
                // Add to navigation stack
                var frame = new NavigationFrame
                {
                    ScreenName = screenName,
                    View = newView,
                    Parameters = parameters
                };
                _navigationStack.Push(frame);
                
                // Perform transition
                await PerformTransition(_currentView, newView, TransitionType.SlideLeftToRight);
                
                // Update current view
                if (_currentView != null)
                {
                    _navigationContainer.Remove(_currentView);
                    _currentView.Dispose();
                }
                
                _currentView = newView;
                _navigationContainer.Add(_currentView);
                
                // Load content
                newView.LoadContent();
                
                // Initialize with parameters
                if (parameters != null && newView is IParameterizedView paramView)
                {
                    await paramView.InitializeAsync(parameters);
                }
                
                // Raise navigation event
                NavigationChanged?.Invoke(this, new NavigationEventArgs(fromScreen, screenName, parameters));
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
                var currentFrame = _navigationStack.Pop();
                var previousFrame = _navigationStack.Peek();
                
                // Recreate previous view (or restore from cache)
                var deviceType = DetectDeviceType();
                var previousView = _viewFactory.CreateView(previousFrame.ScreenName, deviceType);
                
                // Perform transition
                await PerformTransition(_currentView, previousView, TransitionType.SlideRightToLeft);
                
                // Update views
                _navigationContainer.Remove(_currentView);
                _currentView.Dispose();
                
                _currentView = previousView;
                _navigationContainer.Add(_currentView);
                
                // Load content
                previousView.LoadContent();
                
                // Dispose current frame view
                currentFrame.View?.Dispose();
                
                // Raise navigation event
                NavigationChanged?.Invoke(this, new NavigationEventArgs(currentFrame.ScreenName, previousFrame.ScreenName));
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
                // Remove current screen from stack
                if (_navigationStack.Count > 0)
                {
                    var currentFrame = _navigationStack.Pop();
                    currentFrame.View?.Dispose();
                }
                
                // Navigate to new screen
                await NavigateToAsync(screenName, parameters);
            }
            catch (Exception ex)
            {
                throw new NavigationException($"Failed to replace current screen with {screenName}: {ex.Message}", ex);
            }
        }
        
        private async Task PerformTransition(View fromView, View toView, TransitionType transitionType)
        {
            if (fromView == null)
            {
                // No transition needed for first screen
                return;
            }
            
            switch (transitionType)
            {
                case TransitionType.FadeInOut:
                    await PerformFadeTransition(fromView, toView);
                    break;
                case TransitionType.SlideLeftToRight:
                    await PerformSlideTransition(fromView, toView, SlideDirection.FromRight);
                    break;
                case TransitionType.SlideRightToLeft:
                    await PerformSlideTransition(fromView, toView, SlideDirection.FromLeft);
                    break;
            }
        }
        
        private async Task PerformFadeTransition(View fromView, View toView)
        {
            var fadeOut = AnimationHelper.CreateFadeOut(fromView, 200);
            var fadeIn = AnimationHelper.CreateFadeIn(toView, 200);
            
            var tcs = new TaskCompletionSource<bool>();
            
            fadeOut.Finished += (s, e) =>
            {
                fadeIn.Finished += (s2, e2) => tcs.SetResult(true);
                fadeIn.Play();
            };
            
            fadeOut.Play();
            await tcs.Task;
        }
        
        private async Task PerformSlideTransition(View fromView, View toView, SlideDirection direction)
        {
            var slideIn = AnimationHelper.CreateSlideIn(toView, direction, 300);
            
            var tcs = new TaskCompletionSource<bool>();
            slideIn.Finished += (s, e) => tcs.SetResult(true);
            slideIn.Play();
            
            await tcs.Task;
        }
        
        private DeviceType DetectDeviceType()
        {
            // Use device detection service to determine device type
            // This is a simplified implementation
            var windowSize = _mainWindow.Size;
            
            if (windowSize.Width > windowSize.Height)
            {
                return DeviceType.AIHome; // Horizontal layout
            }
            else
            {
                return DeviceType.FamilyHub; // Vertical layout
            }
        }
        
        public async Task ShowLoadingAsync(string message = "Loading...")
        {
            if (_currentView != null)
            {
                await _currentView.ShowLoadingAsync(message);
            }
        }
        
        public async Task HideLoadingAsync()
        {
            if (_currentView != null)
            {
                await _currentView.HideLoadingAsync();
            }
        }
        
        public async Task ShowErrorAsync(string message, string title = "Error")
        {
            if (_currentView != null)
            {
                await _currentView.ShowErrorAsync(message, title);
            }
        }
        
        public void Dispose()
        {
            // Clean up all views in stack
            while (_navigationStack.Count > 0)
            {
                var frame = _navigationStack.Pop();
                frame.View?.Dispose();
            }
            
            _currentView?.Dispose();
            _navigationContainer?.Dispose();
        }
    }
    
    public class NavigationFrame
    {
        public string ScreenName { get; set; }
        public View View { get; set; }
        public object Parameters { get; set; }
    }
}
```

## 🔧 Performance Optimization

### View Recycling and Memory Management

```csharp
public class ViewPool
{
    private readonly Dictionary<string, Queue<View>> _viewPools = new();
    private readonly int _maxPoolSize = 5;
    
    public T GetView<T>(string viewType) where T : View, new()
    {
        if (_viewPools.ContainsKey(viewType) && _viewPools[viewType].Count > 0)
        {
            return (T)_viewPools[viewType].Dequeue();
        }
        
        return new T();
    }
    
    public void ReturnView(string viewType, View view)
    {
        if (!_viewPools.ContainsKey(viewType))
        {
            _viewPools[viewType] = new Queue<View>();
        }
        
        if (_viewPools[viewType].Count < _maxPoolSize)
        {
            // Reset view state before pooling
            ResetView(view);
            _viewPools[viewType].Enqueue(view);
        }
        else
        {
            view.Dispose();
        }
    }
    
    private void ResetView(View view)
    {
        view.Opacity = 1.0f;
        view.Scale = new Vector3(1.0f, 1.0f, 1.0f);
        view.Position = Vector3.Zero;
        // Reset other properties as needed
    }
}
```

---

**Next**: [Testing and Deployment Documentation](testing-strategy.md) for comprehensive testing approach.