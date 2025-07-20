namespace SamsungAccountUI.Services.Config
{
    public class GlobalConfigService : IGlobalConfigService
    {
        public T GetPreferenceValue<T>(string key, T defaultValue = default(T))
        {
            try
            {
                return TizenPreferenceHelper.GetValue<T>(key, defaultValue);
            }
            catch (System.Exception)
            {
                return defaultValue;
            }
        }
        
        public bool SetPreferenceValue<T>(string key, T value)
        {
            try
            {
                return TizenPreferenceHelper.SetValue<T>(key, value);
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        
        public bool HasPreferenceKey(string key)
        {
            return TizenPreferenceHelper.Contains(key);
        }
        
        // Samsung Account preferences
        public bool IsMultiUserEnabled => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.MultiUserEnabled, true);
        
        public bool IsQRLoginEnabled => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.QRLoginEnabled, true);
        
        public bool IsPasswordLoginEnabled => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.PasswordLoginEnabled, true);
        
        public bool IsGoogleLoginEnabled => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.GoogleLoginEnabled, true);
        
        public string DefaultUITheme => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.UITheme, "dark");
        
        public int MaxUserAccounts => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.MaxUsers, 6);
        
        public bool RequirePasswordForLogout => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.RequirePasswordForLogout, true);
        
        public bool EnableLargeText => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.LargeText, false);
        
        // Device preferences
        public bool IsAIHomeDevice => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.DeviceType, "FamilyHub") == "AIHome";
        
        public bool IsFamilyHubDevice => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.DeviceType, "FamilyHub") == "FamilyHub";
        
        public int ScreenWidth => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.ScreenWidth, 1920);
        
        public int ScreenHeight => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.ScreenHeight, 1080);
        
        // Security preferences
        public int LoginTimeoutMinutes => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.LoginTimeout, 30);
        
        public int MaxLoginAttempts => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.MaxAttempts, 3);
        
        public bool EnableSessionTimeout => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.SessionTimeout, true);
        
        // UI preferences
        public string PreferredLanguage => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.Language, "en-US");
        
        public bool ShowUserProfiles => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.ShowProfiles, true);
        
        public bool EnableAnimations => 
            GetPreferenceValue(TizenPreferenceHelper.Keys.Animations, true);
    }
}