# HotelBooking System

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](#)
[![Blazor Server](https://img.shields.io/badge/Blazor-Server-512BD4?style=flat&logo=blazor&logoColor=white)](#)
[![EF Core](https://img.shields.io/badge/Entity_Framework-Core_8-512BD4?style=flat)](#)
[![SQL Server](https://img.shields.io/badge/Database-SQL_Server-CC2927?style=flat&logo=microsoftsqlserver&logoColor=white)](#)
[![Cloudinary](https://img.shields.io/badge/Media-Cloudinary-3448C5?style=flat&logo=cloudinary&logoColor=white)](#)
[![Swagger](https://img.shields.io/badge/API_Docs-Swagger-85EA2D?style=flat&logo=swagger&logoColor=black)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive, scalable **Hotel Booking Platform** built with **.NET 8, Blazor Server, and Domain-Driven Design (DDD) principles**.

The platform is designed for three distinct user roles:

| Role               | Who They Are                        | What They Can Do                                          |
| ------------------ | ----------------------------------- | --------------------------------------------------------- |
| 👤 **Customer**    | General visitors / registered users | Search hotels, submit upgrade requests                    |
| 🏨 **Hotel Owner** | Verified property managers          | Manage hotels, room types, and configurations             |
| 🛠️ **Admin**       | Platform administrators             | Manage global settings, users, roles, and handle requests |

---

## ✨ Features Overview

### 👤 Customer

- **Search & Browse Hotels** — Filter by location, price range, star rating, and amenities.
- **Account Management** — Register, log in, and manage your profile.
- **Upgrade Requests** — Submit a formal request (with supporting documents) to become a verified Hotel Owner.

### 🏨 Hotel Owner

- **Property Management** — Add hotels, define room types, room units, bed configurations, and views.
- **Media Uploads** — Upload hotel and room photos via Cloudinary integration.
- **Configuration** — Attach amenities, services, and policies to your properties.
- **Booking Dashboard** _(In Progress)_ — Track reservations and manage availability.

### 🏨 Hotel Owner

- **Property Management** — Add hotels, define room types, room units, bed configurations, and views.
- **Hotel Creation Wizard** — Multi-step wizard: Basic Info → Photo Uploads → Amenities → Policies.
- **Media Uploads** — Upload hotel and room photos via Cloudinary integration.
- **Room Type Management** — Configure room types with attributes, bed configs, and capacity.
- **Booking Dashboard** _(Planned)_ — Track reservations and manage availability.

### 🛠️ Administrator

- **Global Settings** — Full CRUD on Amenities, Policies, Services, Bed Types, Room Qualities, Unit Types, and Room Views.
- **Upgrade Request Management** — Full list view, filtering, stats dashboard, and detail review to approve or reject Owner upgrade requests.
- **Request Dashboard** — Real-time overview of recent requests and approval statistics.
- **User & Role Management** — Manage user accounts and assign authorization roles.

---

## 🏗️ System Architecture

The project uses a layered **N-Tier / Onion Architecture** that cleanly separates concerns across five projects:

```
HotelBooking.sln
├── HotelBooking.api/           ← REST API (entry point, JWT auth, Swagger)
├── HotelBooking.application/   ← Business logic, Services, DTOs, Validators
├── HotelBooking.infrastructure/← Data access (EF Core DbContext, Repositories, UnitOfWork)
├── HotelBooking.webapp/        ← Frontend UI (Blazor Server)
└── HotelBooking.test/          ← Unit & Integration tests
```

### Layer Responsibilities

| Layer                                | Description                                                                                                                                                                                                            |
| ------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 🌐 **`HotelBooking.api`**            | Exposes RESTful API endpoints under versioned routes (`v1/admin/`, `v1/customer/`, `v1/[public]`). Secured via JWT Bearer authentication. DI is configured via dedicated Extension classes to keep `Program.cs` clean. |
| ⚙️ **`HotelBooking.application`**    | Core business logic organized by domain (`AdminManagement`, `HotelManagement`, `RequestManagement`, `RoomManagement`, `Auth`, `UserManagement`, `Media`). Also contains FluentValidation rules, DTOs, and shared Helpers. |
| 🗄️ **`HotelBooking.infrastructure`** | Connects the app to external data sources. Contains the EF Core `HotelBookingDBContext`, all Repository implementations, and a `UnitOfWork` pattern. Also integrates with **Cloudinary** for media storage.            |
| 💻 **`HotelBooking.webapp`**         | Blazor Server frontend divided into role-based layouts (`AdminLayout`, `OwnerLayout`, `AuthLayout`, `MainLayout`). Communicates with the backend API via scoped HttpClient services.                                    |
| 🧪 **`HotelBooking.test`**           | xUnit-based Unit Tests covering Services, Validators, and Helpers per domain. Integration tests use a dedicated `appsettings.test.json` and connect to a real test database.                                           |

> 📖 **Detailed structure documentation:**
>
> - [Backend & API Folder Structure](PROJECT_STRUCTURE.md)
> - [Frontend (Blazor) Folder Structure](PROJECT_STRUCTURE_WEBAPP.md)
> - [Test Layer Architecture](PROJECT_STRUCTURE_TEST.md)

---

## 🚀 Getting Started

> **For non-developers:** This project runs locally on your machine. Follow each step in order. If you get stuck, the most common issues are covered in the [Troubleshooting](#-troubleshooting) section below.

### Prerequisites

Before you begin, make sure you have the following installed:

| Tool       | Version                              | Download                                                                                    |
| ---------- | ------------------------------------ | ------------------------------------------------------------------------------------------- |
| .NET SDK   | 8.0 or later                         | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)                    |
| SQL Server | 2019 or later                        | [microsoft.com/sql-server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) |
| IDE        | Visual Studio 2022 / VS Code / Rider | —                                                                                           |
| Git        | Any recent version                   | [git-scm.com](https://git-scm.com/)                                                         |

---

### Step 1 — Clone the Repository

```bash
git clone https://github.com/pinnguyen25/Hotel_Blazor.git
cd Hotel_Blazor
```

---

### Step 2 — Configure the Database Connection

Open `HotelBooking.api/appsettings.json` and update the connection string to point to your local SQL Server instance:

```json
"ConnectionStrings": {
  "connectionStringHotelBooking": "Server=YOUR_SERVER,PORT; Database=HotelBooking; User Id=YOUR_USER; Password=YOUR_PASSWORD; TrustServerCertificate=True"
}
```

> **Common values for local development:**
>
> - `Server=127.0.0.1,1433` — default SQL Server local instance
> - `Server=.` or `Server=(localdb)\\mssqllocaldb` — for LocalDB

---

### Step 3 — Apply Database Migrations

This project uses **EF Core Code-First migrations**. Run the following command from the solution root to create and update your database schema:

```bash
dotnet ef database update --project HotelBooking.infrastructure --startup-project HotelBooking.api
```

> **Have an existing database?** If you are restoring from a `.bak` file or a pre-existing schema, skip this step. You can also apply individual SQL scripts from the [`Scripts/`](Scripts/) folder as needed.

---

### Step 4 — (Optional) Configure Cloudinary

The application uses [Cloudinary](https://cloudinary.com/) for image storage. Update the credentials in `HotelBooking.api/appsettings.json`:

```json
"Cloudinary": {
  "CloudName": "your-cloud-name",
  "ApiKey": "your-api-key",
  "ApiSecret": "your-api-secret"
}
```

> Sign up for a free Cloudinary account at [cloudinary.com](https://cloudinary.com/). Without valid credentials, photo upload features will not work.

---

### Step 5 — Run the Application

You need to run **two projects simultaneously**: the API backend and the Blazor frontend.

**Option A: Visual Studio 2022 (Recommended)**

Set both `HotelBooking.api` and `HotelBooking.webapp` as Startup Projects via right-click → _Set as Startup Projects_, then press **F5**.

**Option B: CLI (Two Terminals)**

```bash
# Terminal 1 — Start the API backend
cd HotelBooking.api
dotnet run
```

```bash
# Terminal 2 — Start the Blazor frontend
cd HotelBooking.webapp
dotnet run
```

Once running:

| Service                 | URL                                    |
| ----------------------- | -------------------------------------- |
| 🌐 **Blazor Web App**   | `https://localhost:<port>`             |
| 📄 **Swagger API Docs** | `https://localhost:<api-port>/swagger` |

> The exact ports are shown in the terminal output after `dotnet run`. They are also configurable in `HotelBooking.api/Properties/launchSettings.json` and `HotelBooking.webapp/Properties/launchSettings.json`.

---

## 🧪 Testing

Tests are located in the `HotelBooking.test` project and are split into:

- **Unit Tests** — Test individual service methods in isolation (using Moq for mocking).
- **Integration Tests** — Connect to a real database using settings from `appsettings.test.json`.

Before running integration tests, make sure `appsettings.test.json` has a valid connection string pointing to a test database.

```bash
# Run all tests from the solution root
dotnet test
```

```bash
# Run only unit tests
dotnet test --filter "FullyQualifiedName~UnitTests"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

---

## 🤝 Contribution Guidelines

Contributions are welcome! Please follow the workflow below to keep the codebase consistent.

### Workflow

1. **Fork** the repository.
2. **Create a branch** for your feature or fix:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Commit** with a clear, descriptive message:
   ```bash
   git commit -m "feat: add booking cancellation flow"
   ```
4. **Push** your branch and **open a Pull Request**.

### Code Style

- Follow **Domain-Driven Design** principles — organize code by business domain, not by technical type.
- Prefix all interfaces with `I` (e.g., `IHotelService`, `IAmenityService`).
- **Co-locate interfaces with implementations**: feature-specific service interfaces live in the same `.cs` file as the class (e.g., `IAmenityService` is in `AmenityService.cs`). Only truly shared or utility interfaces go in the `Interfaces/` folder.
- Write XML doc comments (`/// <summary>`) on public methods.
- Keep methods small and focused — delegate to helpers when logic grows complex.

---

## 🔮 Roadmap

Features currently planned or in progress:

| Status | Feature | Description |
|--------|---------|-------------|
| ✅ | **Admin Management** | Full CRUD for Amenities, Policies, Services, Bed Types, Room Qualities, Room Views, Unit Types |
| ✅ | **Upgrade Request Flow** | Complete Admin review + Customer submission with detail pages, filters, and stats |
| ✅ | **Hotel Creation Wizard** | Multi-step hotel creation (Basic Info, Photos, Amenities, Policies) |
| ✅ | **Room Type Management** | Owner-side room type configuration |
| ✅ | **Authentication** | JWT Login / Register flow with role-based access |
| 🔄 | **BookingManagement** | End-to-end booking flow: room selection, reservation, payment processing |
| 🔄 | **Review System** | Customers can leave ratings and reviews after their stay |
| 🔄 | **Owner Dashboard** | Revenue reports, occupancy statistics, and booking management |
| 🔄 | **Customer Portal** | Booking history, saved hotels, and personal settings |
| 🔄 | **Notification System** | In-app and email notifications for booking updates |

---

## ❓ Troubleshooting

| Problem                    | Likely Cause                 | Solution                                                                                         |
| -------------------------- | ---------------------------- | ------------------------------------------------------------------------------------------------ |
| Cannot connect to database | Wrong connection string      | Double-check server name, port, credentials in `appsettings.json`                                |
| Migration fails            | Project not found            | Run the `dotnet ef` command from the **solution root** folder                                    |
| Image upload not working   | Missing Cloudinary config    | Set valid Cloudinary credentials in `appsettings.json`                                           |
| Port conflicts             | Another app on the same port | Change the port in `launchSettings.json` or stop the conflicting process                         |
| `dotnet` command not found | SDK not installed            | Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and restart your terminal |

---

## 📜 License

Distributed under the **MIT License**. See [`LICENSE`](LICENSE) for details.

---

_Built as a final project at CyberSoft Academy. If you find this project helpful for learning Blazor and Clean Architecture, give it a ⭐️!_
