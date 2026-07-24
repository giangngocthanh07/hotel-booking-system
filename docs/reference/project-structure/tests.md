# HotelBooking Project - Test Layer Structure

_Last updated: 21-Apr-2026 | Version: 1.4_

---

## 📁 Test Layer Structure Overview

This document outlines the architecture and organization of the testing layer `HotelBooking.test`.

```
HotelBooking.test/                     # xUnit test project (.NET 9)
├── IntegrationTests/                  # Tests verifying cross-layer interactions
│   ├── Infracstructure/               # EF Core / DB integration tests
│   └── Service/                       # Service + repo integration tests
│       └── UserManagement/
│
├── UnitTests/                         # Isolated tests for individual components
│   ├── Common/
│   │   └── BaseServiceTest.cs         # Shared test base (Moq setup, verify helpers)
│   │
│   ├── Helpers/                       # Tests for pure utility classes
│   │   └── RoomManagement/
│   │       ├── BedConfigurationHelperTests.cs
│   │       └── CapacityHelperTests.cs
│   │
│   ├── Services/                      # Business logic unit tests (mirrors Domains/)
│   │   ├── AdminManagement/
│   │   │   ├── AmenityServiceTests.cs
│   │   │   ├── ManagementAdminServiceTests.cs
│   │   │   ├── PolicyServiceTests.cs
│   │   │   └── RoomAttributes/
│   │   │       ├── BedTypeServiceTests.cs
│   │   │       ├── RoomQualityServiceTests.cs
│   │   │       ├── RoomViewServiceTests.cs
│   │   │       ├── ServiceServiceTests.cs
│   │   │       └── UnitTypeServiceTests.cs
│   │   │
│   │   ├── RequestManagement/
│   │   │   ├── Admin/
│   │   │   │   ├── AdminHotelApprovalRequestServiceTests.cs  ⭐ NEW
│   │   │   │   └── AdminUpgradeRequestServiceTests.cs
│   │   │   ├── Customer/
│   │   │   │   └── CustomerUpgradeRequestServiceTests.cs
│   │   │   └── Owner/
│   │   │       └── OwnerHotelRequestServiceTests.cs          (if applicable)
│   │   │
│   │   ├── RoomManagement/
│   │   │   ├── RoomNameSuggestionTests.cs
│   │   │   └── RoomTypeServiceTests.cs
│   │   │
│   │   └── UserManagement/
│   │       ├── Login/
│   │       ├── Register/
│   │       └── UserServiceTests.cs
│   │
│   └── Validators/                    # FluentValidation rule tests (mirrors Validators/)
│       ├── AdminManagement/
│       │   └── RoomAttributes/
│       │       └── (validators tests)
│       ├── Common/
│       │   └── PagingRequestValidatorTests.cs
│       ├── RequestManagement/
│       │   ├── Admin/
│       │   ├── Customer/
│       │   │   └── CustomerUpgradeRequestValidatorTests.cs
│       │   └── Owner/
│       │       └── HotelRegistrationValidatorTests.cs        ⭐ NEW
│       ├── RoomManagement/
│       │   ├── RoomNameSuggestionValidatorTests.cs
│       │   └── RoomTypeValidatorTests.cs
│       └── UserManagement/
│           ├── Login/
│           └── Register/
│
├── TestResults/                       # Generated test reports (gitignored)
├── appsettings.test.json              # Test-specific DB connection strings
└── HotelBooking.test.csproj           # Test project reference dependencies
```

---

## 🧭 Testing Conventions

### 1. Unit Tests — Services (`UnitTests/Services/`)

- Test isolated business logic (Application layer services) with **no real DB or network calls**.
- All repository/UoW dependencies are mocked with **Moq**.
- Mirror the `Services/Domains/` folder structure exactly.
- Naming: `MethodName_StateUnderTest_ExpectedBehavior`

```csharp
// Example:
ApproveRequest_ValidRequest_ShouldReturnSuccess()
GetPagedRequests_InvalidStatus_ShouldReturnBadRequest()
```

### 2. Unit Tests — Validators (`UnitTests/Validators/`)

- Test `FluentValidation` rule correctness for each input DTO.
- Use `FluentValidation.TestHelper` (`ShouldHaveValidationErrorFor`, `ShouldNotHaveAnyValidationErrors`).
- Mirror the `Validators/` folder in `HotelBooking.application`.
- Covers: valid baseline, boundary values, required-field empty, format violations.

### 3. Unit Tests — Helpers (`UnitTests/Helpers/`)

- Tests for pure helper/utility functions with **no external dependencies**.
- `BedConfigurationHelper`, `CapacityHelper`, etc.

### 4. Integration Tests (`IntegrationTests/`)

- Verify interaction between layers (e.g., EF Core ↔ real DB).
- Uses a dedicated test database from `appsettings.test.json`.
- Never shares configuration with development or production.

### 5. BaseServiceTest Pattern

```csharp
// All service test classes inherit from BaseServiceTest
public class MyServiceTests : BaseServiceTest
{
    // BaseServiceTest provides:
    // - _mockUnitOfWork      (Moq<IUnitOfWork>, pre-setup SaveChangesAsync → 1)
    // - _fixture             (AutoFixture)
    // - Verify_Saved(n)      / Verify_Never_Saved()
    // - Verify_Repo_UpdateAsync<TRepo, TEntity>(mock, n)
    // - Verify_Repo_Never_UpdateAsync<TRepo, TEntity>(mock)
    // - Verify_Repo_AddAsync<TRepo, TEntity>(mock, n)
    // - Verify_Repo_AnyAsync<TRepo, TEntity>(mock, n)
}
```

---

## 📦 Test Dependencies

| Package | Purpose |
|---------|---------|
| `xUnit` | Test framework (.NET 9) |
| `Moq` | Repository/service mocking |
| `FluentAssertions` | Readable assertion syntax (`Should().Be(...)`) |
| `FluentValidation.TestHelper` | Validator-specific assertions |
| `AutoFixture` | Auto-generate test data |
| `Microsoft.EntityFrameworkCore.InMemory` | In-memory DB for integration tests |

---

## 📊 Test Coverage Status

| Domain | Service Tests | Validator Tests | Helper Tests |
|--------|--------------|-----------------|--------------|
| **AdminManagement** | ✅ Amenity, Policy, ManagementAdmin, RoomAttributes | 🔲 Pending | — |
| **RequestManagement** | ✅ AdminUpgrade, **AdminHotelApproval** ⭐, CustomerUpgrade | ✅ HotelRegistration, CustomerUpgrade | — |
| **RoomManagement** | ✅ RoomType, RoomNameSuggestion | ✅ RoomType, RoomNameSuggestion | ✅ BedConfig, Capacity |
| **UserManagement** | ✅ UserService, Register | 🔲 Login / Register pending | — |
| **Auth** | 🔲 Pending | — | — |
| **HotelManagement** | 🔲 Pending | — | — |
| **BookingManagement** | 🔲 Pending | — | — |

---

Created: 07-Mar-2026
Updated: 21-Apr-2026
Version: 1.4
