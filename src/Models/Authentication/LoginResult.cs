using SamsungAccountUI.Models.User;

namespace SamsungAccountUI.Models.Authentication
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public SamsungAccount User { get; set; }
        public string ErrorMessage { get; set; }
        public AuthenticationError ErrorType { get; set; }
        public string SessionToken { get; set; }
        public LoginStatus Status { get; set; }
        
        public LoginResult()
        {
            IsSuccess = false;
            User = null;
            ErrorMessage = string.Empty;
            ErrorType = AuthenticationError.None;
            SessionToken = string.Empty;
            Status = LoginStatus.Pending;
        }
        
        public static LoginResult Success(SamsungAccount user, string sessionToken = "")
        {
            return new LoginResult
            {
                IsSuccess = true,
                User = user,
                SessionToken = sessionToken,
                Status = LoginStatus.Success,
                ErrorType = AuthenticationError.None,
                ErrorMessage = string.Empty
            };
        }
        
        public static LoginResult Failure(AuthenticationError errorType, string errorMessage)
        {
            return new LoginResult
            {
                IsSuccess = false,
                User = null,
                ErrorType = errorType,
                ErrorMessage = errorMessage,
                Status = LoginStatus.Failed,
                SessionToken = string.Empty
            };
        }
    }
}