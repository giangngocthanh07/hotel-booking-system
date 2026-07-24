# Hotel Booking Platform — High-Level Design

| Field | Value |
|---|---|
| Status | Approved baseline |
| Date | 2026-07-25 |
| Architecture style | Project-specific Clean Architecture — Layered Modular Monolith |
| Product stage | Graduation project with a small-production design baseline |
| Diagram source | [`diagrams/01-platform-hld.drawio`](diagrams/01-platform-hld.drawio) |

## 1. Purpose

This document defines the target high-level architecture for the Hotel Booking Platform. It describes the intended product rather than only the features currently implemented. Implementation progress is tracked separately with the following statuses:

- `Implemented`
- `Partial`
- `Planned`
- `Out of MVP`

The design preserves the current solution structure and its project-specific Clean Architecture rules. It does not introduce a separate Domain project or split the platform into microservices.

## 2. Product Scope

The platform connects travelers, hotel owners, and platform administrators.

### 2.1 Actors

| Actor | Responsibility |
|---|---|
| Guest | Searches and views hotels without signing in |
| Customer | Manages a profile, books rooms, pays, cancels, and reviews completed stays |
| Hotel Owner | Registers and manages hotels, rooms, inventory, bookings, and performance |
| Admin | Approves requests, manages users and master data, moderates content, and monitors the platform |
| VNPay | Processes online payments and sends signed callbacks |
| Cloudinary | Stores and serves hotel and room media |
| Email Provider | Delivers transactional notifications |

The Hotel Staff/Receptionist role is a future extension. Hotel Owners perform operational booking actions in the current target scope.

### 2.2 Target capabilities

- Identity, authentication, authorization, and user profiles
- Customer-to-Owner upgrade requests
- Hotel registration and approval
- Hotel catalog, media, amenities, and policies
- Room Type and physical room inventory management
- Hotel search, filtering, details, and availability
- Booking, check-in, check-out, cancellation, and history
- VNPay and pay-at-hotel payment methods
- Refund processing
- Reviews, Owner responses, and Admin moderation
- Admin and Owner dashboards
- In-application and email notifications

### 2.3 Explicitly deferred capabilities

- Hotel Staff/Receptionist role
- Multiple Room Types within one Booking
- Dynamic or seasonal pricing
- Loyalty and reward points
- Promotion engine
- Multi-currency settlement
- Real-time Customer–Hotel chat
- Microservices and message broker
- AI recommendation engine

## 3. Architecture Drivers

The architecture prioritizes:

1. A complete and explainable graduation-project demonstration.
2. Clear module and layer responsibilities.
3. Traceability from User Story to Acceptance Criteria, flow, API, data, and test.
4. Protection against overbooking and concurrent inventory races.
5. Secure and idempotent payment processing.
6. Maintainability within the existing codebase.
7. A practical upgrade path to a small-production deployment.

The design baseline assumes:

- 10,000 hotels
- 100,000 physical rooms
- 1,000,000 bookings
- 200 concurrent users

## 4. Architecture Style

The platform uses a project-specific Clean Architecture implemented as a layered modular monolith.

```text
Blazor Server Web App
        |
        | HTTPS/JSON
        v
ASP.NET Core Web API
        |
        v
Application Layer
        |
        v
Infrastructure Layer
        |
        v
SQL Server
```

Application and Infrastructure are internal layers of the API deployment. They are not independently deployed services.

### 4.1 Dependency and responsibility rules

- Blazor communicates with the backend through REST APIs and never accesses the database.
- API Controllers authenticate, authorize, bind requests, and delegate to Application Services.
- Application Services coordinate use cases, validation, business rules, transactions, and result mapping.
- The Application layer must not access `DbContext` or use EF Core query concepts directly.
- Complex queries and eager-loading strategies belong in specific repository implementations.
- Repositories track changes but never call `SaveChangesAsync()` themselves.
- An Application use case calls `IUnitOfWork.SaveChangesAsync()` exactly once at the end of an atomic transaction.
- Predictable validation and business failures return `Result<T>` or `ApiResponse<T>`.
- Unexpected system exceptions are handled by global API exception middleware.
- Asynchronous database, file, and network operations propagate `CancellationToken`.
- Logical modules remain in one deployable application and communicate through explicit service contracts.

### 4.2 External integrations

```text
API/Application --> VNPay
API/Application --> Cloudinary
API/Application --> Email Provider
```

External providers are accessed through abstractions so their implementations can be replaced or stubbed in tests.

## 5. Platform Modules

| Module | Responsibility |
|---|---|
| Identity & Access | Registration, login, JWT issuance, password management, and RBAC |
| User Profile | Customer and Owner profile management |
| Partner Onboarding | Owner upgrade and hotel approval workflows |
| Hotel Catalog | Hotel descriptions, amenities, policies, location, and property details |
| Media Management | Hotel and Room Type image metadata and Cloudinary integration |
| Room & Inventory | Room Types, physical rooms, capacity, attributes, and maintenance blocks |
| Search & Discovery | Hotel search, filtering, availability, and public details |
| Booking | Reservations, inventory holds, snapshots, assignment, and booking lifecycle |
| Payment & Refund | VNPay, pay-at-hotel, callbacks, idempotency, and refunds |
| Reviews | Guest reviews, Owner responses, and Admin moderation |
| Administration | Master data, user management, request queues, and dashboards |
| Notifications | Email and in-application notifications |
| Reporting | Owner and Admin operational and business metrics |

### 5.1 Module ownership rules

- Identity & Access owns credentials, roles, and authorization rules.
- Partner Onboarding owns request workflows and delegates role or hotel creation to the owning modules.
- Hotel Catalog owns descriptive hotel data but not live availability.
- Room & Inventory owns physical capacity, maintenance blocks, and availability inputs.
- Search & Discovery is read-oriented and does not own transactional data.
- Booking owns reservation state and immutable price and cancellation-policy snapshots.
- Payment & Refund owns financial transaction state and requests Booking transitions through a contract.
- Reviews validates eligibility against completed Bookings.
- Administration must not become a container for unrelated domain logic.
- Reporting reads from other modules and owns no transactional source-of-truth data.
- Notification delivery failure must not roll back a committed Booking or Payment.
- A pending notification-delivery record is written in the same database transaction as the business state. Delivery occurs after commit and is retried by an API-hosted background worker.

### 5.2 Cross-module communication

For the current architecture, modules communicate through Application Service interfaces in the same process:

```text
Controller
  -> Application Use Case
      -> Module Service/Repository
      -> Unit of Work
```

Direct cross-module repository access should be avoided. A consuming module requests a decision from the module that owns the rule.

## 6. Requirements and Test Traceability

### 6.1 Identifier convention

```text
User Story:       BOOK-US-01
Acceptance:       BOOK-US-01-AC-01
Business Flow:    BOOK-BF-01
Technical Flow:   BOOK-TF-01
Test Case:        BOOK-US-01-AC-01-TC-01
Non-Functional:   NFR-PERF-01
```

Legacy User Story IDs are retained in a `Legacy ID` field.

### 6.2 User Story requirements

Every User Story contains:

- Status
- Legacy ID
- Priority
- Actor
- Preconditions
- `As a / I want / So that`
- Acceptance Criteria in `Given / When / Then`
- Business rules
- Business Flow reference
- Technical Flow reference where required
- Test Case references for every Acceptance Criterion

Every Acceptance Criterion must be observable, unambiguous, and testable. A User Story is not `Done` until every Acceptance Criterion has passing test evidence.

### 6.3 Test mapping

| Acceptance Criterion category | Primary test type |
|---|---|
| Validation and calculations | Unit test |
| Application business rules | Unit test |
| Repository queries and transactions | Integration test |
| API contract and authorization | API integration test |
| Blazor interaction and forms | Component or UI test |
| VNPay and Cloudinary integration | Contract or sandbox integration test |
| Complete critical journey | End-to-end test |
| Purely visual presentation | Manual test with screenshot evidence |

Critical flows must cover happy paths, validation failures, authorization failures, duplicate requests, concurrency, and external-provider failures.

## 7. Flow and Diagram Strategy

Every User Story must appear in at least one Business Flow. Technical Flows are mandatory for stories involving significant business rules, transactions, authorization, concurrency, or external integrations.

### 7.1 Business Flows

Business Flows use swimlane activity diagrams and business terminology. They do not expose Controllers, Repositories, or database details.

Planned Business Flows include:

- Customer registration and login
- Customer-to-Owner upgrade
- Hotel registration and approval
- Hotel catalog and media management
- Room Type, inventory, and maintenance management
- Hotel search and discovery
- Customer booking journey
- Online payment and pay-at-hotel
- Cancellation and refund
- Booking operations and history
- Review, response, and moderation
- Master data administration
- Admin and Owner reporting
- Booking and approval notifications

### 7.2 Technical Flows

Technical Flows use UML sequence diagrams and follow:

```text
Blazor Page
  -> API Controller
  -> Application Service
  -> Validator
  -> Repository / Unit of Work
  -> SQL Server
  -> External Provider
```

Required Technical Flows include login and JWT issuance, approval transactions, Cloudinary upload, availability search, booking creation, concurrent booking protection, VNPay initiation, VNPay callback idempotency, cancellation/refund calculation, review eligibility, and dashboard aggregation.

### 7.3 State machines

State diagrams are required for:

- Booking
- Payment
- Refund
- Owner Upgrade Request
- Hotel Approval Request
- Physical Room

Every alternate or failure branch in a flow must map to an Acceptance Criterion and Test Case.

## 8. Booking Business Model

### 8.1 Reservation unit

A Booking contains one Room Type and one or more rooms of that Room Type. A Customer booking different Room Types creates separate Bookings.

Customers reserve a Room Type and quantity. They do not select a physical room number. Physical rooms are assigned by the Owner before or during check-in.

### 8.2 Pricing

The MVP uses a fixed nightly Room Type price:

```text
Subtotal = PricePerNight * RoomQuantity * NumberOfNights
Total = Subtotal + SelectedServices - Discount
```

The Booking stores immutable price and policy snapshots. Later changes to the Room Type price or cancellation policy do not affect existing Bookings.

### 8.3 Payment methods

- VNPay creates a `PendingPayment` Booking with a temporary inventory hold.
- Pay at Hotel creates a `Confirmed` Booking with an `Unpaid` Payment.
- The default online-payment hold duration is 15 minutes.
- A valid successful VNPay callback confirms the Booking.
- A failed callback marks the payment attempt as failed.
- An expired hold releases inventory.

### 8.4 Cancellation policy

Admins manage standardized policies. Owners select an allowed cancellation policy for a Room Type or offer. A snapshot is stored with the Booking and drives cancellation and refund calculations.

## 9. Availability and Concurrency

### 9.1 Availability model

```text
Available Quantity
  = Active Physical Rooms
  - Overlapping Reserved Quantity
  - Date-overlapping Maintenance Blocks
```

Date ranges overlap when:

```text
ExistingCheckIn < RequestedCheckOut
AND
ExistingCheckOut > RequestedCheckIn
```

Search availability is advisory. Availability is always rechecked during Booking creation.

### 9.2 Atomic reservation

SQL Server serializes reservation commands per Room Type:

1. Begin a database transaction.
2. Acquire an update lock on the selected `RoomType` row with `UPDLOCK, HOLDLOCK`.
3. Recalculate active physical capacity, overlapping reserved quantity, and maintenance blocks.
4. Reject the request with `AvailabilityConflict` when capacity is insufficient.
5. Create the Booking and Payment records.
6. Call `SaveChangesAsync()` exactly once and commit.

Booking creation and maintenance-block commands acquire the same Room Type lock before reading or changing availability. A Booking contains only one Room Type, so each transaction acquires one inventory lock. If a future command locks multiple Room Types, it must lock them in ascending ID order.

The overlapping-reservation query requires a supporting index beginning with `RoomTypeId` and including status and stay dates. SQL Server deadlock error `1205` may be retried only after the failed transaction has rolled back, with a bounded retry count and jitter. Stable command or Booking references prevent duplicate creation when clients retry an uncertain request.

Two concurrent requests competing for the final available room must produce:

```text
Request A -> Booking held or confirmed
Request B -> AvailabilityConflict
```

This behavior requires a concurrent integration test.

### 9.3 Booking creation

Online payment:

```text
Validate
-> Begin transaction
-> Lock and recheck availability
-> Create PendingPayment Booking
-> Create Pending Payment
-> Save once
-> Commit
-> Generate signed VNPay URL
```

Pay at Hotel:

```text
Validate
-> Begin transaction
-> Lock and recheck availability
-> Create Confirmed Booking
-> Create Unpaid Payment
-> Save once
-> Commit
```

Expired holds are excluded from the overlapping reserved quantity and therefore become available immediately, even before a cleanup worker persists their final `Expired` state.

### 9.4 Payment callback idempotency

A VNPay callback must:

1. Verify the gateway signature.
2. Match the amount and Booking reference.
3. Enforce a unique gateway transaction/reference.
4. Return success without reapplying an already processed result.
5. Update Payment and Booking atomically.
6. Save exactly once.
7. Persist a pending notification-delivery record in the same transaction.
8. Deliver the notification after commit through the background worker.

## 10. Security

### 10.1 Authentication and authorization

- JWT is used for API authentication.
- User identity is derived from trusted claims, not request-body identifiers.
- Authorization combines role checks and resource ownership.
- Customers can access only their own Bookings, payments, and profiles.
- Owners can manage only Hotels they own and Bookings made for those Hotels.
- Admin approval and moderation actions require Admin authorization.
- VNPay callbacks use signature verification rather than Customer JWT authentication.

### 10.2 Input, upload, and secret security

- FluentValidation runs before business logic.
- API validation remains mandatory even when Blazor validates a form.
- Uploads validate extension, MIME type, size, and allowed content type.
- Server-generated file identifiers replace untrusted uploaded filenames.
- Connection strings, JWT keys, Cloudinary credentials, and VNPay secrets are not committed.
- Logs never contain passwords, JWTs, payment secrets, or sensitive document contents.

### 10.3 Error contract

| HTTP status | Meaning |
|---|---|
| `400` | Invalid format or validation |
| `401` | Missing or invalid authentication |
| `403` | Authenticated identity without permission |
| `404` | Resource missing or not visible |
| `409` | Duplicate data, invalid state, or availability conflict |
| `422` | Valid input that violates a business rule |
| `500` | Unexpected system failure |

API failures include a stable error code and correlation ID. Stack traces and internal exception details are not returned to clients.

### 10.4 Audit

Approval decisions, role changes, Booking transitions, payment/refund transitions, review moderation, and maintenance blocks require an audit record containing:

```text
ActorId
Action
EntityType
EntityId
OldState
NewState
TimestampUtc
CorrelationId
```

## 11. Data Architecture

### 11.1 Logical ownership

| Module | Primary data |
|---|---|
| Identity & Access | `User`, `Role`, `UserRole` |
| Partner Onboarding | `UpgradeRequest`, `HotelApprovalRequest` |
| Hotel Catalog | `Hotel`, amenities, policies, and location data |
| Media Management | `HotelImage`, `RoomImage` |
| Room & Inventory | `RoomType`, `Room`, `RoomBlock`, room attributes |
| Booking | `Booking`, `BookingRoom`, `BookingService` |
| Payment & Refund | `Payment`, `Refund` |
| Reviews | `Review`, `ReviewResponse` |
| Notifications | `Notification`, delivery attempts |
| Audit | `AuditLog` |

### 11.2 Integrity rules

- `CheckOutDate` is later than `CheckInDate`.
- `RoomQuantity` is greater than zero.
- Money uses `decimal`.
- `BookingReference` is stable and unique.
- One completed Booking has at most one Review.
- Gateway transaction/reference is unique.
- Room number is unique within a Hotel.
- Unapproved or soft-deleted Hotels never appear in public search.
- Important invariants are enforced with database constraints in addition to validation.

### 11.3 Snapshot and extensibility rules

Stable, queryable business data uses explicit columns. The existing `Additional` JSON field is reserved for optional metadata, not core business fields.

Target Booking snapshot data includes Room Type name, price per night, quantity, cancellation policy, selected-service prices, subtotal, discounts, total, and currency.

### 11.4 Database-first workflow

```text
Approved data design
-> Update a versioned SQL schema script
-> Apply the schema to a development database
-> Re-scaffold EF Core models
-> Preserve custom behavior in partial classes
-> Update repositories and tests
```

Generated entity files do not contain custom business logic.

## 12. Non-Functional Requirements

### 12.1 Performance

| ID | Requirement |
|---|---|
| `NFR-PERF-01` | Search API p95 is at most 2 seconds |
| `NFR-PERF-02` | Hotel details API p95 is at most 1.5 seconds |
| `NFR-PERF-03` | Booking creation p95 is at most 2 seconds, excluding payment redirect |
| `NFR-PERF-04` | Payment callback processing p95 is at most 1 second |
| `NFR-PERF-05` | Paginated Admin and Owner lists p95 are at most 2 seconds |
| `NFR-PERF-06` | The platform supports 200 concurrent users |

Pagination and filtering execute in the database. Performance tests use a dataset representative of the design baseline.

### 12.2 Caching

Master data, location data, and short-lived public descriptive data may be cached. Live availability, Booking state, Payment state, and ownership decisions never use cache as their source of truth.

The initial deployment may use in-memory caching. Distributed caching is a scale-out option.

### 12.3 Reliability

| Metric | Target |
|---|---|
| Availability | 99.5% monthly |
| Recovery Point Objective | 1 hour |
| Recovery Time Objective | 4 hours |

SQL Server requires scheduled backups and a documented restore procedure. External calls use explicit timeouts and safe retry policies. Payment commands are never retried without idempotency protection.

The API hosts a `BackgroundService` for expired-hold cleanup and pending-notification delivery. The worker is a logical component inside the API process, not a separate deployment. When multiple API instances run, workers coordinate through atomic database claims and idempotent handlers so only one instance processes a due record. Failed notification attempts record an attempt count, next-attempt time, and terminal failure state.

### 12.4 Observability

Every request carries a `CorrelationId` across the Web App, API, Application, Infrastructure, and external-provider calls.

Structured logging includes:

```text
TimestampUtc
LogLevel
CorrelationId
UserId
Module
Operation
EntityId
DurationMs
Outcome
ErrorCode
```

Key metrics include search latency, Booking success/conflict rate, active and expired holds, payment outcomes and callback replays, external-provider latency, database duration, and Blazor circuit failures.

### 12.5 Time

- Technical timestamps are stored in UTC.
- Check-in and check-out are Hotel business dates.
- UI converts timestamps to the display timezone.
- Application logic uses an `IClock` abstraction instead of `DateTime.Now`.

## 13. Deployment

### 13.1 Initial deployment

```text
Browser
  -> Blazor Server
  -> ASP.NET Core API
  -> SQL Server

ASP.NET Core API -> Cloudinary
ASP.NET Core API -> VNPay
ASP.NET Core API -> Email Provider
```

The ASP.NET Core API process also hosts the scheduled background worker. Both request handlers and the worker use the same Application contracts and SQL Server database. No message broker is required.

### 13.2 Scale-out path

```text
Load Balancer
|-- Blazor Server instances
|   `-- Sticky sessions or managed SignalR
`-- API instances
    |-- Distributed cache
    `-- Shared SQL Server
```

Scale-out is not required for the graduation deployment, but the boundaries must allow it without changing core business behavior.

## 14. Current-to-Target Gaps

Initial code inspection identified the following design gaps:

1. Booking creation currently assigns physical rooms immediately instead of reserving Room Type quantity.
2. Booking creation currently calls `SaveChangesAsync()` more than once.
3. The database default Room status is `Available`, while an availability query currently expects `Active`.
4. Booking does not have explicit quantity, stable reference, hold expiry, or snapshot fields.
5. Payment callback processing requires signed-provider verification and stronger idempotency constraints.
6. Refund, RoomBlock, AuditLog, and Review-to-Booking uniqueness are not represented in the current model.
7. Cancellation policy is currently associated at Hotel level rather than the selected Room Type or offer.
8. Durable pending-notification delivery and worker-claim data are not represented in the current model.

These are target-design findings, not authorization to change implementation during the HLD phase.

## 15. Target User Story Catalog

The target catalog contains 47 User Stories across:

- Identity and Profile
- Partner Onboarding
- Hotel Catalog and Media
- Room and Inventory
- Search and Discovery
- Booking Operations
- Payment and Refund
- Reviews
- Administration and Reporting
- Notifications

The detailed catalog and Acceptance Criteria are produced after this HLD baseline is reviewed.

## 16. Documentation Delivery

```text
docs/
|-- architecture/
|   |-- high-level-design.md
|   |-- architecture-decisions/
|   `-- diagrams/
|       |-- 01-platform-hld.drawio
|       |-- 02-business-flows.drawio
|       |-- 03-technical-flows.drawio
|       `-- 04-state-machines.drawio
|-- requirements/
|   |-- product-scope.md
|   |-- user-story-catalog.md
|   |-- traceability-matrix.md
|   `-- domains/
`-- testing/
    |-- acceptance-test-strategy.md
    `-- test-case-catalog.md
```

Delivery proceeds in five phases:

1. HLD baseline and platform diagrams.
2. User Story baseline, legacy mapping, status, priority, and dependency assessment.
3. Domain-by-domain Acceptance Criteria, business rules, flows, and AC-based tests.
4. State machines and cross-domain consistency validation.
5. Traceability matrix, current-to-target gap analysis, and implementation backlog.

## 17. Architecture Diagram Index

The editable draw.io source contains:

1. `System Context`
2. `Container Architecture`
3. `Application Modules`
4. `Deployment View`

See [`diagrams/01-platform-hld.drawio`](diagrams/01-platform-hld.drawio).
