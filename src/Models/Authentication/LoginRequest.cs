using System;

namespace SamsungAccountUI.Models.Authentication
{
    public class LoginRequest
    {
        public AuthenticationType Type { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string QRToken { get; set; }
        public string GoogleToken { get; set; }
        public string DeviceId { get; set; }
        public DateTime RequestTime { get; set; }
        
        public LoginRequest()
        {
            Type = AuthenticationType.QR;
            Email = string.Empty;
            Password = string.Empty;
            QRToken = string.Empty;
            GoogleToken = string.Empty;
            DeviceId = string.Empty;
            RequestTime = DateTime.Now;
        }
        
        public LoginRequest(AuthenticationType type)
        {
            Type = type;
            Email = string.Empty;
            Password = string.Empty;
            QRToken = string.Empty;
            GoogleToken = string.Empty;
            DeviceId = string.Empty;
            RequestTime = DateTime.Now;
        }
        
        public bool IsValid()
        {
            return Type switch
            {
                AuthenticationType.Password => !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password),
                AuthenticationType.QR => !string.IsNullOrEmpty(QRToken),
                AuthenticationType.Google => !string.IsNullOrEmpty(GoogleToken),
                _ => false
            };
        }
    }
}