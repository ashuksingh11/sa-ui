using System;
using System.Collections.Generic;
using System.Linq;

namespace SamsungAccountUI.Models.User
{
    public class AccountState
    {
        public List<SamsungAccount> AllAccounts { get; set; } = new List<SamsungAccount>();
        public SamsungAccount ActiveUser { get; set; }
        
        public bool HasAccounts => AllAccounts.Any();
        public bool SupportsMultiUser => AllAccounts.Count > 1;
        public int AccountCount => AllAccounts.Count;
        
        public AccountState()
        {
            AllAccounts = new List<SamsungAccount>();
            ActiveUser = null;
        }
        
        public SamsungAccount GetActiveUser()
        {
            return ActiveUser ?? AllAccounts.FirstOrDefault(u => u.IsActiveUser);
        }
        
        public bool SetActiveUser(string userId)
        {
            var user = AllAccounts.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                // Reset all users to non-active
                AllAccounts.ForEach(u => u.IsActiveUser = false);
                // Set specified user as active
                user.IsActiveUser = true;
                ActiveUser = user;
                return true;
            }
            return false;
        }
        
        public bool RemoveUser(string userId)
        {
            var user = AllAccounts.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                AllAccounts.Remove(user);
                if (ActiveUser?.UserId == userId)
                {
                    ActiveUser = AllAccounts.FirstOrDefault();
                    if (ActiveUser != null)
                    {
                        ActiveUser.IsActiveUser = true;
                    }
                }
                return true;
            }
            return false;
        }
        
        public void AddUser(SamsungAccount user)
        {
            if (user != null && !AllAccounts.Any(u => u.UserId == user.UserId))
            {
                AllAccounts.Add(user);
                if (!HasActiveUser())
                {
                    SetActiveUser(user.UserId);
                }
            }
        }
        
        private bool HasActiveUser()
        {
            return AllAccounts.Any(u => u.IsActiveUser) || ActiveUser != null;
        }
    }
}