# Message Response Structure Guide

## 📁 Cấu Trúc Thư Mục

```
Services/Helpers/
├── Messages/
│   ├── MessageResponse.cs         # Main consolidated messages file
│   ├── Common/                    # (Placeholder for future common messages)
│   ├── AdminManagement/           # (Placeholder for future admin-specific messages)
│   └── UserManagement/            # (Placeholder for future user-specific messages)
│
├── Hotel/
│   └── MessageResponse.cs         # [OLD - Keep for backward compatibility]
├── User/
│   ├── MessageLogin.cs            # [OLD - Keep for backward compatibility]
│   └── MessageRegister.cs         # [OLD - Keep for backward compatibility]
└── Role/
    └── RoleMessage.cs            # [OLD - Keep for backward compatibility]
```

## 🎯 Tổ Chức Messages

Tất cả messages đã được consolidate vào **`Services/Helpers/Messages/MessageResponse.cs`** với cấu trúc nested class theo domain:

### 1. **Common Messages** (`MessageResponse.Common`)

- Các message chung cho toàn bộ ứng dụng
- CRUD operations: GET, CREATE, UPDATE, DELETE
- Status messages: NOT_FOUND, BAD_REQUEST, etc.

```csharp
MessageResponse.Common.GET_SUCCESSFULLY
MessageResponse.Common.CREATE_SUCCESSFULLY
MessageResponse.Common.NOT_FOUND
```

### 2. **Admin Management Messages** (`MessageResponse.AdminManagement`)

#### 2.1 Amenity (`MessageResponse.AdminManagement.Amenity`)

```csharp
MessageResponse.AdminManagement.Amenity.EMPTY_NAME
MessageResponse.AdminManagement.Amenity.LONG_NAME
MessageResponse.AdminManagement.Amenity.EMPTY_TYPE
```

#### 2.2 Policy (`MessageResponse.AdminManagement.Policy`)

```csharp
MessageResponse.AdminManagement.Policy.EMPTY_NAME
MessageResponse.AdminManagement.Policy.LONG_NAME
MessageResponse.AdminManagement.Policy.INVALID_AMOUNT
```

#### 2.3 Service (`MessageResponse.AdminManagement.Service`)

```csharp
MessageResponse.AdminManagement.Service.EMPTY_NAME
MessageResponse.AdminManagement.Service.INVALID_TYPE
MessageResponse.AdminManagement.Service.EMPTY_UNIT
```

#### 2.4 Room Attributes (`MessageResponse.AdminManagement.RoomAttribute`)

```csharp
// BedType
MessageResponse.AdminManagement.RoomAttribute.BedType.EMPTY_NAME

// RoomView
MessageResponse.AdminManagement.RoomAttribute.RoomView.EMPTY_NAME

// RoomQuality
MessageResponse.AdminManagement.RoomAttribute.RoomQuality.EMPTY_NAME

// UnitType
MessageResponse.AdminManagement.RoomAttribute.UnitType.EMPTY_NAME

// Request validation
MessageResponse.AdminManagement.RoomAttribute.Request.INVALID_TYPE
```

#### 2.5 Role (`MessageResponse.AdminManagement.Role`)

```csharp
MessageResponse.AdminManagement.Role.NOT_FOUND
MessageResponse.AdminManagement.Role.ALREADY_EXISTS
MessageResponse.AdminManagement.Role.ADD_SUCCESS
MessageResponse.AdminManagement.Role.UPDATE_SUCCESS
MessageResponse.AdminManagement.Role.DELETE_SUCCESS
```

### 3. **User Management Messages** (`MessageResponse.UserManagement`)

#### 3.1 Login (`MessageResponse.UserManagement.Login`)

```csharp
MessageResponse.UserManagement.Login.SUCCESS
MessageResponse.UserManagement.Login.FAIL
MessageResponse.UserManagement.Login.USER_NOT_FOUND
MessageResponse.UserManagement.Login.PASSWORD_INCORRECT
MessageResponse.UserManagement.Login.USER_BLOCKED
MessageResponse.UserManagement.Login.USER_DELETED
```

#### 3.2 Register (`MessageResponse.UserManagement.Register`)

```csharp
MessageResponse.UserManagement.Register.SUCCESS
MessageResponse.UserManagement.Register.FAIL
MessageResponse.UserManagement.Register.USERNAME_EXIST
MessageResponse.UserManagement.Register.EMAIL_EXIST
MessageResponse.UserManagement.Register.INVALID_EMAIL
MessageResponse.UserManagement.Register.SHORT_PASSWORD
MessageResponse.UserManagement.Register.EMPTY_PASSWORD
MessageResponse.UserManagement.Register.UPPERCASE_LETTER_PASSWORD
MessageResponse.UserManagement.Register.LOWERCASE_LETTER_PASSWORD
MessageResponse.UserManagement.Register.SPECIAL_CHARACTER_PASSWORD
```

#### 3.3 User Management (`MessageResponse.UserManagement.User`)

```csharp
MessageResponse.UserManagement.User.NOT_FOUND
MessageResponse.UserManagement.User.ALREADY_EXISTS
MessageResponse.UserManagement.User.ADD_SUCCESS
MessageResponse.UserManagement.User.UPDATE_SUCCESS
MessageResponse.UserManagement.User.DELETE_SUCCESS
```

### 4. **Pagination Messages** (`MessageResponse.Pagination`)

```csharp
MessageResponse.Pagination.MISSING_PAGE_INDEX
MessageResponse.Pagination.INVALID_PAGE_INDEX
MessageResponse.Pagination.INVALID_PAGE_SIZE
MessageResponse.Pagination.PAGE_SIZE_TOO_LARGE
```

### 5. **Menu Messages** (`MessageResponse.ManageMenu`)

```csharp
MessageResponse.ManageMenu.INVALID_MODULE
```

## 🔄 Backward Compatibility

Để tránh break code cũ, có **nested class aliases** cho các class cũ:

```csharp
// Old way (still works)
MessageLogin.LOGIN_SUCCESS  // Sẽ route đến MessageResponse.UserManagement.Login.SUCCESS
MessageRegister.USERNAME_EXIST  // Sẽ route đến MessageResponse.UserManagement.Register.USERNAME_EXIST
RoleMessage.ROLE_ADD_SUCCESS  // Sẽ route đến MessageResponse.AdminManagement.Role.ADD_SUCCESS

// New way (recommended)
MessageResponse.UserManagement.Login.SUCCESS
MessageResponse.UserManagement.Register.USERNAME_EXIST
MessageResponse.AdminManagement.Role.ADD_SUCCESS
```

## 📝 Cách Sử Dụng

### Trong Validators

```csharp
RuleFor(x => x.Name)
    .NotEmpty().WithMessage(MessageResponse.Amenity.EMPTY_NAME)
    .MaximumLength(20).WithMessage(MessageResponse.Amenity.LONG_NAME);
```

### Trong Services

```csharp
// Register
if (user.Email == existingUser.Email)
{
    return ResponseFactory.Failure<RegisterResponseDTO>(
        StatusCodeResponse.Conflict,
        MessageResponse.UserManagement.Register.EMAIL_EXIST
    );
}

// Login
if (user == null)
{
    return ResponseFactory.Failure<LoginResponseDTO>(
        StatusCodeResponse.NotFound,
        MessageResponse.UserManagement.Login.USER_NOT_FOUND
    );
}
```

### Trong Controllers

```csharp
var result = await _roleService.AddRoleAsync(newRole);
if (result.IsSuccess)
{
    return Ok(MessageResponse.AdminManagement.Role.ADD_SUCCESS);
}
```

## 🚀 Cách Mở Rộng

### Thêm Messages Cho Module Mới

**Bước 1**: Thêm nested class trong `MessageResponse.cs`

```csharp
public static class MessageResponse
{
    public static class BookingManagement
    {
        public static class Booking
        {
            public const string BOOKING_NOT_FOUND = "Đặt phòng không tìm thấy!";
            public const string BOOKING_CANCELLED = "Đặt phòng đã được hủy!";
            public const string INVALID_CHECK_IN_DATE = "Ngày nhận phòng không hợp lệ!";
            // ... more messages
        }

        public static class Payment
        {
            public const string PAYMENT_FAILED = "Thanh toán thất bại!";
            public const string PAYMENT_PENDING = "Thanh toán đang chờ xử lý!";
            // ... more messages
        }
    }
}
```

**Bước 2**: Sử dụng trong Validators/Services

```csharp
RuleFor(x => x.CheckInDate)
    .GreaterThan(DateTime.Now)
    .WithMessage(MessageResponse.BookingManagement.Booking.INVALID_CHECK_IN_DATE);
```

## 📌 Best Practices

1. **Luôn sử dụng `const string`** - không dùng `public static string` (vì const được compile-time optimization)

2. **Đặt tên consistent**:
   - Success messages: `*_SUCCESS`
   - Error messages: `*_FAILED` hoặc `*_ERROR`
   - Validation: `EMPTY_*`, `INVALID_*`, `MISSING_*`

3. **Sắp xếp messages logic**:

   ```csharp
   // Success messages trước
   public const string ADD_SUCCESS = "...";
   public const string UPDATE_SUCCESS = "...";

   // Failure/Error messages sau
   public const string NOT_FOUND = "...";
   public const string ALREADY_EXISTS = "...";
   ```

4. **Messages tiếng Việt và tiếng Anh**:
   - Tiếng Việt: Dùng cho lỗi validation, user-facing messages
   - Tiếng Anh: Dùng cho API logs, system messages (nếu cần)
   - **Hiện tại project sử dụng tiếng Việt**, giữ consistent

5. **Tránh hardcode messages**:

   ```csharp
   // ❌ BAD
   return ResponseFactory.Failure<UserDTO>(StatusCodeResponse.NotFound, "User not found!");

   // ✅ GOOD
   return ResponseFactory.Failure<UserDTO>(
       StatusCodeResponse.NotFound,
       MessageResponse.UserManagement.User.NOT_FOUND
   );
   ```

## 🔗 Liên Quan

- [MessageResponse.cs](./Messages/MessageResponse.cs) - Main consolidated messages file
- [Validators Structure](../../Validators/README.md) - Cách tổ chức Validators
- [Program.cs](../../../../../../api/Program.cs) - Dependency Injection setup

---

**Lần cập nhật cuối**: January 2026  
**Status**: ✅ Consolidated & Production Ready
