# Architecture Documentation

## 🏗️ System Architecture Overview

The Samsung Account UI application follows a layered MVC architecture designed for Tizen devices with clear separation of concerns and device-specific optimizations.

## 📐 High-Level Architecture

```mermaid
graph TB
    subgraph "Presentation Layer"
        A1[FamilyHub Views] 
        A2[AIHome Views]
        A3[Common Views]
    end
    
    subgraph "Controller Layer"
        B1[Authentication Controllers]
        B2[Account Controllers]
        B3[Navigation Controller]
    end
    
    subgraph "Service Layer"
        C1[Samsung Account Service]
        C2[Config Service]
        C3[Device Detection Service]
        C4[Navigation Service]
    end
    
    subgraph "Data Layer"
        D1[Samsung Account SES API]
        D2[Tizen Preferences]
        D3[Device System APIs]
    end
    
    A1 --> B1
    A2 --> B1
    A3 --> B1
    B1 --> C1
    B2 --> C1
    B3 --> C4
    C1 --> D1
    C2 --> D2
    C3 --> D3
    
    style A1 fill:#e3f2fd
    style A2 fill:#e8f5e8
    style A3 fill:#fff3e0
    style B1 fill:#f3e5f5
    style B2 fill:#f3e5f5
    style B3 fill:#f3e5f5
    style C1 fill:#e0f2f1
    style C2 fill:#e0f2f1
    style C3 fill:#e0f2f1
    style C4 fill:#e0f2f1
```

## 🎯 Core Components Class Diagram

```mermaid
classDiagram
    class BaseController {
        <<abstract>>
        +NavigationService: INavigationService
        +AccountService: ISamsungAccountService
        +ConfigService: IGlobalConfigService
        +DeviceType: DeviceType
        +LoadAsync()* Task
        +HandleInputAsync(object)* Task
        +OnDeviceSpecificAction(string, object) Task
    }
    
    class LoginController {
        +HandleQRLogin(string) Task
        +HandlePasswordLogin(string, string) Task
        +HandleGoogleLogin(string) Task
        -ProcessLogin(LoginRequest) Task
    }
    
    class AccountInfoController {
        -_currentAccountState: AccountState
        +LoadAccountInfo() Task
        +HandleUserSwitch(string) Task
        +HandleLogoutUser(string) Task
        +HandleAddNewUser() Task
    }
    
    class BaseView {
        <<abstract>>
        +DeviceType: DeviceType
        +NavigationService: INavigationService
        +ConfigService: IGlobalConfigService
        +LoadContent()* void
        +UpdateForDevice(DeviceType)* void
        +ApplyConfigSettings() void
    }
    
    class ISamsungAccountService {
        <<interface>>
        +LoginAsync(LoginRequest) Task~LoginResult~
        +LogoutAsync(string) Task~bool~
        +GetAllAccountListAsync() Task~List~SamsungAccount~~
        +SetDefaultUserAsync(string) Task~bool~
        +ChangePasswordAsync(string, string, string) Task~bool~
    }
    
    class MockSamsungAccountService {
        -_mockAccounts: List~SamsungAccount~
        -_mockPasswords: Dictionary~string, string~
        +LoginAsync(LoginRequest) Task~LoginResult~
        +LogoutAsync(string) Task~bool~
        +GetAllAccountListAsync() Task~List~SamsungAccount~~
    }
    
    BaseController <|-- LoginController
    BaseController <|-- AccountInfoController
    ISamsungAccountService <|.. MockSamsungAccountService
    BaseController --> ISamsungAccountService
    BaseView --> INavigationService
```

## 🔄 Data Flow Architecture

```mermaid
flowchart LR
    subgraph "UI Layer"
        A[User Interaction]
    end
    
    subgraph "Controller Layer"
        B[Controller.HandleInputAsync]
        C[Validation]
        D[Business Logic]
    end
    
    subgraph "Service Layer"
        E[Service Method Call]
        F[Data Processing]
    end
    
    subgraph "API Layer"
        G[Samsung Account API]
        H[Tizen System API]
    end
    
    subgraph "Response Flow"
        I[API Response]
        J[Service Response]
        K[Controller Processing]
        L[View Update]
    end
    
    A --> B
    B --> C
    C --> D
    D --> E
    E --> F
    F --> G
    F --> H
    
    G --> I
    H --> I
    I --> J
    J --> K
    K --> L
    L --> A
```

## 📱 Device-Specific Architecture

### FamilyHub Architecture (21"/32" Vertical)
```mermaid
graph TB
    subgraph "FamilyHub Presentation"
        A1[Expanded User Cards]
        A2[Rich Profile Views]
        A3[Detailed Navigation]
        A4[Camera Integration]
    end
    
    subgraph "FamilyHub Controllers"
        B1[Enhanced Authentication]
        B2[Multi-User Management]
        B3[Advanced Features]
    end
    
    subgraph "FamilyHub Services"
        C1[Camera Service]
        C2[Rich Media Service]
        C3[Advanced Config]
    end
    
    A1 --> B1
    A2 --> B2
    A3 --> B3
    A4 --> B1
    B1 --> C1
    B2 --> C2
    B3 --> C3
```

### AIHome Architecture (7"/9" Horizontal)
```mermaid
graph TB
    subgraph "AIHome Presentation"
        A1[Compact Login]
        A2[Simple User List]
        A3[Quick Actions]
        A4[Minimal Navigation]
    end
    
    subgraph "AIHome Controllers"
        B1[Streamlined Auth]
        B2[Quick User Switch]
        B3[Essential Features]
    end
    
    subgraph "AIHome Services"
        C1[Optimized Config]
        C2[Fast Response]
        C3[Minimal Resources]
    end
    
    A1 --> B1
    A2 --> B2
    A3 --> B3
    A4 --> B1
    B1 --> C1
    B2 --> C2
    B3 --> C3
```

## 🔧 Service Architecture Details

### Samsung Account Service Layer
```mermaid
classDiagram
    class ISamsungAccountService {
        <<interface>>
        +LoginAsync(LoginRequest) Task~LoginResult~
        +LogoutAsync(string) Task~bool~
        +FetchProfileInfoAsync(string) Task~SamsungAccount~
        +GetAllAccountListAsync() Task~List~SamsungAccount~~
        +GetAllProfileInfoAsync() Task~List~SamsungAccount~~
        +SetDefaultUserAsync(string) Task~bool~
        +GetDefaultUserAsync() Task~SamsungAccount~
        +ChangePasswordAsync(string, string, string) Task~bool~
        +VerifyPasswordAsync(string, string) Task~bool~
        +IsUserLoggedInAsync(string) Task~bool~
        +GetSessionTokenAsync(string) Task~string~
    }
    
    class MockSamsungAccountService {
        -_mockAccounts: List~SamsungAccount~
        -_mockPasswords: Dictionary~string, string~
        -_mockSessions: Dictionary~string, string~
        +LoginAsync(LoginRequest) Task~LoginResult~
        +LogoutAsync(string) Task~bool~
        -HandlePasswordLogin(LoginRequest) Task~LoginResult~
        -HandleQRLogin(LoginRequest) Task~LoginResult~
        -HandleGoogleLogin(LoginRequest) Task~LoginResult~
        -GenerateSessionToken(string) string
    }
    
    class RealSamsungAccountService {
        -_sesApiClient: SESApiClient
        -_authTokenManager: AuthTokenManager
        +LoginAsync(LoginRequest) Task~LoginResult~
        +LogoutAsync(string) Task~bool~
        -CallSESApi(string, object) Task~ApiResponse~
        -HandleApiError(ApiError) LoginResult
    }
    
    ISamsungAccountService <|.. MockSamsungAccountService
    ISamsungAccountService <|.. RealSamsungAccountService
```

### Configuration Service Architecture
```mermaid
classDiagram
    class IGlobalConfigService {
        <<interface>>
        +GetPreferenceValue~T~(string, T) T
        +SetPreferenceValue~T~(string, T) bool
        +HasPreferenceKey(string) bool
        +IsMultiUserEnabled: bool
        +IsQRLoginEnabled: bool
        +IsGoogleLoginEnabled: bool
        +DefaultUITheme: string
        +MaxUserAccounts: int
    }
    
    class GlobalConfigService {
        +GetPreferenceValue~T~(string, T) T
        +SetPreferenceValue~T~(string, T) bool
        +IsMultiUserEnabled: bool
        +IsQRLoginEnabled: bool
        +IsGoogleLoginEnabled: bool
    }
    
    class TizenPreferenceHelper {
        <<static>>
        -_mockPreferences: Dictionary~string, object~
        +GetValue~T~(string, T) T
        +SetValue~T~(string, T) bool
        +Contains(string) bool
        +Remove(string) bool
        +Keys: class
    }
    
    IGlobalConfigService <|.. GlobalConfigService
    GlobalConfigService --> TizenPreferenceHelper
```

## 🚦 Navigation Architecture

```mermaid
classDiagram
    class INavigationService {
        <<interface>>
        +NavigateToAsync(string, object) Task
        +NavigateBackAsync() Task
        +ReplaceCurrentAsync(string, object) Task
        +ShowLoadingAsync(string) Task
        +HideLoadingAsync() Task
        +ShowErrorAsync(string, string) Task
        +CanNavigateBack: bool
        +CurrentScreen: string
        +NavigationChanged: EventHandler~NavigationEventArgs~
    }
    
    class NavigationService {
        -_navigationStack: Stack~string~
        -_viewFactory: ViewFactory
        -_deviceType: DeviceType
        -_currentView: BaseView
        +NavigateToAsync(string, object) Task
        +NavigateBackAsync() Task
        -InitializeViewWithParameters(BaseView, object) Task
    }
    
    class ViewFactory {
        -_navigationService: INavigationService
        -_configService: IGlobalConfigService
        +CreateView(string, DeviceType) BaseView
        +CreateLoadingView(DeviceType) BaseView
        +CreateErrorView(DeviceType) BaseView
        -CreateAIHomeView(string) BaseView
        -CreateFamilyHubView(string) BaseView
    }
    
    class NavigationController {
        -_deviceDetectionService: IDeviceDetectionService
        -_viewFactory: ViewFactory
        -_currentDevice: DeviceInfo
        +InitializeApplication() Task
        +HandleDeepLink(string) Task
        +HandleBackNavigation() Task
        -DetermineInitialNavigation() Task
    }
    
    INavigationService <|.. NavigationService
    NavigationService --> ViewFactory
    NavigationController --> INavigationService
    NavigationController --> ViewFactory
```

## 📊 Model Architecture

```mermaid
classDiagram
    class SamsungAccount {
        +UserId: string
        +Email: string
        +DisplayName: string
        +ProfilePictureUrl: string
        +IsActiveUser: bool
        +LastLoginTime: DateTime
        +SamsungAccount()
        +SamsungAccount(string, string, string)
    }
    
    class AccountState {
        +AllAccounts: List~SamsungAccount~
        +ActiveUser: SamsungAccount
        +HasAccounts: bool
        +SupportsMultiUser: bool
        +AccountCount: int
        +GetActiveUser() SamsungAccount
        +SetActiveUser(string) bool
        +RemoveUser(string) bool
        +AddUser(SamsungAccount) void
    }
    
    class LoginRequest {
        +Type: AuthenticationType
        +Email: string
        +Password: string
        +QRToken: string
        +GoogleToken: string
        +DeviceId: string
        +RequestTime: DateTime
        +IsValid() bool
    }
    
    class LoginResult {
        +IsSuccess: bool
        +User: SamsungAccount
        +ErrorMessage: string
        +ErrorType: AuthenticationError
        +SessionToken: string
        +Status: LoginStatus
        +Success(SamsungAccount, string)$ LoginResult
        +Failure(AuthenticationError, string)$ LoginResult
    }
    
    class DeviceInfo {
        +Type: DeviceType
        +DeviceId: string
        +Dimensions: ScreenDimensions
        +SupportsMultiUser: bool
        +ModelName: string
        +OSVersion: string
        +IsAIHomeDevice: bool
        +IsFamilyHubDevice: bool
    }
    
    AccountState *-- SamsungAccount
    LoginResult *-- SamsungAccount
```

## 🔄 State Management

### Authentication State Flow
```mermaid
stateDiagram-v2
    [*] --> NoUser: App Start
    NoUser --> Authenticating: Login Attempt
    Authenticating --> SingleUser: Login Success
    Authenticating --> NoUser: Login Failed
    SingleUser --> MultiUser: Add User
    SingleUser --> Authenticating: Switch User
    MultiUser --> SingleUser: Remove User
    MultiUser --> Authenticating: Switch User
    SingleUser --> NoUser: Logout Last User
    MultiUser --> MultiUser: Logout User
    MultiUser --> NoUser: Logout All Users
```

### Navigation State Flow
```mermaid
stateDiagram-v2
    [*] --> Initial: App Launch
    Initial --> Login: No Users
    Initial --> AccountInfo: Has Users
    Login --> AccountInfo: Login Success
    AccountInfo --> Login: Logout All
    AccountInfo --> UserSwitch: Multi-User
    AccountInfo --> ChangePassword: Password Change
    AccountInfo --> LogoutConfirm: Logout
    UserSwitch --> AccountInfo: User Selected
    ChangePassword --> AccountInfo: Password Changed
    LogoutConfirm --> AccountInfo: Logout Complete
    LogoutConfirm --> Login: Last User Logout
```

## 🎯 Key Design Patterns

### Factory Pattern
- **ViewFactory**: Creates device-specific views
- **ControllerFactory**: Instantiates controllers with dependencies

### Strategy Pattern
- **Device-specific behavior**: AIHome vs FamilyHub implementations
- **Authentication methods**: QR, Password, Google strategies

### Observer Pattern
- **Navigation events**: Screen change notifications
- **Configuration changes**: Preference update notifications

### Repository Pattern
- **Samsung Account Service**: Abstracts data access layer
- **Mock vs Real implementations**: Seamless switching

## 🔧 Dependency Injection Structure

```mermaid
graph TB
    A[Application Container] --> B[Services]
    A --> C[Controllers]
    A --> D[Views]
    
    B --> B1[ISamsungAccountService]
    B --> B2[IGlobalConfigService]
    B --> B3[IDeviceDetectionService]
    B --> B4[INavigationService]
    
    C --> C1[LoginController]
    C --> C2[AccountInfoController]
    C --> C3[NavigationController]
    
    D --> D1[ViewFactory]
    D --> D2[NavigationService]
    
    C1 --> B1
    C1 --> B2
    C2 --> B1
    C3 --> B3
    D1 --> B2
    D2 --> D1
```

## 📈 Performance Considerations

### Memory Management
- **View lifecycle management**: Proper disposal of views
- **Service singleton pattern**: Shared service instances
- **Event handler cleanup**: Prevent memory leaks

### Caching Strategy
- **User profiles**: Cache frequently accessed user data
- **Configuration**: Cache preference values
- **Navigation state**: Maintain navigation history

### Async/Await Pattern
- **Non-blocking UI**: All I/O operations are async
- **Task composition**: Parallel operations where possible
- **Cancellation support**: Timeout and user cancellation

---

**Next**: [Sequence Diagrams](../diagrams/authentication-flows.md) for detailed interaction flows.