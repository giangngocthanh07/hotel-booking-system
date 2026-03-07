# HotelBooking Project - Test Layer Structure

## 📁 Test Layer Structure Overview

This document outlines the architecture and organization of the testing layer `HotelBooking.test`.

```
HotelBooking.test/
├── IntegrationTests/              # Tests evaluating how different pieces work together
│   ├── Infracstructure/           # Infrastructure layer integration tests
│   └── Service/                   # Service layer integration tests
│
├── UnitTests/                     # Isolated tests for individual components
│   ├── Common/                    # Tests for shared helpers and utilities
│   └── Services/                  # Business logic unit tests
│       └── UserServiceTest.cs
│
├── TestResults/                   # Generated reports and logs from test runs
│
├── appsettings.test.json          # Configuration specifically for the testing environment
└── HotelBooking.test.csproj       # Project file defining test dependencies
```

### **Testing Strategies & Conventions:**

1. **Unit Tests (UnitTests/)**:
   - Focus on testing isolated business logic (e.g., Domain Services in `HotelBooking.application`) without side effects.
   - External dependencies (like repositories, APIs, or database contexts) should be mocked using libraries such as `Moq` or `NSubstitute`.
   - Method naming convention often follows: `MethodName_StateUnderTest_ExpectedBehavior`.

2. **Integration Tests (IntegrationTests/)**:
   - Verify the interaction between different layers or external systems (e.g., database connectivity, API endpoint behavior).
   - Typically utilizes an in-memory database or a dedicated test database defined in `appsettings.test.json` to prevent modifying production data.

3. **Configuration (appsettings.test.json)**:
   - Always ensure test-specific configurations (like mock connection strings, dummy API keys) are stored here to avoid overlapping with development or production environments.

---

Created: 07-Mar-2026
Version: 1.0
