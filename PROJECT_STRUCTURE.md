# HotelBooking Project - Cấu Trúc Tổ Chức Mới

## 📁 Cấu Trúc Application Layer

```
HotelBooking.application/
├── DTOs/                          # Data Transfer Objects
│   ├── Common/                    # DTO dùng chung (ApiResponse, Pagination...)
│   ├── Hotel/                     # Hotel-related DTOs
│   ├── Admin/                     # Admin-specific DTOs
│   └── User/                      # User-related DTOs
│
├── Services/
│   ├── Base/                      # BaseManage, BaseService (abstract)
│   ├── Common/                    # Common services (Auth, Photo, Email...)
│   │   ├── JwtAuthService.cs
│   │   ├── PhotoService.cs
│   │   └── ...
│   │
│   ├── Domains/                   # ⭐ Business logic tổ chức theo domain
│   │   ├── AdminManagement/
│   │   │   ├── ManagementAdminService.cs (+ IManagementAdminService interface)
│   │   │   ├── AmenityService.cs (+ IAmenityService interface)
│   │   │   ├── PolicyService.cs (+ IPolicyService interface)
│   │   │   └── ServiceService.cs (+ IServiceService interface)
│   │   │
│   │   ├── HotelManagement/
│   │   │   ├── IHotelService.cs (interface)
│   │   │   └── HotelService.cs (implementation tại Services/HotelService.cs)
│   │   │
│   │   ├── UserManagement/
│   │   │   ├── IUserService.cs (interface)
│   │   │   └── UserService.cs (implementation tại Services/UserService.cs)
│   │   │
│   │   ├── RequestManagement/    # TODO
│   │   │   └── ...
│   │   │
│   │   └── BookingManagement/    # TODO
│   │       └── ...
│   │
│   ├── Features/                  # Quản lý các tính năng phụ
│   │   ├── AmenityManage.cs       # Sẽ di chuyển sang AdminManagement
│   │   ├── PolicyManage.cs        # Sẽ di chuyển sang AdminManagement
│   │   ├── ServiceManage.cs       # Sẽ di chuyển sang AdminManagement
│   │   ├── RoomAttributes/
│   │   └── ...
│   │
│   ├── Helpers/                   # Validation, Response, FileHelper...
│   │   ├── ApiResponseHandlerHelper.cs
│   │   ├── FileHelper.cs
│   │   ├── Validation.cs
│   │   └── ...
│   │
│   ├── HotelService.cs            # (Sẽ di chuyển sang HotelManagement)
│   ├── UserService.cs             # (Sẽ di chuyển sang UserManagement)
│   └── ...
│
├── Interfaces/                    # ⭐ Interface KHÔNG có implementation logic
│   ├── ICrudManage.cs
│   ├── IPhotoService.cs
│   └── ITypedManage.cs
│
└── Mappings/                      # AutoMapper profiles (future)
    ├── HotelProfiles.cs
    └── ...
```

### **Quy tắc Interfaces:**
- ✅ Interface của nghiệp vụ → **Đặt trong cùng file** với implementation
- ✅ Interface không có logic → Bỏ vào `Interfaces/` folder (ICrudManage, IPhotoService...)

**Ví dụ:**
```csharp
// File: Services/Domains/AdminManagement/AmenityService.cs
public interface IAmenityService : ITypedManage<...> { }

public class AmenityService : BaseManage<...>, IAmenityService { }
```

---

## 📁 Cấu Trúc API Layer

```
HotelBooking.api/
├── Controllers/
│   ├── V1/                        # API Version 1
│   │   ├── Admin/                 # Admin endpoints
│   │   │   ├── AdminManagementController.cs  (Amenity, Policy, Service management)
│   │   │   ├── AdminRequestsController.cs    (Manage upgrade requests)
│   │   │   └── RolesController.cs            (Role management)
│   │   │
│   │   ├── Customer/              # Customer endpoints
│   │   │   └── RequestsController.cs         (Create/track upgrade requests)
│   │   │
│   │   └── Public/                # Public endpoints (no auth required)
│   │       ├── HotelsController.cs           (Search, view hotel info)
│   │       └── AuthenticationController.cs   (Login, register)
│   │
│   ├── Middlewares/               # Custom middlewares
│   │   └── PerformanceMiddleware.cs
│   │
│   ├── Filters/                   # Swagger/API filters
│   │   └── SwaggerFilters/
│   │       └── EnumSchemaFilter.cs
│   │
│   └── Extensions/                # (Future) Extension methods
│
├── Properties/
│   └── launchSettings.json
│
├── appsettings.json               # Configuration
└── Program.cs                     # Startup config, DI registration
```

### **Routing Convention:**
- Admin endpoints: `api/v1/admin/*`
- Customer endpoints: `api/v1/customer/*`
- Public endpoints: `api/v1/hotels`, `api/v1/auth/*`

---

## 🔄 Dependency Injection (Program.cs)

```csharp
// Admin Management Services
builder.Services.AddScoped<IManagementAdminService, ManagementAdminService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IServiceService, ServiceService>();

// Hotel Management
builder.Services.AddScoped<IHotelService, HotelService>();

// User Management
builder.Services.AddScoped<IUserService, UserService>();
```

---

## 🎯 Lợi Ích Của Cấu Trúc Mới

| Lợi ích | Chi tiết |
|---------|---------|
| **Dễ mở rộng** | Thêm feature mới = tạo folder domain mới |
| **Dễ tìm kiếm** | Code liên quan nằm cùng thư mục |
| **Rõ ràng trách nhiệm** | Mỗi domain có trách nhiệm riêng |
| **Dễ test** | Mỗi service độc lập, dễ mock |
| **API clarity** | V1/Admin, V1/Customer, V1/Public rõ ràng |
| **Không lẫn lộn** | Interface có logic nằm cùng service |

---

## 📝 Next Steps

### Để hoàn thành refactor:
1. ✅ Tạo cấu trúc Domains (AdminManagement, HotelManagement...)
2. ✅ Tạo Controllers V1 (Admin, Customer, Public)
3. ✅ Cập nhật Program.cs DI
4. ⏳ Di chuyển file HotelService.cs → Services/Domains/HotelManagement/
5. ⏳ Di chuyển file UserService.cs → Services/Domains/UserManagement/
6. ⏳ Xóa/cập nhật các file Features cũ (AmenityManage, PolicyManage...)
7. ⏳ Cập nhật tất cả import statements

### Domains cần implement tiếp:
- **RequestManagement** - Quản lý đơn yêu cầu từ customer
- **BookingManagement** - Quản lý booking, payment, review
- **OwnerManagement** - Dashboard owner, quản lý khách sạn

---

## 📚 File Mapping (Cũ → Mới)

| Cũ | Mới | Status |
|-----|-----|--------|
| Services/HotelService.cs | Services/Domains/HotelManagement/HotelService.cs | ⏳ Sẽ di chuyển |
| Services/UserService.cs | Services/Domains/UserManagement/UserService.cs | ⏳ Sẽ di chuyển |
| Services/Features/ManagementAdmin.cs | Services/Domains/AdminManagement/ManagementAdminService.cs | ✅ Done |
| Services/Features/AmenityManage.cs | Services/Domains/AdminManagement/AmenityService.cs | ✅ Done |
| Services/Features/PolicyManage.cs | Services/Domains/AdminManagement/PolicyService.cs | ✅ Done |
| Services/Features/ServiceManage.cs | Services/Domains/AdminManagement/ServiceService.cs | ✅ Done |
| Controllers/AccountController.cs | Controllers/V1/Public/AuthenticationController.cs | ✅ Done |
| Controllers/RequestController.cs | Controllers/V1/Customer/RequestsController.cs | ✅ Done |
| Controllers/RoleController.cs | Controllers/V1/Admin/RolesController.cs | ✅ Done |
| Controllers/HotelController.cs | Controllers/V1/Admin/AdminManagementController.cs (partial) | ⏳ Refactor needed |

---

## 🧹 Cleanup

Sau khi refactor hoàn tất:
- Xóa các file Features cũ (AmenityManage.cs, PolicyManage.cs, ServiceManage.cs)
- Xóa Controllers cũ (AccountController.cs, RequestController.cs, RoleController.cs, HotelController.cs)
- Xóa Interfaces folder nếu thích
- Giữ lại các file Services cũ chỉ làm import lại từ Domains

---

Created: 26-Jan-2026
Version: 1.0
