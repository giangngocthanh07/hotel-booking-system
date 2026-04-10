# HotelBooking Project - WebApp (Frontend) Structure

_Last updated: 10-Apr-2026 | Version: 1.3_

---

## 📁 Blazor WebApp Structure Overview

This document outlines the architecture and organization of the frontend layer `HotelBooking.webapp`, which is built using Blazor Server.

```
HotelBooking.webapp/
├── Pages/                         # Routable Blazor components (Views)
│   ├── Admin/                     # Pages accessible only by Admin role
│   │   ├── Base/
│   │   │   └── AdminPageBase.cs   # Shared code-behind base class for Admin pages
│   │   ├── Manage/
│   │   │   ├── Groups/            # Individual manager components per entity
│   │   │   │   ├── AmenityManager.razor
│   │   │   │   ├── BedTypeManager.razor
│   │   │   │   ├── PolicyManager.razor
│   │   │   │   ├── RoomQualityManager.razor
│   │   │   │   ├── RoomTypeManager.razor
│   │   │   │   ├── RoomViewManager.razor
│   │   │   │   ├── ServiceManager.razor
│   │   │   │   └── UnitTypeManager.razor
│   │   │   └── SharedLayouts/
│   │   │       ├── GlobalNavMenu.razor     # Side-nav for management section
│   │   │       └── MasterDetailLayout.razor # Reusable master/detail page layout
│   │   ├── Request/               # Admin-side request management
│   │   │   ├── Components/
│   │   │   │   ├── RecentRequests.razor    # Dashboard: recent request widget
│   │   │   │   └── RequestOverview.razor   # Dashboard: statistics overview card
│   │   │   ├── UpgradeOwner/      # ⭐ Full Owner Upgrade Request flow
│   │   │   │   ├── Components/
│   │   │   │   │   ├── UpgradeFilters.razor   # Filter bar for upgrade list
│   │   │   │   │   ├── UpgradeStatsCard.razor # Stats summary card
│   │   │   │   │   └── UpgradeTable.razor     # Data table with pagination
│   │   │   │   ├── Details.razor  # Full detail view of a single upgrade request
│   │   │   │   └── Index.razor    # List view of all upgrade requests
│   │   │   ├── Details.razor      # General request details page (shared/overview)
│   │   │   └── Index.razor        # General request dashboard (with stats widgets)
│   │   └── AdminHomeDashboard.razor
│   │
│   ├── Hotel/                     # Pages for hotel browsing
│   │   └── SearchResult.razor
│   │
│   ├── Public/                    # Publicly accessible pages (no auth required)
│   │   ├── About.razor
│   │   └── Hotels.razor
│   │
│   ├── User/                      # User-specific pages
│   │   ├── Customer/
│   │   │   ├── CustomerLogin.razor
│   │   │   ├── CustomerRegister.razor
│   │   │   └── UpgradeOwnerForm.razor    # Form to submit upgrade request
│   │   │
│   │   └── Owner/
│   │       ├── Steps/             # ⭐ Multi-step Hotel Creation Wizard
│   │       │   ├── HotelWizard.razor     # Wizard container / step-controller
│   │       │   ├── StepBasicInfo.razor   # Step 1: Hotel basic info
│   │       │   ├── StepImages.razor      # Step 2: Photo uploads (Cloudinary)
│   │       │   ├── StepAmenities.razor   # Step 3: Assign amenities
│   │       │   └── StepPolicies.razor    # Step 4: Assign policies
│   │       └── OwnerDashboard.razor
│   │
│   ├── Index.razor                # Main landing page
│   └── _Host.cshtml               # Root Razor Page hosting the Blazor app
│
├── Components/                    # Reusable, non-routable Blazor UI components
│   ├── Admin/
│   │   └── AdminSidebar.razor     # Sidebar navigation used in AdminLayout
│   ├── DualValidation.razor       # ⭐ NEW - Shared dual-field validation component
│   ├── HotelCard.razor
│   ├── OwnerHeader.razor
│   └── SearchForm.razor
│
├── ViewModels/                    # Data models specifically designed for Views
│   ├── Base/                      # Shared/Common ViewModels
│   ├── Form/                      # ViewModels for form inputs (Login, Register...)
│   ├── Hotel/                     # ViewModels for Hotel data display
│   ├── Request/                   # ViewModels for Request processes
│   ├── Response/                  # ViewModels mapping API responses
│   └── State/                     # ViewModels for application state management
│
├── Services/                      # Frontend services for API communication
│   ├── Base/                      # Base HttpClient configuration
│   ├── Interface/                 # Interfaces for frontend services
│   ├── HotelFormState.cs          # State management service for hotel creation wizard
│   ├── ManageService.cs           # Service: Admin/Management API calls
│   └── RequestService.cs          # Service: Request (Upgrade) API calls
│
├── Shared/                        # Shared layouts and global components
│   ├── AdminLayout.razor
│   ├── AuthLayout.razor           # ⭐ NEW - Dedicated layout for Login/Register pages
│   ├── ConfirmModal.razor         # ⭐ NEW - Global reusable confirmation modal
│   ├── Footer.razor
│   ├── Header.razor
│   ├── MainLayout.razor
│   ├── OwnerLayout.razor
│   └── Pagination.razor
│
├── Authentication/                # Custom authentication state providers and handlers
│   └── CustomAuthStateProvider.cs
│
├── Helpers/                       # Frontend helper utilities
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
├── App.razor                      # Root component of the Blazor application
├── _Imports.razor                 # Global using directives for Razor components
├── appsettings.json               # Frontend config (API Base URL, etc.)
└── Program.cs                     # WebAssembly/Server startup config and DI
```

---

### **Architecture & Conventions:**

1. **Pages vs Components**:
   - `Pages/`: Components with an `@page` directive, navigated via URL. Organized by user role (`Admin`, `User`) or functional domain (`Hotel`, `Public`).
   - `Components/`: Reusable UI elements embedded inside Pages or other Components — no `@page` directive.

2. **ViewModels**:
   - Serve as view-layer DTOs, decoupling UI from raw API response formats.
   - Separated into `Form` (user input), `Response` (API data display), `State` (navigation/wizard state), etc.

3. **Services**:
   - Encapsulate HTTP calls to `HotelBooking.api`.
   - Interfaces defined in `Services/Interface/` for testability and DI.
   - `Program.cs` registers services using `AddHttpClient` and `AddScoped`.

4. **Authentication**:
   - Custom `AuthenticationStateProvider` handles JWT tokens stored in Local Storage.
   - `AuthLayout.razor` provides a dedicated layout for Login/Register routes.

5. **Admin Management Pattern**:
   - `MasterDetailLayout.razor` provides a reusable two-panel layout for all entity managers.
   - `GlobalNavMenu.razor` lets users navigate between entity types (Amenities, Policies, Services, etc.).
   - `AdminPageBase.cs` provides code-behind logic (pagination, delete confirm, etc.) shared across Admin pages.

6. **Owner Hotel Wizard**:
   - `HotelWizard.razor` acts as the orchestrating container.
   - Each `Step*.razor` handles one phase of hotel creation.
   - `HotelFormState.cs` persists the multi-step form state as a scoped service.

---

Created: 07-Mar-2026
Updated: 10-Apr-2026
Version: 1.3
