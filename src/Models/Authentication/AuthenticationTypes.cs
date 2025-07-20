namespace SamsungAccountUI.Models.Authentication
{
    public enum AuthenticationType
    {
        QR,
        Password,
        Google
    }
    
    public enum AuthenticationError
    {
        None,
        InvalidCredentials,
        NetworkError,
        ServiceUnavailable,
        InvalidQRCode,
        GoogleAuthFailed,
        PasswordExpired,
        AccountLocked,
        TooManyAttempts,
        UnknownError
    }
    
    public enum LoginStatus
    {
        Pending,
        InProgress,
        Success,
        Failed,
        Cancelled
    }
}