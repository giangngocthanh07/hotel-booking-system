using HotelBooking.Tests.Integration.RequestManagement;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration.Media.HotelImages;

[Trait("Requirement", "FR-MED-005")]
[Trait("Category", "SqlIntegration")]
public class HotelImageOrderingSchemaSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Fact]
    public async Task Upgrade_BackfillsEachHotelInIdOrderWithoutChangingExistingRows()
    {
        await using var connection = await CreateLegacyDatabaseAsync();
        await ExecuteAsync(connection, "INSERT dbo.HotelImages(HotelId, ImageUrl, IsDeleted) VALUES (10,N'a',0),(20,N'b',0),(10,N'c',1),(20,N'd',NULL),(10,N'e',0);");
        await ApplyAsync(connection);
        await using var command = new SqlCommand("SELECT DisplayOrder FROM dbo.HotelImages ORDER BY Id;", connection);
        await using var reader = await command.ExecuteReaderAsync();
        var positions = new List<int>();
        while (await reader.ReadAsync()) positions.Add(reader.GetInt32(0));
        Assert.Equal(new[] { 0, 0, 1, 1, 2 }, positions);
        await reader.DisposeAsync();
        Assert.Equal(5, await ScalarAsync(connection, "SELECT COUNT(*) FROM dbo.HotelImages;"));
        Assert.Equal(1, await ScalarAsync(connection, "SELECT COUNT(*) FROM dbo.HotelImages WHERE ImageUrl=N'c' AND IsDeleted=1;"));
        Assert.Equal(1, await ScalarAsync(connection, "SELECT COUNT(*) FROM dbo.HotelImages WHERE ImageUrl=N'd' AND IsDeleted IS NULL;"));
    }

    [Fact]
    public async Task RepeatingUpgrade_PreservesCustomOrderAndSuppliesDefaultForNewRows()
    {
        await using var connection = await CreateLegacyDatabaseAsync();
        await ExecuteAsync(connection, "INSERT dbo.HotelImages(HotelId,ImageUrl) VALUES(10,N'a');");
        await ApplyAsync(connection);
        await ExecuteAsync(connection, "UPDATE dbo.HotelImages SET DisplayOrder=7 WHERE Id=1;");
        await ApplyAsync(connection);
        Assert.Equal(7, await ScalarAsync(connection, "SELECT DisplayOrder FROM dbo.HotelImages WHERE Id=1;"));
        await ExecuteAsync(connection, "INSERT dbo.HotelImages(HotelId,ImageUrl) VALUES(10,N'b');");
        Assert.Equal(0, await ScalarAsync(connection, "SELECT DisplayOrder FROM dbo.HotelImages WHERE Id=2;"));
        Assert.Equal(0, await ScalarAsync(connection, "SELECT CONVERT(INT,is_nullable) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.HotelImages') AND name=N'DisplayOrder';"));
        Assert.Equal(1, await ScalarAsync(connection, "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.HotelImages') AND name=N'IX_HotelImages_DisplayOrder' AND is_unique=0;"));
    }

    [Fact]
    public async Task NegativePosition_IsRejectedByTrustedConstraint()
    {
        await using var connection = await CreateLegacyDatabaseAsync();
        await ApplyAsync(connection);
        var error = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(connection,
            "INSERT dbo.HotelImages(HotelId,ImageUrl,DisplayOrder) VALUES(10,N'bad',-1);"));
        Assert.Equal(547, error.Number);
        Assert.Equal(1, await ScalarAsync(connection, "SELECT COUNT(*) FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.HotelImages') AND name=N'CK_HotelImages_DisplayOrder' AND is_not_trusted=0 AND is_disabled=0;"));
    }

    [Fact]
    public async Task IncompatibleExistingColumn_IsRejectedWithoutChangingRows()
    {
        await using var connection = await CreateLegacyDatabaseAsync();
        await ExecuteAsync(connection, "ALTER TABLE dbo.HotelImages ADD DisplayOrder NVARCHAR(10) NULL;");
        await ExecuteAsync(connection, "INSERT dbo.HotelImages(HotelId,ImageUrl,DisplayOrder) VALUES(10,N'a',N'kept');");
        var error = await Assert.ThrowsAsync<SqlException>(() => ApplyAsync(connection));
        Assert.Equal(51102, error.Number);
        Assert.Equal(1, await ScalarAsync(connection, "SELECT COUNT(*) FROM dbo.HotelImages WHERE DisplayOrder=N'kept';"));
    }

    [Fact]
    public async Task MissingTable_IsRejected()
    {
        await using var connection = await CreateLegacyDatabaseAsync();
        await ExecuteAsync(connection, "DROP TABLE dbo.HotelImages;");
        Assert.Equal(51101, (await Assert.ThrowsAsync<SqlException>(() => ApplyAsync(connection))).Number);
    }

    [Fact]
    public async Task FailureAfterBackfill_RollsBackColumnAndPreservesOriginalRows()
    {
        await using var connection = await CreateLegacyDatabaseAsync();
        await ExecuteAsync(connection, "INSERT dbo.HotelImages(HotelId,ImageUrl,IsDeleted) VALUES(10,N'kept',NULL);");
        await ExecuteAsync(connection, "CREATE TRIGGER FailImageOrderingIndex ON DATABASE FOR CREATE_INDEX AS BEGIN THROW 51110, 'Simulated index creation failure.', 1; END;");
        var error = await Assert.ThrowsAsync<SqlException>(() => ApplyAsync(connection));
        Assert.Contains(error.Errors.Cast<SqlError>(), value => value.Number == 51110);
        Assert.Equal(0, await ScalarAsync(connection, "SELECT ISNULL(COL_LENGTH(N'dbo.HotelImages',N'DisplayOrder'),0);"));
        Assert.Equal(1, await ScalarAsync(connection, "SELECT COUNT(*) FROM dbo.HotelImages WHERE ImageUrl=N'kept' AND IsDeleted IS NULL;"));
        Assert.Equal(0, await ScalarAsync(connection, "SELECT @@TRANCOUNT;"));
    }

    [Fact]
    public async Task SystemDatabase_IsRejectedBeforeSchemaChanges()
    {
        await using var connection = await CreateLegacyDatabaseAsync();
        await connection.ChangeDatabaseAsync("master");
        Assert.Equal(51100, (await Assert.ThrowsAsync<SqlException>(() => ApplyAsync(connection))).Number);
    }

    private async Task<SqlConnection> CreateLegacyDatabaseAsync()
    {
        // Each test gets an isolated legacy schema, independent of future EF scaffolding.
        await using var context = fixture.CreateContext();
        var builder = new SqlConnectionStringBuilder(context.Database.GetConnectionString()) { InitialCatalog = "master" };
        await using var master = new SqlConnection(builder.ConnectionString);
        await master.OpenAsync();
        var database = $"HotelImageOrder_{Guid.NewGuid():N}";
        await ExecuteAsync(master, $"CREATE DATABASE [{database}];");
        builder.InitialCatalog = database;
        var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await ExecuteAsync(connection, "CREATE TABLE dbo.HotelImages(Id INT IDENTITY PRIMARY KEY, HotelId INT NOT NULL, ImageUrl NVARCHAR(500) NOT NULL, IsDeleted BIT NULL);");
        return connection;
    }

    private static async Task ApplyAsync(SqlConnection connection) =>
        await ExecuteAsync(connection, await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Sql", "2026-11-11-hotel-image-display-order.sql")));

    private static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> ScalarAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}
