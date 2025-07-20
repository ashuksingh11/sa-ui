# Authentication Flow Sequence Diagrams

## 🔐 Overview

This document provides detailed sequence diagrams for all authentication flows in the Samsung Account UI application, including login, logout, user switching, and password management.

## 🚀 Application Startup Flow

```mermaid
sequenceDiagram
    participant User
    participant App as Samsung Account App
    participant Nav as NavigationController
    participant Device as DeviceDetectionService
    participant Account as SamsungAccountService
    participant View as ViewFactory
    
    User->>App: Launch Application
    App->>Nav: InitializeApplication()
    Nav->>Device: GetCurrentDeviceInfo()
    Device-->>Nav: DeviceInfo
    Nav->>Nav: SetDeviceType()
    Nav->>Account: GetAllAccountListAsync()
    Account-->>Nav: List<SamsungAccount>
    
    alt Has Existing Accounts
        Nav->>Account: GetDefaultUserAsync()
        Account-->>Nav: SamsungAccount
        Nav->>View: NavigateToScreen("AccountInfo")
    else No Accounts
        Nav->>View: NavigateToScreen("QRLogin")
    end
    
    View-->>User: Display Initial Screen
```

## 🔑 QR Login Flow

```mermaid
sequenceDiagram
    participant User
    participant QRView as QR Login View
    participant LoginCtrl as LoginController
    participant Account as SamsungAccountService
    participant Nav as NavigationService
    
    User->>QRView: Scan QR Code
    QRView->>QRView: Extract QR Token
    QRView->>LoginCtrl: HandleQRLogin(qrToken)
    LoginCtrl->>LoginCtrl: Create LoginRequest(QR)
    LoginCtrl->>Nav: ShowLoading("Signing in...")
    LoginCtrl->>Account: LoginAsync(loginRequest)
    
    Account->>Account: HandleQRLogin(request)
    Account->>Account: Validate QR Token
    
    alt Valid QR Token
        Account->>Account: Create User Account
        Account->>Account: GenerateSessionToken()
        Account-->>LoginCtrl: LoginResult.Success(user, token)
        LoginCtrl->>Nav: HideLoading()
        LoginCtrl->>Nav: ReplaceCurrentAsync("AccountInfo", user)
        Nav-->>User: Navigate to Account Info
    else Invalid QR Token
        Account-->>LoginCtrl: LoginResult.Failure(InvalidQRCode)
        LoginCtrl->>Nav: HideLoading()
        LoginCtrl->>Nav: ShowError("Invalid QR code")
        Nav-->>User: Display Error Message
    end
```

## 🔐 Password Login Flow

```mermaid
sequenceDiagram
    participant User
    participant PassView as Password Login View
    participant LoginCtrl as LoginController
    participant Validator as ValidationHelper
    participant Account as SamsungAccountService
    participant Nav as NavigationService
    
    User->>PassView: Enter Email & Password
    User->>PassView: Click Login Button
    PassView->>LoginCtrl: HandlePasswordLogin(email, password)
    
    LoginCtrl->>Validator: ValidateEmail(email)
    Validator-->>LoginCtrl: ValidationResult
    LoginCtrl->>Validator: ValidatePassword(password)
    Validator-->>LoginCtrl: ValidationResult
    
    alt Validation Fails
        LoginCtrl->>Nav: ShowError("Invalid input")
        Nav-->>User: Display Validation Error
    else Validation Success
        LoginCtrl->>LoginCtrl: Create LoginRequest(Password)
        LoginCtrl->>Nav: ShowLoading("Signing in...")
        LoginCtrl->>Account: LoginAsync(loginRequest)
        
        Account->>Account: HandlePasswordLogin(request)
        Account->>Account: Verify Credentials
        
        alt Valid Credentials
            Account->>Account: Find/Create User
            Account->>Account: Update LastLoginTime
            Account->>Account: SetActiveUser()
            Account->>Account: GenerateSessionToken()
            Account-->>LoginCtrl: LoginResult.Success(user, token)
            LoginCtrl->>Nav: HideLoading()
            LoginCtrl->>Nav: ReplaceCurrentAsync("AccountInfo", user)
            Nav-->>User: Navigate to Account Info
        else Invalid Credentials
            Account-->>LoginCtrl: LoginResult.Failure(InvalidCredentials)
            LoginCtrl->>Nav: HideLoading()
            LoginCtrl->>Nav: ShowError("Invalid email or password")
            Nav-->>User: Display Error Message
        end
    end
```

## 🌐 Google OAuth Login Flow

```mermaid
sequenceDiagram
    participant User
    participant GoogleView as Google Login View
    participant LoginCtrl as LoginController
    participant Account as SamsungAccountService
    participant GoogleAPI as Google OAuth API
    participant Nav as NavigationService
    
    User->>GoogleView: Click "Sign in with Google"
    GoogleView->>GoogleAPI: Initiate OAuth Flow
    GoogleAPI-->>User: OAuth Consent Screen
    User->>GoogleAPI: Grant Permissions
    GoogleAPI-->>GoogleView: OAuth Token
    
    GoogleView->>LoginCtrl: HandleGoogleLogin(googleToken)
    LoginCtrl->>LoginCtrl: Create LoginRequest(Google)
    LoginCtrl->>Nav: ShowLoading("Signing in...")
    LoginCtrl->>Account: LoginAsync(loginRequest)
    
    Account->>Account: HandleGoogleLogin(request)
    Account->>Account: Validate Google Token
    
    alt Valid Google Token
        Account->>Account: Create User Account
        Account->>Account: GenerateSessionToken()
        Account-->>LoginCtrl: LoginResult.Success(user, token)
        LoginCtrl->>Nav: HideLoading()
        LoginCtrl->>Nav: ReplaceCurrentAsync("AccountInfo", user)
        Nav-->>User: Navigate to Account Info
    else Invalid Google Token
        Account-->>LoginCtrl: LoginResult.Failure(GoogleAuthFailed)
        LoginCtrl->>Nav: HideLoading()
        LoginCtrl->>Nav: ShowError("Google authentication failed")
        Nav-->>User: Display Error Message
    end
```

## 👥 User Switching Flow

```mermaid
sequenceDiagram
    participant User
    participant AccountView as Account Info View
    participant AccountCtrl as AccountInfoController
    participant SwitchCtrl as UserSwitchController
    participant Account as SamsungAccountService
    participant Config as GlobalConfigService
    participant Nav as NavigationService
    
    User->>AccountView: Click "Switch User"
    AccountView->>Nav: NavigateToAsync("UserSwitch")
    Nav->>SwitchCtrl: LoadAsync()
    
    SwitchCtrl->>Account: GetAllAccountListAsync()
    Account-->>SwitchCtrl: List<SamsungAccount>
    SwitchCtrl->>Account: GetDefaultUserAsync()
    Account-->>SwitchCtrl: ActiveUser
    SwitchCtrl->>SwitchCtrl: Create AccountState
    SwitchCtrl-->>User: Display User List
    
    User->>SwitchCtrl: Select User
    SwitchCtrl->>Config: GetPreferenceValue("require.password")
    Config-->>SwitchCtrl: requirePassword
    
    alt Requires Password
        SwitchCtrl-->>User: Show Password Dialog
        User->>SwitchCtrl: Enter Password
        SwitchCtrl->>Account: VerifyPasswordAsync(userId, password)
        Account-->>SwitchCtrl: isValid
        
        alt Invalid Password
            SwitchCtrl-->>User: Show Error
        else Valid Password
            SwitchCtrl->>Account: SetDefaultUserAsync(userId)
            Account-->>SwitchCtrl: success
            SwitchCtrl->>Nav: ReplaceCurrentAsync("AccountInfo")
            Nav-->>User: Navigate to Account Info
        end
    else No Password Required
        SwitchCtrl->>Account: SetDefaultUserAsync(userId)
        Account-->>SwitchCtrl: success
        SwitchCtrl->>Nav: ReplaceCurrentAsync("AccountInfo")
        Nav-->>User: Navigate to Account Info
    end
```

## 🚪 User Logout Flow

```mermaid
sequenceDiagram
    participant User
    participant AccountView as Account Info View
    participant LogoutCtrl as LogoutController
    participant Account as SamsungAccountService
    participant Config as GlobalConfigService
    participant Nav as NavigationService
    
    User->>AccountView: Click "Logout"
    AccountView->>Nav: NavigateToAsync("LogoutConfirm", user)
    Nav->>LogoutCtrl: LoadAsync()
    LogoutCtrl-->>User: Show Logout Confirmation
    
    User->>LogoutCtrl: Confirm Logout
    LogoutCtrl->>Config: GetPreferenceValue("require.password")
    Config-->>LogoutCtrl: requirePassword
    
    alt Requires Password Confirmation
        LogoutCtrl-->>User: Show Password Input
        User->>LogoutCtrl: Enter Password
        LogoutCtrl->>Account: VerifyPasswordAsync(userId, password)
        Account-->>LogoutCtrl: isValid
        
        alt Invalid Password
            LogoutCtrl-->>User: Show Error
        else Valid Password
            LogoutCtrl->>LogoutCtrl: ProcessLogout()
        end
    else No Password Required
        LogoutCtrl->>LogoutCtrl: ProcessLogout()
    end
    
    LogoutCtrl->>Nav: ShowLoading("Signing out...")
    LogoutCtrl->>Account: LogoutAsync(userId)
    Account->>Account: Remove User from List
    Account->>Account: Clear Session Token
    Account-->>LogoutCtrl: success
    
    LogoutCtrl->>Account: GetAllAccountListAsync()
    Account-->>LogoutCtrl: remainingAccounts
    LogoutCtrl->>Nav: HideLoading()
    
    alt No Remaining Accounts
        LogoutCtrl->>Nav: ReplaceCurrentAsync("QRLogin")
        Nav-->>User: Navigate to Login
    else Has Remaining Accounts
        LogoutCtrl->>Nav: ReplaceCurrentAsync("AccountInfo")
        Nav-->>User: Navigate to Account Info
    end
```

## 🔑 Password Change Flow

```mermaid
sequenceDiagram
    participant User
    participant PassView as Change Password View
    participant PassCtrl as PasswordController
    participant Validator as ValidationHelper
    participant Account as SamsungAccountService
    participant Nav as NavigationService
    
    User->>PassView: Enter Current & New Passwords
    User->>PassView: Click "Change Password"
    PassView->>PassCtrl: HandlePasswordChange(userId, current, new, confirm)
    
    PassCtrl->>PassCtrl: Create PasswordChangeRequest
    PassCtrl->>PassCtrl: ValidatePasswordChangeRequest()
    PassCtrl->>Validator: ValidatePasswordStrength(newPassword)
    Validator-->>PassCtrl: ValidationResult
    
    alt Validation Fails
        PassCtrl->>Nav: ShowError(validationError)
        Nav-->>User: Display Validation Error
    else Validation Success
        PassCtrl->>Nav: ShowLoading("Changing password...")
        PassCtrl->>Account: ChangePasswordAsync(userId, current, new)
        
        Account->>Account: Verify Current Password
        
        alt Invalid Current Password
            Account-->>PassCtrl: false
            PassCtrl->>Nav: HideLoading()
            PassCtrl->>Nav: ShowError("Invalid current password")
            Nav-->>User: Display Error
        else Valid Current Password
            Account->>Account: Update Password
            Account-->>PassCtrl: true
            PassCtrl->>Nav: HideLoading()
            PassCtrl->>Nav: NavigateToAsync("Success", "Password changed")
            Nav-->>User: Navigate to Success Screen
        end
    end
```

## 🚨 Error Handling Flow

```mermaid
sequenceDiagram
    participant User
    participant Controller
    participant Service
    participant Nav as NavigationService
    participant ErrorView as Error View
    
    User->>Controller: Perform Action
    Controller->>Service: Service Call
    
    alt Service Success
        Service-->>Controller: Success Response
        Controller-->>User: Continue Normal Flow
    else Network Error
        Service-->>Controller: NetworkException
        Controller->>Nav: ShowError("Network connection failed")
        Nav->>ErrorView: ShowErrorAsync(message)
        ErrorView-->>User: Display Network Error
        User->>ErrorView: Click "Retry"
        ErrorView->>Controller: Retry Action
    else Service Unavailable
        Service-->>Controller: ServiceException
        Controller->>Nav: ShowError("Service temporarily unavailable")
        Nav->>ErrorView: ShowErrorAsync(message)
        ErrorView-->>User: Display Service Error
    else Authentication Error
        Service-->>Controller: AuthenticationException
        Controller->>Nav: ReplaceCurrentAsync("QRLogin")
        Nav-->>User: Navigate to Login
    else Unknown Error
        Service-->>Controller: GeneralException
        Controller->>Nav: ShowError("An unexpected error occurred")
        Nav->>ErrorView: ShowErrorAsync(message)
        ErrorView-->>User: Display Generic Error
    end
```

## 📱 Device-Specific Authentication Flows

### AIHome Quick Login Flow
```mermaid
sequenceDiagram
    participant User
    participant AIHomeView as AIHome Login View
    participant LoginCtrl as LoginController
    participant Account as SamsungAccountService
    
    Note over User, Account: Optimized for 7"/9" horizontal displays
    
    User->>AIHomeView: Quick Action (Touch)
    AIHomeView->>LoginCtrl: OnDeviceSpecificAction("quick_login", credentials)
    LoginCtrl->>LoginCtrl: HandleAIHomeAction()
    LoginCtrl->>LoginCtrl: HandlePasswordLogin() [Simplified]
    LoginCtrl->>Account: LoginAsync(request)
    Account-->>LoginCtrl: LoginResult
    LoginCtrl-->>AIHomeView: Navigate to Compact Account View
```

### FamilyHub Enhanced Authentication Flow
```mermaid
sequenceDiagram
    participant User
    participant FHView as FamilyHub Login View
    participant LoginCtrl as LoginController
    participant Camera as Camera Service
    participant Account as SamsungAccountService
    
    Note over User, Account: Enhanced for 21"/32" vertical displays
    
    User->>FHView: Select QR Login
    FHView->>Camera: EnableCamera()
    Camera-->>FHView: Camera Ready
    FHView-->>User: Show QR Scanner with Preview
    User->>FHView: Point to QR Code
    FHView->>FHView: Process QR Code
    FHView->>LoginCtrl: OnDeviceSpecificAction("camera_qr", qrData)
    LoginCtrl->>LoginCtrl: HandleFamilyHubAction()
    LoginCtrl->>LoginCtrl: HandleQRLogin(extractedToken)
    LoginCtrl->>Account: LoginAsync(request)
    Account-->>LoginCtrl: LoginResult
    LoginCtrl-->>FHView: Navigate to Rich Account View
```

## 🔄 Session Management Flow

```mermaid
sequenceDiagram
    participant App as Application
    participant Session as SessionManager
    participant Account as SamsungAccountService
    participant Config as GlobalConfigService
    
    loop Session Monitoring
        App->>Config: GetPreferenceValue("session.timeout")
        Config-->>App: timeoutMinutes
        App->>Session: CheckSessionTimeout()
        Session->>Account: GetSessionTokenAsync(userId)
        Account-->>Session: sessionToken
        
        alt Session Valid
            Session-->>App: Session OK
        else Session Expired
            Session->>Account: RefreshSessionAsync(userId)
            Account-->>Session: refreshResult
            
            alt Refresh Success
                Session-->>App: Session Refreshed
            else Refresh Failed
                Session->>App: ForceLogout()
                App->>Account: LogoutAsync(userId)
                App->>App: NavigateToLogin()
            end
        end
    end
```

## 📊 Performance Monitoring Flow

```mermaid
sequenceDiagram
    participant User
    participant Controller
    participant Monitor as PerformanceMonitor
    participant Service
    
    User->>Controller: Initiate Action
    Controller->>Monitor: StartTimer("action_name")
    Controller->>Service: Service Call
    
    alt Fast Response (< 2s)
        Service-->>Controller: Response
        Controller->>Monitor: EndTimer() [Success]
        Monitor->>Monitor: Log Performance Metrics
    else Slow Response (> 2s)
        Service-->>Controller: Response
        Controller->>Monitor: EndTimer() [Slow]
        Monitor->>Monitor: Log Slow Response Warning
    else Timeout (> 30s)
        Service-->>Controller: TimeoutException
        Controller->>Monitor: EndTimer() [Timeout]
        Monitor->>Monitor: Log Timeout Error
        Controller-->>User: Show Timeout Error
    end
```

---

**Next**: [API Documentation](../api/README.md) for detailed service interfaces and methods.