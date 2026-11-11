# Schema upgrades

- Hotel gallery ordering: [2026-11-11-hotel-image-display-order.README.md](2026-11-11-hotel-image-display-order.README.md).
- Business-license upload registry: instructions below.

## Business license upload registry — step 1

Run `2026-11-09-business-license-uploads.sql` against the intended application database after review. This is a database-first schema upgrade, not an EF migration. The script rejects system databases and requires existing `dbo.Users` and `dbo.HotelApprovalRequests` tables.

It adds:

- `BusinessLicenseUploads`: opaque GUID ID, authenticated Owner ID, provider asset/resource/delivery metadata, stored URL, original filename, MIME, byte count, UTC creation time and soft-delete flag.
- Nullable `HotelApprovalRequests.BusinessLicenseUploadId`, preserving legacy requests and their existing URLs.
- A composite FK matching upload ID **and Owner ID**, plus lookup indexes. A foreign Owner's upload cannot be linked even by a direct SQL update. An Owner may reuse an upload for corrected submissions.

The script runs in a transaction and can be repeated for the schema it creates. It creates no upload records, assigns no ownership to legacy URLs, deletes no data and adds no expiry/one-use rules. `IsDeleted` eligibility still needs service enforcement in a later checkpoint.

## Apply and scaffold

1. Back up the application database through the normal development workflow.
2. Select the application database in your SQL query tool and execute the complete SQL file as one batch.
3. Scaffold the updated database into a temporary output directory, using the existing project settings and configuration-based connection string. Do not place credentials in commands or tracked files.
4. Review the generated changes for `BusinessLicenseUpload`, `HotelApprovalRequest`, `User`, the new DbSet, keys/relationships and context mapping. Preserve the existing `HotelBookingDBContext.Partial.cs` custom configuration.
5. Do not scaffold only a subset of tables over the current context with `--force`; that can remove mappings for unrelated tables. Keep unrelated scaffold differences out of this commit.

The upload-persistence checkpoint now stores registry rows through a specific repository and one UnitOfWork save. The API returns `UploadId` alongside `Uploaded`, `FileName` and `StoredFileName` only after SQL persistence succeeds. Provider identifiers/resource/delivery metadata remain internal, and the stored URL is the canonical `SecureUrl` returned by Cloudinary. Filename validation includes the database's 255-character limit.

New registrations now require `BusinessLicenseUploadId`: the service resolves a non-deleted upload owned by the authenticated Owner and derives the URL from that record. Unknown, foreign and deleted receipts receive the same validation response; client URLs cannot override stored URLs. React sends the returned receipt ID. Blazor was explicitly excluded from this frontend checkpoint, so its legacy URL-only registration flow requires a later update. Existing request history remains readable.

SQL and Cloudinary are not a distributed transaction; a failed SQL save can leave an orphaned provider asset, but no usable receipt is returned. Private document delivery is also pending.

## Isolated verification

```powershell
dotnet test HotelBooking.test/HotelBooking.test.csproj --artifacts-path artifacts/owner-inventory --no-restore --filter FullyQualifiedName~BusinessLicenseSchemaSqlTests --logger 'console;verbosity=minimal'
```

Requires the Docker Linux engine. Tests build a mapped pre-registry baseline in a disposable SQL Server container, then apply this exact script. This preserves SQL defaults/check constraints that are not fully represented by scaffolded EF mappings. The application database is not modified. Coverage includes repeat execution, legacy compatibility, defaults, size/MIME constraints, ownership FKs and reuse. The original full database creation script predates the current HotelApprovalRequests schema; this upgrade assumes the current application schema already exists.
