@./skills/using-superpowers/SKILL.md
@./skills/using-superpowers/references/gemini-tools.md

# Hotel Booking Project - SDLC & Standards

## 1. Project Overview

A hotel booking platform (Booking.com clone) connecting Hotel Owners and Customers.

## 2. Core Requirements

- **Guest:** Search, View Details, Book, Pay, Review.
- **Owner:** Register, Manage Hotels/Rooms, Track Bookings, Upgrade Account.
- **Admin:** Approve Hotels/Owners, Manage Users, System Reports.

## 3. High-Level Architecture (Clean Architecture)

- **Domain (Infrastructure/Models):** Entities, Value Objects, Domain Exceptions.
- **Infrastructure:** Data Access (EF Core), Repository Implementations, Unit of Work, External Services (Email, Payment).
- **Application:** Interfaces, DTOs, Services (Use Cases), Validators (FluentValidation), Mappers.
- **API (Presentation):** Controllers (RESTful), Middlewares, Swagger, DI Configuration.
- **Web UI:** Blazor WebAssembly/Server.

## 4. Coding Standards

- **Naming:** PascalCase for Classes/Methods, camelCase for local variables, `_camelCase` for private fields.
- **SOLID:** Strictly follow SOLID principles.
- **Clean Code:** Methods < 20 lines, Classes < 300 lines (if possible).
- **Async/Await:** All I/O operations must be asynchronous.
- **Validation:** Use `FluentValidation` in the Application layer.
- **Error Handling:** Use global exception middleware; return structured responses (e.g., `Result<T>` or `ApiResponse<T>`).

## 5. Definition of Done (DoD)

- [ ] Code is formatted and linted.
- [ ] All tests (Unit & Integration) pass.
- [ ] Logic is implemented as per Acceptance Criteria (AC).
- [ ] No hardcoded strings/configurations.
- [ ] Documentation updated (if applicable).
- [ ] Pull Request reviewed and approved.

## 6. Bad Practices to Avoid

- **Fat Controllers:** Logic must reside in Application Services.
- **Leaky Abstractions:** Infrastructure details (like SQL/EF) should not leak into Domain/Application.
- **Anemic Domain Model:** (Optional) Try to put logic in Entities if it makes sense, or keep Services focused.
- **God Services:** Avoid services that do too many things. Break them down.
- **Circular Dependencies:** Never allow layers to depend on each other in a circle.
- **Manual Mapping:** Avoid manually mapping properties everywhere; use AutoMapper or similar, but keep it explicit if it gets too complex.
- **Ignoring Cancellations:** Always pass `CancellationToken` through the entire call stack.
- **Hardcoding Secrets:** Never commit API keys or connection strings. Use Environment Variables or KeyVault.

## 7. Project Rules

- **Result Wrapper:** Always return a `Result<T>` or `ApiResponse<T>` from Services to indicate success/failure with error messages.
- **Validation First:** No service logic should run until the input DTO is validated via `FluentValidation`.
- **Repository Only:** Never use `DbContext` directly in the Application layer.
- **Unit of Work:** Use `IUnitOfWork` to commit changes at the end of a transaction.
- **Meaningful Tests:** Tests should focus on "Behavior" (e.g., "Should return error if hotel name is empty") rather than "Implementation".
- **Documentation:** Every new API endpoint must be documented with XML comments for Swagger.
- **Consistency:** Follow the existing direct repository injection pattern in Services.
