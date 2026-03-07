# HotelBooking System

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](#)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly_/_Server-512BD4?style=flat&logo=blazor&logoColor=white)](#)
[![EF Core](https://img.shields.io/badge/Entity_Framework-Core-512BD4?style=flat)](#)
[![Swagger](https://img.shields.io/badge/Swagger-API_Docs-85EA2D?style=flat&logo=swagger&logoColor=black)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive, scalable **Hotel Booking Platform** built with **.NET Core, Domain-Driven Design (DDD) principles, and Blazor**.
This application provides separate spaces for **Customers** (to search and book rooms), **Hotel Owners** (to manage their properties and receive bookings), and **Administrators** (to manage platform global settings such as amenities, policies, and upgrade requests).

## ✨ Features

### 👤 Customer Features

- **Search & Browse:** Find hotels with rich filtering options (by location, price, rating, and amenities).
- **Booking & Reservations:** (Coming Soon) Book rooms, manage upcoming reservations, and history.
- **Account Upgrades:** Submit requests with required documents for approval to become a Hotel Owner.

### 🏨 Hotel Owner Features

- **Property Management:** Add and define hotel details, manage room types, units, and views.
- **Booking Dashboard:** (Coming Soon) Track incoming reservations, update statuses, and generate revenue reports.
- **Configuration:** Attach specific amenities and policies to hotel properties.

### 🛠️ Admin Features

- **Global Settings:** Perform full CRUD operations on Amenities, Global Policies, Service types, and Bed Types.
- **Request Management:** Review, approve, or reject "Upgrade to Owner" requests from customers.
- **User & Role Management:** Manage user profiles and their authorization roles.

---

## 🏗️ System Architecture

The project employs a structured `N-Tier` / modified `Onion Architecture`, separating concerns distinctly.

| Project Layer                    | Description                                                                                                                                                                                              |
| -------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 🌐 `HotelBooking.api`            | The entry point. Exposes RESTful API endpoints. Secured via JWT authentication, heavily reliant on Swagger documentation. Routed as `v1/admin/`, `v1/customer/`, and `v1/public/`.                       |
| ⚙️ `HotelBooking.application`    | The core layer containing the business logic organized into domains (`AdminManagement`, `HotelManagement`, `RequestManagement`, `Auth`, etc.). Also holds FluentValidation constraints and DTO mappings. |
| 🗄️ `HotelBooking.infrastructure` | Connects the application to external services - usually contains the DbContext, Repository implementations, caching, and 3rd party API integrations.                                                     |
| 💻 `HotelBooking.webapp`         | The frontend interface built with **Blazor**. Divided into distinct components and pages enforcing Role-based layouts (`AdminLayout`, `OwnerLayout`, `MainLayout`).                                      |
| 🧪 `HotelBooking.test`           | Houses Unit tests (using xUnit/NUnit, Moq) and Integration tests ensuring regression-free changes.                                                                                                       |

> 📖 **Dive deeper into the architecture:**
>
> - [Backend Folder Structure](PROJECT_STRUCTURE.md)
> - [Frontend (Blazor) Folder Structure](PROJECT_STRUCTURE_WEBAPP.md)
> - [Test Layer Architecture](PROJECT_STRUCTURE_TEST.md)

---

## 🚀 Getting Started

Follow these steps to replicate and run the project locally.

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.
- **IDE:** Visual Studio 2022, JetBrains Rider, or VS Code.
- A compatible database engine (e.g., SQL Server or PostgreSQL).

### 1. Clone the repository

```bash
git clone https://github.com/your-username/HotelBooking.git
cd HotelBooking
```

### 2. Configure Database

Updating connection string inside `HotelBooking.api/appsettings.json` (and optionally `appsettings.Development.json`):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=HotelBookingDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 3. Apply Migrations (If Using Code-First)

```bash
cd HotelBooking.infrastructure
dotnet ef database update --startup-project ../HotelBooking.api
```

_(Skip this step if using a Database-First approach with an existing schema)._

### 4. Run the Application

You can run the application either directly from your IDE by setting both `HotelBooking.api` and `HotelBooking.webapp` as Startup Projects, or via CLI:

**Terminal 1 (Backend - API):**

```bash
cd HotelBooking.api
dotnet run
```

_API Swagger Documentation will be accessible at: `https://localhost:<port>/swagger`_

**Terminal 2 (Frontend - Blazor WebApp):**

```bash
cd HotelBooking.webapp
dotnet run
```

_The App will be accessible at: `https://localhost:<port>`_

---

## 🧪 Testing

The solution incorporates tests mapped within the `HotelBooking.test` project.

To run all unit and integration tests:

```bash
dotnet test
```

---

## 🤝 Contribution Guidelines

We welcome contributions! Please adhere to the following workflow:

1. **Fork the repository**.
2. **Create a Feature Branch** (`git checkout -b feature/AmazingFeature`).
3. **Commit your changes** (`git commit -m 'Add some AmazingFeature'`).
4. **Push to the Branch** (`git push origin feature/AmazingFeature`).
5. **Open a Pull Request**.

### Code Style Requirements:

- Write semantic, domain-driven code.
- Prefix interfaces with `I` (e.g., `IHotelService`).
- Place feature-specific business interfaces directly in the same file as the class implementation to promote locality of behavior, unless it's a globally shared interface.
- Add meaningful English summaries and comments.

---

## 📜 License

Distributed under the MIT License. See `LICENSE` for more information.

---

_If you find this project helpful for learning Blazor and Clean Architecture, give it a ⭐️!_
