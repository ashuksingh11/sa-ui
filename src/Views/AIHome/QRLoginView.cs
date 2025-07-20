using System;
using System.Threading.Tasks;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using SamsungAccountUI.Views.Common;
using SamsungAccountUI.Controllers.Base;
using SamsungAccountUI.Controllers.Authentication;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Models.Authentication;

namespace SamsungAccountUI.Views.AIHome
{
    /// <summary>
    /// QR Login view optimized for AIHome devices (7", 9" horizontal displays)
    /// Shows compact QR code with horizontal layout and minimal UI elements
    /// </summary>
    public class QRLoginView : BaseView
    {
        private LoginController _loginController;
        
        // Main layout containers
        private View _leftPanel;  // QR code side
        private View _rightPanel; // Info and buttons side
        
        // QR Code UI elements
        private View _qrContainer;
        private ImageView _qrCodeImage;
        private TextLabel _qrStatusIndicator;
        
        // Info and button elements
        private TextLabel _titleText;
        private TextLabel _instructionText;
        private View _buttonPanel;
        private Button _passwordLoginButton;
        private Button _googleLoginButton;
        
        // Timer for QR code refresh
        private Timer _qrRefreshTimer;
        private string _currentQRToken;
        
        public QRLoginView(LoginController loginController, IGlobalConfigService configService, DeviceInfo deviceInfo) 
            : base(loginController, configService, deviceInfo)
        {
            _loginController = loginController;
            
            // Wire up controller events
            _loginController.LoginStarted += OnLoginStarted;
            _loginController.LoginCompleted += OnLoginCompleted;
            _loginController.LoginFailed += OnLoginFailed;
        }
        
        #region BaseView Implementation
        
        protected override string GetViewTitle()
        {
            return "Samsung Account"; // Shorter title for compact display
        }
        
        public override async Task LoadContentAsync()
        {
            try
            {
                await ShowLoadingAsync("Loading...");
                
                // Create AIHome-specific compact layout
                CreateAIHomeCompactLayout();
                
                // Generate initial QR code
                await RefreshQRCodeAsync();
                
                // Start QR refresh timer (refresh every 90 seconds for AIHome)
                StartQRRefreshTimer();
                
                await HideLoadingAsync();
            }
            catch (Exception ex)
            {
                await HideLoadingAsync();
                await ShowErrorAsync($"Load failed: {ex.Message}");
            }
        }
        
        public override async Task<bool> OnBackPressedAsync()
        {
            // For QR login (entry point), back button should minimize or exit app
            return false;
        }
        
        #endregion
        
        #region AIHome-Specific Compact Layout Creation
        
        private void CreateAIHomeCompactLayout()
        {
            // Clear any existing content
            ContentContainer.Children.Clear();
            ButtonContainer.Children.Clear();
            
            // Set up horizontal layout for compact display
            ContentContainer.Layout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Horizontal,
                LinearAlignment = LinearLayout.Alignment.Center,
                CellPadding = new Size2D(20, 0)
            };
            
            // Create left panel (QR code)
            CreateLeftPanel();
            
            // Create right panel (info and buttons)
            CreateRightPanel();
            
            ContentContainer.Add(_leftPanel);
            ContentContainer.Add(_rightPanel);
        }
        
        private void CreateLeftPanel()
        {
            _leftPanel = new View
            {
                Size = new Size(200, 200), // Compact QR area
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(0, 10)
                }
            };
            
            // Compact QR container
            _qrContainer = new View
            {
                Size = new Size(160, 160),
                BackgroundColor = Color.White,
                CornerRadius = 8.0f,
                Layout = new LinearLayout
                {
                    LinearAlignment = LinearLayout.Alignment.Center
                }
            };
            
            _qrCodeImage = new ImageView
            {
                Size = new Size(140, 140),
                ResourceUrl = "images/qr_placeholder_small.png",
                FittingMode = FittingModeType.ScaleToFill
            };
            
            _qrContainer.Add(_qrCodeImage);
            _leftPanel.Add(_qrContainer);
            
            // Compact status indicator
            _qrStatusIndicator = new View
            {
                Size = new Size(12, 12),
                BackgroundColor = new Color(0.2f, 0.8f, 0.2f, 1.0f), // Green dot
                CornerRadius = 6.0f
            };
            
            _leftPanel.Add(_qrStatusIndicator);
        }
        
        private void CreateRightPanel()
        {
            _rightPanel = new View
            {
                WidthSpecification = LayoutParamPolicies.FillToParent,
                HeightSpecification = 200,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(0, 15)
                },
                Padding = new Extents(20, 0, 0, 0)
            };
            
            // Compact title
            _titleText = new TextLabel
            {
                Text = "Sign In",
                TextColor = GetThemeTextColor(),
                PointSize = 16,
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold")),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _rightPanel.Add(_titleText);
            
            // Compact instruction
            _instructionText = new TextLabel
            {
                Text = "Scan QR with mobile app or use alternative login",
                TextColor = GetThemeTextColor(),
                PointSize = 11,
                HorizontalAlignment = HorizontalAlignment.Left,
                MultiLine = true,
                WidthSpecification = 200,
                LineSpacing = 1.1f,
                Opacity = 0.8f
            };
            _rightPanel.Add(_instructionText);
            
            // Compact button panel
            CreateCompactButtonPanel();
            _rightPanel.Add(_buttonPanel);
        }
        
        private void CreateCompactButtonPanel()
        {
            _buttonPanel = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = 80,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Start,
                    CellPadding = new Size2D(0, 8)
                }
            };
            
            // Compact password login button
            if (ConfigService.IsPasswordLoginEnabled)
            {
                _passwordLoginButton = CreateStyledButton(
                    "Password", 
                    new Size(120, 30), 
                    isPrimary: false
                );
                _passwordLoginButton.PointSize = 11; // Smaller text for compact
                _passwordLoginButton.Clicked += OnPasswordLoginClicked;
                _buttonPanel.Add(_passwordLoginButton);
            }
            
            // Compact Google login button
            if (ConfigService.IsGoogleLoginEnabled)
            {
                _googleLoginButton = CreateStyledButton(
                    "Google", 
                    new Size(120, 30), 
                    isPrimary: false
                );
                _googleLoginButton.PointSize = 11; // Smaller text for compact
                _googleLoginButton.Clicked += OnGoogleLoginClicked;
                _buttonPanel.Add(_googleLoginButton);
            }
        }
        
        #endregion
        
        #region QR Code Management
        
        private async Task RefreshQRCodeAsync()
        {
            try
            {
                // Update status indicator to orange (generating)
                _qrStatusIndicator.BackgroundColor = new Color(0.8f, 0.6f, 0.2f, 1.0f);
                
                // Generate new QR token
                _currentQRToken = await GenerateQRTokenAsync();
                
                // Update QR code image
                await UpdateQRCodeImageAsync(_currentQRToken);
                
                // Update status indicator to green (active)
                _qrStatusIndicator.BackgroundColor = new Color(0.2f, 0.8f, 0.2f, 1.0f);
                
                Console.WriteLine($"AIHome QR Code refreshed: {_currentQRToken}");
            }
            catch (Exception ex)
            {
                // Update status indicator to red (error)
                _qrStatusIndicator.BackgroundColor = new Color(0.8f, 0.2f, 0.2f, 1.0f);
                Console.WriteLine($"AIHome QR refresh failed: {ex.Message}");
            }
        }
        
        private async Task<string> GenerateQRTokenAsync()
        {
            // Simulate API call - shorter for AIHome
            await Task.Delay(500);
            return $"AH_{Guid.NewGuid().ToString("N")[..6]}";
        }
        
        private async Task UpdateQRCodeImageAsync(string qrToken)
        {
            await Task.Delay(300);
            
            // Update QR image resource
            _qrCodeImage.ResourceUrl = $"images/qr_aihome_{qrToken.Substring(0, 6)}.png";
            
            // Subtle fade animation for compact display
            var animation = new Animation(300);
            animation.AnimateTo(_qrCodeImage, "Opacity", 0.5f, 0, 150);
            animation.AnimateTo(_qrCodeImage, "Opacity", 1.0f, 150, 150);
            animation.Play();
        }
        
        private void StartQRRefreshTimer()
        {
            _qrRefreshTimer = new Timer(90000); // 90 seconds for AIHome
            _qrRefreshTimer.Tick += async (sender, e) =>
            {
                await RefreshQRCodeAsync();
                return true;
            };
            _qrRefreshTimer.Start();
        }
        
        #endregion
        
        #region Event Handlers
        
        private async void OnPasswordLoginClicked(object sender, ClickedEventArgs e)
        {
            try
            {
                _passwordLoginButton.IsEnabled = false;
                RequestNavigation("PasswordLogin");
                Console.WriteLine("AIHome: Navigating to Password Login");
            }
            finally
            {
                _passwordLoginButton.IsEnabled = true;
            }
        }
        
        private async void OnGoogleLoginClicked(object sender, ClickedEventArgs e)
        {
            try
            {
                _googleLoginButton.IsEnabled = false;
                RequestNavigation("GoogleLogin");
                Console.WriteLine("AIHome: Navigating to Google Login");
            }
            finally
            {
                _googleLoginButton.IsEnabled = true;
            }
        }
        
        private async void OnLoginStarted(object sender, EventArgs e)
        {
            await ShowLoadingAsync("Signing in...");
            
            // Update status indicator to blue (processing)
            _qrStatusIndicator.BackgroundColor = new Color(0.2f, 0.6f, 1.0f, 1.0f);
            
            // Disable buttons
            if (_passwordLoginButton != null)
                _passwordLoginButton.IsEnabled = false;
            if (_googleLoginButton != null)
                _googleLoginButton.IsEnabled = false;
        }
        
        private async void OnLoginCompleted(object sender, LoginResult result)
        {
            await HideLoadingAsync();
            
            if (result.IsSuccess)
            {
                // Stop QR refresh timer
                _qrRefreshTimer?.Stop();
                
                // Update status to success (brighter green)
                _qrStatusIndicator.BackgroundColor = new Color(0.1f, 1.0f, 0.1f, 1.0f);
                
                // Navigate to account info
                RequestNavigation("AccountInfo");
                
                Console.WriteLine($"AIHome login successful: {result.User.DisplayName}");
            }
            else
            {
                // Update status to error
                _qrStatusIndicator.BackgroundColor = new Color(0.8f, 0.2f, 0.2f, 1.0f);
                
                await ShowErrorAsync(result.ErrorMessage, "Retry");
                
                // Re-enable buttons
                RestoreButtonStates();
            }
        }
        
        private async void OnLoginFailed(object sender, string errorMessage)
        {
            await HideLoadingAsync();
            
            // Update status to error
            _qrStatusIndicator.BackgroundColor = new Color(0.8f, 0.2f, 0.2f, 1.0f);
            
            await ShowErrorAsync(errorMessage, "Retry");
            
            // Re-enable buttons
            RestoreButtonStates();
        }
        
        private void RestoreButtonStates()
        {
            if (_passwordLoginButton != null)
                _passwordLoginButton.IsEnabled = true;
            if (_googleLoginButton != null)
                _googleLoginButton.IsEnabled = true;
            
            // Restore QR status to active
            _qrStatusIndicator.BackgroundColor = new Color(0.2f, 0.8f, 0.2f, 1.0f);
        }
        
        #endregion
        
        #region AIHome-Specific Features
        
        /// <summary>
        /// Handle device-specific touch interactions for AIHome
        /// </summary>
        public void HandleQuickTouch()
        {
            // AIHome devices might have quick-touch features
            // Animate the QR code briefly to show interaction
            var pulseAnimation = new Animation(1000);
            pulseAnimation.AnimateTo(_qrContainer, "Scale", new Vector3(1.1f, 1.1f, 1.0f), 0, 500);
            pulseAnimation.AnimateTo(_qrContainer, "Scale", new Vector3(1.0f, 1.0f, 1.0f), 500, 500);
            pulseAnimation.Play();
        }
        
        /// <summary>
        /// Optimize for AIHome's lower processing power
        /// </summary>
        private void OptimizeForAIHome()
        {
            // Reduce animation complexity
            // Minimize background processes
            // Use lower resolution assets if needed
            Console.WriteLine("AIHome optimizations applied");
        }
        
        /// <summary>
        /// Handle AIHome-specific power saving mode
        /// </summary>
        public void EnterPowerSaveMode()
        {
            // Slow down QR refresh rate
            _qrRefreshTimer?.Stop();
            _qrRefreshTimer = new Timer(180000); // 3 minutes in power save
            _qrRefreshTimer.Tick += async (sender, e) =>
            {
                await RefreshQRCodeAsync();
                return true;
            };
            _qrRefreshTimer.Start();
            
            // Dim the display slightly
            Opacity = 0.9f;
            
            Console.WriteLine("AIHome: Entered power save mode");
        }
        
        public void ExitPowerSaveMode()
        {
            // Restore normal QR refresh rate
            _qrRefreshTimer?.Stop();
            StartQRRefreshTimer();
            
            // Restore full brightness
            Opacity = 1.0f;
            
            Console.WriteLine("AIHome: Exited power save mode");
        }
        
        #endregion
        
        #region QR Code Scanning Simulation
        
        /// <summary>
        /// Simulate QR code scanning for AIHome device
        /// </summary>
        public async Task SimulateQRScanAsync()
        {
            if (string.IsNullOrEmpty(_currentQRToken))
            {
                Console.WriteLine("AIHome: No QR token available");
                return;
            }
            
            Console.WriteLine($"AIHome: Simulating QR scan for token: {_currentQRToken}");
            
            // Update status to scanning (blue)
            _qrStatusIndicator.BackgroundColor = new Color(0.2f, 0.6f, 1.0f, 1.0f);
            
            // Brief scanning animation
            var scanAnimation = new Animation(1500);
            scanAnimation.AnimateTo(_qrCodeImage, "Opacity", 0.7f, 0, 750);
            scanAnimation.AnimateTo(_qrCodeImage, "Opacity", 1.0f, 750, 750);
            scanAnimation.Play();
            
            // Simulate shorter processing time for AIHome
            await Task.Delay(1500);
            
            // Trigger login
            await _loginController.HandleQRLoginAsync(_currentQRToken);
        }
        
        #endregion
        
        #region Helper Methods
        
        private Color GetThemeTextColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? Color.White : Color.Black;
        }
        
        #endregion
        
        #region Lifecycle and Cleanup
        
        public override async Task OnAppearingAsync()
        {
            await base.OnAppearingAsync();
            
            // Apply AIHome optimizations
            OptimizeForAIHome();
            
            Console.WriteLine("AIHome QR Login view ready");
        }
        
        public override async Task OnDisappearingAsync()
        {
            await base.OnDisappearingAsync();
            
            // Stop QR refresh timer
            _qrRefreshTimer?.Stop();
        }
        
        protected override void Dispose(DisposeTypes type)
        {
            if (type == DisposeTypes.Explicit)
            {
                // Stop timer
                _qrRefreshTimer?.Stop();
                _qrRefreshTimer?.Dispose();
                
                // Unsubscribe from controller events
                if (_loginController != null)
                {
                    _loginController.LoginStarted -= OnLoginStarted;
                    _loginController.LoginCompleted -= OnLoginCompleted;
                    _loginController.LoginFailed -= OnLoginFailed;
                }
                
                // Dispose UI elements
                _leftPanel?.Dispose();
                _rightPanel?.Dispose();
                _qrContainer?.Dispose();
                _qrCodeImage?.Dispose();
                _qrStatusIndicator?.Dispose();
                _titleText?.Dispose();
                _instructionText?.Dispose();
                _buttonPanel?.Dispose();
                _passwordLoginButton?.Dispose();
                _googleLoginButton?.Dispose();
            }
            
            base.Dispose(type);
        }
        
        #endregion
    }
}