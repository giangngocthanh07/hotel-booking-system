# HotelBooking Project - Test Layer Structure

_Last updated: 10-Apr-2026 | Version: 1.3_

---

## 📁 Test Layer Structure Overview

This document outlines the architecture and organization of the testing layer `HotelBooking.test`.

```
HotelBooking.test/
├── IntegrationTests/              # Tests evaluating how different pieces work together
│   ├── Infracstructure/           # Infrastructure layer integration tests
│   └── Service/                   # Service layer integration tests
│
├── UnitTests/                     # Isolated tests for individual components
│   ├── Common/                    # Tests for shared base classes and utilities
│   │   └── BaseServiceTest.cs     # Shared test base (mock setup, common asserts)
│   │
│   ├── Helpers/                   # Tests for Helpers in Application layer
│   │   └── RoomManagement/
│   │       ├── BedConfigurationHelperTests.cs
│   │       └── CapacityHelperTests.cs
│   │
│   ├── Services/                  # Business logic unit tests per domain
│   │   ├── AdminManagement/
│   │   │   └── ManagementAdminServiceTests.cs
│   │   ├── RequestManagement/
│   │   │   ├── Admin/
│   │   │   │   └── AdminUpgradeRequestServiceTests.cs
│   │   │   └── Customer/
│   │   │       └── CustomerUpgradeRequestServiceTests.cs
│   │   ├── RoomManagement/
│   │   │   ├── RoomNameSuggestionTests.cs
│   │   │   └── RoomTypeServiceTests.cs
│   │   └── UserManagement/
│   │       ├── Register/
│   │       └── UserServiceTests.cs
│   │
│   └── Validators/                # FluentValidation rule tests
│       ├── AdminManagement/       # (placeholder — tests to be added)
│       ├── Common/
│       ├── RequestManagement/
│       │   ├── Admin/
│       │   └── Customer/
│       ├── RoomManagement/
│       │   ├── RoomNameSuggestionValidatorTests.cs
│       │   └── RoomTypeValidatorTests.cs
│       └── UserManagement/
│           ├── Login/
│           └── Register/
│
├── TestResults/                   # Generated reports and logs from test runs
│
├── appsettings.test.json          # Configuration for testing environment
└── HotelBooking.test.csproj       # Project file defining test dependencies
```

---

### **Testing Strategies & Conventions:**

1. **Unit Tests — Services (`UnitTests/Services/`)**:
   - Test isolated business logic (Domain Services in `HotelBooking.application`) without external side effects.
   - Dependencies (repositories, DBContext, external APIs) are mocked using `Moq`.
   - Tests are mirrored to the domain structure in `Services/Domains/`.
   - Naming convention: `MethodName_StateUnderTest_ExpectedBehavior`.

2. **Unit Tests — Validators (`UnitTests/Validators/`)**:
   - Test `FluentValidation` rules for each request DTO.
   - Structured to mirror the `Validators/` folder organization in `HotelBooking.application`.
   - Covers valid, boundary, and invalid input scenarios.

3. **Unit Tests — Helpers (`UnitTests/Helpers/`)**:
   - Tests for pure helper/utility functions with no external dependencies.
   - Examples: `BedConfigurationHelper`, `CapacityHelper`.

4. **Integration Tests (`IntegrationTests/`)**:
   - Verify the interaction between layers or external systems (e.g., EF Core to database).
   - Uses a dedicated test database defined in `appsettings.test.json`.

5. **Configuration (`appsettings.test.json`)**:
   - Stores test-specific connection strings and dummy API keys.
   - Never overlaps with `appsettings.json` used in development or production.

---

## 📊 Test Coverage Status

| Domain                   | Services Tests           | Validator Tests          | Helper Tests         |
| ------------------------ | ------------------------ | ------------------------ | -------------------- |
| **AdminManagement**      | ✅ ManagementAdminService | 🔲 Pending               | —                    |
| **RequestManagement**    | ✅ Admin + Customer       | 🔲 Pending               | —                    |
| **RoomManagement**       | ✅ RoomType + Suggestion  | ✅ RoomType + Suggestion  | ✅ BedConfig + Capacity |
| **UserManagement**       | ✅ UserService + Register | 🔲 Login / Register pending | —                |
| **Auth**                 | 🔲 Pending               | —                        | —                    |
| **HotelManagement**      | 🔲 Pending               | —                        | —                    |
| **BookingManagement**    | 🔲 Pending               | —                        | —                    |

---

Created: 07-Mar-2026
Updated: 10-Apr-2026
Version: 1.3
