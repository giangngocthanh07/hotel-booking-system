# User Management - Design Document

## 1. Process Flow: User Registration

```mermaid
graph TD
    A["Guest: Enter registration info"] --> B["System: Validate Email/Phone format"]
    B -- "Invalid" --> C["Display Validation Errors on UI"]
    B -- "Valid" --> D["Send Request to API /register"]
    
    D --> E["RegisterService.RegisterCustomer"]
    E --> F{"Username/Email already exists?"}
    F -- "Yes" --> G["Error: Account already exists"]
    F -- "No" --> H["Hash Password (BCrypt)"]
    
    H --> I["Begin Transaction"]
    I --> J["Create new User (Status: Active)"]
    J --> K["Assign default role: Customer"]
    K --> L["Commit Transaction"]
    
    L --> M["Return ApiResponse Success"]
    M --> N["Success Message & Redirect to Login"]
```

## 2. Process Flow: User Login (JWT)

```mermaid
graph TD
    A["User: Enter Username & Password"] --> B["Send Request to API /login"]
    
    B --> C["LoginService.LoginUser"]
    C --> D{"User found by Username?"}
    
    D -- "No" --> E["Error: Invalid credentials"]
    D -- "Yes" --> F{"Verify Password Hash"}
    
    F -- "Wrong" --> E
    F -- "Correct" --> G["Call JwtAuthService.CreateToken"]
    
    G --> H["Generate JWT (Claims: UserId, Roles, Expire)"]
    H --> I["Return ApiResponse Success with Token"]
    
    I --> J["Frontend: Save Token in LocalStorage/Cookie"]
    J --> K["Redirect to Home or Dashboard"]
```

---

## 2. Wireframe (UI/UX Draft)

### 3.1. Login Page
```text
+-------------------------------------------------------+
|                    [ LOG IN ]                         |
+-------------------------------------------------------+
|  Username/Email:                                      |
|  [_________________________________________________]  |
|                                                       |
|  Password:                                            |
|  [_________________________________________________]  |
|                                                       |
|  [ ] Remember me                  [ Forgot Password? ]|
|                                                       |
|                     [ LOGIN ]                         |
|                                                       |
|  Don't have an account? [ REGISTER ]                  |
+-------------------------------------------------------+
```

### 3.2. User Profile Page
```text
+---------------------------------------------------------------------------------------+
|  MY PROFILE                                                                           |
+-----------------+---------------------------------------------------------------------+
| [D] Dashboard   |  PERSONAL INFORMATION                                               |
| [B] Bookings    |  +-----------------------+                                          |
| [P] Profile     |  |      [ AVATAR ]       |  Full Name: [ John Doe            ]     |
| [L] Logout      |  +-----------------------+  Email:     [ guest@example.com    ]     |
|                 |                             Phone:     [ 0901234567           ]     |
|                 |  [ CHANGE AVATAR ]          Birthday:  [ 01/01/1995           ]     |
|                 |                                                                     |
|                 |  [ CHANGE PASSWORD ]                            [ SAVE CHANGES ]    |
+-----------------+---------------------------------------------------------------------+
```

---

## 3. Technical Implementation Details
- **Authentication:** Token-based (JWT). Token sent in `Authorization: Bearer <token>` header.
- **Password Security:** Never store clear text passwords. Use secure hashing libraries.
- **Role-Based Access Control (RBAC):** Admin, Owner, and Customer roles restrict endpoint access via `[Authorize(Roles = "...")]`.
- **Identity Facade:** `UserService` acts as a Facade to hide registration/login complexity.
