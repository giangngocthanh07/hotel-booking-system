## Validators Folder Structure

Cấu trúc thư mục Validators được tổ chức theo nhóm chức năng để dễ mở rộng và bảo trì.

### 📁 Cấu trúc Thư Mục

```
Validators/
├── Common/
│   ├── PagingRequestValidator.cs          # Validate phân trang chung
│   ├── ManageMenuRequestValidator.cs       # Validate menu request
│   └── GetRoomAttributeRequestValidator.cs # Validate room attribute request
│
└── AdminManagement/
    ├── Amenity/
    │   └── AmenityCreateOrUpdateValidator.cs
    │
    ├── Policy/
    │   └── PolicyValidator.cs
    │
    ├── Service/
    │   └── ServiceValidator.cs              # Chứa cả ServiceValidator, StdServiceValidator, AirportServiceValidator
    │
    └── RoomAttributes/
        ├── BedTypeValidator.cs
        ├── RoomViewValidator.cs
        ├── RoomQualityValidator.cs
        └── UnitTypeValidator.cs
```

### 📝 Hướng Dẫn Sử Dụng

#### Import Validators

```csharp
// Common Validators
using HotelBooking.application.Validators.Common;

// AdminManagement Validators
using HotelBooking.application.Validators.AdminManagement.Amenity;
using HotelBooking.application.Validators.AdminManagement.Policy;
using HotelBooking.application.Validators.AdminManagement.Service;
using HotelBooking.application.Validators.AdminManagement.RoomAttributes;
```

#### Đăng Ký DI trong Program.cs

```csharp
// Quét toàn bộ Assembly chứa Validators và đăng ký hết
builder.Services.AddValidatorsFromAssemblyContaining<AmenityCreateOrUpdateValidator>();
```

### 🚀 Cách Mở Rộng

#### Thêm Validator cho Module Mới

Ví dụ: Thêm validators cho User Management

1. Tạo thư mục mới:

```
Validators/
└── UserManagement/
    ├── Register/
    │   └── RegisterUserValidator.cs
    └── Login/
        └── LoginValidator.cs
```

2. Tạo file validator với namespace đúng:

```csharp
namespace HotelBooking.application.Validators.UserManagement.Register;

public class RegisterUserValidator : AbstractValidator<RegisterUserDTO>
{
    public RegisterUserValidator()
    {
        // Rules...
    }
}
```

3. Import và sử dụng trong service:

```csharp
using HotelBooking.application.Validators.UserManagement.Register;
```

### 📌 Quy Tắc

1. **Namespace**: Phải khớp với cấu trúc thư mục
   - Ví dụ: `Validators/AdminManagement/Service/` → `HotelBooking.application.Validators.AdminManagement.Service`

2. **Đặt tên file**:
   - Đặt theo tên class validator
   - Ví dụ: `ServiceValidator.cs`, `AmenityCreateOrUpdateValidator.cs`

3. **Tổ chức logic**:
   - Nhóm các validator liên quan vào cùng thư mục
   - Nếu một file chứa nhiều validator (ví dụ ServiceValidator, StdServiceValidator), đặt tên file theo tên validator chính

4. **Messages**:
   - Sử dụng `MessageResponse.*` từ file MessageResponse.cs
   - Không hardcode message string

### 🔗 Liên Quan

- [MessageResponse.cs](../Helpers/Hotel/MessageResponse.cs) - Tất cả các message constants
- [Program.cs](../../api/Program.cs) - Đăng ký DI cho validators
- [BaseManage.cs](../Base/BaseManage.cs) - Base class sử dụng validators

---

**Lần cập nhật cuối**: January 2026
