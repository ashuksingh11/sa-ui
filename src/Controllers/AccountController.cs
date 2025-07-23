using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SamsungAccountUI.Models.User;
using SamsungAccountUI.Services.API;

namespace SamsungAccountUI.Controllers
{
    /// <summary>
    /// Consolidated account management controller
    /// Combines AccountInfoController and UserSwitchController functionality
    /// Handles account information display and user switching operations
    /// </summary>
    public class AccountController
    {
        private readonly ISamsungAccountService _accountService;

        public AccountController(ISamsungAccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        #region Account Information

        /// <summary>
        /// Get all user accounts on the device
        /// </summary>
        public async Task<List<SamsungAccount>> GetAllAccountsAsync()
        {
            try
            {
                return await _accountService.GetAllAccountListAsync();
            }
            catch (Exception)
            {
                return new List<SamsungAccount>();
            }
        }

        /// <summary>
        /// Get currently active user
        /// </summary>
        public async Task<SamsungAccount> GetActiveUserAsync()
        {
            try
            {
                return await _accountService.GetDefaultUserAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get specific user account by ID
        /// </summary>
        public async Task<SamsungAccount> GetUserByIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return null;
                }

                var allAccounts = await GetAllAccountsAsync();
                return allAccounts.FirstOrDefault(account => account.UserId == userId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get detailed profile information for a user
        /// </summary>
        public async Task<SamsungAccount> GetDetailedProfileAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return null;
                }

                return await _accountService.FetchProfileInfoAsync(userId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get account state with active user and all accounts
        /// </summary>
        public async Task<AccountState> GetAccountStateAsync()
        {
            try
            {
                var allAccounts = await GetAllAccountsAsync();
                var activeUser = await GetActiveUserAsync();

                return new AccountState
                {
                    AllAccounts = allAccounts,
                    ActiveUser = activeUser
                };
            }
            catch (Exception)
            {
                return new AccountState
                {
                    AllAccounts = new List<SamsungAccount>(),
                    ActiveUser = null
                };
            }
        }

        #endregion

        #region User Switching

        /// <summary>
        /// Switch to a different user account
        /// </summary>
        public async Task<bool> SwitchToUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return false;
                }

                // Verify the user exists on the device
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Switch to the user using Samsung Account API
                var success = await _accountService.SetDefaultUserAsync(userId);
                
                if (success)
                {
                    // Update the user's active state
                    user.IsActiveUser = true;
                    OnUserSwitched(user);
                }

                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Switch to next user in the account list (for quick switching)
        /// </summary>
        public async Task<bool> SwitchToNextUserAsync()
        {
            try
            {
                var allAccounts = await GetAllAccountsAsync();
                if (allAccounts.Count <= 1)
                {
                    return false; // No other users to switch to
                }

                var activeUser = await GetActiveUserAsync();
                if (activeUser == null)
                {
                    // Switch to first available user
                    return await SwitchToUserAsync(allAccounts.First().UserId);
                }

                // Find current user index and switch to next
                var currentIndex = allAccounts.FindIndex(u => u.UserId == activeUser.UserId);
                var nextIndex = (currentIndex + 1) % allAccounts.Count;
                
                return await SwitchToUserAsync(allAccounts[nextIndex].UserId);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get available users for switching (excludes current active user)
        /// </summary>
        public async Task<List<SamsungAccount>> GetAvailableUsersForSwitchingAsync()
        {
            try
            {
                var allAccounts = await GetAllAccountsAsync();
                var activeUser = await GetActiveUserAsync();

                if (activeUser == null)
                {
                    return allAccounts;
                }

                return allAccounts.Where(account => account.UserId != activeUser.UserId).ToList();
            }
            catch (Exception)
            {
                return new List<SamsungAccount>();
            }
        }

        #endregion

        #region Account Validation

        /// <summary>
        /// Check if user switching is available (multiple users exist)
        /// </summary>
        public async Task<bool> IsUserSwitchingAvailableAsync()
        {
            try
            {
                var allAccounts = await GetAllAccountsAsync();
                return allAccounts.Count > 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the device has reached maximum user limit
        /// </summary>
        public async Task<bool> IsAtMaxUserLimitAsync(int maxUsers = 6)
        {
            try
            {
                var allAccounts = await GetAllAccountsAsync();
                return allAccounts.Count >= maxUsers;
            }
            catch (Exception)
            {
                return true; // Assume at limit if we can't check
            }
        }

        /// <summary>
        /// Validate if a user can be added to the device
        /// </summary>
        public async Task<bool> CanAddNewUserAsync(int maxUsers = 6)
        {
            return !await IsAtMaxUserLimitAsync(maxUsers);
        }

        #endregion

        #region Account Statistics

        /// <summary>
        /// Get account statistics for display
        /// </summary>
        public async Task<AccountStatistics> GetAccountStatisticsAsync()
        {
            try
            {
                var allAccounts = await GetAllAccountsAsync();
                var activeUser = await GetActiveUserAsync();

                return new AccountStatistics
                {
                    TotalAccounts = allAccounts.Count,
                    HasActiveUser = activeUser != null,
                    ActiveUserId = activeUser?.UserId,
                    ActiveUserDisplayName = activeUser?.DisplayName,
                    CanSwitchUsers = allAccounts.Count > 1,
                    LastLoginTime = activeUser?.LastLoginTime ?? DateTime.MinValue
                };
            }
            catch (Exception)
            {
                return new AccountStatistics();
            }
        }

        /// <summary>
        /// Get recent activity summary
        /// </summary>
        public async Task<List<AccountActivity>> GetRecentActivityAsync(int maxItems = 5)
        {
            try
            {
                var allAccounts = await GetAllAccountsAsync();
                
                // Sort by last login time and take most recent
                var recentActivity = allAccounts
                    .OrderByDescending(account => account.LastLoginTime)
                    .Take(maxItems)
                    .Select(account => new AccountActivity
                    {
                        UserId = account.UserId,
                        DisplayName = account.DisplayName,
                        LastActivity = account.LastLoginTime,
                        ActivityType = account.IsActiveUser ? "Active" : "Recent Login"
                    })
                    .ToList();

                return recentActivity;
            }
            catch (Exception)
            {
                return new List<AccountActivity>();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event raised when user is switched
        /// </summary>
        public event EventHandler<SamsungAccount> UserSwitched;

        /// <summary>
        /// Event raised when account information is updated
        /// </summary>
        public event EventHandler<AccountState> AccountStateChanged;

        protected virtual void OnUserSwitched(SamsungAccount newActiveUser)
        {
            UserSwitched?.Invoke(this, newActiveUser);
        }

        protected virtual void OnAccountStateChanged(AccountState newState)
        {
            AccountStateChanged?.Invoke(this, newState);
        }

        #endregion
    }

    /// <summary>
    /// Account statistics for dashboard display
    /// </summary>
    public class AccountStatistics
    {
        public int TotalAccounts { get; set; }
        public bool HasActiveUser { get; set; }
        public string ActiveUserId { get; set; }
        public string ActiveUserDisplayName { get; set; }
        public bool CanSwitchUsers { get; set; }
        public DateTime LastLoginTime { get; set; }
    }

    /// <summary>
    /// Account activity for recent activity display
    /// </summary>
    public class AccountActivity
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public DateTime LastActivity { get; set; }
        public string ActivityType { get; set; }
    }
}