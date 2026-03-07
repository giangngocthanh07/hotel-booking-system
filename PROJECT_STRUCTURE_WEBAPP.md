# HotelBooking Project - WebApp (Frontend) Structure

## 📁 Blazor WebApp Structure Overview

This document outlines the architecture and organization of the frontend layer `HotelBooking.webapp`, which is built using Blazor.

```
HotelBooking.webapp/
├── Pages/                         # Routable Blazor components (Views)
│   ├── Admin/                     # Pages accessible only by Admin role
│   │   ├── Base/
│   │   ├── Manage/
│   │   │   ├── Groups/            # Components for managing amenities, policies, etc.
│   │   │   └── SharedLayouts/
│   │   ├── Request/
│   │   │   ├── Components/
│   │   │   ├── UpgradeOwner/
│   │   │   ├── Details.razor
│   │   │   └── Index.razor
│   │   └── AdminHomeDashboard.razor
│   │
│   ├── Hotel/                     # Pages for hotel browsing and details
│   │   └── SearchResult.razor
│   │
│   ├── Public/                    # Publicly accessible pages
│   │   ├── About.razor
│   │   └── Hotels.razor
│   │
│   ├── User/                      # User-specific pages (e.g., Customer Dashboard)
│   │   ├── Customer/
│   │   │   ├── CustomerLogin.razor
│   │   │   ├── CustomerRegister.razor
│   │   │   └── UpgradeOwnerForm.razor
│   │   │
│   │   └── Owner/
│   │       ├── Steps/
│   │       └── OwnerDashboard.razor
│   │
│   ├── Index.razor                # Main landing page
│   └── _Host.cshtml               # The root Razor Page hosting the Blazor app
│
├── Components/                    # Reusable, non-routable Blazor UI components
│   ├── Admin/
│   ├── HotelCard.razor
│   ├── OwnerHeader.razor
│   └── SearchForm.razor
│
├── ViewModels/                    # Data models specifically designed for Views
│   ├── Base/                      # Shared/Common ViewModels
│   ├── Form/                      # ViewModels for handling form inputs (Login, Register...)
│   ├── Hotel/                     # ViewModels for displaying Hotel data
│   ├── Request/                   # ViewModels for Request processes
│   ├── Response/                  # ViewModels mapping API responses
│   └── State/                     # ViewModels for managing application state
│
├── Services/                      # Frontend services to communicate with the API
│   ├── Base/                      # Base configurations for HttpClients
│   ├── Interface/                 # Interfaces for frontend services
│   ├── HotelFormState.cs          # State management service for hotel forms
│   ├── ManageService.cs           # Service interacting with Admin/Management APIs
│   └── RequestService.cs          # Service interacting with Request APIs
│
├── Shared/                        # Shared layouts and components (NavMenu, MainLayout...)
│   ├── AdminLayout.razor
│   ├── ConfirmModal.razor
│   ├── Footer.razor
│   ├── Header.razor
│   ├── MainLayout.razor
│   ├── OwnerLayout.razor
│   └── Pagination.razor
│
├── Authentication/                # Custom authentication state providers and handlers
│   └── CustomAuthStateProvider.cs
│
├── Helpers/                       # Frontend helper utilities (Formatting, LocalStorage...)
│   ├── Common/
│   ├── Manage/
│   ├── MessageResponse.cs
│   └── StatusCodeResponse.cs
│
├── wwwroot/                       # Static web assets
│   ├── css/                       # Stylesheets
│   ├── images/                    # Image assets
│   ├── js/                        # JavaScript interoperability files
│   └── favicon.ico
│
├── App.razor                      # The root component of the Blazor application
├── _Imports.razor                 # Global using directives for Razor components
├── appsettings.json               # Frontend configuration (e.g., API Base URL)
└── Program.cs                     # WebAssembly/Server startup configuration and DI
```

### **Architecture & Conventions:**

1. **Pages vs Components**:
   - `Pages/`: Components that have an `@page` directive and can be navigated to via a URL. Organized by user roles or functional domains (`Admin`, `Hotel`, `User`).
   - `Components/`: Reusable UI elements that do not have an `@page` directive and are embedded inside Pages or other Components.

2. **ViewModels**:
   - Serve as DTOs for the frontend. They decouple the UI from the raw API formats and provide a structure tailored for data binding in Blazor forms and tables.
   - Separated into logical folders like `Form` (for user input validation) and `Response` (for displaying API data).

3. **Services**:
   - Encapsulate HTTP calls to the backend `HotelBooking.api`.
   - Interfaces should be defined in `Services/Interface/` for easier testing and dependency injection.
   - `Program.cs` handles registering these services (e.g., `AddHttpClient`, `AddScoped`).

4. **Authentication**:
   - Implements custom `AuthenticationStateProvider` to manage JWT tokens stored in Local Storage or Cookies, ensuring the UI reacts to login/logout states.

---

Created: 07-Mar-2026
Version: 1.1
