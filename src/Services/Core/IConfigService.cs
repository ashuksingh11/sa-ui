using System;

namespace SamsungAccountUI.Services.Core
{
    /// <summary>
    /// Simplified configuration service for Tizen
    /// Replaces the complex IGlobalConfigService with essential functionality
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// Get a configuration value with optional default
        /// </summary>
        T GetValue<T>(string key, T defaultValue = default(T));

        /// <summary>
        /// Set a configuration value
        /// </summary>
        bool SetValue<T>(string key, T value);

        /// <summary>
        /// Check if a configuration key exists
        /// </summary>
        bool HasKey(string key);

        /// <summary>
        /// Remove a configuration key
        /// </summary>
        bool RemoveKey(string key);

        // Common Samsung Account UI preferences with sensible defaults
        
        /// <summary>
        /// Whether multi-user support is enabled
        /// </summary>
        bool IsMultiUserEnabled { get; }

        /// <summary>
        /// Whether QR code login is enabled
        /// </summary>
        bool IsQRLoginEnabled { get; }

        /// <summary>
        /// Whether Google OAuth login is enabled
        /// </summary>
        bool IsGoogleLoginEnabled { get; }

        /// <summary>
        /// Whether password login is enabled
        /// </summary>
        bool IsPasswordLoginEnabled { get; }

        /// <summary>
        /// Maximum number of user accounts supported
        /// </summary>
        int MaxUserAccounts { get; }

        /// <summary>
        /// UI theme preference (dark, light, auto)
        /// </summary>
        string UITheme { get; }

        /// <summary>
        /// Whether animations are enabled
        /// </summary>
        bool AnimationsEnabled { get; }

        /// <summary>
        /// Whether large text mode is enabled for accessibility
        /// </summary>
        bool LargeTextEnabled { get; }

        /// <summary>
        /// App language preference
        /// </summary>
        string Language { get; }
    }
}