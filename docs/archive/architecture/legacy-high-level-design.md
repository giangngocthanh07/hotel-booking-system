# High-Level Design (HLD)

## 1. Request Flow
1. **Client (Blazor)** sends an HTTP request to the **API**.
2. **API Controller** receives the request, validates the JWT token.
3. **Controller** delegates the task to a **Service** in the **Application Layer**.
4. **Service** performs business logic, using **Validators** for data integrity.
5. **Service** interacts with **Repositories** (Infrastructure) to fetch/persist data.
6. **Unit of Work** ensures atomic transactions across multiple repositories.
7. **Infrastructure** uses **EF Core** to talk to **SQL Server**.
8. **Service** maps Entities to **DTOs** and returns an `ApiResponse<T>` to the Controller.
9. **Controller** returns appropriate HTTP status code via `ApiResponseHandlerHelper`.

## 2. Key Components
- **Identity:** Handles Authentication and RBAC (Admin, Owner, Customer).
- **Booking Engine:** Core logic for checking availability and calculating prices.
- **Search Engine:** Optimized queries for finding hotels based on location and dates.
- **Notification Service:** Handles emails for booking confirmations.
- **Media Service:** Integrates with Cloudinary for image management.

## 3. Data Flow Diagram (Conceptual)
`[User] <-> [Blazor UI] <-> [Web API] <-> [Application Services] <-> [Repositories/UoW] <-> [SQL DB]`

## 4. Security
- HTTPS everywhere.
- JWT-based Auth.
- Sensitive data (passwords) hashed with BCrypt.
- CORS policy restricted to known origins.
