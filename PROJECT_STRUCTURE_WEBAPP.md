# HotelBooking Project - WebApp (Frontend) Structure

_Last updated: 21-Apr-2026 | Version: 1.4_

---

## 📁 Blazor Server Frontend — `HotelBooking.webapp`

Built with **Blazor Server (.NET 9)**. Communicates with `HotelBooking.api` via typed `HttpClient` services.

```
HotelBooking.webapp/
├── Pages/                             # Routable Blazor components (@page directive)
│   │
│   ├── Admin/                         # Admin-only pages (requires Admin role)
│   │   ├── Base/
│   │   │   └── AdminPageBase.cs       # Shared code-behind base for all Admin pages
│   │   │
│   │   ├── Manage/                    # Entity management pages
│   │   │   ├── Groups/                # One manager component per entity type
│   │   │   │   ├── AmenityManager.razor
│   │   │   │   ├── BedTypeManager.razor
│   │   │   │   ├── PolicyManager.razor
│   │   │   │   ├── RoomQualityManager.razor
│   │   │   │   ├── RoomTypeManager.razor
│   │   │   │   ├── RoomViewManager.razor
│   │   │   │   ├── ServiceManager.razor
│   │   │   │   └── UnitTypeManager.razor
│   │   │   └── SharedLayouts/
│   │   │       ├── GlobalNavMenu.razor       # Sidebar nav for the management section
│   │   │       └── MasterDetailLayout.razor  # Two-panel layout (list + edit form)
│   │   │
│   │   ├── Request/                   # Admin-side request management
│   │   │   ├── Components/            # ⭐ Shared components reused across request pages
│   │   │   │   ├── RecentRequests.razor         # Dashboard: recent requests table widget
│   │   │   │   ├── RequestHistoryCard.razor      # Shared audit info card (approver, date)
│   │   │   │   ├── RequestPageHeader.razor       # Unified header with breadcrumbs + actions
│   │   │   │   ├── RequestOverview.razor         # Dashboard: stats summary cards
│   │   │   │   └── RequestStatusBadge.razor      # Centralized status badge styling
│   │   │   │
│   │   │   ├── HotelApproval/         # ⭐ NEW — Hotel registration approval flow
│   │   │   │   ├── Detail.razor       # Full detail view of a hotel registration request
│   │   │   │   └── Index.razor        # Paginated list of all hotel approval requests
│   │   │   │
│   │   │   ├── UpgradeOwner/          # Owner upgrade request flow
│   │   │   │   ├── Components/
│   │   │   │   │   ├── UpgradeFilters.razor   # Filter bar (status, search)
│   │   │   │   │   ├── UpgradeStatsCard.razor # Stats summary card
│   │   │   │   │   └── UpgradeTable.razor     # Data table with pagination
│   │   │   │   ├── Details.razor      # Full detail view of an upgrade request
│   │   │   │   └── Index.razor        # Paginated list of all upgrade requests
│   │   │   │
│   │   │   └── Index.razor            # Main request dashboard (overview + recent list)
│   │   │
│   │   └── AdminHomeDashboard.razor
│   │
│   ├── Hotel/
│   │   └── SearchResult.razor         # Hotel search results page
│   │
│   ├── Public/                        # No auth required
│   │   ├── About.razor
│   │   └── Hotels.razor
│   │
│   ├── User/
│   │   ├── Customer/
│   │   │   ├── CustomerLogin.razor
│   │   │   ├── CustomerRegister.razor
│   │   │   └── UpgradeOwnerForm.razor   # Customer: submit upgrade-to-owner request
│   │   │
│   │   └── Owner/
│   │       ├── Steps/                 # ⭐ Multi-step Hotel Creation Wizard
│   │       │   ├── HotelWizard.razor        # Wizard container / step controller
│   │       │   ├── StepBasicInfo.razor      # Step 1: Hotel basic info
│   │       │   ├── StepImages.razor         # Step 2: Photo uploads (Cloudinary)
│   │       │   ├── StepAmenities.razor      # Step 3: Assign amenities
│   │       │   └── StepPolicies.razor       # Step 4: Assign policies
│   │       └── OwnerDashboard.razor
│   │
│   ├── Index.razor                    # Landing page
│   └── _Host.cshtml                   # Root Razor Page that hosts the Blazor app
│
├── Components/                        # Reusable non-routable UI components
│   ├── Admin/
│   │   └── AdminSidebar.razor         # Sidebar navigation used in AdminLayout
│   ├── DualValidation.razor           # Shared two-field form validation component
│   ├── HotelCard.razor
│   ├── OwnerHeader.razor
│   └── SearchForm.razor
│
├── ViewModels/                        # View-layer data models (decouple UI from raw API)
│   ├── Base/                          # Shared/common ViewModels
│   ├── Form/                          # Input form ViewModels (Login, Register)
│   ├── Hotel/                         # Hotel display ViewModels
│   ├── Request/                       # Request-related ViewModels
│   │   ├── Base/                      # Shared request ViewModels
│   │   ├── HotelApproval/             # ⭐ NEW — HotelApproval-specific ViewModels
│   │   └── UpgradeRequest/            # UpgradeRequest-specific ViewModels
│   ├── Response/                      # ViewModels wrapping API responses
│   └── State/                         # Application/wizard state ViewModels
│
├── Services/                          # Frontend HTTP services (typed HttpClient)
│   ├── Base/                          # Base HttpClient wrapper/config
│   ├── Interface/                     # IRequestService, IManageService, etc.
│   ├── HotelFormState.cs              # Scoped state service for hotel creation wizard
│   ├── ManageService.cs               # Admin/Management API calls
│   └── RequestService.cs             # Request management API calls (Upgrade + HotelApproval facade)
│
├── Shared/                            # Shared layouts and globally available components
│   ├── AdminLayout.razor
│   ├── AuthLayout.razor               # Dedicated layout for Login/Register pages
│   ├── ConfirmModal.razor             # Global reusable confirmation modal
│   ├── Footer.razor
│   ├── Header.razor
│   ├── MainLayout.razor
│   ├── OwnerLayout.razor
│   └── Pagination.razor
│
├── Authentication/
│   └── CustomAuthStateProvider.cs     # Custom JWT-based AuthenticationStateProvider
│
├── Helpers/                           # Frontend-side utilities
│   ├── Common/
│   ├── Manage/
│   │   └── Icon/
│   ├── MessageResponse.cs             # Local frontend message constants (mirrors API)
│   └── StatusCodeResponse.cs          # HTTP status constants
│
├── wwwroot/                           # Static web assets
│   ├── css/                           # Custom stylesheets + Bootstrap + Open Iconic
│   ├── images/                        # Image assets (hotels, etc.)
│   └── js/                            # JavaScript interop files
│
├── App.razor                          # Root Blazor component (router config)
├── _Imports.razor                     # Global @using directives for Razor components
├── appsettings.json                   # Frontend config: API base URL, auth settings
└── Program.cs                         # Blazor Server startup, DI, middleware
```

---

## 🏗️ Architecture & Conventions

### 1. Pages vs Components

| | `Pages/` | `Components/` |
|-|----------|--------------|
| Has `@page` directive | ✅ Yes | ❌ No |
| Navigable via URL | ✅ Yes | ❌ No |
| Can embed Components | ✅ Yes | ✅ Yes |
| Reused across pages | ❌ Typically not | ✅ Yes |

### 2. Shared Request Components (`Pages/Admin/Request/Components/`)

Introduced in v1.4 to eliminate duplication between `HotelApproval` and `UpgradeOwner` pages:

| Component | Purpose |
|-----------|---------|
| `RequestPageHeader.razor` | Unified header with breadcrumbs and action buttons |
| `RequestStatusBadge.razor` | Consistent status badge styling (Pending/Approved/Rejected) |
| `RequestHistoryCard.razor` | Audit trail display (approver name, processed date) |
| `RecentRequests.razor` | Dashboard widget — recent requests across both types |
| `RequestOverview.razor` | Dashboard stats summary cards |

### 3. ViewModels

- **`Form/`** — Bind directly to form inputs (Login, Register, hotel wizard steps)
- **`Response/`** — Map and display API responses (lists, detail views)
- **`State/`** — Track UI state (wizard step, filter selections, modal visibility)
- **`Request/`** — Request-specific VMs split by domain (`HotelApproval/`, `UpgradeRequest/`)

### 4. RequestService Facade

`RequestService.cs` implements `IRequestService` and serves as a facade over both request types:

```csharp
// Single service, two domains — no code duplication at call-site
await RequestService.ApproveHotelAsync(hotelId, adminId);
await RequestService.ApproveUpgradeAsync(requestId, adminId);
```

### 5. Authentication

- `CustomAuthStateProvider` reads JWT from local storage and builds `ClaimsPrincipal`.
- `AuthLayout.razor` applies a minimal layout for Login/Register routes.
- Admin/Owner pages protected via route-level `[Authorize]` or `<AuthorizeView>`.

### 6. Admin Management Pattern

- `MasterDetailLayout.razor` → two-panel layout (entity list + inline edit form).
- `GlobalNavMenu.razor` → sidebar navigation across entity types.
- `AdminPageBase.cs` → shared code-behind (pagination, confirm delete, state resets).

### 7. Hotel Creation Wizard (Owner)

- `HotelWizard.razor` orchestrates step transitions.
- Each `Step*.razor` handles one isolated phase.
- `HotelFormState.cs` (scoped DI) persists data across steps without URL state.

---

## 📊 Feature Status

| Feature | Pages | Shared Components | Status |
|---------|-------|-------------------|--------|
| Admin — Entity Management | AmenityManager, PolicyManager... | MasterDetailLayout | ✅ Complete |
| Admin — Request Dashboard | Request/Index.razor | RequestOverview, RecentRequests | ✅ Complete |
| Admin — Upgrade Requests | UpgradeOwner/Index, Details | RequestPageHeader, RequestStatusBadge, RequestHistoryCard | ✅ Complete |
| Admin — Hotel Approval | **HotelApproval/Index, Detail** | Same shared components | ✅ Complete ⭐ |
| Owner — Hotel Wizard | Steps/HotelWizard + 4 Steps | — | ✅ Complete |
| Customer — Upgrade Form | UpgradeOwnerForm.razor | — | ✅ Complete |
| Public — Hotel Search | SearchResult.razor | SearchForm, HotelCard | ✅ Complete |
| Auth — Login/Register | CustomerLogin, CustomerRegister | AuthLayout | ✅ Complete |

---

Created: 07-Mar-2026
Updated: 21-Apr-2026
Version: 1.4
