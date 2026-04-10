# HotelBooking Project - Structure Organization

_Last updated: 10-Apr-2026 | Version: 1.3_

---

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
│   │   ├── RequestManagement/     # ⭐ Upgrade request management (split by role)
│   │   │   ├── Admin/
│   │   │   │   └── AdminUpgradeRequestService.cs (+ IAdminUpgradeRequestService)
│   │   │   ├── Base/              # Shared request base types/abstractions
│   │   │   ├── Customer/
│   │   │   │   └── CustomerUpgradeRequestService.cs (+ ICustomerUpgradeRequestService)
│   │   │   └── RequestOverviewService.cs (+ IRequestOverviewService)
│   │   │
│   │   ├── RoomManagement/        # ⭐ NEW - Room type and unit management
│   │   │   ├── RoomNameSuggestionService.cs (+ IRoomNameSuggestionService)
│   │   │   └── RoomTypeService.cs (+ IRoomTypeService)
│   │   │
│   │   └── UserManagement/
│   │       └── UserService.cs (+ IUserService interface)
│   │
│   └── Helpers/                   # Shared utilities and response handlers
│       ├── ApiResponseHandlerHelper.cs
│       ├── BedTypeHelper.cs
│       ├── FileHelper.cs
│       ├── Hotel/
│       ├── IImageHelper.cs
│       ├── ManagementAdminHelper.cs
│       ├── Messages/
│       │   ├── AdminManagement/
│       │   ├── Common/
│       │   ├── UserManagement/
│       │   ├── MessageRegister.cs
│       │   ├── MessageResponse.cs
│       │   └── README.md
│       ├── PasswordHelper.cs
│       ├── PolicyHelper.cs
│       ├── Role/
│       ├── RoomManagement/        # ⭐ NEW - Room-specific helpers
│       │   ├── BedConfigurationHelper.cs
│       │   └── CapacityHelper.cs
│       ├── ServiceHelper.cs
│       ├── User/
│       └── ValidationHelper.cs    # (renamed from Validation.cs)
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
    ├── RequestManagement/         # ⭐ NEW - Request validation rules
    │   ├── Admin/
    │   └── Customer/
    ├── RoomManagement/            # ⭐ NEW - Room type validation rules
    │   ├── RoomNameSuggestionValidator.cs
    │   └── RoomTypeValidator.cs
    ├── UserManagement/
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
│       │   ├── AccountController.cs              (User/Owner accounts management)
│       │   ├── AdminUpgradeRequestController.cs  (Admin: review/approve/reject upgrade requests)
│       │   ├── ManagementController.cs           (Amenities, Policies, Services management)
│       │   ├── RequestOverviewController.cs      (Requests dashboard statistics)
│       │   └── RoleController.cs                 (Role management)
│       │
│       ├── Customer/              # Customer-authenticated endpoints
│       │   └── CustomerUpgradeRequestController.cs  (Customer: submit upgrade requests)
│       │
│       ├── Owner/                 # Owner-authenticated endpoints
│       │   └── RoomTypesController.cs            (Room type management for hotel owners)
│       │
│       └── Public/                # Public endpoints (no auth required)
│           ├── AuthenticationController.cs       (Login, Register)
│           └── HotelController.cs                (Search, View hotel details)
│
├── Middlewares/               # Custom middlewares (e.g., GlobalExceptionMiddleware)
├── Filters/                   # Swagger/API filters
├── Extensions/                # Extension methods for DI registration and setup
├── Properties/
│   └── launchSettings.json
├── appsettings.json               # Configuration (DB, JWT, Cloudinary)
└── Program.cs                     # Startup config, DI registration
```

### **Routing Convention:**

- Admin endpoints: `api/v1/admin/[controller]`
- Customer endpoints: `api/v1/customer/[controller]`
- Owner endpoints: `api/v1/owner/[controller]`
- Public endpoints: `api/v1/[controller]`

### **Key Changes since v1.2:**

- `UpgradeRequestController.cs` (Admin) → renamed to **`AdminUpgradeRequestController.cs`** for clarity
- `UpgradeRequestController.cs` (Public) → renamed to **`CustomerUpgradeRequestController.cs`**, moved to `Customer/`
- Added **`Owner/`** folder with `RoomTypesController.cs` for hotel owner room management

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
| **API Clarity**            | Clear distinction between V1/Admin, V1/Customer, V1/Owner, and V1/Public      |
| **No Confusion**           | Interfaces containing logic are placed with their services for easier tracking |

---

## 📝 Implementation Status

### ✅ Completed Domains:

| Domain               | Services                                    | API Controllers                         | Status        |
| -------------------- | ------------------------------------------- | --------------------------------------- | ------------- |
| **AdminManagement**  | Amenity, Policy, Service, Role, BedType...  | ManagementController, RoleController    | ✅ Complete   |
| **Auth**             | JwtAuthService                              | AuthenticationController                | ✅ Complete   |
| **HotelManagement**  | HotelService                                | HotelController (Public)                | ✅ Complete   |
| **Media**            | PhotoService (Cloudinary)                   | —                                       | ✅ Complete   |
| **RequestManagement**| AdminUpgradeRequestService, CustomerUpgradeRequestService, RequestOverviewService | AdminUpgradeRequestController, CustomerUpgradeRequestController, RequestOverviewController | ✅ Complete |
| **RoomManagement**   | RoomTypeService, RoomNameSuggestionService  | RoomTypesController (Owner)             | ✅ Complete   |
| **UserManagement**   | UserService                                 | AccountController                       | ✅ Complete   |

### 🔄 Pending Domains:

- **BookingManagement** — End-to-end booking flow, payment processing, review system.
- **Customer Portal** — Customer-specific functionalities (booking history, saved hotels).

---

## 🧹 Refactoring Status

The structural transition has been completed for all core modules as of v1.3:

- ✅ Entirely reorganized `Controllers` to `V1/Admin`, `V1/Customer`, `V1/Owner`, `V1/Public`
- ✅ Deleted and restructured all `Features` classes into `Domains/` (e.g., `AdminManagement/AmenityService.cs`)
- ✅ `RequestManagement` split into `Admin/` and `Customer/` sub-domains
- ✅ New `RoomManagement` domain added with RoomType and RoomNameSuggestion services
- ✅ New `RoomManagement` validators added (`RoomTypeValidator.cs`, `RoomNameSuggestionValidator.cs`)
- ✅ New `RoomManagement/` helpers subfolder added under `Helpers/`
- ✅ `Validation.cs` renamed to `ValidationHelper.cs` for consistency
- ✅ Moved supporting logic into `Helpers/` and `Base/` folders
- ✅ DI now closely follows the architecture

---
