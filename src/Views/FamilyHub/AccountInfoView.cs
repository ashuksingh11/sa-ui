using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using SamsungAccountUI.Views.Common;
using SamsungAccountUI.Controllers.Base;
using SamsungAccountUI.Controllers.Account;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Models.Device;
using SamsungAccountUI.Models.User;

namespace SamsungAccountUI.Views.FamilyHub
{
    /// <summary>
    /// Account Info view for FamilyHub devices
    /// Shows active user, all logged-in users, and user management options
    /// Demonstrates dynamic content updates and controller-view communication
    /// </summary>
    public class AccountInfoView : BaseView
    {
        private AccountInfoController _accountController;
        
        // Main sections
        private View _activeUserSection;
        private View _otherUsersSection;
        private View _actionButtonsSection;
        
        // Active user elements
        private ImageView _activeUserAvatar;
        private TextLabel _activeUserName;
        private TextLabel _activeUserEmail;
        private TextLabel _activeUserStatus;
        private View _activeUserBadge;
        
        // Other users list
        private ScrollableBase _usersScrollView;
        private View _usersContainer;
        private List<View> _userCards = new List<View>();
        
        // Action buttons
        private Button _logoutButton;
        private Button _addUserButton;
        private Button _settingsButton;
        private Button _refreshButton;
        
        // Data
        private AccountState _accountState;
        
        public AccountInfoView(AccountInfoController accountController, IGlobalConfigService configService, DeviceInfo deviceInfo) 
            : base(accountController, configService, deviceInfo)
        {
            _accountController = accountController;
            
            // Wire up controller events
            _accountController.AccountStateChanged += OnAccountStateChanged;
            _accountController.UserSwitched += OnUserSwitched;
            _accountController.UserLoggedOut += OnUserLoggedOut;
            _accountController.UserAdded += OnUserAdded;
        }
        
        #region BaseView Implementation
        
        protected override string GetViewTitle()
        {
            return "Samsung Account";
        }
        
        public override async Task LoadContentAsync()
        {
            try
            {
                await ShowLoadingAsync("Loading account information...");
                
                // Load account data from controller
                await _accountController.LoadAccountInfoAsync();
                
                // Create account info layout
                CreateAccountInfoLayout();
                
                await HideLoadingAsync();
            }
            catch (Exception ex)
            {
                await HideLoadingAsync();
                await ShowErrorAsync($"Failed to load account info: {ex.Message}");
            }
        }
        
        public override async Task<bool> OnBackPressedAsync()
        {
            // For account info (main screen), back button minimizes app
            return false;
        }
        
        #endregion
        
        #region Layout Creation
        
        private void CreateAccountInfoLayout()
        {
            // Clear existing content
            ContentContainer.Children.Clear();
            ButtonContainer.Children.Clear();
            
            // Set up scrollable container for large content
            var scrollView = new ScrollableBase
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.FillToParent,
                ScrollingDirection = ScrollableBase.Direction.Vertical,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    CellPadding = new Size2D(0, 30)
                }
            };
            
            // Create sections
            CreateActiveUserSection();
            CreateOtherUsersSection();
            CreateActionButtonsSection();
            
            scrollView.Add(_activeUserSection);
            scrollView.Add(_otherUsersSection);
            
            ContentContainer.Add(scrollView);
            ButtonContainer.Add(_actionButtonsSection);
        }
        
        private void CreateActiveUserSection()
        {
            _activeUserSection = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    LinearAlignment = LinearLayout.Alignment.CenterVertical,
                    CellPadding = new Size2D(25, 0)
                },
                Padding = new Extents(30, 30, 20, 20),
                BackgroundColor = GetActiveUserBackgroundColor(),
                CornerRadius = 16.0f,
                BoxShadow = new Shadow(6.0f, new Color(0, 0, 0, 0.1f), new Vector2(0, 3))
            };
            
            // Avatar section
            CreateActiveUserAvatar();
            
            // Info section
            CreateActiveUserInfo();
            
            // Status badge
            CreateActiveUserBadge();
        }
        
        private void CreateActiveUserAvatar()
        {
            var avatarContainer = new View
            {
                Size = new Size(100, 100),
                Layout = new LinearLayout
                {
                    LinearAlignment = LinearLayout.Alignment.Center
                }
            };
            
            _activeUserAvatar = new ImageView
            {
                Size = new Size(90, 90),
                ResourceUrl = "images/default_avatar_large.png",
                CornerRadius = 45.0f,
                BorderlineWidth = 3.0f,
                BorderlineColor = GetPrimaryColor()
            };
            
            avatarContainer.Add(_activeUserAvatar);
            _activeUserSection.Add(avatarContainer);
        }
        
        private void CreateActiveUserInfo()
        {
            var infoContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.FillToParent,
                HeightSpecification = 100,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.CenterVertical,
                    CellPadding = new Size2D(0, 8)
                }
            };
            
            _activeUserName = new TextLabel
            {
                Text = "Loading...",
                TextColor = GetThemeTextColor(),
                PointSize = 20,
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold")),
                HorizontalAlignment = HorizontalAlignment.Begin
            };
            infoContainer.Add(_activeUserName);
            
            _activeUserEmail = new TextLabel
            {
                Text = "",
                TextColor = GetThemeTextColor(),
                PointSize = 14,
                HorizontalAlignment = HorizontalAlignment.Begin,
                Opacity = 0.8f
            };
            infoContainer.Add(_activeUserEmail);
            
            _activeUserStatus = new TextLabel
            {
                Text = "Active User",
                TextColor = GetSuccessColor(),
                PointSize = 12,
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("medium")),
                HorizontalAlignment = HorizontalAlignment.Begin
            };
            infoContainer.Add(_activeUserStatus);
            
            _activeUserSection.Add(infoContainer);
        }
        
        private void CreateActiveUserBadge()
        {
            _activeUserBadge = new View
            {
                Size = new Size(20, 20),
                BackgroundColor = GetSuccessColor(),
                CornerRadius = 10.0f
            };
            
            _activeUserSection.Add(_activeUserBadge);
        }
        
        private void CreateOtherUsersSection()
        {
            _otherUsersSection = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    CellPadding = new Size2D(0, 15)
                }
            };
            
            // Section header
            var headerContainer = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = 40,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    LinearAlignment = LinearLayout.Alignment.CenterVertical
                },
                Padding = new Extents(10, 10, 0, 0)
            };
            
            var headerText = new TextLabel
            {
                Text = "Other Users",
                TextColor = GetThemeTextColor(),
                PointSize = 16,
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold")),
                HorizontalAlignment = HorizontalAlignment.Begin
            };
            headerContainer.Add(headerText);
            
            _otherUsersSection.Add(headerContainer);
            
            // Users scroll view
            CreateUsersScrollView();
            _otherUsersSection.Add(_usersScrollView);
        }
        
        private void CreateUsersScrollView()
        {
            _usersScrollView = new ScrollableBase
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = 200, // Fixed height for other users
                ScrollingDirection = ScrollableBase.Direction.Horizontal,
                Padding = new Extents(10, 10, 0, 0)
            };
            
            _usersContainer = new View
            {
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    CellPadding = new Size2D(15, 0)
                }
            };
            
            _usersScrollView.Add(_usersContainer);
        }
        
        private void CreateActionButtonsSection()
        {
            _actionButtonsSection = new View
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.WrapContent,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(20, 0)
                }
            };
            
            // Refresh button
            _refreshButton = CreateStyledButton("Refresh", new Size(120, 45), false);
            _refreshButton.Clicked += OnRefreshClicked;
            _actionButtonsSection.Add(_refreshButton);
            
            // Add user button
            if (ConfigService.IsMultiUserEnabled)
            {
                _addUserButton = CreateStyledButton("Add User", new Size(140, 45), false);
                _addUserButton.Clicked += OnAddUserClicked;
                _actionButtonsSection.Add(_addUserButton);
            }
            
            // Settings button
            _settingsButton = CreateStyledButton("Settings", new Size(120, 45), false);
            _settingsButton.Clicked += OnSettingsClicked;
            _actionButtonsSection.Add(_settingsButton);
            
            // Logout button
            _logoutButton = CreateStyledButton("Logout", new Size(120, 45), true);
            _logoutButton.BackgroundColor = GetDangerColor();
            _logoutButton.Clicked += OnLogoutClicked;
            _actionButtonsSection.Add(_logoutButton);
        }
        
        #endregion
        
        #region User Cards Management
        
        private void UpdateUserCards()
        {
            // Clear existing user cards
            ClearUserCards();
            
            if (_accountState?.AllAccounts == null)
                return;
            
            // Create cards for non-active users
            var otherUsers = _accountState.AllAccounts
                .Where(u => !u.IsActiveUser)
                .ToList();
            
            foreach (var user in otherUsers)
            {
                var userCard = CreateUserCard(user);
                _userCards.Add(userCard);
                _usersContainer.Add(userCard);
            }
            
            // Show/hide section based on other users
            _otherUsersSection.Visibility = otherUsers.Any();
        }
        
        private View CreateUserCard(SamsungAccount user)
        {
            var card = new View
            {
                Size = new Size(150, 180),
                BackgroundColor = GetCardBackgroundColor(),
                CornerRadius = 12.0f,
                BoxShadow = new Shadow(4.0f, new Color(0, 0, 0, 0.1f), new Vector2(0, 2)),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(0, 10)
                },
                Padding = new Extents(15, 15, 15, 15)
            };
            
            // User avatar
            var avatar = new ImageView
            {
                Size = new Size(60, 60),
                ResourceUrl = user.ProfilePictureUrl ?? "images/default_avatar.png",
                CornerRadius = 30.0f
            };
            card.Add(avatar);
            
            // User name
            var nameLabel = new TextLabel
            {
                Text = user.DisplayName,
                TextColor = GetThemeTextColor(),
                PointSize = 14,
                FontStyle = new PropertyMap().Add("weight", new PropertyValue("medium")),
                HorizontalAlignment = HorizontalAlignment.Center,
                MultiLine = true,
                WidthSpecification = 120
            };
            card.Add(nameLabel);
            
            // Switch button
            var switchButton = CreateStyledButton("Switch", new Size(100, 30), true);
            switchButton.PointSize = 11;
            switchButton.Clicked += async (sender, e) => await OnUserSwitchClicked(user.UserId);
            card.Add(switchButton);
            
            // Add touch event for card interaction
            card.TouchEvent += (source, e) =>
            {
                if (e.Touch.GetState(0) == PointStateType.Down)
                {
                    // Highlight card on touch
                    card.BackgroundColor = GetCardHighlightColor();
                }
                else if (e.Touch.GetState(0) == PointStateType.Up)
                {
                    // Restore normal color
                    card.BackgroundColor = GetCardBackgroundColor();
                }
                return false;
            };
            
            return card;
        }
        
        private void ClearUserCards()
        {
            foreach (var card in _userCards)
            {
                _usersContainer.Remove(card);
                card.Dispose();
            }
            _userCards.Clear();
        }
        
        #endregion
        
        #region Data Updates
        
        private void UpdateActiveUserDisplay()
        {
            if (_accountState?.ActiveUser == null)
            {
                _activeUserName.Text = "No active user";
                _activeUserEmail.Text = "";
                _activeUserStatus.Text = "Please sign in";
                _activeUserAvatar.ResourceUrl = "images/default_avatar_large.png";
                return;
            }
            
            var user = _accountState.ActiveUser;
            
            _activeUserName.Text = user.DisplayName;
            _activeUserEmail.Text = user.Email;
            _activeUserStatus.Text = "Active User";
            _activeUserAvatar.ResourceUrl = user.ProfilePictureUrl ?? "images/default_avatar_large.png";
            
            // Animate user info update
            var animation = new Animation(300);
            animation.AnimateTo(_activeUserSection, "Opacity", 0.7f, 0, 150);
            animation.AnimateTo(_activeUserSection, "Opacity", 1.0f, 150, 150);
            animation.Play();
        }
        
        #endregion
        
        #region Event Handlers
        
        private async void OnRefreshClicked(object sender, ClickedEventArgs e)
        {
            try
            {
                _refreshButton.IsEnabled = false;
                await ShowLoadingAsync("Refreshing...");
                
                await _accountController.LoadAccountInfoAsync();
                
                await HideLoadingAsync();
            }
            finally
            {
                _refreshButton.IsEnabled = true;
            }
        }
        
        private void OnAddUserClicked(object sender, ClickedEventArgs e)
        {
            RequestNavigation("QRLogin");
        }
        
        private void OnSettingsClicked(object sender, ClickedEventArgs e)
        {
            RequestNavigation("Settings");
        }
        
        private async void OnLogoutClicked(object sender, ClickedEventArgs e)
        {
            if (_accountState?.ActiveUser == null)
                return;
            
            try
            {
                // Show confirmation dialog
                await ShowLogoutConfirmation();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Logout failed: {ex.Message}");
            }
        }
        
        private async Task ShowLogoutConfirmation()
        {
            // Custom confirmation dialog for logout
            var confirmDialog = new View
            {
                WidthSpecification = 400,
                HeightSpecification = 200,
                BackgroundColor = GetFormBackgroundColor(),
                CornerRadius = 16.0f,
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    LinearAlignment = LinearLayout.Alignment.Center,
                    CellPadding = new Size2D(0, 20)
                },
                Padding = new Extents(30, 30, 30, 30)
            };
            
            var confirmText = new TextLabel
            {
                Text = $"Logout {_accountState.ActiveUser.DisplayName}?",
                TextColor = GetThemeTextColor(),
                PointSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                MultiLine = true
            };
            
            var buttonContainer = new View
            {
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    CellPadding = new Size2D(20, 0)
                }
            };
            
            var cancelButton = CreateStyledButton("Cancel", new Size(100, 40), false);
            var logoutButton = CreateStyledButton("Logout", new Size(100, 40), true);
            logoutButton.BackgroundColor = GetDangerColor();
            
            cancelButton.Clicked += (s, e) => HideLogoutConfirmation(confirmDialog);
            logoutButton.Clicked += async (s, e) =>
            {
                HideLogoutConfirmation(confirmDialog);
                await PerformLogout();
            };
            
            buttonContainer.Add(cancelButton);
            buttonContainer.Add(logoutButton);
            
            confirmDialog.Add(confirmText);
            confirmDialog.Add(buttonContainer);
            
            // Add to error overlay for modal display
            ErrorOverlay.Children.Clear();
            ErrorOverlay.Add(confirmDialog);
            ErrorOverlay.Visibility = true;
        }
        
        private void HideLogoutConfirmation(View dialog)
        {
            ErrorOverlay.Visibility = false;
            dialog?.Dispose();
        }
        
        private async Task PerformLogout()
        {
            await ShowLoadingAsync("Logging out...");
            
            var userId = _accountState.ActiveUser.UserId;
            await _accountController.HandleLogoutAsync(userId);
        }
        
        private async Task OnUserSwitchClicked(string userId)
        {
            try
            {
                await ShowLoadingAsync("Switching user...");
                await _accountController.HandleUserSwitchAsync(userId);
            }
            catch (Exception ex)
            {
                await HideLoadingAsync();
                await ShowErrorAsync($"User switch failed: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Controller Event Handlers
        
        private async void OnAccountStateChanged(object sender, AccountState accountState)
        {
            _accountState = accountState;
            
            await HideLoadingAsync();
            
            // Update UI with new account state
            UpdateActiveUserDisplay();
            UpdateUserCards();
            
            Console.WriteLine($"Account state updated: {accountState.AllAccounts.Count} total users");
        }
        
        private async void OnUserSwitched(object sender, SamsungAccount newActiveUser)
        {
            await HideLoadingAsync();
            
            // Reload account info to get updated state
            await _accountController.LoadAccountInfoAsync();
            
            Console.WriteLine($"User switched to: {newActiveUser.DisplayName}");
        }
        
        private async void OnUserLoggedOut(object sender, string userId)
        {
            await HideLoadingAsync();
            
            // Check if any users remain
            if (_accountState != null && !_accountState.AllAccounts.Any())
            {
                // No users left, navigate to login
                RequestNavigation("QRLogin");
            }
            else
            {
                // Reload account info
                await _accountController.LoadAccountInfoAsync();
            }
            
            Console.WriteLine($"User logged out: {userId}");
        }
        
        private async void OnUserAdded(object sender, SamsungAccount newUser)
        {
            await HideLoadingAsync();
            
            // Reload account info to show new user
            await _accountController.LoadAccountInfoAsync();
            
            Console.WriteLine($"User added: {newUser.DisplayName}");
        }
        
        #endregion
        
        #region Helper Methods
        
        private Color GetActiveUserBackgroundColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? new Color(0.15f, 0.25f, 0.35f, 1.0f) : new Color(0.95f, 0.98f, 1.0f, 1.0f);
        }
        
        private Color GetCardBackgroundColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? new Color(0.2f, 0.2f, 0.2f, 1.0f) : new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        
        private Color GetCardHighlightColor()
        {
            var theme = ConfigService.DefaultUITheme;
            return theme == "dark" ? new Color(0.3f, 0.3f, 0.3f, 1.0f) : new Color(0.95f, 0.95f, 0.95f, 1.0f);
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
        
        private Color GetPrimaryColor()
        {
            return new Color(0.2f, 0.6f, 1.0f, 1.0f);
        }
        
        private Color GetSuccessColor()
        {
            return new Color(0.2f, 0.8f, 0.2f, 1.0f);
        }
        
        private Color GetDangerColor()
        {
            return new Color(0.9f, 0.3f, 0.3f, 1.0f);
        }
        
        #endregion
        
        #region Lifecycle and Cleanup
        
        protected override void Dispose(DisposeTypes type)
        {
            if (type == DisposeTypes.Explicit)
            {
                // Unsubscribe from controller events
                if (_accountController != null)
                {
                    _accountController.AccountStateChanged -= OnAccountStateChanged;
                    _accountController.UserSwitched -= OnUserSwitched;
                    _accountController.UserLoggedOut -= OnUserLoggedOut;
                    _accountController.UserAdded -= OnUserAdded;
                }
                
                // Clear user cards
                ClearUserCards();
                
                // Dispose UI elements
                _activeUserSection?.Dispose();
                _otherUsersSection?.Dispose();
                _actionButtonsSection?.Dispose();
                _activeUserAvatar?.Dispose();
                _activeUserName?.Dispose();
                _activeUserEmail?.Dispose();
                _activeUserStatus?.Dispose();
                _activeUserBadge?.Dispose();
                _usersScrollView?.Dispose();
                _usersContainer?.Dispose();
                _logoutButton?.Dispose();
                _addUserButton?.Dispose();
                _settingsButton?.Dispose();
                _refreshButton?.Dispose();
            }
            
            base.Dispose(type);
        }
        
        #endregion
    }
}