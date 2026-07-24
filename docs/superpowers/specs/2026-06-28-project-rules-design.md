# Specification: Project Rules and Clean Code Standards for Hotel_Blazor

**Date:** 2026-06-28

**Status:** Approved

**Version:** 1.0

---

## 1. Introduction & Objectives
This document establishes the official project standards, architecture constraints, and clean code rules for the **Hotel_Blazor** application. These rules must be strictly adhered to by any developer or AI Agent working on this codebase.

The goal is to maintain a high-quality, highly consistent codebase across the backend Web API, the application service layer, the database integration layer, and the Blazor Server frontend.

---

## 2. Core Architecture Rules

### 2.1 No Independent Domain Project
* The traditional Clean Architecture "Domain" project is omitted in this solution.
* **Core Infrastructure**: The `HotelBooking.infrastructure` project functions as the system core / foundation layer.
  * It houses the database entities (Database-First generated EF Core classes inside `Models/`) and the `HotelBookingDBContext`.
  * It also contains the implementations of the Repositories and the Unit of Work.
* **Dependency Rule**: The dependency flow is strictly linear:
  `HotelBooking.webapp` (FE) -> `HotelBooking.api` (Web API) -> `HotelBooking.application` (App Services) -> `HotelBooking.infrastructure` (Core DB & Repos).
* **Layer Isolation**:
  * The `Application` layer must **never** reference `Microsoft.EntityFrameworkCore` or access the `DbContext` directly.
  * All database operations must go through the Repository interfaces defined in `Application` (or `Infrastructure` depending on placement) and executed via `IUnitOfWork`.

---

## 3. C# Clean Code Guidelines

### 3.1 Class & Method Size Limits
* **Method Length**: Methods should not exceed **20 lines** of functional code (excluding opening/closing brackets, whitespace, or comments). If a method exceeds this, it must be refactored into smaller, single-purpose helper methods.
* **Class Length**: Classes should be kept under **300 lines** where possible, emphasizing the Single Responsibility Principle (SRP).

### 3.2 Naming Conventions
* **Private fields**: Use `_camelCase` (e.g., `_hotelRepository`, `_unitOfWork`).
* **Local variables & Parameters**: Use `camelCase` (e.g., `hotelId`, `requestDto`).
* **Classes, Methods, Properties, Interfaces**: Use `PascalCase`. Interfaces must be prefixed with `I` (e.g., `IHotelService`, `GetHotelByIdAsync`).

### 3.3 Asynchronous Programming (Async/Await)
* Every database query, API call, file access, or network interaction **must** be written asynchronously using `async/await`.
* Pass `CancellationToken` through all method signatures down to EF Core calls (e.g., `ToListAsync(cancellationToken)`).

### 3.4 Error Handling & Validation
* **Predictable Failures**: Do not throw exceptions for business logic failures or validation issues. Instead, return a structured wrapper: `Result<T>` or `ApiResponse<T>`.
* **System Exceptions**: Unhandled system-level exceptions (e.g., database connection issues, server errors) are caught at the API boundary using a global exception handling middleware.
* **Validation First**: Every application service method must run validation on input DTOs using `FluentValidation` before executing any business logic.

---

## 4. Repository & Unit of Work (UoW) Pattern

### 4.1 Encapsulation of EF Core
* `DbContext` must reside strictly inside `HotelBooking.infrastructure`. No EF Core concepts may leak to the `Application` or `API` layers.
* Query definitions involving eager loading (`Include`, `ThenInclude`) or complex conditional filters must be defined inside repository implementations (e.g., `HotelRepository.cs`) rather than being built dynamically inside application services.

### 4.2 Generic vs Specific Repositories
* **Generic Repository**: Use `IRepository<T>` for standard, simple CRUD operations.
* **Specific Repositories**: Create specialized repository interfaces (e.g., `IHotelRepository`) for custom queries to ensure optimized SQL execution plans and clear data access boundaries.

### 4.3 Atomic Operations via Unit of Work
* Repositories should never call `SaveChangesAsync()` directly. They only perform tracking actions on the DbContext change tracker.
* Persisting changes to the database (calling `_unitOfWork.SaveChangesAsync()`) must occur exactly **once** at the end of a transaction in the Application Service method.

---

## 5. Blazor Server Frontend Best Practices

### 5.1 Component Architecture (Decoupling UI and Logic)
* **Pages (`Pages/`)**:
  * Routable components using the `@page` directive.
  * Act as UI controllers; they fetch data from services, manage local state, and pass state down to child components.
* **Components (`Components/`)**:
  * Non-routable, reusable UI elements.
  * Communication: Receive data via `[Parameter]` and bubble events back up using `[Parameter] EventCallback`.
  * Avoid injecting API services directly; keep them pure for presentation and max reusability.
* **Layouts (`Shared/`)**:
  * Layout files (like `MainLayout.razor`, `AdminLayout.razor`) that frame pages.

### 5.2 Code-Behind Conventions
* **Base Class Pattern**: For large pages (e.g., Admin Pages) sharing common behavior (like paging, grid controls, confirmation dialogs), inherit from a C# base class (e.g., `AdminPageBase`).
* **Partial Class Pattern**: For standard pages and reusable components, separate UI markup from C# logic by utilizing partial classes (e.g., `MyComponent.razor` paired with `MyComponent.razor.cs`).

### 5.3 State Management
* **Scoped Services**: Share data between components or wizard steps (e.g., `HotelFormState.cs`) using Scoped DI registration.
* **Static Member Warning**: Never use `static` members to hold user-specific state in Blazor Server, as they are shared globally across all user circuits connected to the server process.

### 5.4 ViewModels for Form Binding
* Bind forms directly to dedicated `ViewModel` classes (e.g. inside `ViewModels/Form/`) rather than raw API DTOs.
* This keeps UI input constraints and change tracking isolated from the backend data contracts.

### 5.5 Circuit Preservation (ErrorBoundary)
* Prevent component failures from crashing the entire Blazor circuit (WebSocket connection) by wrapping major UI regions (e.g., `@Body` in layouts or standalone dashboard widgets) inside `<ErrorBoundary>` components.
* Implement custom `<ErrorContent>` to show a friendly error interface with a "Try Again" recovery action calling `.Recover()`.
