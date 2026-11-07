# Public Hotel search

`sp_SearchHotels.sql` is a database-first `CREATE OR ALTER` deployment script. It does not add columns, change tables, run EF migrations or alter application data.

The previous deployed definition referenced the removed `Cities` table and `Hotels.CityId`. This version uses `Hotels.ProvinceId` and `Hotels.CountryId`, keeps the `CityName` result alias for client compatibility, and excludes soft-deleted Hotels using the same nullable convention as public details. Existing approval/Active checks, result columns, pricing, capacity, review aggregation, booking-overlap rules, room-count filter and ordering are retained.

## Apply

1. Connect to the intended SQL Server instance and select the application database (`HotelBooking` in the current local configuration).
2. Save the current `dbo.sp_SearchHotels` definition before replacement if a rollback copy is needed.
3. Execute `sp_SearchHotels.sql` against that database. It is one batch and may be run again.
4. Check public search with an eligible Hotel and ensure a soft-deleted or suspended Hotel is absent.

The script has been tested on an isolated SQL Server 2022 database using the current EF mapping. It has not been applied automatically to the application database. It requires the current Province/Ward schema, not the older Cities schema in the original full creation script.

## Regression verification

```powershell
dotnet test HotelBooking.test/HotelBooking.test.csproj --artifacts-path artifacts/owner-inventory --no-restore --filter FullyQualifiedName~HotelPublicSearchSqlTests --logger 'console;verbosity=minimal'
```

Requires the Docker Linux engine. The tests install this exact checked-in script in a disposable container and call the real repository. They cover public eligibility, Province search mapping, price/rating projections, availability at overlapping and adjacent booking dates, capacity and requested room count. They do not certify database-script/model parity, load behavior or every existing search rule.
