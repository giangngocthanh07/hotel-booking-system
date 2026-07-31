# Hotel Booking Platform — Software Requirements Specification

| Field | Value |
|---|---|
| Document ID | `HBP-SRS-001` |
| Version | `0.1` |
| Status | Review Draft |
| Date | 2026-07-25 |
| Product | Hotel Booking Platform |
| Document owner | Product / Business Analysis |
| Language | English |
| Requirements baseline | Target product |
| Architecture reference | [High-Level Design](../architecture/high-level-design.md) |
| Requirements standard | ISO/IEC/IEEE 29148:2018, tailored for this project |

## Revision History

| Version | Date | Author | Description |
|---|---|---|---|
| `0.1` | 2026-07-25 | Project Team | Initial review draft derived from the approved HLD, current source inspection, and archived requirements |

## Approval

| Role | Responsibility | Status |
|---|---|---|
| Product Owner | Confirms product scope and business value | Pending Review |
| Business Analyst | Confirms requirement clarity, completeness, and traceability | Pending Review |
| Solution Architect | Confirms alignment with architecture constraints | Pending Review |
| Quality Assurance | Confirms that requirements are verifiable | Pending Review |

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Product Overview](#2-product-overview)
3. [External Interface Requirements](#3-external-interface-requirements)
4. [Functional Requirements](#4-functional-requirements)
5. [Business Rules](#5-business-rules)
6. [Data Requirements](#6-data-requirements)
7. [Non-Functional Requirements](#7-non-functional-requirements)
8. [Verification and Acceptance](#8-verification-and-acceptance)
9. [Traceability Model](#9-traceability-model)
10. [Requirement Quality Checklist](#10-requirement-quality-checklist)
11. [Glossary](#appendix-a--glossary)
12. [Implementation Status Interpretation](#appendix-b--implementation-status-interpretation)
13. [Review Focus](#appendix-c--review-focus)

---

## 1. Introduction

### 1.1 Purpose

This document defines the features and constraints of the Hotel Booking Platform. It provides the product requirements that guide User Stories, Acceptance Criteria, business flows, and test cases while remaining aligned with the approved High-Level Design.

This SRS describes the target product. It does not imply that every requirement has already been implemented.

### 1.2 Scope

The Hotel Booking Platform serves Guests, Customers, Hotel Owners, and Admins. It supports hotel discovery, partner onboarding, hotel and room management, booking, payment, reviews, operational management, reporting, and notifications.

The product is a graduation project inspired by Booking.com and designed to support both MVP delivery and future production deployment.

### 1.3 Intended Audience

- Product Owners and Business Analysts
- Solution Architects and Developers
- Quality Assurance Engineers
- Project Reviewers and Assessors
- Operations and Support Teams

### 1.4 Requirement Language

The keywords in this document have the following meanings:

| Keyword | Meaning |
|---|---|
| `must` | Required system behavior or constraint |
| `can` | Required capability provided to an actor |
| `should` | Recommended behavior that may be deferred with approval |
| `may` | Permitted behavior or optional capability |

Requirement statements use `must` for required system behavior and `can` for required actor capabilities. Explanatory text does not create additional requirements.

### 1.5 Requirement Attributes

| Attribute | Values |
|---|---|
| Priority | `Must`, `Should`, `Could` |
| Implementation Status | `Implemented`, `Partial`, `Planned`, `Out of MVP` |
| Verification Method | `Test`, `Inspection`, `Analysis`, `Demonstration` |

Priority values have the following meanings:

| Priority | Meaning |
|---|---|
| `Must` | Required for the MVP baseline |
| `Should` | Important but may be deferred with approval |
| `Could` | Optional and implemented only when time and resources allow |

Implementation Status is based on repository inspection at the date of this draft and is not acceptance evidence. A requirement is complete only when its Acceptance Criteria are satisfied and its specified verification method has passed.

### 1.6 Requirement Identifier Convention

```text
Functional Requirement:       FR-<DOMAIN>-NNN
Business Rule:                BR-<DOMAIN>-NNN
Data Requirement:             DR-NNN
External Interface:           EIR-<INTERFACE>-NNN
Non-Functional Requirement:   NFR-<QUALITY>-NNN
```

Domain codes used in this SRS:

| Code | Domain |
|---|---|
| `IAM` | Identity and Access |
| `PROF` | User Profile |
| `ONB` | Partner Onboarding |
| `CAT` | Hotel Catalog |
| `MED` | Media Management |
| `INV` | Room and Inventory |
| `SRCH` | Search and Discovery |
| `BOOK` | Booking |
| `PAY` | Payment and Refund |
| `REV` | Reviews |
| `ADM` | Administration |
| `NOTIF` | Notifications |
| `RPT` | Reporting |
| `AUD` | Audit Events |

### 1.7 References

1. [ISO/IEC/IEEE 29148:2018](https://www.iso.org/standard/72089.html), Systems and software engineering — Life cycle processes — Requirements engineering.
2. [Hotel Booking Platform High-Level Design](../architecture/high-level-design.md).
3. [Platform HLD draw.io source](../architecture/diagrams/01-platform-hld.drawio).
4. Project engineering rules in [CLAUDE.md](../../CLAUDE.md) and [GEMINI.md](../../GEMINI.md).

---

## 2. Product Overview

### 2.1 Product Perspective

The platform is a layered modular monolith consisting of:

```text
Web Browser
  -> Blazor Server Web App
      -> ASP.NET Core Web API
          -> Application Layer
              -> Infrastructure Layer
                  -> SQL Server
```

The API integrates with VNPay, Cloudinary, and an Email Provider. It also runs scheduled tasks for expired Booking holds, notification delivery, and retryable operations.

### 2.2 Product Objectives

The product aims to:

1. Enable Guests to discover and compare approved Hotels.
2. Enable Customers to book, pay for, manage, and review Hotel stays.
3. Enable approved Hotel Owners to manage their Hotels, rooms, inventory, and Bookings.
4. Enable Admins to manage users, approvals, master data, content moderation, and reports.
5. Prevent overbooking and keep Booking and Payment states consistent.

### 2.3 User Classes

| User Class | Description | Primary Capabilities |
|---|---|---|
| Guest | Unauthenticated visitor | Search hotels, view details, read reviews, register, and log in |
| Customer | Authenticated traveler | Manage profile, book, pay, cancel, view history, and review completed stays |
| Hotel Owner | Approved Hotel Owner | Manage owned Hotels, Room Types, physical rooms, Bookings, review responses, and operational reports |
| Admin | Platform operator | Approve requests, manage users and master data, moderate content, and view platform reports |

Hotel Staff or Receptionist is not an active user class in the MVP. Hotel Owners perform hotel operational actions.

### 2.4 External Systems and Data Services

| System | Responsibility |
|---|---|
| VNPay | Processes online payment and sends signed callbacks |
| Cloudinary | Stores and serves hotel, room, profile, and supporting media |
| Email Provider | Delivers transactional email |
| SQL Server | Stores transactional and configuration data |
| RabbitMQ | Future durable transport for published Audit Events |
| MongoDB | Future append-oriented query store for the standalone Audit Event Service |

### 2.5 Operating Environment

- Web browsers supported by `NFR-COMP-001`
- ASP.NET Core and Blazor Server runtime
- SQL Server relational database
- HTTPS network connectivity
- External-provider sandbox or production endpoints configured per environment

### 2.6 Product Constraints

#### 2.6.1 Architecture Constraints

1. The solution must retain the current project-specific Clean Architecture.
2. The solution must not introduce a separate Domain project.
3. The Application layer must not access Entity Framework Core or `DbContext`.
4. All database operations must pass through repositories and `IUnitOfWork`.
5. The initial deployment must remain a modular monolith.
6. The database-first EF Core workflow must be preserved.

#### 2.6.2 MVP Business Constraints

1. An MVP Booking may reserve one or more rooms, but all rooms must belong to the same Room Type.
2. The MVP must use VND as the transaction currency.
3. Hotel business dates must use the Hotel's configured time zone, while technical timestamps must be stored in UTC.

### 2.7 Assumptions and Dependencies

1. Hotel Owners are responsible for the accuracy and legal validity of submitted property information.
2. Admins are authorized to decide account-upgrade and hotel-approval requests.
3. VNPay, Cloudinary, and Email Provider availability is outside the direct control of the platform.
4. A Hotel must have physical room inventory before it can provide bookable availability.

### 2.8 Out of MVP Scope

#### 2.8.1 Functional Scope Exclusions

- Hotel Staff or Receptionist role
- Multiple Room Types under one Booking reference; the MVP may reserve multiple rooms only when they belong to the same Room Type
- Accommodation inventory models beyond the MVP Hotel, Room Type, and physical-room model
- Dynamic or seasonal pricing
- Promotion engine
- Loyalty and reward points
- Multi-currency settlement
- Real-time Customer-to-Hotel chat
- AI recommendation engine

#### 2.8.2 Architecture Scope Exclusions

- RabbitMQ, standalone Audit Event Service, and MongoDB Audit Event storage
- Other microservices and message-broker-based integrations

### 2.9 Future Evolution

#### 2.9.1 Accommodation Property Model

The current code and MVP terminology use `Hotel`, `RoomType`, and `Room`. Future expansion is intended to follow the conceptual model below while reusing the core Booking, Payment, Refund, Review, and Notification workflows:

```text
Accommodation Property
├── Property Type
│   ├── Hotel
│   ├── Resort
│   ├── Serviced Apartment
│   ├── Villa
│   ├── Homestay
│   └── Hostel
├── Inventory Model
│   ├── UnitBased
│   └── EntirePlace
└── Accommodation Unit Type
    └── Physical Unit
```

`UnitBased` inventory represents Hotels, Resorts, and similar properties where Customers reserve a sellable unit type and quantity. `EntirePlace` inventory represents a Villa, Serviced Apartment, or Homestay that is reserved as one sellable unit.

A future multi-item Reservation may contain different Accommodation Unit Types, but all Reservation Items are intended to belong to one Accommodation Property.

#### 2.9.2 Audit Event Architecture

Operational logs describe technical execution, while Audit Events describe material business actions and state changes.

A future event-driven audit solution may use a SQL Outbox, RabbitMQ, a standalone Audit Event Service, and MongoDB. Audit delivery is intended to remain asynchronous so that an audit-system failure does not reverse a completed business operation.

---

## 3. External Interface Requirements

### 3.1 User Interface

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `EIR-UI-001` | The Web App must provide responsive interfaces for Guest, Customer, Hotel Owner, and Admin workflows. | Must | Partial | Demonstration |
| `EIR-UI-002` | The Web App must present role-appropriate navigation and prevent display of actions unavailable to the signed-in role. | Must | Partial | Test |
| `EIR-UI-003` | The Web App must display validation errors adjacent to the affected input or in a clearly associated validation summary. | Must | Partial | Demonstration |
| `EIR-UI-004` | The Web App must display loading, empty, success, and failure states for asynchronous operations. | Must | Partial | Demonstration |
| `EIR-UI-005` | The Web App must require explicit confirmation before destructive or irreversible actions. | Must | Partial | Test |
| `EIR-UI-006` | The Web App must preserve the active Blazor circuit when a recoverable component error occurs. | Should | Partial | Test |

### 3.2 Web API

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `EIR-API-001` | The Web App must communicate with the backend through versioned HTTPS JSON APIs. | Must | Implemented | Inspection |
| `EIR-API-002` | The API must use JWT bearer authentication for protected Customer, Owner, and Admin endpoints. | Must | Implemented | Test |
| `EIR-API-003` | The API must return a consistent success and error envelope for application outcomes. | Must | Partial | Test |
| `EIR-API-004` | API errors must include a stable error code and correlation identifier. | Must | Planned | Test |
| `EIR-API-005` | The API must not expose stack traces, connection details, or secrets to clients. | Must | Partial | Test |
| `EIR-API-006` | The API must apply request validation before application business logic executes. | Must | Partial | Test |

### 3.3 VNPay

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `EIR-VNPAY-001` | The platform must create VNPay payment requests using provider-required fields and a server-side signature. | Must | Planned | Contract Test |
| `EIR-VNPAY-002` | The platform must expose a callback endpoint that can receive VNPay payment results without Customer JWT authentication. | Must | Partial | Contract Test |
| `EIR-VNPAY-003` | The platform must verify the VNPay callback signature before changing Payment or Booking state. | Must | Planned | Security Test |
| `EIR-VNPAY-004` | The platform must match callback amount, currency, Booking reference, and provider transaction reference to platform records. | Must | Planned | Integration Test |
| `EIR-VNPAY-005` | The platform must return the provider-required acknowledgement for valid duplicate callbacks without reapplying the transaction. | Must | Planned | Integration Test |

### 3.4 Cloudinary

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `EIR-CLOUD-001` | The platform must upload accepted media through a server-controlled Cloudinary integration. | Must | Partial | Integration Test |
| `EIR-CLOUD-002` | The platform must store only required Cloudinary identifiers, metadata, and delivery URLs in SQL Server. | Must | Partial | Inspection |
| `EIR-CLOUD-003` | The platform must delete or replace Cloudinary assets only after confirming the requesting user's ownership or Admin authority. | Must | Partial | Test |
| `EIR-CLOUD-004` | A Cloudinary failure must return a recoverable application error without committing incomplete media metadata. | Must | Partial | Integration Test |

### 3.5 Email Provider

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `EIR-EMAIL-001` | The platform must deliver transactional email through a configurable Email Provider adapter. | Should | Planned | Integration Test |
| `EIR-EMAIL-002` | Email delivery failure must not roll back a committed Booking, Payment, or approval decision. | Must | Planned | Integration Test |
| `EIR-EMAIL-003` | Email messages must not include passwords, JWTs, payment secrets, or sensitive identity documents. | Must | Planned | Inspection |

### 3.6 SQL Server

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `EIR-DB-001` | The API Infrastructure layer must access SQL Server through Entity Framework Core and repository implementations. | Must | Implemented | Inspection |
| `EIR-DB-002` | Database connection strings and credentials must be provided through environment-specific configuration and must not be committed to source control. | Must | Partial | Inspection |
| `EIR-DB-003` | Transactional use cases must commit through `IUnitOfWork`. | Must | Partial | Integration Test |

### 3.7 RabbitMQ — Future

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `EIR-RMQ-001` | The future Outbox Publisher must publish versioned Audit Events to a durable RabbitMQ exchange after the originating SQL transaction commits. | Should | Out of MVP | Integration Test |
| `EIR-RMQ-002` | RabbitMQ delivery must use persistent messages and durable queues for Audit Events. | Should | Out of MVP | Inspection |
| `EIR-RMQ-003` | The Audit Event Service must acknowledge a message only after the event is durably stored or recognized as an already processed duplicate. | Should | Out of MVP | Integration Test |
| `EIR-RMQ-004` | Messages that exceed the configured retry policy must be routed to a Dead Letter Queue. | Should | Out of MVP | Integration Test |
| `EIR-RMQ-005` | RabbitMQ unavailability must not roll back an already committed business operation. | Should | Out of MVP | Failure Test |

### 3.8 MongoDB — Future

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `EIR-MONGO-001` | The future Audit Event Service must persist consumed Audit Events to MongoDB. | Should | Out of MVP | Integration Test |
| `EIR-MONGO-002` | The Audit Event Service must use a unique `EventId` constraint to make message consumption idempotent. | Should | Out of MVP | Integration Test |
| `EIR-MONGO-003` | MongoDB audit documents must be append-oriented and must not be changed by ordinary business workflows. | Should | Out of MVP | Inspection |

---

## 4. Functional Requirements

### 4.1 Identity and Access

#### 4.1.1 Account Registration and Password Protection

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-IAM-001` | A Guest can create a Customer account by providing a username, full name, email address, phone number, password, and matching password confirmation. All fields are required. | Must | Implemented | API Test |
| `FR-IAM-002` | The same username or email can belong to only one account. The system must reject a new registration if either one is already used, even by an account that has been deactivated or marked as deleted. | Must | Implemented | Test |
| `FR-IAM-003` | A password must contain between 8 and 64 characters, including at least one uppercase letter, one lowercase letter, one number, and one special character. The same rules must apply during registration and password reset, and the password confirmation must match exactly. | Must | Partial | Test |
| `FR-IAM-004` | After a Guest registers successfully, the system must automatically assign the `Customer` role. The registration form must not allow the Guest to choose or submit another role. | Must | Implemented | Test |
| `FR-IAM-005` | The system must never store a password in readable form. Before saving it, the system must protect it with BCrypt using a separate salt for each password. Passwords must never appear in API responses or application logs. | Must | Implemented | Inspection |

#### 4.1.2 Sign-In and Access Token

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-IAM-006` | A user can sign in with either their username or email address and password, but only when the account is active and not marked as deleted. If sign-in fails, the system must show the same general error message without revealing whether the username, email, or password was incorrect. | Must | Partial | API Test |
| `FR-IAM-007` | After a successful sign-in, the system must issue a signed access token that is valid for 24 hours. The token must contain the user's stable account ID, assigned roles, issue time, and expiration time. It must not contain passwords, phone numbers, dates of birth, email addresses, full names, avatars, or other unnecessary profile data. | Must | Partial | Security Test |
| `FR-IAM-008` | When a user tries to access a protected feature, the system must reject the request if the access token is missing, expired, invalid, changed after it was issued, or belongs to an account that is no longer active. The system must perform no protected action and return `401 Unauthorized` without revealing security details. | Must | Partial | Security Test |
| `FR-IAM-009` | A signed-in user can view their own account ID, username, and currently assigned roles. This information must come from the latest account data so that role changes are reflected, and it must not include passwords, access tokens, or internal security data. | Must | Implemented | API Test |

#### 4.1.3 Role and Ownership Checks

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-IAM-010` | Before allowing a protected action, the system must check that the signed-in user has the required role. A Customer must not access Hotel Owner or Admin functions, and a Hotel Owner must not access Admin functions unless the account also has the required role. If the role is missing, the system must perform no action and return `403 Forbidden`. | Must | Partial | Authorization Test |
| `FR-IAM-011` | Having the correct role is not enough to access another user's data. A Customer can access only their own Bookings, Payments, Refunds, Reviews, and Notifications. A Hotel Owner can access only Hotels they own and the Rooms, Bookings, Payments, and Reviews belonging to those Hotels. If the data does not belong to the user, the system must perform no action and return `403 Forbidden` without showing private details. | Must | Partial | Authorization Test |

#### 4.1.4 Password Recovery

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-IAM-012` | A user can request a password-reset link using their registered email address. The system must show the same response whether the email exists or not. For an active account, the link must be sent by email, work only once, and expire after 15 minutes. After a successful reset, the link must no longer work, the new password must meet the registration password rules, and all existing login sessions must be invalidated. | Must | Planned | Security Test |

### 4.2 User Profile

#### 4.2.1 Profile View

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-PROF-001` | A signed-in user can view their own username, full name, email address, phone number, address, date of birth, and avatar. The profile must not show passwords, password hashes, access tokens, or internal security data. | Must | Partial | API Test |

#### 4.2.2 Profile Details Update

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-PROF-002` | A signed-in user can update their own full name, phone number, address, and date of birth. Full name and phone number must not be left empty, while address and date of birth are optional. Username and email address cannot be changed through the profile-update feature. | Must | Partial | Test |
| `FR-PROF-003` | The system must update only the profile of the currently signed-in user. It must not accept another user's account ID for a profile update, and it must reject any attempt to change the username, email address, password, assigned roles, or account status through this feature. | Must | Partial | Authorization Test |
| `FR-PROF-004` | Before saving a profile update, the system must check that the full name contains 2 to 100 characters, the phone number contains exactly 10 digits, the optional address contains no more than 255 characters, and the optional date of birth is earlier than today. If any value is invalid, the system must show the reason and save none of the changes. | Must | Partial | Test |
| `FR-PROF-005` | After a profile update is saved successfully, the system must return the user's current saved profile with the updated values. If saving fails, the system must show an error and keep the previous profile unchanged. | Must | Implemented | API Test |

#### 4.2.3 Avatar Management

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-PROF-006` | A signed-in user can upload or replace only their own avatar using a JPEG, PNG, or WebP image of no more than 5 MB. The system must reject an unsupported or invalid file. When replacing an avatar, the current avatar must remain unchanged until the new image has been uploaded successfully. | Should | Planned | Integration Test |

### 4.3 Partner Onboarding

#### 4.3.1 Account Upgrade Request

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-ONB-001` | A signed-in Customer who does not already have the `Hotel Owner` role can submit an account-upgrade request by providing a business address of no more than 500 characters and a tax code containing either 10 or 13 digits. Submitting the request must not grant the `Hotel Owner` role until an Admin approves it. | Must | Implemented | API Test |
| `FR-ONB-002` | After a valid account-upgrade request is submitted, the system must set its status to `Pending` and record the submission time. It must remain `Pending` until an Admin approves or rejects it, or the Customer cancels it. | Must | Implemented | Test |
| `FR-ONB-003` | A Customer can have only one `Pending` account-upgrade request at a time. If the same Customer submits multiple requests at the same time, the system must save only one. After a request is `Rejected` or `Cancelled`, the Customer can submit a new one. | Must | Partial | Test |
| `FR-ONB-004` | The user who submitted an account-upgrade request can view only their own request history, even after becoming a Hotel Owner. Requests must be shown from newest to oldest and include the business address, tax code, status, submission time, decision time when available, and rejection reason when applicable. | Must | Partial | Authorization Test |

#### 4.3.2 Account Upgrade Admin Review

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-ONB-005` | Only an authorized Admin can view account-upgrade requests. The list must be divided into pages, ordered from newest to oldest, and filterable by `Pending`, `Approved`, `Rejected`, or `Cancelled` status. Admins can open a request to view the requester's full name, email address, business address, tax code, submission time, current status, processing time, processing Admin, and rejection reason when applicable. | Must | Partial | API Test |
| `FR-ONB-006` | An authorized Admin can approve a `Pending` account-upgrade request, but not one submitted by their own account. When the request is approved, the system must record the Admin and approval time. | Must | Partial | Test |
| `FR-ONB-007` | When an account-upgrade request is approved, the system must keep the `Customer` role, add the `Hotel Owner` role, change the request status to `Approved`, and record the approving Admin and approval time. All changes must be saved together. If any change cannot be saved, none of them must be saved and the request must remain `Pending`. | Must | Partial | Integration Test |
| `FR-ONB-008` | An authorized Admin can reject a `Pending` account-upgrade request, but not one submitted by their own account. The Admin must provide a reason containing 10 to 500 characters. The system must change the request status to `Rejected` and record the reason, rejecting Admin, and rejection time. The requester must be able to view the reason. | Must | Partial | Test |

#### 4.3.3 Hotel Registration Request

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-ONB-009` | A signed-in Hotel Owner can submit a registration request for a Hotel. The request must include the Hotel name, Property Type, public phone number, public email address, full address with province and ward, tax code, and an uploaded business-license document. Description, star rating, latitude, and longitude are optional. | Must | Partial | API Test |
| `FR-ONB-010` | Before accepting a hotel-registration request, the system must check that the Hotel name contains 6 to 50 characters, the optional description contains no more than 500 characters, the address contains 10 to 500 characters, the selected Property Type is active, the optional star rating is from 1 to 5, the public phone number contains exactly 10 digits, and the public email address is valid. The province and ward must exist, and the ward must belong to the selected province. If latitude or longitude is provided, both must be provided; latitude must be from -90 to 90 and longitude from -180 to 180. | Must | Partial | Test |
| `FR-ONB-011` | The tax code must contain either 10 or 13 digits. The business-license document must be a PDF, JPEG, or PNG file of no more than 10 MB. The file extension and actual file type must match. If any information or file is invalid, the system must show the reason and must not create the request. | Must | Partial | Security Test |
| `FR-ONB-012` | After a valid hotel-registration request is submitted, the system must set its status to `Pending` and record the submission time. The Hotel must not be created or published at this stage. The request must remain `Pending` until an Admin approves or rejects it, or the Hotel Owner cancels it. | Must | Implemented | Test |
| `FR-ONB-013` | A Hotel Owner can view only their own hotel-registration request history, ordered from newest to oldest. Each request must show the submitted Hotel information, business-license document, status, submission time, decision time when available, and rejection reason when applicable. The business-license document must be accessible only to the requesting Owner and authorized Admins. | Must | Partial | Authorization Test |

#### 4.3.4 Hotel Registration Admin Review

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-ONB-014` | Only an authorized Admin can view hotel-registration requests. The list must be divided into pages, ordered from newest to oldest, and filterable by `Pending`, `Approved`, `Rejected`, or `Cancelled` status. An Admin can open a request to view the Owner's name and email, the submitted Hotel information, business-license document, current status, submission time, decision time, processing Admin, and rejection reason when available. | Must | Partial | Authorization Test |
| `FR-ONB-015` | Only an authorized Admin can approve a hotel-registration request that is currently `Pending`. An Admin cannot approve a request submitted by their own account. | Must | Partial | Authorization Test |
| `FR-ONB-016` | When an Admin approves a hotel-registration request, the system must create one Hotel owned by the requesting Hotel Owner, set the request status to `Approved`, and record the processing Admin and decision time. All these changes must succeed together. If any change fails, the Hotel must not be created and the request must remain `Pending`. The new Hotel must not appear in public search until it meets the publication requirements in Section 4.4. | Must | Partial | Integration Test |
| `FR-ONB-017` | Only an authorized Admin can reject a hotel-registration request that is currently `Pending`. The Admin must provide a rejection reason containing 10 to 500 characters and cannot reject a request submitted by their own account. The system must set the request status to `Rejected` and record the reason, processing Admin, and decision time. The requesting Hotel Owner can view the rejection reason. | Must | Partial | Authorization Test |

#### 4.3.5 Common Request Rules

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-ONB-018` | A requester can cancel only their own onboarding request while it is `Pending`. The system must change the status to `Cancelled` and record the cancellation time. A request that is already `Approved`, `Rejected`, or `Cancelled` cannot be cancelled. | Must | Implemented | Authorization Test |
| `FR-ONB-019` | If two or more people try to approve, reject, or cancel the same onboarding request at nearly the same time, only the action saved successfully first is accepted. All later actions must be refused, and the request's final status must not be changed again. For a hotel-registration request, the system must not create more than one Hotel. | Must | Partial | Concurrency Test |

### 4.4 Hotel Catalog

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-CAT-001` | Hotel Owners can view Hotels owned by their authenticated account. | Must | Partial | Authorization Test |
| `FR-CAT-002` | Hotel Owners can update descriptive and contact information for a Hotel they own. | Must | Partial | Authorization Test |
| `FR-CAT-003` | The system must prevent a Hotel Owner from accessing or changing another Owner's Hotel. | Must | Partial | Authorization Test |
| `FR-CAT-004` | Hotel Owners can assign approved amenities, services, and policies to an owned Hotel. | Must | Partial | Test |
| `FR-CAT-005` | The system must validate that referenced catalog and master-data records are active. | Must | Partial | Test |
| `FR-CAT-006` | The system must publish a Hotel in public discovery only when its approval and active-state rules are satisfied. | Must | Partial | Integration Test |
| `FR-CAT-007` | A soft-deleted or suspended Hotel must not appear in public search or details. | Must | Partial | Integration Test |
| `FR-CAT-008` | Hotel Owners can view the publication and operational status of an owned Hotel. | Must | Partial | Demonstration |
| `FR-CAT-009` | The future platform must represent Hotel, Resort, Serviced Apartment, Villa, Homestay, and Hostel categories through configurable Property Types. | Should | Out of MVP | Test |
| `FR-CAT-010` | A future Accommodation Property must declare either a `UnitBased` or `EntirePlace` Inventory Model. | Should | Out of MVP | Test |
| `FR-CAT-011` | Future Property Types must reuse the core Booking, Payment, Refund, Review, and Notification workflows instead of duplicating them by Property Type. | Should | Out of MVP | Inspection |

### 4.5 Media Management

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-MED-001` | Hotel Owners can upload a cover image for an owned Hotel. | Must | Partial | Integration Test |
| `FR-MED-002` | Hotel Owners can upload multiple gallery images for an owned Hotel or Room Type. | Must | Partial | Integration Test |
| `FR-MED-003` | The system must validate file extension, MIME type, file size, and supported media category before upload. | Must | Partial | Security Test |
| `FR-MED-004` | The system must replace untrusted client filenames with server-generated asset identifiers. | Must | Partial | Inspection |
| `FR-MED-005` | The system must preserve an explicit display order for Hotel and Room Type galleries. | Should | Planned | Test |
| `FR-MED-006` | Hotel Owners can remove an image associated with an owned Hotel or Room Type. | Must | Partial | Authorization Test |
| `FR-MED-007` | Public Hotel details must return only active media belonging to the requested Hotel and its Room Types. | Must | Partial | API Test |

### 4.6 Room and Inventory

#### 4.6.1 Room Type Definition

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-INV-001` | Hotel Owners can create a Room Type for an owned Hotel. | Must | Implemented | API Test |
| `FR-INV-002` | A Room Type must define a name, description, price per night, guest capacity, and applicable room attributes. | Must | Partial | Test |
| `FR-INV-003` | Hotel Owners can configure bed types and quantities for an owned Room Type. | Must | Partial | Test |
| `FR-INV-004` | The system must provide an option to suggest Room Type names from selected room attributes. | Could | Implemented | Demonstration |

#### 4.6.2 Physical Room Management

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-INV-005` | An authenticated Hotel Owner must be able to create a physical room and assign it to a Room Type belonging to a Hotel they own. | Must | Implemented | API Test |
| `FR-INV-006` | A physical room number must be unique within a Hotel. | Must | Partial | Integration Test |
| `FR-INV-007` | Hotel Owners can view physical rooms grouped by Room Type. | Must | Implemented | API Test |

#### 4.6.3 Room Status and Maintenance

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-INV-008` | Hotel Owners can mark a physical room as active, unavailable, or under maintenance. | Must | Partial | Test |
| `FR-INV-009` | Hotel Owners can block a physical room for a date range with a reason. | Must | Planned | Test |
| `FR-INV-010` | The system must reject creating or updating an active maintenance block when its date range overlaps another active maintenance block for the same physical room. An update must exclude the block being updated from the overlap check. Date ranges must use half-open intervals `[start, end)`, allowing one block to start when another ends. Overlapping blocks must not be merged automatically. | Must | Planned | Integration Test |

#### 4.6.4 Availability Calculation

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-INV-011` | The system must calculate the number of rooms available for a requested stay by subtracting rooms already reserved or blocked for maintenance from the active physical rooms of the Room Type. | Must | Partial | Integration Test |
| `FR-INV-012` | The system must treat the check-in date as inclusive and the check-out date as exclusive when calculating room availability. A room released on a check-out date must be available for another Booking starting on that same date. | Must | Partial | Unit Test |

#### 4.6.5 Ownership and Authorization

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-INV-013` | The system must allow a Hotel Owner to create, update, or delete Room Types, physical rooms, and maintenance blocks only for Hotels they own. | Must | Partial | Authorization Test |

### 4.7 Search and Discovery

#### 4.7.1 Search Request and Validation

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-SRCH-001` | Guests can search for Hotels by province or city, check-in date, check-out date, adult count, child count, and requested room quantity. | Must | Partial | API Test |
| `FR-SRCH-002` | The system must reject a search when check-out is not later than check-in. | Must | Partial | Test |

#### 4.7.2 Hotel Matching

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-SRCH-003` | Search results must include only approved, active, and non-deleted Hotels. | Must | Partial | Integration Test |
| `FR-SRCH-004` | Search results must include only Hotels having at least one Room Type with enough available rooms for the entire requested stay and enough total adult and child capacity, calculated separately, for the requested room quantity. | Must | Partial | Integration Test |

#### 4.7.3 Filtering and Sorting

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-SRCH-005` | Guests can filter search results by supported property, amenity, price, rating, and room criteria. | Should | Partial | Test |
| `FR-SRCH-006` | Guests can sort search results by lowest available price or highest rating. | Should | Planned | Test |

#### 4.7.4 Search Results

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-SRCH-007` | Search results must provide the Hotel name, location, rating summary, cover image, and the lowest available price per night among matching Room Types. The displayed price is indicative and is not the final Booking price. | Must | Partial | API Test |
| `FR-SRCH-008` | Search results must be paginated. | Must | Partial | API Test |
| `FR-SRCH-009` | The system must return a successful empty result when no Hotel satisfies the search criteria. | Must | Partial | API Test |

#### 4.7.5 Public Hotel Details

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-SRCH-010` | Guests can view public Hotel details including description, location, amenities, policies, media, active Room Types, and review summary. | Must | Partial | API Test |
| `FR-SRCH-011` | When valid stay dates, adult count, child count, and room quantity are provided, the system must identify which active Room Types have sufficient capacity and availability for the requested stay. | Must | Partial | Integration Test |
| `FR-SRCH-012` | Public Hotel details must expose only information intended for Customers and must not expose Owner-only, Admin-only, security-sensitive, or internal operational data. | Must | Partial | Security Test |

### 4.8 Booking

#### 4.8.1 Booking Request

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-BOOK-001` | The system must allow only an authenticated Customer to create a Booking. | Must | Implemented | Authorization Test |
| `FR-BOOK-002` | A Booking request must identify one Room Type, room quantity, check-in date, check-out date, adult count, child count, and the primary guest's full name, email address, and phone number. | Must | Partial | API Test |

#### 4.8.2 Validation, Availability, and Pricing

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-BOOK-003` | The system must reject a Booking request when the check-in date is before the Hotel's current business date, the check-out date is not later than the check-in date, room quantity or adult count is less than one, child count is negative, or the primary guest's required contact information is missing or invalid. | Must | Partial | Test |
| `FR-BOOK-004` | The system must count one night for each night from the check-in date up to the night before the check-out date. | Must | Implemented | Unit Test |
| `FR-BOOK-005` | The system must calculate the final Booking total by multiplying the nightly price by the room quantity and number of nights, then adding the charges for all selected services. | Must | Partial | Unit Test |
| `FR-BOOK-006` | Before holding rooms for a Booking, the system must check that the selected Room Type has enough available rooms for every night of the requested stay. | Must | Implemented | Integration Test |
| `FR-BOOK-007` | If the selected Room Type has insufficient availability or adult or child capacity for the Booking request, the system must reject the Booking without creating or changing any Booking, Payment, or inventory reservation data. | Must | Partial | Integration Test |
| `FR-BOOK-008` | If several Customers try to book the same Room Type for overlapping dates at the same time, the first request that successfully holds the requested rooms must be accepted. After each successful hold, the system must reject any request that cannot be fulfilled with the rooms still available. | Must | Planned | Concurrent Integration Test |

#### 4.8.3 Booking Creation

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-BOOK-009` | When a Booking is successfully created, the system must assign it a unique customer-facing Booking reference that must never change or be reused, regardless of later Booking status changes. | Must | Partial | Integration Test |
| `FR-BOOK-010` | When a Booking is created, the system must store the booking-time values of the selected Room Type name, nightly price, room quantity, selected service names, prices and quantities, cancellation-policy terms, currency, subtotal, service total, and final total. Later changes to Room Types, prices, services, or policies must not alter these stored values. | Must | Planned | Integration Test |
| `FR-BOOK-011` | When a Customer chooses VNPay, the system must create the Booking with the `PendingPayment` status and hold the requested rooms for 15 minutes to allow the Customer to complete payment. | Must | Planned | Integration Test |
| `FR-BOOK-012` | When a Customer chooses pay-at-hotel, the system must create a `Confirmed` Booking, reserve the requested rooms, and create a Payment record with the `PayAtHotel` method and `Unpaid` status. | Must | Planned | Integration Test |
| `FR-BOOK-013` | Booking creation must save the Booking, room hold or reservation, Payment record, and notification request together. If any part fails, the system must save none of them. | Must | Planned | Integration Test |

#### 4.8.4 Booking Access and Management

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-BOOK-014` | Customers can view only their own Booking history and details. | Must | Implemented | Authorization Test |
| `FR-BOOK-015` | Customers can filter their own Booking history by `PendingPayment`, `Confirmed`, `CheckedIn`, `Completed`, `Cancelled`, or `Expired` status. | Must | Implemented | API Test |
| `FR-BOOK-016` | Hotel Owners can view Booking lists and details only for Hotels they own. | Must | Implemented | Authorization Test |
| `FR-BOOK-017` | Hotel Owners can search their Hotels' Bookings by Booking reference or primary guest name and filter them by owned Hotel, Booking status, and check-in date range. | Must | Partial | API Test |

#### 4.8.5 Room Assignment and Stay

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-BOOK-018` | An authorized Hotel Owner must be able to assign the booked quantity of physical rooms to a `Confirmed` Booking before completing check-in. Every assigned room must belong to the booked Hotel and selected Room Type. | Must | Partial | Test |
| `FR-BOOK-019` | The system must reject a physical room assignment if the room is not `Active`, has a maintenance block overlapping the booked stay, or is already assigned to another `Confirmed` or `CheckedIn` Booking with overlapping stay dates. | Must | Planned | Integration Test |
| `FR-BOOK-020` | An authorized Hotel Owner can mark a `Confirmed` Booking as `CheckedIn` only after all booked rooms have been assigned. Check-in must occur on or after the scheduled check-in date and before the scheduled check-out date. | Must | Planned | State Transition Test |
| `FR-BOOK-021` | When the Customer checks out, an authorized Hotel Owner can mark a `CheckedIn` Booking as `Completed` only if any `PayAtHotel` payment has been marked as paid. | Must | Planned | State Transition Test |

#### 4.8.6 Cancellation and Expiration

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-BOOK-022` | Customers can cancel their own `PendingPayment` or `Confirmed` Booking before the scheduled check-in date. The system must reject cancellation of a `CheckedIn`, `Completed`, `Cancelled`, or `Expired` Booking. | Must | Planned | API Test |
| `FR-BOOK-023` | When a paid Booking is cancelled, the system must use the cancellation terms saved when the Booking was created. If the Customer cancels early enough under those terms, the system must refund the stated percentage of the amount successfully paid. If the cancellation is too late or the Booking is non-refundable, the refund must be zero. | Must | Planned | Unit Test |
| `FR-BOOK-024` | If a VNPay Booking is not paid within the 15-minute hold period, its held rooms must immediately become available for other Bookings. Releasing the rooms must not wait for the Booking status to be changed to `Expired`. | Must | Planned | Integration Test |
| `FR-BOOK-025` | If a VNPay Booking remains unpaid after its 15-minute hold period ends, the system must automatically change its status from `PendingPayment` to `Expired` without requiring action from the Customer, Hotel Owner, or Admin. | Must | Planned | Integration Test |

#### 4.8.7 Future Multi-item Reservations

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-BOOK-026` | In a future version, Customers may create one Reservation containing multiple room or accommodation unit types under a single Reservation reference. | Should | Out of MVP | Integration Test |
| `FR-BOOK-027` | Every Reservation Item must identify one Accommodation Unit Type, quantity, price snapshot, policy snapshot, and calculated item total. | Should | Out of MVP | Integration Test |
| `FR-BOOK-028` | A future multi-item Reservation must belong to exactly one Accommodation Property. | Should | Out of MVP | Integration Test |
| `FR-BOOK-029` | In a future multi-item Reservation, Customers may cancel one or more Reservation Items without cancelling the remaining Items. Any refund must be calculated only from the cancelled Items, while the prices and status of the remaining Items stay unchanged. | Should | Out of MVP | Integration Test |

### 4.9 Payment and Refund

#### 4.9.1 Payment Options and Records

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-PAY-001` | An authorized Hotel Owner can enable or disable VNPay and `PayAtHotel` for a Hotel they own, but at least one payment method must remain enabled. | Must | Planned | Authorization Test |
| `FR-PAY-002` | Customers can choose VNPay or `PayAtHotel` when the selected payment method is enabled for the Hotel. | Must | Partial | Test |
| `FR-PAY-003` | Each Payment record must represent one payment attempt and belong to exactly one Booking. A Booking may have multiple VNPay Payment records when the Customer retries payment, but no more than one Payment can have the `Paid` status. | Must | Partial | Integration Test |
| `FR-PAY-004` | Every Payment must store its amount, currency, method, status, unique platform Payment reference, and creation time. It must also store the paid time, VNPay transaction reference, or failure reason when applicable. | Must | Partial | Inspection |

#### 4.9.2 VNPay Payment Flow

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-PAY-005` | Customers can request a VNPay payment URL only for their own unpaid `PendingPayment` Booking while its original 15-minute room hold is still active. Requesting another payment URL must not extend the original hold period. | Must | Partial | Authorization Test |
| `FR-PAY-006` | The system can accept a VNPay payment only when the Booking is unpaid, has the `PendingPayment` status, and its original 15-minute room hold is still active. Otherwise, the system must not confirm the Booking or reserve its rooms. | Must | Planned | State Transition Test |
| `FR-PAY-007` | When a valid VNPay callback reports a successful payment, the system must mark the Payment as `Paid`, change the Booking from `PendingPayment` to `Confirmed`, and keep the held rooms reserved for the stay. If any of these updates fails, the system must save none of them. | Must | Partial | Integration Test |
| `FR-PAY-008` | When a valid VNPay callback reports a failed payment, the system must mark that Payment attempt as `Failed` and store the failure reason. It must not confirm the Booking or extend the original room-hold period. The Customer may retry payment while the original hold remains active. | Must | Partial | Integration Test |
| `FR-PAY-009` | If VNPay sends the same callback more than once for the same transaction or request reference, the system must return the same result but process the Payment only once. Repeated callbacks must not repeat status changes, room reservations, Payment records, or notifications. | Must | Planned | Integration Test |
| `FR-PAY-010` | The system must reject a VNPay callback if its signature is invalid or its amount, currency, Booking reference, Payment reference, or VNPay transaction reference does not match the platform records. A rejected callback must not mark the Payment as `Paid`, confirm the Booking, or reserve rooms. | Must | Planned | Security Test |

#### 4.9.3 PayAtHotel Settlement

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-PAY-011` | An authorized Hotel Owner can mark an `Unpaid` `PayAtHotel` Payment as `Paid` only for a `Confirmed` or `CheckedIn` Booking belonging to a Hotel they own. The system must record the paid time and the Owner who confirmed the Payment. Hotel Owners must not manually mark VNPay Payments as `Paid`. | Must | Planned | Authorization and State Transition Test |

#### 4.9.4 Payment Visibility

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-PAY-012` | Customers can view the Payment history only for their own Bookings. Each Payment entry must show its platform reference, amount, currency, method, status, creation time, paid time when available, and VNPay transaction reference when available, without exposing provider secrets or internal security data. | Must | Planned | Authorization Test |
| `FR-PAY-013` | Hotel Owners can view Payment summaries only for Bookings belonging to Hotels they own. Each summary must show the platform Payment reference, amount, currency, method, status, paid time when available, and VNPay transaction reference when available, without exposing provider secrets or internal security data. | Must | Partial | Authorization Test |

#### 4.9.5 Refund Creation and Limits

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-PAY-014` | When a paid Booking is cancelled and its calculated refund amount is greater than zero, the system must create a `Pending` Refund for that amount and link it to the Booking's `Paid` Payment. If the calculated refund amount is zero, the system must not create a Refund. | Must | Planned | Integration Test |
| `FR-PAY-015` | Every Refund must store its unique platform Refund reference, related Payment reference, amount, currency, reason, status, and creation time. It must also store the completion time, processing Admin, VNPay refund reference, or failure reason when applicable. | Must | Planned | Inspection |
| `FR-PAY-016` | The system must never refund more than the amount successfully paid. Before creating or completing a Refund, it must include all `Pending` and `Completed` Refunds for the same Payment and reject the operation if their total would exceed the paid amount. `Failed` Refunds must not count toward this total. | Must | Planned | Integration Test |

#### 4.9.6 Refund Processing

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-PAY-017` | Admins can process a `Pending` Refund and mark it as `Completed` with a refund reference or `Failed` with a reason. Only authorized Admins can perform this action. | Must | Planned | Authorization and State Transition Test |
| `FR-PAY-018` | When a Refund is completed, the system must mark it as `Completed`, record its completion time, and update the related Payment to `PartiallyRefunded` or `Refunded` based on the total amount already refunded. The related Booking must remain `Cancelled`. If any update fails, the system must save none of these changes. | Must | Planned | Integration Test |

### 4.10 Reviews

#### 4.10.1 Review Submission

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-REV-001` | Only an authenticated Customer can submit a Review, and only for a Booking that belongs to them and has the `Completed` status. | Must | Partial | Authorization Test |
| `FR-REV-002` | Each completed Booking can have only one Customer Review. Hiding the Review does not allow the Customer to submit another one. | Must | Planned | Integration Test |
| `FR-REV-003` | A Review must include a whole-number rating from 1 to 10. It may also include an optional comment of up to 1,000 characters. | Must | Partial | Test |

#### 4.10.2 Public Review Display

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-REV-004` | Public Hotel details must show Reviews that have not been hidden by an Admin, their total count, and their average rating. If there are no such Reviews, the page must show a review count of 0 and no average rating. | Must | Partial | API Test |

#### 4.10.3 Hotel Owner Review Management

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-REV-005` | An authenticated Hotel Owner can use the Owner dashboard to view Reviews only for Hotels they own. This does not prevent them from viewing public Reviews for other Hotels on public Hotel pages. | Must | Planned | Authorization Test |
| `FR-REV-006` | An authenticated Hotel Owner can add one response of up to 1,000 characters to a Review for a Hotel they own and can edit that response later. The response is displayed together with the Review while the Review is public. | Should | Planned | Authorization Test |

#### 4.10.4 Review Moderation

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-REV-007` | Only an authorized Admin can hide or restore a Review. The Review must not be permanently deleted, and the system must record the Admin, action, reason, and time. | Must | Planned | Test |
| `FR-REV-008` | A hidden Review and its Hotel Owner response must not appear on public Hotel pages. Its rating must not be included in the public review count or average rating. When the Review is restored, it must appear publicly again and its rating must be included in both calculations. | Must | Planned | Integration Test |

### 4.11 Administration

#### 4.11.1 User Account Management

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-ADM-001` | Only an authenticated Admin can view the platform's user accounts. The list must be divided into pages and show each user's name, email, role, account status, and creation date. Admins can search by name or email and filter by role or account status. | Must | Implemented | API Test |
| `FR-ADM-002` | An authenticated Admin can view a user account's username, full name, email, assigned roles, current account status, and creation date. Phone numbers, dates of birth, passwords, authentication tokens, and other sensitive account data must not be displayed. | Must | Implemented | Authorization Test |
| `FR-ADM-003` | An authenticated Admin can deactivate or reactivate another user account. A deactivated account must not be able to sign in or access authenticated features, including through an existing login session. An Admin must not deactivate their own account or the only remaining active Admin account. | Must | Partial | Test |

#### 4.11.2 Shared Option Management

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-ADM-004` | An authenticated Admin can add, edit, and remove the shared options that Hotel Owners use to configure Hotels and Room Types. These options include amenities, services, policies, property types, and room attributes. | Must | Implemented | API Test |
| `FR-ADM-005` | Each shared-option list must be divided into pages. Admins can search items by name and, when the list contains categories, filter items by category. | Must | Implemented | API Test |
| `FR-ADM-006` | The system must not allow an Admin to remove a shared option while an active Hotel, Room Type, or other current configuration is using it. The system must tell the Admin that the option is in use and cannot be removed. | Must | Partial | Integration Test |
| `FR-ADM-007` | When an Admin removes a shared option that is no longer used by any current configuration but still appears in historical records, the system must keep it in those records and prevent it from being selected for new or updated Hotels and Room Types. | Must | Partial | Inspection |

#### 4.11.3 Onboarding Dashboard

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-ADM-008` | An authenticated Admin can view how many account-upgrade and hotel-registration requests are `Pending`, `Approved`, or `Rejected`. The dashboard must show both the combined totals and separate counts for each request type. | Must | Implemented | API Test |
| `FR-ADM-009` | The Admin dashboard must show the 10 most recent account-upgrade and hotel-registration requests in one list, ordered from newest to oldest. Each item must show the request type, requester, current status, submission time, and a way to open its details. | Must | Implemented | API Test |

#### 4.11.4 Administrative Accountability and History

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-ADM-010` | Whenever an Admin performs a management action, the system must record the currently signed-in Admin as the person who performed it. The Admin must not be able to provide another Admin's identity for that action. | Must | Partial | Audit Test |
| `FR-ADM-011` | The system must keep a history entry whenever an onboarding request is approved or rejected, a user's role or account status changes, a Booking, Payment, or Refund changes status, or an Admin hides or restores a Review. Each entry must record who or what performed the action, the affected record, the previous and new values, the reason when required, and the time of the action. These history entries must not be editable or removable through normal platform features. | Must | Planned | Integration Test |

### 4.12 Notifications

#### 4.12.1 Notification Events and Recipients

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-NOTIF-001` | The system must create an in-app notification when a Booking is created, confirmed, cancelled, expired, checked in, or completed; when a Payment succeeds or fails; when a Refund is created, completed, or fails; and when an account-upgrade or hotel-registration request is submitted, approved, or rejected. The notification must be sent to the users affected by the event. | Must | Planned | Integration Test |
| `FR-NOTIF-002` | Booking, Payment, and Refund notifications must be addressed to the related Customer and, when relevant, the Hotel Owner whose Hotel is affected. Onboarding decision notifications must be addressed to the user who submitted the request, while new onboarding submissions must notify authorized Admins. A user must not be able to view a notification addressed to another account. | Must | Planned | Authorization Test |

#### 4.12.2 In-App Notification Experience

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-NOTIF-003` | A user can mark their own in-app notifications as read, either individually or all at once. The system must show the number of unread notifications and must not allow a user to change the read status of another account's notification. | Should | Planned | Test |

#### 4.12.3 Saving and Sending Notifications Safely

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-NOTIF-004` | Whenever a Booking, Payment, Refund, or onboarding status changes, the system must save the related notification at the same time. It must never save the status change without the notification or the notification without the status change. The notification may be sent afterward. | Must | Planned | Integration Test |
| `FR-NOTIF-005` | After a status change and its notification have been saved, the system must complete the user's action without waiting for the notification to be sent. If sending fails, the saved status must remain unchanged and the system can try sending the notification again later. | Must | Planned | Integration Test |
| `FR-NOTIF-006` | The system must create only one in-app notification for each event and recipient. It may retry a failed email delivery, but once the email has been sent successfully, it must not send it again. | Must | Planned | Integration Test |

#### 4.12.4 Delivery Failure and Retry

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-NOTIF-007` | After each failed email delivery, the system must record how many times it has tried, when the latest attempt failed, why it failed, and when it will try again. After the fifth failure, no further attempt is scheduled. | Must | Planned | Integration Test |
| `FR-NOTIF-008` | After five unsuccessful delivery attempts in total, the system must stop retrying automatically and mark the notification delivery as `Failed`. It must keep the attempt history, latest failure reason, and latest attempt time. The related Booking, Payment, Refund, or onboarding action must remain unchanged. | Must | Planned | Test |

### 4.13 Reporting

#### 4.13.1 Admin Reports

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-RPT-001` | The Admin dashboard must show the number of user accounts, Hotels that are approved and currently active, and Bookings in every status. It must also show the total amount paid for Bookings after subtracting completed refunds. Deleted user accounts are not counted, deactivated accounts are still counted, and all money is shown in VND. | Must | Partial | API Test |
| `FR-RPT-002` | Admins can choose a start month and an end month and view one result for each month in that range. Each month must show how many Bookings were created, regardless of status, and the amount paid during that month after subtracting Refunds completed during that month. A month with no data must show 0. | Should | Partial | Analysis |

#### 4.13.2 Hotel Owner Reports

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-RPT-003` | An authenticated Hotel Owner can choose one or more Hotels they own and view a report using only those Hotels' data. They must not be able to view or include another Owner's Hotel in a report. | Must | Partial | Authorization Test |
| `FR-RPT-004` | For the selected Hotels and dates, the Owner report must show the number of Bookings scheduled to check in, scheduled to check out, and staying during the selected dates. It must also show the total number of Bookings, the amount paid after subtracting completed refunds, the percentage of rooms in use, and the number of rooms that are available, occupied, or blocked for maintenance. | Should | Partial | Analysis |

#### 4.13.3 How Report Totals Are Calculated

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-RPT-005` | The selected start and end dates are both included. Scheduled check-ins and check-outs include only Bookings with `Confirmed`, `CheckedIn`, or `Completed` status whose check-in or check-out date falls within the selected dates. A Booking counts as staying when at least one night of its stay falls within the selected dates. | Must | Partial | Integration Test |
| `FR-RPT-006` | The Booking total includes every Booking created within the selected dates, regardless of status. The money total includes Payments completed within the selected dates minus Refunds completed within the same dates. | Must | Partial | Integration Test |
| `FR-RPT-007` | One room used for one night counts as one room-night. The percentage of rooms in use is calculated by dividing booked room-nights by room-nights available for sale during the selected dates, then multiplying by 100. Rooms blocked for maintenance are not counted as available for sale. | Must | Partial | Integration Test |
| `FR-RPT-008` | Summary cards, charts, and Booking lists must use the same filters and calculation rules so that they show matching totals. | Must | Partial | Integration Test |

#### 4.13.4 Report Data Safety

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-RPT-009` | Reporting features may only read existing data and calculate results. Viewing or filtering a report must not create, edit, delete, or change the status of any Booking, Payment, Refund, Hotel, Room, or user account. | Must | Partial | Inspection |

### 4.14 Audit Events — Future

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `FR-AUD-001` | The future platform must create a versioned Audit Event for each configured material business action or state transition. | Should | Out of MVP | Integration Test |
| `FR-AUD-002` | The originating use case must persist its Audit Event to a SQL Outbox in the same transaction as the related business state. | Should | Out of MVP | Integration Test |
| `FR-AUD-003` | An Outbox Publisher must publish committed Audit Events to RabbitMQ without requiring the originating request to wait for broker delivery. | Should | Out of MVP | Integration Test |
| `FR-AUD-004` | A standalone Audit Event Service must consume RabbitMQ Audit Events and persist them to MongoDB. | Should | Out of MVP | Integration Test |
| `FR-AUD-005` | The Audit Event Service must process repeated messages idempotently using `EventId`. | Should | Out of MVP | Integration Test |
| `FR-AUD-006` | The audit pipeline must retain or dead-letter an event that cannot be processed within the configured retry policy. | Should | Out of MVP | Failure Test |
| `FR-AUD-007` | Audit Event queries must support entity history, actor activity, event type, time range, and correlation identifier. | Could | Out of MVP | API Test |
| `FR-AUD-008` | Audit Event payloads must exclude passwords, complete JWTs, provider secrets, payment signatures, and sensitive legal-document contents. | Should | Out of MVP | Security Test |

---

## 5. Business Rules

### 5.1 Identity and Ownership

| ID | Rule |
|---|---|
| `BR-IAM-001` | Public registration creates Customer accounts only. |
| `BR-IAM-002` | Only an approved account-upgrade request may grant the Hotel Owner role through the onboarding workflow. |
| `BR-IAM-003` | Client-provided user or Owner identifiers do not override the identity derived from trusted authentication claims. |
| `BR-IAM-004` | Admin role possession does not remove the requirement to record the acting Admin for audited decisions. |

### 5.2 Onboarding

| ID | Rule |
|---|---|
| `BR-ONB-001` | Admins can only decide on an onboarding request if its status is `Pending`. |
| `BR-ONB-002` | Rejection requires a non-empty reason visible to the requester. |
| `BR-ONB-003` | Approving a request and updating the role or Hotel state must happen in a single, atomic operation. |
| `BR-ONB-004` | Supporting legal documents are visible only to the requester and authorized Admins. |

### 5.3 Inventory and Availability

| ID | Rule |
|---|---|
| `BR-INV-001` | `AvailableQuantity = ActivePhysicalRooms - OverlappingReservedQuantity - OverlappingMaintenanceBlocks`. |
| `BR-INV-002` | Date ranges overlap when `ExistingCheckIn < RequestedCheckOut` and `ExistingCheckOut > RequestedCheckIn`. |
| `BR-INV-003` | Public search availability is advisory; Booking creation is authoritative. |
| `BR-INV-004` | Booking creation and maintenance-block commands acquire the same Room Type inventory lock before recalculating availability. |
| `BR-INV-005` | If a future command locks multiple Room Types, it acquires locks in ascending Room Type identifier order. |
| `BR-INV-006` | Expired holds do not reduce availability even before cleanup persists their final state. |

### 5.4 Booking and Pricing

| ID | Rule |
|---|---|
| `BR-BOOK-001` | One Booking contains one Room Type and one or more rooms of that Room Type. |
| `BR-BOOK-002` | Customers reserve Room Type and quantity; they do not select physical room numbers. |
| `BR-BOOK-003` | `RoomSubtotal = NightlyPrice × RoomQuantity × NumberOfNights`. |
| `BR-BOOK-004` | `ServiceTotal = Sum of selected service charges`; `FinalTotal = RoomSubtotal + ServiceTotal`. |
| `BR-BOOK-005` | Booking price and cancellation-policy snapshots do not change when source catalog data changes. |
| `BR-BOOK-006` | The default online-payment hold duration is 15 minutes. |
| `BR-BOOK-007` | Two requests competing for the final available capacity cannot both succeed. |
| `BR-BOOK-008` | A failed atomic Booking operation persists no partial Booking, Payment, inventory, or notification state. |
| `BR-BOOK-009` | An MVP Booking may reserve multiple physical units only when all units belong to the same Room Type. |
| `BR-BOOK-010` | In the MVP, reserving different Room Types requires separate Booking references. |
| `BR-BOOK-011` | A future multi-item Reservation belongs to exactly one Accommodation Property. |
| `BR-BOOK-012` | Each future Reservation Item owns its quantity, price snapshot, policy snapshot, and item-level financial allocation. |

### 5.5 Booking State

| ID | Rule |
|---|---|
| `BR-BOOK-013` | A VNPay Booking begins in `PendingPayment`. |
| `BR-BOOK-014` | A pay-at-hotel Booking begins in `Confirmed`. |
| `BR-BOOK-015` | A valid successful VNPay callback transitions an eligible `PendingPayment` Booking to `Confirmed`. |
| `BR-BOOK-016` | An expired unpaid hold transitions an eligible `PendingPayment` Booking to `Expired`. |
| `BR-BOOK-017` | Only an eligible `Confirmed` Booking may transition to `CheckedIn`. |
| `BR-BOOK-018` | Only an eligible `CheckedIn` Booking may transition to `Completed`. |
| `BR-BOOK-019` | Cancellation is permitted only when the current Booking state and stored cancellation-policy snapshot allow it. |

### 5.6 Payment and Refund

| ID | Rule |
|---|---|
| `BR-PAY-001` | A Payment belongs to exactly one Booking; a Booking may have multiple payment attempts. |
| `BR-PAY-002` | Payment success is accepted only from a valid provider callback or an authorized pay-at-hotel settlement operation. |
| `BR-PAY-003` | A duplicate successful callback returns success without repeating state changes or notifications. |
| `BR-PAY-004` | Payment and Booking changes caused by one callback are committed atomically. |
| `BR-PAY-005` | Refund amount is calculated from the successful paid amount and the Booking cancellation-policy snapshot. |
| `BR-PAY-006` | The cumulative successful refund amount cannot exceed the successful Payment amount. |

### 5.7 Reviews

| ID | Rule |
|---|---|
| `BR-REV-001` | Review eligibility is determined from an authenticated Customer's completed Booking. |
| `BR-REV-002` | A completed Booking has at most one active Customer Review. |
| `BR-REV-003` | A Review rating is an integer from 1 through 10. |
| `BR-REV-004` | Hidden Reviews and soft-deleted Reviews do not contribute to public rating summaries. |
| `BR-REV-005` | An Owner response does not alter the Customer's rating or comment. |

### 5.8 Audit Events — Future

| ID | Rule |
|---|---|
| `BR-AUD-001` | Operational logs and Audit Events are separate information products. |
| `BR-AUD-002` | Application business logic does not publish directly to RabbitMQ or write directly to the Audit Event MongoDB. |
| `BR-AUD-003` | Business state and its Outbox Event are committed in the same SQL transaction. |
| `BR-AUD-004` | Audit Event delivery uses at-least-once semantics; therefore every consumer is idempotent. |
| `BR-AUD-005` | Broker or Audit Event Service failure does not reverse a committed business operation. |

---

## 6. Data Requirements

### 6.1 Logical Data Ownership

| Module | Primary Data |
|---|---|
| Identity and Access | `User`, `Role`, `UserRole` |
| Partner Onboarding | `UpgradeRequest`, `HotelApprovalRequest` |
| Hotel Catalog | `Hotel`, location, amenity, service, and policy associations |
| Media Management | `HotelImage`, `RoomImage`, media metadata |
| Room and Inventory | `RoomType`, `Room`, `RoomBlock`, room attributes |
| Booking | `Booking`, `BookingRoom`, `BookingService` |
| Payment and Refund | `Payment`, `Refund` |
| Reviews | `Review`, `ReviewResponse` |
| Notifications | `Notification`, delivery attempts |
| Audit | MVP `AuditLog`; future SQL Outbox and MongoDB Audit Event documents |

### 6.2 Integrity and Storage Requirements

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `DR-001` | The database must enforce that Booking check-out is later than check-in. | Must | Planned | Integration Test |
| `DR-002` | The database must enforce that Booking room quantity is greater than zero. | Must | Planned | Integration Test |
| `DR-003` | Monetary values must use SQL `decimal(18,2)` storage or an equivalent lossless decimal representation. | Must | Partial | Inspection |
| `DR-004` | Booking reference must be stable and unique. | Must | Planned | Integration Test |
| `DR-005` | Provider transaction or request references used for idempotency must be unique. | Must | Planned | Integration Test |
| `DR-006` | A Booking must reference an existing Customer, Hotel, and Room Type. | Must | Implemented | Integration Test |
| `DR-007` | A Payment must reference an existing Booking. | Must | Implemented | Integration Test |
| `DR-008` | A Refund must reference an existing successful Payment. | Must | Planned | Integration Test |
| `DR-009` | One completed Booking must have at most one active Review. | Must | Planned | Integration Test |
| `DR-010` | Physical room number must be unique within a Hotel. | Must | Planned | Integration Test |
| `DR-011` | Technical event timestamps must be stored in UTC. | Must | Partial | Inspection |
| `DR-012` | Hotel check-in and check-out values must be stored and evaluated as Hotel business dates. | Must | Implemented | Test |
| `DR-013` | Core business fields must use explicit queryable columns rather than unstructured `Additional` metadata. | Must | Partial | Inspection |
| `DR-014` | Soft-deleted records must be excluded from active public and operational queries unless explicitly requested by an authorized recovery workflow. | Must | Partial | Integration Test |
| `DR-015` | Generated EF Core entity files must not contain custom business logic. | Must | Implemented | Inspection |
| `DR-016` | Schema changes must be represented by a versioned SQL script before database-first model regeneration. | Must | Partial | Inspection |
| `DR-017` | Audit records must contain actor, action, entity type, entity identifier, old state, new state, UTC timestamp, and correlation identifier. | Must | Planned | Integration Test |
| `DR-018` | Pending delivery records must contain delivery status, attempt count, next-attempt time, and terminal failure information. | Must | Planned | Integration Test |
| `DR-019` | A future Audit Event must contain `EventId`, `EventType`, `SchemaVersion`, `OccurredAtUtc`, `CorrelationId`, optional `CausationId`, actor identity, entity identity, optional old and new state, and a sanitized payload. | Should | Out of MVP | Schema Test |
| `DR-020` | Future MongoDB Audit Event storage must enforce uniqueness of `EventId`. | Should | Out of MVP | Integration Test |
| `DR-021` | Future MongoDB Audit Event storage must index entity history by entity type, entity identifier, and occurrence time. | Should | Out of MVP | Inspection |
| `DR-022` | Future MongoDB Audit Event storage must index actor activity, event type, and correlation identifier. | Should | Out of MVP | Inspection |
| `DR-023` | A future Outbox record must retain publication state, attempt count, next-attempt time, and last failure information until successful publication or terminal handling. | Should | Out of MVP | Integration Test |
| `DR-024` | Audit Event schemas must be versioned so consumers can distinguish incompatible payload revisions. | Should | Out of MVP | Contract Test |

---

## 7. Non-Functional Requirements

### 7.1 Performance and Capacity

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `NFR-PERF-001` | Hotel search API response time must be at most 2 seconds at the 95th percentile under the design-baseline workload. | Must | Planned | Performance Test |
| `NFR-PERF-002` | Hotel details API response time must be at most 1.5 seconds at the 95th percentile under the design-baseline workload. | Must | Planned | Performance Test |
| `NFR-PERF-003` | Booking creation response time must be at most 2 seconds at the 95th percentile, excluding external payment redirect time. | Must | Planned | Performance Test |
| `NFR-PERF-004` | Payment callback processing time must be at most 1 second at the 95th percentile. | Must | Planned | Performance Test |
| `NFR-PERF-005` | Paginated Admin and Owner list response time must be at most 2 seconds at the 95th percentile. | Must | Planned | Performance Test |
| `NFR-PERF-006` | The platform must support 200 concurrent active users under the agreed representative workload. | Must | Planned | Load Test |
| `NFR-PERF-007` | Performance validation must use a representative dataset up to 10,000 Hotels, 100,000 physical rooms, and 1,000,000 Bookings. | Should | Planned | Analysis |
| `NFR-PERF-008` | Filtering, sorting, aggregation, and pagination over persistent business data must execute in SQL Server rather than over an unbounded in-memory result. | Must | Partial | Inspection |

### 7.2 Security

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `NFR-SEC-001` | All production browser, API, and external-provider communication must use HTTPS or another encrypted transport approved for the provider. | Must | Partial | Inspection |
| `NFR-SEC-002` | Authentication, authorization, and resource ownership must be enforced by the API even when the Web App hides unauthorized actions. | Must | Partial | Security Test |
| `NFR-SEC-003` | The system must validate all external input at the server boundary and before business-state changes. | Must | Partial | Security Test |
| `NFR-SEC-004` | The system must protect database, JWT, Cloudinary, VNPay, and Email Provider secrets from source control and client responses. | Must | Partial | Inspection |
| `NFR-SEC-005` | Logs must not contain passwords, complete JWTs, provider secrets, payment signatures, or sensitive legal-document contents. | Must | Partial | Inspection |
| `NFR-SEC-006` | Uploaded content must be validated for allowed type and size before it is made available to users. | Must | Partial | Security Test |
| `NFR-SEC-007` | Predictable authorization failures must return `401` or `403` without revealing the existence of unauthorized private records. | Must | Partial | Security Test |
| `NFR-SEC-008` | The API must support independently configurable per-client rate limits for login, search, upload, and payment-callback endpoints. | Should | Planned | Security Test |

### 7.3 Reliability and Availability

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `NFR-REL-001` | The initial production target must provide 99.5% monthly service availability, excluding approved maintenance. | Should | Planned | Analysis |
| `NFR-REL-002` | The database recovery point objective must be at most 1 hour. | Should | Planned | Recovery Test |
| `NFR-REL-003` | The service recovery time objective must be at most 4 hours. | Should | Planned | Recovery Test |
| `NFR-REL-004` | SQL Server must have scheduled backups and a documented restore procedure. | Must | Planned | Recovery Test |
| `NFR-REL-005` | External-provider calls must use explicit timeouts. | Must | Planned | Integration Test |
| `NFR-REL-006` | The system must not retry a transient external failure unless the operation is safe or protected by idempotency. | Must | Planned | Integration Test |
| `NFR-REL-007` | Deadlock retry must occur only after rollback, with bounded attempts and jitter. | Must | Planned | Integration Test |
| `NFR-REL-008` | Multiple API-hosted workers must coordinate scheduled work through atomic database claims and idempotent handlers. | Must | Planned | Concurrent Integration Test |
| `NFR-REL-009` | The future Audit Event pipeline must provide at-least-once delivery from SQL Outbox through RabbitMQ to MongoDB. | Should | Out of MVP | Failure Test |
| `NFR-REL-010` | A future duplicate Audit Event delivery must create no more than one MongoDB Audit Event document for the same `EventId`. | Should | Out of MVP | Integration Test |
| `NFR-REL-011` | The future Audit Event pipeline must preserve failed messages in a retry queue or Dead Letter Queue instead of silently discarding them. | Should | Out of MVP | Failure Test |

### 7.4 Usability and Accessibility

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `NFR-USE-001` | Critical search, booking, payment, approval, and management workflows must provide clear current-state and next-action information. | Must | Partial | Usability Test |
| `NFR-USE-002` | User-facing errors must explain the recoverable action without exposing internal implementation details. | Must | Partial | Demonstration |
| `NFR-USE-003` | Critical forms and actions must be operable using a keyboard. | Should | Partial | Accessibility Test |
| `NFR-USE-004` | Form fields must have programmatically associated labels and validation messages. | Should | Partial | Accessibility Test |
| `NFR-USE-005` | Status must not be communicated by color alone. | Should | Partial | Accessibility Test |
| `NFR-USE-006` | The Web App must remain usable without horizontal page scrolling at viewport widths from 360 through 1440 CSS pixels, excluding intentionally scrollable data tables. | Must | Partial | Demonstration |

### 7.5 Maintainability and Architecture

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `NFR-MNT-001` | Dependency direction must remain `Web App -> API -> Application -> Infrastructure`. | Must | Implemented | Inspection |
| `NFR-MNT-002` | API Controllers must delegate business use cases to Application Services and must not query the database directly. | Must | Partial | Inspection |
| `NFR-MNT-003` | The Application layer must not reference Entity Framework Core or `DbContext`. | Must | Implemented | Inspection |
| `NFR-MNT-004` | Complex data queries and eager-loading definitions must reside in specific repository implementations. | Must | Partial | Inspection |
| `NFR-MNT-005` | Repositories must not call `SaveChangesAsync`. | Must | Implemented | Inspection |
| `NFR-MNT-006` | An atomic Application use case must call `IUnitOfWork.SaveChangesAsync` once at the end of the transaction. | Must | Partial | Inspection |
| `NFR-MNT-007` | Asynchronous database, file, and network operations must accept or propagate cancellation. | Must | Partial | Inspection |
| `NFR-MNT-008` | Predictable validation and business failures must use a structured result rather than exceptions for control flow. | Must | Partial | Inspection |
| `NFR-MNT-009` | Automated tests must be traceable to requirement and Acceptance Criterion identifiers and must use the test-level mapping defined in Section 8.2. | Must | Partial | Inspection |

### 7.6 Observability and Supportability

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `NFR-OBS-001` | Each request must have a correlation identifier propagated across Web App, API, Application, Infrastructure, and external-provider calls. | Must | Planned | Integration Test |
| `NFR-OBS-002` | Structured logs must include timestamp, level, correlation identifier, user identifier when available, module, operation, duration, outcome, and error code. | Must | Planned | Inspection |
| `NFR-OBS-003` | The platform must record metrics for search latency, Booking success and conflict, active and expired holds, Payment outcome and callback replay, external-provider latency, database duration, and Blazor circuit failure. | Should | Planned | Analysis |
| `NFR-OBS-004` | Unexpected system exceptions must be handled at the API boundary and logged with correlation context. | Must | Partial | Integration Test |
| `NFR-OBS-005` | Operational logs and metrics must distinguish validation, authorization, business conflict, provider failure, and system failure outcomes. | Should | Planned | Inspection |
| `NFR-OBS-006` | Operational logging must remain independent from the future RabbitMQ Audit Event pipeline. | Should | Out of MVP | Inspection |
| `NFR-OBS-007` | Future Audit Events must preserve correlation and causation identifiers across the Outbox Publisher, RabbitMQ, Audit Event Service, and MongoDB. | Should | Out of MVP | Integration Test |
| `NFR-OBS-008` | The future Audit Event Service must expose health information for RabbitMQ consumption, retry backlog, Dead Letter Queue backlog, and MongoDB persistence. | Could | Out of MVP | Demonstration |

### 7.7 Compatibility and Time

| ID | Requirement | Priority | Status | Verification |
|---|---|---:|---|---|
| `NFR-COMP-001` | The Web App must support the latest two stable major versions of Chrome and Edge used during acceptance testing. | Must | Partial | Compatibility Test |
| `NFR-COMP-002` | The primary UI language must be English for the MVP. | Must | Partial | Demonstration |
| `NFR-COMP-003` | The transaction currency must be VND for the MVP. | Must | Partial | Test |
| `NFR-TIME-001` | Technical timestamps must be generated and stored in UTC. | Must | Partial | Test |
| `NFR-TIME-002` | The UI must display timestamps in the configured display timezone. | Should | Partial | Test |
| `NFR-TIME-003` | Application business logic must use an injectable clock abstraction instead of direct system-clock access. | Must | Planned | Inspection |

---

## 8. Verification and Acceptance

### 8.1 Verification Methods

| Method | Usage |
|---|---|
| Test | Automated unit, component, integration, API, contract, security, performance, or end-to-end execution |
| Inspection | Review of source code, configuration, schema, or documentation |
| Analysis | Calculation or evaluation using collected data |
| Demonstration | Observed product behavior in a controlled environment |

### 8.2 Requirement-to-Test Guidance

| Requirement Category | Primary Verification |
|---|---|
| Validation and calculation | Unit Test |
| Application business rule | Unit or Application Service Test |
| Repository query and transaction | Integration Test |
| API contract and authorization | API Integration Test |
| Blazor form and interaction | Component or UI Test |
| VNPay, Cloudinary, and Email Provider | Contract or Sandbox Integration Test |
| Critical Customer or Owner journey | End-to-End Test |
| Performance and capacity | Load or Performance Test |
| Source and dependency constraint | Inspection |

### 8.3 SRS Acceptance Conditions

This SRS may move from `Review Draft` to `Approved Baseline` when:

1. Product scope and out-of-scope decisions are approved.
2. Every active requirement is necessary, singular, unambiguous, feasible, and verifiable.
3. Requirement conflicts with the HLD are resolved.
4. Priorities and target implementation statuses are accepted.
5. Business rules and state terminology are consistent.
6. No unresolved placeholder or undefined requirement identifier remains.

Approval of this SRS does not mean the software implementation is accepted.

---

## 9. Traceability Model

### 9.1 Forward Traceability

```text
Product Objective
  -> SRS Requirement
      -> User Story
          -> Acceptance Criterion
              -> Business or Technical Flow
                  -> Test Case
                      -> Test Evidence
```

### 9.2 Planned Identifier Mapping

```text
FR-BOOK-006
  -> BOOK-US-<number>
      -> BOOK-US-<number>-AC-<number>
          -> BOOK-BF-<number> / BOOK-TF-<number>
              -> BOOK-US-<number>-AC-<number>-TC-<number>
```

### 9.3 Traceability Rules

1. Every User Story must trace to at least one active SRS requirement.
2. Every active functional SRS requirement must trace to one or more User Stories.
3. Every Acceptance Criterion must trace to at least one test case.
4. Every material business-rule branch must appear in an Acceptance Criterion.
5. Critical transaction, concurrency, authorization, and external-integration requirements must have a Technical Flow.
6. Pure architecture constraints may trace directly to inspection tests rather than User Stories.

---

## 10. Requirement Quality Checklist

Each requirement must be reviewed for:

- Necessary
- Appropriate
- Unambiguous
- Complete
- Singular
- Feasible
- Verifiable
- Correct
- Conforming
- Traceable

Requirements must avoid vague terms such as “fast,” “user-friendly,” “secure,” “etc.,” or “as needed” unless the document supplies an objective interpretation.

---

## Appendix A — Glossary

| Term | Definition |
|---|---|
| Accommodation Property | Generic future listing abstraction for Hotel, Resort, Serviced Apartment, Villa, Homestay, Hostel, and similar accommodation |
| Accommodation Unit Type | Future generalized sellable category corresponding to the current Room Type concept |
| Audit Event | Versioned record of a material business action or state transition |
| Audit Event Service | Future standalone service that consumes Audit Events from RabbitMQ and persists them to MongoDB |
| Booking | A reservation for one Room Type and a positive room quantity over a stay date range |
| Booking Reference | Stable public identifier for a Booking |
| Customer | Authenticated traveler account |
| Dead Letter Queue | Queue holding messages that could not be processed within the configured retry policy |
| Guest | Unauthenticated visitor |
| Hotel Owner | Verified user authorized to manage owned Hotels |
| Inventory Model | Strategy describing whether accommodation is reserved by unit quantity or as an entire place |
| Inventory Hold | Time-limited reservation of capacity while online payment is pending |
| Master Data | Admin-managed reusable configuration such as amenities, policies, services, and room attributes |
| Operational Log | Technical execution record used for diagnostics, performance, and support |
| Outbox Event | Event stored atomically with business data and published asynchronously after commit |
| Payment Attempt | One Payment record created for a Booking settlement attempt |
| Physical Room | Individually identified room assigned to a Room Type |
| Reservation Item | Future line item identifying one Accommodation Unit Type, quantity, snapshots, and allocated financial amounts |
| Room Block | Date-bound unavailability of a physical room, commonly for maintenance |
| Room Type | Sellable accommodation category with price and capacity characteristics |
| Snapshot | Immutable Booking copy of business data used for pricing and cancellation decisions |
| VNPay Callback | Provider-to-platform payment result request authenticated by provider signature |

## Appendix B — Implementation Status Interpretation

| Status | Interpretation |
|---|---|
| `Implemented` | A recognizable end-to-end or core implementation exists in the repository; acceptance evidence is still required |
| `Partial` | Some supporting code or behavior exists, but one or more normative clauses are incomplete |
| `Planned` | No sufficient implementation evidence was found or the target design materially differs from current code |
| `Out of MVP` | Explicitly excluded from the active MVP baseline |

## Appendix C — Review Focus

The first review should prioritize:

1. Scope and actor correctness.
2. Booking, availability, and pricing rules.
3. Booking, Payment, and Refund state behavior.
4. Owner and Admin authorization boundaries.
5. Requirement priorities.
6. Whether `Implemented`, `Partial`, and `Planned` assessments reflect the current repository.
7. Future Accommodation Property and multi-item Reservation boundaries.
8. Future RabbitMQ, Audit Event Service, and MongoDB responsibilities.
