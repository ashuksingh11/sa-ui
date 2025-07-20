namespace SamsungAccountUI.Services.Config
{
    public interface IGlobalConfigService
    {
        T GetPreferenceValue<T>(string key, T defaultValue = default(T));
        bool SetPreferenceValue<T>(string key, T value);
        bool HasPreferenceKey(string key);
        
        // Common Samsung Account UI preferences
        bool IsMultiUserEnabled { get; }
        bool IsQRLoginEnabled { get; }
        bool IsPasswordLoginEnabled { get; }
        bool IsGoogleLoginEnabled { get; }
        string DefaultUITheme { get; }
        int MaxUserAccounts { get; }
        bool RequirePasswordForLogout { get; }
        bool EnableLargeText { get; }
        
        // Device-specific preferences
        bool IsAIHomeDevice { get; }
        bool IsFamilyHubDevice { get; }
        int ScreenWidth { get; }
        int ScreenHeight { get; }
        
        // Security preferences
        int LoginTimeoutMinutes { get; }
        int MaxLoginAttempts { get; }
        bool EnableSessionTimeout { get; }
        
        // UI preferences
        string PreferredLanguage { get; }
        bool ShowUserProfiles { get; }
        bool EnableAnimations { get; }
    }
}