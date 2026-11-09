# Hotel request concurrency — FR-ONB-019

Requires Docker Desktop with the Linux engine running. Run from the repository root:

```powershell
dotnet test HotelBooking.test/HotelBooking.test.csproj --artifacts-path artifacts/owner-inventory --no-restore --filter FullyQualifiedName~HotelRequestConcurrencySqlTests --logger 'console;verbosity=minimal'
```

The fixture creates and disposes its own SQL Server 2022 container. It creates a mapped pre-registry baseline, then applies the checked-in business-license registry upgrade so SQL defaults/check constraints survive scaffolding. It does not run migrations or touch the application's configured database. This verifies SQL Server persistence behavior, not parity with every database creation script.

Two independent contexts run the real application services, repositories and UnitOfWork. Each pauses immediately before saving, after both have read Pending. The test releases one action, waits for its successful completion/commit, then releases the stale competing action. No delay-based scheduling or persistence mocks are used.

Nine ordered combinations cover approve/approve, approve/reject, approve/cancel, reject/cancel, reject/reject and cancel/cancel, including both winner orders for mixed actions. Assertions require Success for the winner, Conflict for the loser, the winner's stored status and decision metadata, and exactly one Hotel for an approval winner (zero otherwise). A fresh context verifies persisted state after both actions finish.

These deterministic overlapping-request tests do not constitute load testing or verification of deployment-specific database settings.
