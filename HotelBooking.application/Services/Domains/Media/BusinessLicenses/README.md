# Business-license module

Keep business-license use cases and provider adapters in this folder. Photo upload remains in the parent Media module.

| Responsibility | Repository location |
|---|---|
| Access context, document response and provider DTOs | `HotelBooking.application/DTOs/Media/BusinessLicenses/` |
| Shared upload DTOs used by documents and photos | `HotelBooking.application/DTOs/Media/` |
| Services | `HotelBooking.application/Services/Domains/Media/BusinessLicenses/` |
| File rules | `HotelBooking.application/Services/Helpers/Media/BusinessLicenses/` |
| Validators | `HotelBooking.application/Validators/Media/BusinessLicenses/` |
| Upload/document controllers | `HotelBooking.api/Controllers/V1/Media/BusinessLicenses/` |
| Service unit tests | `HotelBooking.test/UnitTests/Services/Media/BusinessLicenses/` |
| DTO serialization tests | `HotelBooking.test/UnitTests/DTOs/Media/BusinessLicenses/` |
| Dependency registration tests | `HotelBooking.test/UnitTests/DependencyInjection/Media/BusinessLicenses/` |
| Controller unit tests | `HotelBooking.test/UnitTests/Controllers/Media/BusinessLicenses/` |
| SQL integration tests | `HotelBooking.test/IntegrationTests/Media/BusinessLicenses/` |

SQL tests reuse `HotelRequestSqlFixture` from the RequestManagement integration module because their registry links refer to hotel-registration requests. Repository implementations and DB-first generated models stay in Infrastructure.

Folder/namespace organization does not change HTTP routes, authorization or file delivery. Upload remains `POST /api/v1/files/business-licenses`; viewing remains `GET /api/v1/hotel-registrations/{requestId}/business-license`.
