using HotelBooking.Tests.Integration.RequestManagement;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration.Media.BusinessLicenses;

[Trait("Requirement", "FR-ONB-009")]
[Trait("Requirement", "FR-ONB-011")]
[Trait("Requirement", "FR-ONB-013")]
[Trait("Category", "SqlIntegration")]
public class BusinessLicenseSchemaSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Fact]
    public async Task Script_CanRunTwiceAndPreservesLegacyRequests()
    {
        var seed = await fixture.SeedAsync();
        await ApplyAsync();
        await ApplyAsync();
        await using var context = fixture.CreateContext();
        var request = await context.HotelApprovalRequests.SingleAsync(value => value.Id == seed.RequestId);
        Assert.Equal("https://example.com/license.pdf", request.BusinessLicenseUrl);
        Assert.Equal(1, await context.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM dbo.HotelApprovalRequests WHERE Id = {seed.RequestId} AND BusinessLicenseUploadId IS NULL").SingleAsync());
        Assert.Equal(1, await context.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.foreign_keys WHERE name = N'FK_HotelApprovalRequests_BusinessLicenseUploads_Owner' AND is_not_trusted = 0").SingleAsync());
    }

    [Fact]
    public async Task Registry_AssignsOpaqueIdUtcTimestampAndNotDeletedDefault()
    {
        var seed = await fixture.SeedAsync();
        await ApplyAsync();
        await using var context = fixture.CreateContext();
        // Compare timestamps from the same SQL clock; host/container clocks can differ slightly.
        var before = await context.Database.SqlQuery<DateTime>($"SELECT SYSUTCDATETIME() AS [Value]").SingleAsync();
        await context.Database.ExecuteSqlInterpolatedAsync($"INSERT dbo.BusinessLicenseUploads (OwnerId, PublicId, ResourceType, DeliveryType, StoredUrl, OriginalFileName, ContentType, SizeBytes) VALUES ({seed.OwnerId}, N'test/default', 'image', 'upload', N'https://example.com/file.pdf', N'file.pdf', 'application/pdf', 10)");
        var created = await context.Database.SqlQuery<DateTime>($"SELECT CreatedAt AS [Value] FROM dbo.BusinessLicenseUploads WHERE PublicId = N'test/default'").SingleAsync();
        var id = await context.Database.SqlQuery<Guid>($"SELECT Id AS [Value] FROM dbo.BusinessLicenseUploads WHERE PublicId = N'test/default'").SingleAsync();
        Assert.NotEqual(Guid.Empty, id);
        var after = await context.Database.SqlQuery<DateTime>($"SELECT SYSUTCDATETIME() AS [Value]").SingleAsync();
        Assert.InRange(created, before, after);
        Assert.Equal(1, await context.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM dbo.BusinessLicenseUploads WHERE Id = {id} AND IsDeleted = 0").SingleAsync());
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(10485760, true)]
    [InlineData(10485761, false)]
    public async Task Registry_EnforcesSizeBounds(long bytes, bool valid)
    {
        var seed = await fixture.SeedAsync();
        await ApplyAsync();
        if (valid) await InsertAsync(seed.OwnerId, Guid.NewGuid(), bytes);
        else Assert.Equal(547, (await Assert.ThrowsAsync<SqlException>(() => InsertAsync(seed.OwnerId, Guid.NewGuid(), bytes))).Number);
    }

    [Theory]
    [InlineData("application/pdf", true)]
    [InlineData("image/jpeg", true)]
    [InlineData("image/png", true)]
    [InlineData("text/plain", false)]
    [InlineData("application/octet-stream", false)]
    public async Task Registry_EnforcesSupportedMime(string mime, bool valid)
    {
        var seed = await fixture.SeedAsync();
        await ApplyAsync();
        if (valid) await InsertAsync(seed.OwnerId, Guid.NewGuid(), 10, mime);
        else Assert.Equal(547, (await Assert.ThrowsAsync<SqlException>(() => InsertAsync(seed.OwnerId, Guid.NewGuid(), 10, mime))).Number);
    }

    [Fact]
    public async Task Registry_RejectsUnknownOwner()
    {
        await ApplyAsync();
        Assert.Equal(547, (await Assert.ThrowsAsync<SqlException>(() => InsertAsync(int.MaxValue, Guid.NewGuid()))).Number);
    }

    [Fact]
    public async Task Request_RejectsForeignOwnerUpload()
    {
        var owner = await fixture.SeedAsync();
        var other = await fixture.SeedAsync();
        await ApplyAsync();
        var id = Guid.NewGuid();
        await InsertAsync(owner.OwnerId, id);
        Assert.Equal(547, (await Assert.ThrowsAsync<SqlException>(() => LinkAsync(other.RequestId, id))).Number);
        await LinkAsync(owner.RequestId, id);
    }

    [Fact]
    public async Task Request_RejectsUnknownUpload()
    {
        var seed = await fixture.SeedAsync();
        await ApplyAsync();
        Assert.Equal(547, (await Assert.ThrowsAsync<SqlException>(() => LinkAsync(seed.RequestId, Guid.NewGuid()))).Number);
    }

    [Fact]
    public async Task OwnUpload_CanBeReusedForCorrectedSubmission()
    {
        var first = await fixture.SeedAsync();
        var second = await fixture.SeedAsync();
        await ApplyAsync();
        await using var context = fixture.CreateContext();
        await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE dbo.HotelApprovalRequests SET OwnerId = {first.OwnerId} WHERE Id = {second.RequestId}");
        var id = Guid.NewGuid();
        await InsertAsync(first.OwnerId, id);
        await LinkAsync(first.RequestId, id);
        await LinkAsync(second.RequestId, id);
        Assert.Equal(2, await context.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM dbo.HotelApprovalRequests WHERE BusinessLicenseUploadId = {id}").SingleAsync());
    }

    private async Task ApplyAsync()
    {
        var script = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Sql", "2026-11-09-business-license-uploads.sql"));
        await using var context = fixture.CreateContext();
        await context.Database.ExecuteSqlRawAsync(script);
    }

    private async Task InsertAsync(int ownerId, Guid id, long bytes = 10, string mime = "application/pdf")
    {
        await using var context = fixture.CreateContext();
        await context.Database.ExecuteSqlInterpolatedAsync($"INSERT dbo.BusinessLicenseUploads (Id, OwnerId, PublicId, ResourceType, DeliveryType, StoredUrl, OriginalFileName, ContentType, SizeBytes) VALUES ({id}, {ownerId}, N'test/license', 'image', 'upload', N'https://example.com/file.pdf', N'file.pdf', {mime}, {bytes})");
    }

    private async Task LinkAsync(int requestId, Guid uploadId)
    {
        await using var context = fixture.CreateContext();
        await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE dbo.HotelApprovalRequests SET BusinessLicenseUploadId = {uploadId} WHERE Id = {requestId}");
    }
}
