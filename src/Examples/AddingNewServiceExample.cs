using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SamsungAccountUI.Services.Config;
using SamsungAccountUI.Controllers.Base;
using SamsungAccountUI.Views.Common;
using SamsungAccountUI.Models.Device;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;

namespace SamsungAccountUI.Examples
{
    /// <summary>
    /// Complete example showing how to add a new service and use it in views
    /// This demonstrates the full process from service creation to view integration
    /// </summary>
    public class AddingNewServiceExample
    {
        #region Step 1: Define Service Interface
        
        /// <summary>
        /// Example: Theme service for managing UI themes dynamically
        /// </summary>
        public interface IThemeService
        {
            Theme CurrentTheme { get; }
            event EventHandler<ThemeChangedEventArgs> ThemeChanged;
            
            Task ApplyThemeAsync(string themeName);
            Task<List<Theme>> GetAvailableThemesAsync();
            Color GetThemedColor(string colorKey);
            void RegisterThemeableView(IThemeable view);
            void UnregisterThemeableView(IThemeable view);
        }
        
        public class Theme
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public Dictionary<string, Color> Colors { get; set; }
            public Dictionary<string, float> FontSizes { get; set; }
            public bool IsDark { get; set; }
        }
        
        public class ThemeChangedEventArgs : EventArgs
        {
            public Theme OldTheme { get; set; }
            public Theme NewTheme { get; set; }
        }
        
        public interface IThemeable
        {
            void ApplyTheme(Theme theme);
        }
        
        #endregion
        
        #region Step 2: Implement the Service
        
        public class TizenThemeService : IThemeService
        {
            private readonly ILogger<TizenThemeService> _logger;
            private readonly IGlobalConfigService _configService;
            private readonly List<WeakReference<IThemeable>> _themeableViews = new();
            private Theme _currentTheme;
            
            public Theme CurrentTheme => _currentTheme;
            public event EventHandler<ThemeChangedEventArgs> ThemeChanged;
            
            public TizenThemeService(
                ILogger<TizenThemeService> logger,
                IGlobalConfigService configService)
            {
                _logger = logger;
                _configService = configService;
                
                // Initialize with default theme
                InitializeDefaultTheme();
            }
            
            private void InitializeDefaultTheme()
            {
                var defaultThemeName = _configService.DefaultUITheme;
                _currentTheme = CreateTheme(defaultThemeName);
            }
            
            public async Task ApplyThemeAsync(string themeName)
            {
                try
                {
                    _logger.LogInformation($"Applying theme: {themeName}");
                    
                    var oldTheme = _currentTheme;
                    var newTheme = CreateTheme(themeName);
                    
                    _currentTheme = newTheme;
                    
                    // Save theme preference
                    _configService.SetPreferenceValue("ui.theme", themeName);
                    
                    // Notify all registered views
                    NotifyThemeableViews();
                    
                    // Raise theme changed event
                    ThemeChanged?.Invoke(this, new ThemeChangedEventArgs
                    {
                        OldTheme = oldTheme,
                        NewTheme = newTheme
                    });
                    
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to apply theme: {themeName}");
                    throw;
                }
            }
            
            public async Task<List<Theme>> GetAvailableThemesAsync()
            {
                return await Task.FromResult(new List<Theme>
                {
                    CreateTheme("dark"),
                    CreateTheme("light"),
                    CreateTheme("high-contrast"),
                    CreateTheme("samsung-blue")
                });
            }
            
            public Color GetThemedColor(string colorKey)
            {
                if (_currentTheme?.Colors?.ContainsKey(colorKey) == true)
                {
                    return _currentTheme.Colors[colorKey];
                }
                
                // Return default color
                return colorKey switch
                {
                    "background" => Color.Black,
                    "text" => Color.White,
                    "primary" => new Color(0.2f, 0.6f, 1.0f, 1.0f),
                    "error" => new Color(0.9f, 0.3f, 0.3f, 1.0f),
                    _ => Color.Gray
                };
            }
            
            public void RegisterThemeableView(IThemeable view)
            {
                if (view != null)
                {
                    _themeableViews.Add(new WeakReference<IThemeable>(view));
                    
                    // Apply current theme immediately
                    view.ApplyTheme(_currentTheme);
                }
            }
            
            public void UnregisterThemeableView(IThemeable view)
            {
                _themeableViews.RemoveAll(wr => 
                {
                    if (wr.TryGetTarget(out var target))
                    {
                        return target == view;
                    }
                    return true; // Remove dead references
                });
            }
            
            private void NotifyThemeableViews()
            {
                var deadReferences = new List<WeakReference<IThemeable>>();
                
                foreach (var weakRef in _themeableViews)
                {
                    if (weakRef.TryGetTarget(out var view))
                    {
                        try
                        {
                            view.ApplyTheme(_currentTheme);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error applying theme to view");
                        }
                    }
                    else
                    {
                        deadReferences.Add(weakRef);
                    }
                }
                
                // Clean up dead references
                foreach (var deadRef in deadReferences)
                {
                    _themeableViews.Remove(deadRef);
                }
            }
            
            private Theme CreateTheme(string themeName)
            {
                return themeName switch
                {
                    "dark" => new Theme
                    {
                        Name = "dark",
                        DisplayName = "Dark Theme",
                        IsDark = true,
                        Colors = new Dictionary<string, Color>
                        {
                            ["background"] = new Color(0.1f, 0.1f, 0.1f, 1.0f),
                            ["surface"] = new Color(0.15f, 0.15f, 0.15f, 1.0f),
                            ["text"] = Color.White,
                            ["textSecondary"] = new Color(0.7f, 0.7f, 0.7f, 1.0f),
                            ["primary"] = new Color(0.2f, 0.6f, 1.0f, 1.0f),
                            ["error"] = new Color(0.9f, 0.3f, 0.3f, 1.0f)
                        },
                        FontSizes = new Dictionary<string, float>
                        {
                            ["title"] = 24f,
                            ["body"] = 16f,
                            ["caption"] = 12f
                        }
                    },
                    "light" => new Theme
                    {
                        Name = "light",
                        DisplayName = "Light Theme",
                        IsDark = false,
                        Colors = new Dictionary<string, Color>
                        {
                            ["background"] = new Color(0.95f, 0.95f, 0.95f, 1.0f),
                            ["surface"] = Color.White,
                            ["text"] = Color.Black,
                            ["textSecondary"] = new Color(0.4f, 0.4f, 0.4f, 1.0f),
                            ["primary"] = new Color(0.2f, 0.6f, 1.0f, 1.0f),
                            ["error"] = new Color(0.8f, 0.2f, 0.2f, 1.0f)
                        },
                        FontSizes = new Dictionary<string, float>
                        {
                            ["title"] = 24f,
                            ["body"] = 16f,
                            ["caption"] = 12f
                        }
                    },
                    _ => CreateTheme("dark") // Default to dark
                };
            }
        }
        
        #endregion
        
        #region Step 3: Register Service in DI Container
        
        public static void RegisterThemeService(IServiceCollection services)
        {
            // Register as Singleton since theme is app-wide
            services.AddSingleton<IThemeService, TizenThemeService>();
            
            // Or with different implementations for debug/release
            #if DEBUG
            services.AddSingleton<IThemeService, MockThemeService>();
            #else
            services.AddSingleton<IThemeService, TizenThemeService>();
            #endif
        }
        
        // In Program.cs ConfigureServices method:
        private void ConfigureServicesExample(IServiceCollection services)
        {
            // ... existing services ...
            
            // Add Theme Service
            RegisterThemeService(services);
            
            // ... rest of services ...
        }
        
        #endregion
        
        #region Step 4: Create Themeable Base View
        
        /// <summary>
        /// Enhanced BaseView that supports theming
        /// </summary>
        public abstract class ThemeableBaseView : BaseView, IThemeable
        {
            protected IThemeService ThemeService { get; private set; }
            
            public ThemeableBaseView(
                IController controller,
                IGlobalConfigService configService,
                DeviceInfo deviceInfo,
                IServiceProvider serviceProvider)
                : base(controller, configService, deviceInfo)
            {
                // Get theme service
                ThemeService = serviceProvider.GetRequiredService<IThemeService>();
                
                // Register for theme changes
                ThemeService.RegisterThemeableView(this);
            }
            
            public virtual void ApplyTheme(Theme theme)
            {
                // Apply theme to base elements
                BackgroundColor = theme.Colors["background"];
                
                if (TitleLabel != null)
                {
                    TitleLabel.TextColor = theme.Colors["text"];
                    TitleLabel.PointSize = theme.FontSizes["title"];
                }
                
                // Let derived classes apply their specific theming
                ApplyViewSpecificTheme(theme);
            }
            
            protected abstract void ApplyViewSpecificTheme(Theme theme);
            
            protected override void Dispose(DisposeTypes type)
            {
                if (type == DisposeTypes.Explicit)
                {
                    // Unregister from theme service
                    ThemeService?.UnregisterThemeableView(this);
                }
                
                base.Dispose(type);
            }
        }
        
        #endregion
        
        #region Step 5: Use Service in Views
        
        /// <summary>
        /// Example view that uses the theme service
        /// </summary>
        public class ThemedSettingsView : ThemeableBaseView
        {
            private View _themeSelectionContainer;
            private TextLabel _currentThemeLabel;
            private List<Button> _themeButtons = new();
            
            public ThemedSettingsView(
                IController controller,
                IGlobalConfigService configService,
                DeviceInfo deviceInfo,
                IServiceProvider serviceProvider)
                : base(controller, configService, deviceInfo, serviceProvider)
            {
                _loginController = controller;
            }
            
            protected override string GetViewTitle() => "Settings";
            
            public override async Task LoadContentAsync()
            {
                try
                {
                    await ShowLoadingAsync("Loading settings...");
                    
                    CreateThemeSection();
                    await LoadAvailableThemes();
                    
                    await HideLoadingAsync();
                }
                catch (Exception ex)
                {
                    await HideLoadingAsync();
                    await ShowErrorAsync($"Failed to load settings: {ex.Message}");
                }
            }
            
            private void CreateThemeSection()
            {
                _themeSelectionContainer = new View
                {
                    WidthSpecification = LayoutParamPolicies.MatchParent,
                    HeightSpecification = LayoutParamPolicies.WrapContent,
                    Layout = new LinearLayout
                    {
                        LinearOrientation = LinearLayout.Orientation.Vertical,
                        CellPadding = new Size2D(0, 20)
                    },
                    Padding = new Extents(20, 20, 20, 20)
                };
                
                // Section title
                var sectionTitle = new TextLabel
                {
                    Text = "Theme Selection",
                    TextColor = ThemeService.GetThemedColor("text"),
                    PointSize = 18,
                    FontStyle = new PropertyMap().Add("weight", new PropertyValue("bold"))
                };
                _themeSelectionContainer.Add(sectionTitle);
                
                // Current theme display
                _currentThemeLabel = new TextLabel
                {
                    Text = $"Current Theme: {ThemeService.CurrentTheme.DisplayName}",
                    TextColor = ThemeService.GetThemedColor("textSecondary"),
                    PointSize = 14
                };
                _themeSelectionContainer.Add(_currentThemeLabel);
                
                ContentContainer.Add(_themeSelectionContainer);
            }
            
            private async Task LoadAvailableThemes()
            {
                var themes = await ThemeService.GetAvailableThemesAsync();
                
                var buttonContainer = new View
                {
                    WidthSpecification = LayoutParamPolicies.MatchParent,
                    HeightSpecification = LayoutParamPolicies.WrapContent,
                    Layout = new LinearLayout
                    {
                        LinearOrientation = LinearLayout.Orientation.Horizontal,
                        CellPadding = new Size2D(15, 0)
                    }
                };
                
                foreach (var theme in themes)
                {
                    var themeButton = CreateStyledButton(
                        theme.DisplayName,
                        new Size(120, 40),
                        theme.Name == ThemeService.CurrentTheme.Name
                    );
                    
                    themeButton.Clicked += async (sender, e) => await OnThemeButtonClicked(theme);
                    _themeButtons.Add(themeButton);
                    buttonContainer.Add(themeButton);
                }
                
                _themeSelectionContainer.Add(buttonContainer);
            }
            
            private async Task OnThemeButtonClicked(Theme theme)
            {
                try
                {
                    await ShowLoadingAsync($"Applying {theme.DisplayName}...");
                    
                    // Apply the theme
                    await ThemeService.ApplyThemeAsync(theme.Name);
                    
                    // Update UI
                    _currentThemeLabel.Text = $"Current Theme: {theme.DisplayName}";
                    
                    // Update button states
                    UpdateThemeButtonStates(theme.Name);
                    
                    await HideLoadingAsync();
                    
                    // Show success toast
                    await ShowToastAsync($"{theme.DisplayName} applied successfully!");
                }
                catch (Exception ex)
                {
                    await HideLoadingAsync();
                    await ShowErrorAsync($"Failed to apply theme: {ex.Message}");
                }
            }
            
            private void UpdateThemeButtonStates(string activeThemeName)
            {
                // Update button visual states based on active theme
                foreach (var button in _themeButtons)
                {
                    var isActive = button.Text.Contains(activeThemeName);
                    button.BackgroundColor = isActive ? 
                        ThemeService.GetThemedColor("primary") : 
                        ThemeService.GetThemedColor("surface");
                }
            }
            
            protected override void ApplyViewSpecificTheme(Theme theme)
            {
                // Apply theme to view-specific elements
                if (_themeSelectionContainer != null)
                {
                    _themeSelectionContainer.BackgroundColor = theme.Colors["surface"];
                }
                
                if (_currentThemeLabel != null)
                {
                    _currentThemeLabel.TextColor = theme.Colors["textSecondary"];
                }
                
                // Update all theme buttons
                UpdateThemeButtonStates(theme.Name);
            }
            
            private async Task ShowToastAsync(string message)
            {
                // Simple toast implementation
                var toast = new View
                {
                    WidthSpecification = LayoutParamPolicies.WrapContent,
                    HeightSpecification = 50,
                    BackgroundColor = ThemeService.GetThemedColor("primary"),
                    CornerRadius = 25f,
                    Layout = new LinearLayout
                    {
                        LinearOrientation = LinearLayout.Orientation.Horizontal,
                        LinearAlignment = LinearLayout.Alignment.Center
                    },
                    Padding = new Extents(20, 20, 10, 10)
                };
                
                var toastText = new TextLabel
                {
                    Text = message,
                    TextColor = Color.White,
                    PointSize = 14
                };
                
                toast.Add(toastText);
                
                // Position at bottom of screen
                toast.Position = new Position(
                    Window.Instance.Size.Width / 2 - 100,
                    Window.Instance.Size.Height - 100
                );
                
                Window.Instance.Add(toast);
                
                // Animate toast
                var showAnimation = new Animation(300);
                showAnimation.AnimateTo(toast, "Opacity", 1.0f);
                showAnimation.Play();
                
                // Auto-hide after 2 seconds
                await Task.Delay(2000);
                
                var hideAnimation = new Animation(300);
                hideAnimation.AnimateTo(toast, "Opacity", 0.0f);
                hideAnimation.Finished += (sender, e) =>
                {
                    Window.Instance.Remove(toast);
                    toast.Dispose();
                };
                hideAnimation.Play();
            }
            
            public override async Task<bool> OnBackPressedAsync()
            {
                RequestNavigation("AccountInfo");
                return true;
            }
        }
        
        #endregion
        
        #region Step 6: Use Service in Controllers
        
        /// <summary>
        /// Example controller using the theme service
        /// </summary>
        public class SettingsController : BaseController
        {
            private readonly IThemeService _themeService;
            private readonly ISamsungAccountService _accountService;
            
            public SettingsController(
                INavigationService navigationService,
                ISamsungAccountService accountService,
                IGlobalConfigService configService,
                IThemeService themeService) // Inject theme service
                : base(navigationService, accountService, configService)
            {
                _themeService = themeService;
                _accountService = accountService;
                
                // Subscribe to theme changes
                _themeService.ThemeChanged += OnThemeChanged;
            }
            
            public async Task ApplyUserPreferredThemeAsync(string userId)
            {
                try
                {
                    // Get user's preferred theme from account service
                    var userPreferences = await _accountService.GetUserPreferencesAsync(userId);
                    var preferredTheme = userPreferences?.PreferredTheme ?? "dark";
                    
                    // Apply the theme
                    await _themeService.ApplyThemeAsync(preferredTheme);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail - use default theme
                    Console.WriteLine($"Failed to apply user theme: {ex.Message}");
                }
            }
            
            private void OnThemeChanged(object sender, ThemeChangedEventArgs e)
            {
                // Handle theme change - maybe update user preferences
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var currentUser = await _accountService.GetDefaultUserAsync();
                        if (currentUser != null)
                        {
                            await _accountService.UpdateUserPreferenceAsync(
                                currentUser.UserId,
                                "theme",
                                e.NewTheme.Name
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to save theme preference: {ex.Message}");
                    }
                });
            }
            
            public override void Dispose()
            {
                // Unsubscribe from events
                if (_themeService != null)
                {
                    _themeService.ThemeChanged -= OnThemeChanged;
                }
                
                base.Dispose();
            }
        }
        
        #endregion
        
        #region Testing the New Service
        
        /// <summary>
        /// Unit tests for the theme service
        /// </summary>
        public class ThemeServiceTests
        {
            private IServiceProvider CreateTestServiceProvider()
            {
                var services = new ServiceCollection();
                
                // Add logging
                services.AddLogging();
                
                // Add mock config service
                services.AddSingleton<IGlobalConfigService>(new MockConfigService
                {
                    DefaultUITheme = "dark"
                });
                
                // Add theme service
                services.AddSingleton<IThemeService, TizenThemeService>();
                
                return services.BuildServiceProvider();
            }
            
            public async Task TestThemeService()
            {
                // Arrange
                var serviceProvider = CreateTestServiceProvider();
                var themeService = serviceProvider.GetRequiredService<IThemeService>();
                
                // Act & Assert
                Console.WriteLine($"Current theme: {themeService.CurrentTheme.Name}");
                
                // Test getting available themes
                var themes = await themeService.GetAvailableThemesAsync();
                Console.WriteLine($"Available themes: {themes.Count}");
                
                // Test applying theme
                var themeChanged = false;
                themeService.ThemeChanged += (s, e) => themeChanged = true;
                
                await themeService.ApplyThemeAsync("light");
                
                Console.WriteLine($"Theme changed: {themeChanged}");
                Console.WriteLine($"New theme: {themeService.CurrentTheme.Name}");
                
                // Test getting themed colors
                var backgroundColor = themeService.GetThemedColor("background");
                Console.WriteLine($"Background color: R={backgroundColor.R}, G={backgroundColor.G}, B={backgroundColor.B}");
            }
        }
        
        // Mock config service for testing
        public class MockConfigService : IGlobalConfigService
        {
            public string DefaultUITheme { get; set; } = "dark";
            public bool IsMultiUserEnabled => true;
            public bool IsQRLoginEnabled => true;
            public bool IsGoogleLoginEnabled => true;
            public bool IsPasswordLoginEnabled => true;
            public int MaxUserAccounts => 6;
            public bool EnableAnimations => true;
            public bool EnableLargeText => false;
            public string PreferredLanguage => "en";
            public bool IsAnalyticsEnabled => true;
            
            public T GetPreferenceValue<T>(string key, T defaultValue = default)
            {
                // Simple mock implementation
                return defaultValue;
            }
            
            public bool SetPreferenceValue<T>(string key, T value)
            {
                // Simple mock implementation
                return true;
            }
            
            public bool HasPreferenceKey(string key)
            {
                return false;
            }
        }
        
        #endregion
    }
}