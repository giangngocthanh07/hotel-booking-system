# HotelBooking Project - Backend & Application Structure

_Last updated: 21-Apr-2026 | Version: 1.4_

---

## 📁 Solution Overview

```
Hotel_Blazor/                          # Solution root (.NET 9)
├── HotelBooking.api/                  # ASP.NET Core Web API (entry point)
├── HotelBooking.application/          # Business logic layer (Clean Architecture)
├── HotelBooking.infrastructure/       # Data access layer (EF Core, Repositories)
├── HotelBooking.webapp/               # Blazor Server frontend
├── HotelBooking.test/                 # xUnit test project
├── Scripts/                           # SQL scripts and migration helpers
├── docs/
│   ├── architecture/                  # Authoritative architecture
│   ├── reference/project-structure/
│   ├── archive/                       # Historical documents
│   └── assets/screenshots/            # Project screenshots
├── README.md
└── HotelBooking.sln                   # Root solution file
```

---

## 📁 Application Layer — `HotelBooking.application`

```
HotelBooking.application/
├── DTOs/                              # Data Transfer Objects (Input/Output contracts)
│   ├── Base/                          # Shared DTOs: ApiResponse<T>, PagedResult<T>, PagingRequest
│   ├── Hotel/                         # Hotel DTOs: HotelRegistrationDTO, HotelAdditionalInfo
│   ├── Request/                       # Request-related DTOs
│   │   ├── Base/                      # Shared request DTOs and interfaces
│   │   ├── HotelApproval/             # ⭐ NEW - HotelRegistrationDetailDTO
│   │   └── UpgradeRequest/            # UpgradeRequestDTO
│   ├── Role/                          # Role DTOs and constants (RoleTypeConstDTO)
│   └── User/                          # User DTOs
│
├── Services/
│   ├── Base/                          # BaseManage.cs (abstract base for CRUD services)
│   │
│   ├── Domains/                       # ⭐ Business logic organized by domain
│   │   ├── AdminManagement/
│   │   │   ├── AmenityService.cs              (+ IAmenityService)
│   │   │   ├── ManagementAdminService.cs      (+ IManagementAdminService)  [Facade]
│   │   │   ├── PolicyService.cs               (+ IPolicyService)
│   │   │   ├── RoleService.cs                 (+ IRoleService)
│   │   │   ├── ServiceService.cs              (+ IServiceService)
│   │   │   └── RoomAttributes/
│   │   │       ├── BedTypeService.cs
│   │   │       ├── RoomAttributeFacade.cs     [Facade]
│   │   │       ├── RoomQualityService.cs
│   │   │       ├── RoomViewService.cs
│   │   │       └── UnitTypeService.cs
│   │   │
│   │   ├── Auth/
│   │   │   └── JwtAuthService.cs              (+ IJwtAuthService)
│   │   │
│   │   ├── HotelManagement/
│   │   │   └── HotelService.cs                (+ IHotelService)
│   │   │
│   │   ├── Media/
│   │   │   └── PhotoService.cs                (+ IPhotoService — Cloudinary)
│   │   │
│   │   ├── RequestManagement/         # ⭐ Request approval workflows (split by role)
│   │   │   ├── Admin/
│   │   │   │   ├── AdminHotelApprovalRequestService.cs  (+ IAdminHotelApprovalRequestService) [NEW]
│   │   │   │   └── AdminUpgradeRequestService.cs        (+ IAdminUpgradeRequestService)
│   │   │   ├── Base/                  # IBaseAdminRequestService<TDto> shared contract
│   │   │   ├── Customer/
│   │   │   │   └── CustomerUpgradeRequestService.cs     (+ ICustomerUpgradeRequestService)
│   │   │   └── RequestOverviewService.cs                (+ IRequestOverviewService)  [Dashboard stats]
│   │   │
│   │   ├── RoomManagement/
│   │   │   ├── RoomNameSuggestionService.cs   (+ IRoomNameSuggestionService)
│   │   │   └── RoomTypeService.cs             (+ IRoomTypeService)
│   │   │
│   │   └── UserManagement/
│   │       └── UserService.cs                 (+ IUserService)
│   │
│   └── Helpers/                       # Shared utilities and response factories
│       ├── Common/
│       │   ├── Messages/              # Centralized string constants
│       │   │   ├── MessageResponse.cs         # All domain message constants
│       │   │   └── README.md
│       │   ├── ApiResponseHandlerHelper.cs
│       │   ├── ResponseFactory.cs             # Builds ApiResponse<T> consistently
│       │   └── StatusCodeResponse.cs          # HTTP status code constants
│       ├── Hotel/
│       ├── RoomManagement/
│       │   ├── BedConfigurationHelper.cs
│       │   └── CapacityHelper.cs
│       ├── ManagementAdminHelper.cs
│       ├── PasswordHelper.cs
│       ├── PolicyHelper.cs
│       ├── ServiceHelper.cs
│       ├── User/
│       └── ValidationHelper.cs
│
├── Interfaces/                        # Pure contracts (no logic)
│   └── ICrudManage.cs
│
└── Validators/                        # FluentValidation rules (mirrored by domain)
    ├── AdminManagement/
    │   ├── Amenities/
    │   ├── Policies/
    │   ├── RoomAttributes/
    │   └── Services/
    ├── Common/
    │   ├── GetRoomAttributeRequestValidator.cs
    │   ├── ManageMenuRequestValidator.cs
    │   └── PagingRequestValidator.cs
    ├── RequestManagement/
    │   ├── Admin/                     # Admin-side request validators
    │   ├── Customer/                  # CustomerUpgradeRequestValidator.cs
    │   └── Owner/                     # ⭐ NEW - HotelRegistrationValidator.cs
    ├── RoomManagement/
    │   ├── RoomNameSuggestionValidator.cs
    │   └── RoomTypeValidator.cs
    ├── UserManagement/
    │   ├── Login/
    │   └── Register/
    └── README.md
```

### **Interface Placement Rule:**

- ✅ Service interfaces → **same file** as implementation (`IAmenityService` inside `AmenityService.cs`)
- ✅ Pure/shared contracts → `Interfaces/` or `Helpers/` folder

```csharp
// File: Services/Domains/AdminManagement/AmenityService.cs
public interface IAmenityService : ICrudManage<...> { }
public class AmenityService : BaseManage<...>, IAmenityService { }
```

---

## 📁 API Layer — `HotelBooking.api`

```
HotelBooking.api/
├── Controllers/
│   └── V1/                            # All routes versioned under /api/v1/
│       ├── Admin/                     # Requires [Authorize(Roles = "Admin")]
│       │   ├── AccountController.cs             (User/Owner account management)
│       │   ├── AdminHotelApprovalController.cs  (Approve/reject hotel registrations) [NEW]
│       │   ├── AdminUpgradeRequestController.cs (Review/approve/reject upgrade requests)
│       │   ├── ManagementController.cs          (Amenities, Policies, Services, Room Attrs)
│       │   ├── RequestOverviewController.cs     (Request dashboard stats & recent list)
│       │   └── RoleController.cs                (Role management)
│       │
│       ├── Customer/                  # Requires [Authorize(Roles = "Customer")]
│       │   └── CustomerUpgradeRequestController.cs  (Submit/cancel upgrade requests)
│       │
│       ├── Owner/                     # Requires [Authorize(Roles = "Owner")]
│       │   └── RoomTypesController.cs           (Room type CRUD for hotel owners)
│       │
│       └── Public/                    # No authentication required
│           ├── AuthenticationController.cs      (Login, Register)
│           └── HotelController.cs               (Search, browse hotels)
│
├── Middlewares/                       # GlobalExceptionMiddleware
├── Filters/                           # Swagger/OpenAPI filters
├── Extensions/                        # DI registration extension methods
│   ├── ApplicationServiceExtension.cs
│   ├── InfrastructureServiceExtension.cs
│   └── SwaggerServiceExtension.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json                   # DB connection, JWT secret, Cloudinary config
└── Program.cs                         # App startup, middleware pipeline, DI
```

### **Routing Convention:**

| Scope    | Route prefix          | Auth                 |
| -------- | --------------------- | -------------------- |
| Admin    | `api/v1/admin/...`    | `Roles = "Admin"`    |
| Customer | `api/v1/customer/...` | `Roles = "Customer"` |
| Owner    | `api/v1/owner/...`    | `Roles = "Owner"`    |
| Public   | `api/v1/...`          | Anonymous            |

---

## 📁 Infrastructure Layer — `HotelBooking.infrastructure`

```
HotelBooking.infrastructure/
├── Models/                            # EF Core entity classes (Database-First)
│   ├── Hotel.cs                       # ⭐ UPDATED — ApprovedBy, ApprovedAt fields added
│   ├── User.cs
│   ├── UpgradeRequest.cs
│   ├── ...
│   └── HotelBookingDBContext.cs       # EF Core DbContext with model configurations
│
├── Repositories/                      # Repository implementations
│   ├── Base/
│   │   └── Repository.cs             # Generic Repository<TEntity> : IRepository<TEntity>
│   ├── HotelRepository.cs             # IHotelRepository (GetByIdWithOwnerAsync, GetPagedWithUserAsync)
│   ├── UpgradeRequestRepository.cs    # IUpgradeRequestRepository (GetByIdWithUserAsync)
│   ├── UserRepository.cs
│   └── ...
│
├── UnitOfWork/
│   └── UnitOfWork.cs                  # IUnitOfWork — wraps DbContext.SaveChangesAsync()
│
└── Properties/
    └── launchSettings.json
```

---

## 🔄 Dependency Flow

```
HotelBooking.webapp  ──────►  HotelBooking.api
                                     │
                                     ▼
                         HotelBooking.application
                                     │
                                     ▼
                         HotelBooking.infrastructure
                                     │
                                     ▼
                               SQL Server (MSSQL)
```

---

## 📊 Implementation Status

| Domain                | Services                                                                 | API Controllers                      | Status      |
| --------------------- | ------------------------------------------------------------------------ | ------------------------------------ | ----------- |
| **AdminManagement**   | Amenity, Policy, Service, Role, BedType, RoomQuality, RoomView, UnitType | ManagementController, RoleController | ✅ Complete |
| **Auth**              | JwtAuthService                                                           | AuthenticationController             | ✅ Complete |
| **HotelManagement**   | HotelService                                                             | HotelController (Public)             | ✅ Complete |
| **Media**             | PhotoService (Cloudinary)                                                | —                                    | ✅ Complete |
| **RequestManagement** | AdminUpgradeRequestService,                                              | AdminUpgradeRequestController,       | ✅ Complete |
                          **AdminHotelApprovalRequestService** ⭐,                                  **AdminHotelApprovalController** ⭐, | ✅ Complete |
                          CustomerUpgradeRequestService,                                             CustomerUpgradeRequestController,    | ✅ Complete |
                          RequestOverviewService                                                   | RequestOverviewController            | ✅ Complete |
| **RoomManagement**    | RoomTypeService, RoomNameSuggestionService                               | RoomTypesController (Owner)          | ✅ Complete |
| **UserManagement**    | UserService                                                              | AccountController                    | ✅ Complete |
| **BookingManagement** | —                                                                        | —                                    | 🔲 Pending  |

---

## 📝 Key Changes in v1.4 (21-Apr-2026)

- ✅ **Hotel model** — Added `ApprovedBy (int?)`, `ApprovedAt (DateTime?)`, `ApprovedByNavigation` for audit trail
- ✅ **AdminHotelApprovalRequestService** — New service implementing `IBaseAdminRequestService<HotelRegistrationDetailDTO>` for hotel registration approval workflow
- ✅ **AdminHotelApprovalController** — New API controller with Approve/Reject/GetPaged/GetById endpoints
- ✅ **HotelRegistrationDetailDTO** — New response DTO with full hotel + owner + location data
- ✅ **HotelRegistrationValidator** — FluentValidation rules for hotel registration input (`Validators/RequestManagement/Owner/`)
- ✅ **RequestManagement/Base** — `IBaseAdminRequestService<TDto>` shared contract enabling polymorphic service pattern
- ✅ **MessageResponse** — Added `HotelApproval`, `AdminHotelApprovalRequestService` constant classes under `RequestManagement`
- ✅ Upgraded from **.NET 8 → .NET 9**

---

Created: 07-Mar-2026
Updated: 21-Apr-2026
Version: 1.4
