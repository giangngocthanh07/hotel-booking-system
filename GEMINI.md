@./skills/using-superpowers/SKILL.md
@./skills/using-superpowers/references/gemini-tools.md

# Hotel Booking Project - SDLC & Standards

## 1. Project Overview

A hotel booking platform (Booking.com clone) connecting Hotel Owners and Customers.

## 2. Core Requirements

- **Guest:** Search, View Details, Book, Pay, Review.
- **Owner:** Register, Manage Hotels/Rooms, Track Bookings, Upgrade Account.
- **Admin:** Approve Hotels/Owners, Manage Users, System Reports.

## 3. Core Architecture Rules

### 3.1 No Independent Domain Project
- The traditional Clean Architecture "Domain" project is omitted in this solution.
- **Core Infrastructure**: The `HotelBooking.infrastructure` project functions as the system core / foundation layer.
  - It houses the database entities (Database-First generated EF Core classes inside `Models/`) and the `HotelBookingDBContext`.
  - It also contains the implementations of the Repositories and the Unit of Work.
- **Dependency Rule**: The dependency flow is strictly linear:
  `HotelBooking.webapp` (FE) -> `HotelBooking.api` (Web API) -> `HotelBooking.application` (App Services) -> `HotelBooking.infrastructure` (Core DB & Repos).
- **Layer Isolation**:
  - The `Application` layer must **never** reference `Microsoft.EntityFrameworkCore` or access the `DbContext` directly.
  - All database operations must go through the Repository interfaces defined in `Application` (or `Infrastructure` depending on placement) and executed via `IUnitOfWork`.

## 4. C# Clean Code Guidelines

### 4.1 Class & Method Size Limits
- **Method Length**: Methods should not exceed **20 lines** of functional code (excluding opening/closing brackets, whitespace, or comments). If a method exceeds this, it must be refactored into smaller, single-purpose helper methods.
- **Class Length**: Classes should be kept under **300 lines** where possible, emphasizing the Single Responsibility Principle (SRP).

### 4.2 Naming Conventions
- Private fields: Use `_camelCase` (e.g., `_hotelRepository`, `_unitOfWork`).
- Local variables & Parameters: Use `camelCase` (e.g., `hotelId`, `requestDto`).
- Classes, Methods, Properties, Interfaces: Use `PascalCase`. Interfaces must be prefixed with `I` (e.g., `IHotelService`); methods should use PascalCase (e.g., `GetHotelByIdAsync`).

### 4.3 Asynchronous Programming (Async/Await)
- Every database query, API call, file access, or network interaction **must** be written asynchronously using `async/await`.
- Pass `CancellationToken` through all method signatures down to EF Core calls (e.g., `ToListAsync(cancellationToken)`).

### 4.4 Error Handling & Validation
- **Predictable Failures**: Do not throw exceptions for business logic failures or validation issues. Instead, return a structured wrapper: `Result<T>` or `ApiResponse<T>`.
- **System Exceptions**: Unhandled system-level exceptions (e.g., database connection issues, server errors) are caught at the API boundary using a global exception handling middleware.
- **Validation First**: Every application service method must run validation on input DTOs using `FluentValidation` before executing any business logic.

## 5. Repository & Unit of Work (UoW) Pattern

### 5.1 Encapsulation of EF Core
- `DbContext` must reside strictly inside `HotelBooking.infrastructure`. No EF Core concepts may leak to the `Application` or `API` layers.
- Query definitions involving eager loading (`Include`, `ThenInclude`) or complex conditional filters must be defined inside repository implementations (e.g., `HotelRepository.cs`) rather than being built dynamically inside application services.

### 5.2 Generic vs Specific Repositories
- **Generic Repository**: Use `IRepository<T>` for standard, simple CRUD operations.
- **Specific Repositories**: Create specialized repository interfaces (e.g., `IHotelRepository`) for custom queries to ensure optimized SQL execution plans and clear data access boundaries.

### 5.3 Atomic Operations via Unit of Work
- Repositories should never call `SaveChangesAsync()` directly. They only perform tracking actions on the DbContext change tracker.
- Persisting changes to the database (calling `_unitOfWork.SaveChangesAsync()`) must occur exactly **once** at the end of a transaction in the Application Service method.

## 6. Blazor Server Frontend Best Practices

### 6.1 Component Architecture (Decoupling UI and Logic)
- **Pages (`Pages/`)**:
  - Routable components using the `@page` directive.
  - Act as UI controllers; they fetch data from services, manage local state, and pass state down to child components.
- **Components (`Components/`)**:
  - Non-routable, reusable UI elements.
  - Communication: Receive data via `[Parameter]` and bubble events back up using `[Parameter] EventCallback`.
  - Avoid injecting API services directly; keep them pure for presentation and max reusability.
- **Layouts (`Shared/`)**:
  - Layout files (like `MainLayout.razor`, `AdminLayout.razor`) that frame pages.

### 6.2 Code-Behind Conventions
- **Base Class Pattern**: For large pages (e.g., Admin Pages) sharing common behavior (like paging, grid controls, confirmation dialogs), inherit from a C# base class (e.g., `AdminPageBase`).
- **Partial Class Pattern**: For standard pages and reusable components, separate UI markup from C# logic by utilizing partial classes (e.g., `MyComponent.razor` paired with `MyComponent.razor.cs`).

### 6.3 State Management
- **Scoped Services**: Share data between components or wizard steps (e.g., `HotelFormState.cs`) using Scoped DI registration.
- **Static Member Warning**: Never use `static` members to hold user-specific state in Blazor Server, as they are shared globally across all user circuits connected to the server process.

### 6.4 ViewModels for Form Binding
- Bind forms directly to dedicated `ViewModel` classes (e.g. inside `ViewModels/Form/`) rather than raw API DTOs.
- This keeps UI input constraints and change tracking isolated from the backend data contracts.

### 6.5 Circuit Preservation (ErrorBoundary)
- Prevent component failures from crashing the entire Blazor circuit (WebSocket connection) by wrapping major UI regions (e.g., `@Body` in layouts or standalone dashboard widgets) inside `<ErrorBoundary>` components.
- Implement custom `<ErrorContent>` to show a friendly error interface with a "Try Again" recovery action calling `.Recover()`.

## 7. Definition of Done (DoD)
- [ ] Code is formatted and linted.
- [ ] Code follows size limits (methods < 20 lines, classes < 300 lines).
- [ ] All tests (Unit & Integration) pass.
- [ ] Logic is implemented as per Acceptance Criteria (AC).
- [ ] No direct `DbContext` usage in Application or API layers (must go through Repositories and `IUnitOfWork`).
- [ ] All asynchronous database/network operations accept and pass `CancellationToken`.
- [ ] Validation is executed first via `FluentValidation` before business logic.
- [ ] Services return `Result<T>` or `ApiResponse<T>` instead of throwing business exceptions.
- [ ] Pull Request reviewed and approved.

## 8. Bad Practices to Avoid
- **Direct DbContext reference in Application**: Never reference `Microsoft.EntityFrameworkCore` or access the `DbContext` directly outside the Infrastructure layer.
- **Fat Controllers & Direct DB Queries in Controllers**: API controllers must only delegate to Application Services.
- **Method Length Over 20 Lines**: Methods exceeding 20 lines of functional code must be refactored into smaller helper methods.
- **Ignoring Cancellations**: Always pass `CancellationToken` through the entire call stack.
- **Hardcoding Connection Strings / Secrets**: Never commit configuration secrets or connection strings to git.
- **Leaking EF Core concepts**: Don't use EF Core types, tracking states, or query concepts dynamically inside Application services.
- **Synchronous Database / API Calls**: All DB/network operations must be run asynchronously.
