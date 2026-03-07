# HotelBooking Project - Structure Organization

## 📁 Application Layer Structure

```
HotelBooking.application/
├── DTOs/                          # Data Transfer Objects
│   ├── Base/                      # Shared DTOs (ApiResponse, Pagination...)
│   ├── Hotel/                     # Hotel-related DTOs
│   ├── Request/                   # Request-related DTOs
│   ├── Role/                      # Role-related DTOs
│   └── User/                      # User-related DTOs
│
├── Services/
│   ├── Base/                      # BaseManage.cs (abstract)
│   │
│   ├── Domains/                   # ⭐ Business logic organized by domain
│   │   ├── AdminManagement/
│   │   │   ├── AmenityService.cs (+ IAmenityService interface)
│   │   │   ├── ManagementAdminService.cs (+ IManagementAdminService interface)
│   │   │   ├── PolicyService.cs (+ IPolicyService interface)
│   │   │   ├── RoleService.cs (+ IRoleService interface)
│   │   │   ├── RoomAttributes/
│   │   │   │   ├── BedTypeService.cs
│   │   │   │   ├── RoomAttributeFacade.cs
│   │   │   │   ├── RoomQualityService.cs
│   │   │   │   ├── RoomViewService.cs
│   │   │   │   └── UnitTypeService.cs
│   │   │   └── ServiceService.cs (+ IServiceService interface)
│   │   │
│   │   ├── Auth/
│   │   │   └── JwtAuthService.cs (+ IJwtAuthService interface)
│   │   │
│   │   ├── BookingManagement/     # TODO - booking, payment, review processes
│   │   │
│   │   ├── HotelManagement/
│   │   │   └── HotelService.cs (+ IHotelService interface)
│   │   │
│   │   ├── Media/
│   │   │   └── PhotoService.cs (+ IPhotoService interface)
│   │   │
│   │   ├── RequestManagement/     # Upgrade and approval request management
│   │   │   ├── Base/
│   │   │   ├── RequestOverviewService.cs
│   │   │   └── UpgradeRequestService.cs
│   │   │
│   │   └── UserManagement/
│   │       └── UserService.cs (+ IUserService interface)
│   │
│   ├── Helpers/                   # Navigation, Validation, Response, FileHelper...
│   │   ├── ApiResponseHandlerHelper.cs
│   │   ├── BedTypeHelper.cs
│   │   ├── FileHelper.cs
│   │   ├── Hotel/
│   │   ├── IImageHelper.cs
│   │   ├── ManagementAdminHelper.cs
│   │   ├── Messages/
│   │   │   ├── AdminManagement/
│   │   │   ├── Common/
│   │   │   ├── UserManagement/
│   │   │   ├── MessageRegister.cs
│   │   │   ├── MessageResponse.cs
│   │   │   └── README.md
│   │   ├── PasswordHelper.cs
│   │   ├── PolicyHelper.cs
│   │   ├── Role/
│   │   ├── ServiceHelper.cs
│   │   ├── User/
│   │   └── Validation.cs
│
├── Interfaces/                    # ⭐ Interfaces WITHOUT implementation logic
│   └── ICrudManage.cs
│
└── Validators/                    # FluentValidation rules
    ├── AdminManagement/
    │   ├── Amenities/
    │   ├── Policies/
    │   ├── RoomAttributes/
    │   └── Services/
    ├── Common/
    │   ├── GetRoomAttributeRequestValidator.cs
    │   ├── ManageMenuRequestValidator.cs
    │   └── PagingRequestValidator.cs
    ├── UserManagement/
    │   ├── CreateUpgradeRequestValidator.cs
    │   ├── Login/
    │   └── Register/
    └── README.md
```

### **Interface Rules:**

- ✅ Business logic interfaces → **Placed in the same file** as the implementation (e.g., `IHotelService` inside `HotelService.cs`)
- ✅ Interfaces purely with no logic / shared → Placed in the `Interfaces/` folder or `Helpers/`

**Example:**

```csharp
// File: Services/Domains/AdminManagement/AmenityService.cs
public interface IAmenityService : ICrudManage<...> { }

public class AmenityService : BaseManage<...>, IAmenityService { }
```

---

## 📁 API Layer Structure

```
HotelBooking.api/
├── Controllers/
│   └── V1/                        # API Version 1
│       ├── Admin/                 # Admin endpoints (Require Admin role)
│       │   ├── AccountController.cs          (User/Owner accounts management)
│       │   ├── ManagementController.cs       (Amenities, Policies, Services management)
│       │   ├── RequestOverviewController.cs  (Requests statistics)
│       │   ├── RoleController.cs             (Role management)
│       │   └── UpgradeRequestController.cs   (Owner upgrade requests handling)
│       │
│       ├── Customer/              # Customer endpoints
│       │   └── (Empty - will contain APIs for specific Customer functionalities)
│       │
│       └── Public/                # Public endpoints (no auth/basic auth)
│           ├── AuthenticationController.cs   (Login, Register transactions)
│           ├── HotelController.cs            (Search, View hotel info)
│           └── UpgradeRequestController.cs   (Create upgrade request from User)
│
├── Middlewares/               # Custom middlewares (e.g., GlobalExceptionMiddleware)
├── Filters/                   # Swagger/API filters
├── Extensions/                # Extension methods for DI, Setup
├── Properties/
│   └── launchSettings.json
├── appsettings.json               # Configuration
└── Program.cs                     # Startup config, DI registration
```

### **Routing Convention:**

- Admin endpoints: `api/v1/admin/[controller]`
- Customer endpoints: `api/v1/customer/[controller]`
- Public endpoints: `api/v1/[controller]` (or public)

---

## 🔄 Dependency Injection (Program.cs)

All Controllers and Services have been divided according to Domain-Driven principles, making it easier to map 1-1 interface and implementation when registering in `Program.cs`. Extension methods in the `Extensions/` folder (within the `api` project) will usually be responsible for setting up DI to keep `Program.cs` clean.

---

## 🎯 Benefits of the New Structure

| Benefit                    | Details                                                                        |
| -------------------------- | ------------------------------------------------------------------------------ |
| **Scalable**               | Add new feature = create a new domain folder                                   |
| **Searchability**          | Related code is in the same directory (DTOs, Services, Helpers)                |
| **Clear Responsibilities** | Each domain has its own responsibility, reducing bloat in `Services/`          |
| **Testable**               | Each service is independent, easier to mock                                    |
| **API Clarity**            | Clear distinction between V1/Admin, V1/Customer, and V1/Public                 |
| **No Confusion**           | Interfaces containing logic are placed with their services for easier tracking |

---

## 📝 Next Steps (Planning)

### Domains to implement next:

- **BookingManagement** - Booking process management, payment processing, review system (Pending implementation).
- **Customer/Owner Management** - Specific functionalities for each user type (might be distributed into controllers within `V1/Customer` or `V1/Owner`).

---

## 🧹 Refactoring Status

The structural transition has been completed for mostly all core modules:

- Entirely reorganized `Controllers` to `V1/Admin`, `V1/Customer`, `V1/Public`.
- Deleted and restructured all `Features` classes in the Application layer into the `Domains/` directory (e.g., `AdminManagement/AmenityService.cs`).
- Moved supporting logic into `Helpers/` and `Base/` folders.
- DI now closely follows the architecture.

---
