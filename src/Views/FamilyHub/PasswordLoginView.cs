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
using SamsungAccountUI.Utils;

namespace SamsungAccountUI.Views.FamilyHub
{
    /// <summary>
    /// Password Login view for FamilyHub devices
    /// Demonstrates form validation, input handling, and controller communication
    /// </summary>
    public class PasswordLoginView : BaseView
    {
        private LoginController _loginController;
        
        // Form UI elements
        private View _formContainer;
        private TextField _emailField;
        private TextField _passwordField;
        private TextLabel _emailErrorLabel;
        private TextLabel _passwordErrorLabel;
        private Button _signInButton;
        private Button _backButton;
        private Button _forgotPasswordButton;
        
        // Validation state
        private bool _isEmailValid = false;
        private bool _isPasswordValid = false;
        private bool _isFormSubmitting = false;
        
        // Input focus management
        private View _emailContainer;
        private View _passwordContainer;
        
        public PasswordLoginView(LoginController loginController, IGlobalConfigService configService, DeviceInfo deviceInfo) 
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
            return "Sign in with Password";
        }
        
        public override async Task LoadContentAsync()
        {
            try
            {
                // Create password login form
                CreatePasswordLoginForm();
                
                // Set focus to email field
                await Task.Delay(100); // Small delay for UI to render
                _emailField?.SetFocus(true);
                
                Console.WriteLine("Password login form loaded");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Failed to load login form: {ex.Message}");
            }
        }
        
        public override async Task<bool> OnBackPressedAsync()
        {
            // Navigate back to QR login
            RequestNavigation("QRLogin");
            return true;
        }
        
        #endregion
        
        #region Form Creation
        
        private void CreatePasswordLoginForm()
        {
            // Clear existing content
            ContentContainer.Children.Clear();
            ButtonContainer.Children.Clear();
            
            // Create form container
            CreateFormContainer();
            
            // Create input fields
            CreateEmailField();
            CreatePasswordField();
            
            // Create action buttons
            CreateActionButtons();
            
            ContentContainer.Add(_formContainer);
        }
        
        private void CreateFormContainer()
        {
            _formContainer = new View
            {
                WidthSpecification = 500, // Fixed width for FamilyHub
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(0, 25)
                },
                Padding = new Extents(40, 40, 20, 20),
                BackgroundColor = GetFormBackgroundColor(),
                CornerRadius = 16.0f,
                BoxShadow = new Shadow(8.0f, new Color(0, 0, 0, 0.1f), new Vector2(0, 4))
            };
        }
        
        private void CreateEmailField()
        {
            // Email container with border and focus states
            _emailContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    CellPadding = new Size2D(0, 8)
                }
            };
            
            // Email label
            var emailLabel = new TextLabel
            {
                Text = "Email Address",
                TextColor = GetThemeTextColor(),
                PointSize = 14,
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("medium")),
                HorizontalAlignment = HorizontalAlignment.Begin
            };
            _emailContainer.Add(emailLabel);
            
            // Email input field
            _emailField = new TextField
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = 50,
                PlaceholderText = "Enter your Samsung Account email",
                PlaceholderTextColor = GetPlaceholderColor(),
                TextColor = GetThemeTextColor(),
                PointSize = 16,
                BackgroundColor = GetInputBackgroundColor(),
                Padding = new Extents(15, 15, 12, 12),
                CornerRadius = 8.0f,
                BorderlineWidth = 2.0f,
                BorderlineColor = GetInputBorderColor(),
                InputMethodSettings = new PropertyMap()
                    .Add("PANEL_LAYOUT", new PropertyValue((int)InputMethod.PanelLayout.Email))
                    .Add("AUTO_CAPITALIZE", new PropertyValue((int)InputMethod.AutoCapital.None))
            };
            
            // Wire up email field events
            _emailField.TextChanged += OnEmailTextChanged;
            _emailField.FocusGained += OnEmailFocusGained;
            _emailField.FocusLost += OnEmailFocusLost;
            
            _emailContainer.Add(_emailField);
            
            // Email error label
            _emailErrorLabel = new TextLabel
            {
                Text = "",
                TextColor = GetErrorColor(),
                PointSize = 12,
                HorizontalAlignment = HorizontalAlignment.Begin,
                Visibility = false
            };
            _emailContainer.Add(_emailErrorLabel);
            
            _formContainer.Add(_emailContainer);
        }
        
        private void CreatePasswordField()
        {
            // Password container
            _passwordContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    CellPadding = new Size2D(0, 8)
                }
            };
            
            // Password label
            var passwordLabel = new TextLabel
            {
                Text = "Password",
                TextColor = GetThemeTextColor(),
                PointSize = 14,
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("medium")),
                HorizontalAlignment = HorizontalAlignment.Begin
            };
            _passwordContainer.Add(passwordLabel);
            
            // Password input field
            _passwordField = new TextField
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = 50,
                PlaceholderText = "Enter your password",
                PlaceholderTextColor = GetPlaceholderColor(),
                TextColor = GetThemeTextColor(),
                PointSize = 16,
                BackgroundColor = GetInputBackgroundColor(),
                Padding = new Extents(15, 15, 12, 12),
                CornerRadius = 8.0f,
                BorderlineWidth = 2.0f,
                BorderlineColor = GetInputBorderColor(),
                HiddenInputSettings = new PropertyMap()
                    .Add("MODE", new PropertyValue((int)HiddenInput.Mode.ShowLastCharacter))
                    .Add("SHOW_LAST_CHARACTER_DURATION", new PropertyValue(1000))
            };
            
            // Wire up password field events
            _passwordField.TextChanged += OnPasswordTextChanged;
            _passwordField.FocusGained += OnPasswordFocusGained;
            _passwordField.FocusLost += OnPasswordFocusLost;
            
            _passwordContainer.Add(_passwordField);
            
            // Password error label
            _passwordErrorLabel = new TextLabel
            {
                Text = "",
                TextColor = GetErrorColor(),
                PointSize = 12,
                HorizontalAlignment = HorizontalAlignment.Begin,
                Visibility = false
            };
            _passwordContainer.Add(_passwordErrorLabel);
            
            _formContainer.Add(_passwordContainer);
        }
        
        private void CreateActionButtons()
        {
            // Sign in button
            _signInButton = CreateStyledButton(
                "Sign In", 
                new Size(200, 50), 
                isPrimary: true
            );
            _signInButton.IsEnabled = false; // Initially disabled
            _signInButton.Clicked += OnSignInClicked;
            
            // Back button
            _backButton = CreateStyledButton(
                "Back to QR", 
                new Size(150, 45), 
                isPrimary: false
            );
            _backButton.Clicked += OnBackClicked;
            
            // Forgot password button
            _forgotPasswordButton = CreateStyledButton(
                "Forgot Password?", 
                new Size(180, 40), 
                isPrimary: false
            );
            _forgotPasswordButton.PointSize = 14;
            _forgotPasswordButton.Clicked += OnForgotPasswordClicked;
            
            // Arrange buttons
            ButtonContainer.Layout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Horizontal,
                LinearAlignment = LinearLayout.Alignment.Center,
                CellPadding = new Size2D(20, 0)
            };
            
            ButtonContainer.Add(_backButton);
            ButtonContainer.Add(_signInButton);
            
            // Add forgot password link below main buttons
            var forgotContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = 60,
                Layout = new LinearLayout
                {
                    LinearAlignment = LinearLayout.Alignment.Center
                }
            };
            forgotContainer.Add(_forgotPasswordButton);
            
            _formContainer.Add(forgotContainer);
        }
        
        #endregion
        
        #region Validation Logic
        
        private void OnEmailTextChanged(object sender, TextField.TextChangedEventArgs e)
        {
            var email = e.TextField.Text?.Trim() ?? "";
            ValidateEmail(email);
            UpdateFormState();
        }
        
        private void OnPasswordTextChanged(object sender, TextField.TextChangedEventArgs e)
        {
            var password = e.TextField.Text ?? "";
            ValidatePassword(password);
            UpdateFormState();
        }
        
        private void ValidateEmail(string email)
        {
            var validation = ValidationHelper.ValidateEmail(email);
            _isEmailValid = validation.IsValid;
            
            if (!validation.IsValid && !string.IsNullOrEmpty(email))
            {
                ShowEmailError(validation.ErrorMessage);
            }
            else
            {
                HideEmailError();
            }
        }
        
        private void ValidatePassword(string password)
        {
            var validation = ValidationHelper.ValidatePassword(password);
            _isPasswordValid = validation.IsValid;
            
            if (!validation.IsValid && !string.IsNullOrEmpty(password))
            {
                ShowPasswordError(validation.ErrorMessage);
            }
            else
            {
                HidePasswordError();
            }
        }
        
        private void UpdateFormState()
        {
            // Enable sign in button only if both fields are valid
            _signInButton.IsEnabled = _isEmailValid && _isPasswordValid && !_isFormSubmitting;
            
            // Update button appearance
            if (_signInButton.IsEnabled)
            {
                _signInButton.BackgroundColor = GetPrimaryButtonColor();
                _signInButton.Opacity = 1.0f;
            }
            else
            {
                _signInButton.BackgroundColor = GetDisabledButtonColor();
                _signInButton.Opacity = 0.6f;
            }
        }
        
        private void ShowEmailError(string message)
        {
            _emailErrorLabel.Text = message;
            _emailErrorLabel.Visibility = true;
            _emailField.BorderlineColor = GetErrorColor();
            
            // Animate error appearance
            var animation = new Animation(200);
            animation.AnimateTo(_emailErrorLabel, "Opacity", 1.0f);
            animation.Play();
        }
        
        private void HideEmailError()
        {
            _emailErrorLabel.Visibility = false;
            _emailField.BorderlineColor = GetInputBorderColor();
        }
        
        private void ShowPasswordError(string message)
        {
            _passwordErrorLabel.Text = message;
            _passwordErrorLabel.Visibility = true;
            _passwordField.BorderlineColor = GetErrorColor();
            
            // Animate error appearance
            var animation = new Animation(200);
            animation.AnimateTo(_passwordErrorLabel, "Opacity", 1.0f);
            animation.Play();
        }
        
        private void HidePasswordError()
        {
            _passwordErrorLabel.Visibility = false;
            _passwordField.BorderlineColor = GetInputBorderColor();
        }
        
        #endregion
        
        #region Focus Management
        
        private void OnEmailFocusGained(object sender, EventArgs e)
        {
            // Highlight email field
            _emailField.BorderlineColor = GetFocusColor();
            _emailField.BorderlineWidth = 3.0f;
        }
        
        private void OnEmailFocusLost(object sender, EventArgs e)
        {
            // Restore normal border
            _emailField.BorderlineWidth = 2.0f;
            if (_isEmailValid || string.IsNullOrEmpty(_emailField.Text))
            {
                _emailField.BorderlineColor = GetInputBorderColor();
            }
        }
        
        private void OnPasswordFocusGained(object sender, EventArgs e)
        {
            // Highlight password field
            _passwordField.BorderlineColor = GetFocusColor();
            _passwordField.BorderlineWidth = 3.0f;
        }
        
        private void OnPasswordFocusLost(object sender, EventArgs e)
        {
            // Restore normal border
            _passwordField.BorderlineWidth = 2.0f;
            if (_isPasswordValid || string.IsNullOrEmpty(_passwordField.Text))
            {
                _passwordField.BorderlineColor = GetInputBorderColor();
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private async void OnSignInClicked(object sender, ClickedEventArgs e)
        {
            if (!_isEmailValid || !_isPasswordValid || _isFormSubmitting)
                return;
            
            try
            {
                _isFormSubmitting = true;
                UpdateFormState();
                
                var email = _emailField.Text?.Trim();
                var password = _passwordField.Text;
                
                Console.WriteLine($"Attempting password login for: {email}");
                
                // Send login request to controller
                var loginData = new
                {
                    Email = email,
                    Password = password,
                    DeviceInfo = DeviceInfo
                };
                
                SendActionToController(loginData);
                
                // Controller will trigger login through events
                await _loginController.HandlePasswordLoginAsync(email, password);
            }
            catch (Exception ex)
            {
                _isFormSubmitting = false;
                UpdateFormState();
                await ShowErrorAsync($"Login failed: {ex.Message}");
            }
        }
        
        private void OnBackClicked(object sender, ClickedEventArgs e)
        {
            RequestNavigation("QRLogin");
        }
        
        private void OnForgotPasswordClicked(object sender, ClickedEventArgs e)
        {
            // Navigate to forgot password or show help
            RequestNavigation("ForgotPassword");
        }
        
        private async void OnLoginStarted(object sender, EventArgs e)
        {
            await ShowLoadingAsync("Signing in...");
        }
        
        private async void OnLoginCompleted(object sender, LoginResult result)
        {
            _isFormSubmitting = false;
            await HideLoadingAsync();
            
            if (result.IsSuccess)
            {
                // Clear form for security
                ClearForm();
                
                // Navigate to account info
                RequestNavigation("AccountInfo");
                
                Console.WriteLine($"Password login successful for: {result.User.DisplayName}");
            }
            else
            {
                UpdateFormState();
                await ShowErrorAsync(result.ErrorMessage, "Try Again");
                
                // Focus back to email field for retry
                _emailField?.SetFocus(true);
            }
        }
        
        private async void OnLoginFailed(object sender, string errorMessage)
        {
            _isFormSubmitting = false;
            await HideLoadingAsync();
            UpdateFormState();
            
            await ShowErrorAsync(errorMessage, "Try Again");
            
            // Focus back to email field for retry
            _emailField?.SetFocus(true);
        }
        
        #endregion
        
        #region Helper Methods
        
        private void ClearForm()
        {
            _emailField.Text = "";
            _passwordField.Text = "";
            _isEmailValid = false;
            _isPasswordValid = false;
            HideEmailError();
            HidePasswordError();
            UpdateFormState();
        }
        
        private Color GetFormBackgroundColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? new Color(0.15f, 0.15f, 0.15f, 1.0f) : new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        
        private Color GetThemeTextColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? Color.White : Color.Black;
        }
        
        private Color GetInputBackgroundColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? new Color(0.2f, 0.2f, 0.2f, 1.0f) : new Color(0.98f, 0.98f, 0.98f, 1.0f);
        }
        
        private Color GetInputBorderColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? new Color(0.4f, 0.4f, 0.4f, 1.0f) : new Color(0.8f, 0.8f, 0.8f, 1.0f);
        }
        
        private Color GetFocusColor()
        {
            return new Color(0.2f, 0.6f, 1.0f, 1.0f); // Samsung blue
        }
        
        private Color GetErrorColor()
        {
            return new Color(0.9f, 0.3f, 0.3f, 1.0f); // Red
        }
        
        private Color GetPlaceholderColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? new Color(0.6f, 0.6f, 0.6f, 1.0f) : new Color(0.5f, 0.5f, 0.5f, 1.0f);
        }
        
        private Color GetPrimaryButtonColor()
        {
            return new Color(0.2f, 0.6f, 1.0f, 1.0f); // Samsung blue
        }
        
        private Color GetDisabledButtonColor()
        {
            return new Color(0.6f, 0.6f, 0.6f, 1.0f); // Gray
        }
        
        #endregion
        
        #region Lifecycle and Cleanup
        
        public override async Task OnAppearingAsync()
        {
            await base.OnAppearingAsync();
            
            // Set focus to email field when view appears
            await Task.Delay(200);
            _emailField?.SetFocus(true);
        }
        
        protected override void Dispose(DisposeTypes type)
        {
            if (type == DisposeTypes.Explicit)
            {
                // Unsubscribe from controller events
                if (_loginController != null)
                {
                    _loginController.LoginStarted -= OnLoginStarted;
                    _loginController.LoginCompleted -= OnLoginCompleted;
                    _loginController.LoginFailed -= OnLoginFailed;
                }
                
                // Dispose UI elements
                _formContainer?.Dispose();
                _emailField?.Dispose();
                _passwordField?.Dispose();
                _emailErrorLabel?.Dispose();
                _passwordErrorLabel?.Dispose();
                _signInButton?.Dispose();
                _backButton?.Dispose();
                _forgotPasswordButton?.Dispose();
                _emailContainer?.Dispose();
                _passwordContainer?.Dispose();
            }
            
            base.Dispose(type);
        }
        
        #endregion
    }
}