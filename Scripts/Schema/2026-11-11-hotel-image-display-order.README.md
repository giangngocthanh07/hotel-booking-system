# Hotel gallery display order — FR-MED-005

Run `2026-11-11-hotel-image-display-order.sql` against the intended application database. This is the schema checkpoint for Owner image management; it does not expose new APIs or upload/change/remove any Cloudinary asset.

## Apply and scaffold

1. Review the SQL file and select the application database in your SQL editor. System databases are rejected.
2. Execute the entire SQL file as one batch. It adds `HotelImages.DisplayOrder` (`int`, required, default 0), a nonnegative CHECK and a non-unique ordered read index.
3. On first addition only, existing rows receive zero-based positions per Hotel in Id order. Soft-deleted rows are retained. Re-running the script preserves later custom positions.
4. Scaffold using the existing project command and configuration-based connection string. Generate/review the complete context in a temporary output folder; do not overwrite the context with a subset-table scaffold. Preserve `HotelBookingDBContext.Partial.cs`.
5. Verify the generated `HotelImage.DisplayOrder` property and context index/default mappings. CHECK constraints may not be represented by scaffolding; the SQL script remains authoritative. Keep unrelated scaffold differences out of this commit.

The backend does not yet read/write DisplayOrder. After scaffolding, the next checkpoint adds owned image APIs and ordered public reads, then React image management in a separate commit. No manual edit to generated models is needed for this checkpoint.

Schema tests create isolated legacy catalogs inside a disposable SQL Server container; they do not connect to the application's configured database:

```powershell
dotnet test HotelBooking.test/HotelBooking.test.csproj --no-restore --filter FullyQualifiedName~HotelImageOrderingSchemaSqlTests
```

If localhost holds the default output DLLs, use an isolated output directory with `-p:OutDir="$env:TEMP\hotelbooking-image-schema-check\"`.
