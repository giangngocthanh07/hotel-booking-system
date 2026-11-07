using HotelBooking.infrastructure.Models;
using HotelBooking.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelBooking.test.UnitTests.Infrastructure;

[Trait("Requirement", "FR-ONB-019")]
public class HotelRequestConcurrencyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task TrySaveChanges_ReturnsAffectedRowsAndForwardsCancellation(int rows)
    {
        using var cancellation = new CancellationTokenSource();
        await using var context = new SaveContext(token =>
        {
            Assert.Equal(cancellation.Token, token);
            return Task.FromResult(rows);
        });
        await using var unit = new UnitOfWork(context);
        var result = await unit.TrySaveChangesAsync(cancellation.Token);
        Assert.Equal(rows, result.AffectedRows);
        Assert.False(result.HasConcurrencyConflict);
    }

    [Fact]
    public async Task TrySaveChanges_ConcurrencyConflict_ReturnsConflictAndClearsStaleTracking()
    {
        await using var context = new SaveContext(_ => throw new DbUpdateConcurrencyException());
        context.Attach(new HotelApprovalRequest { Id = 99, Status = "Pending" });
        await using var unit = new UnitOfWork(context);
        var result = await unit.TrySaveChangesAsync();
        Assert.True(result.HasConcurrencyConflict);
        Assert.Equal(0, result.AffectedRows);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task TrySaveChanges_SystemFailure_IsNotReportedAsConcurrency()
    {
        var failure = new DbUpdateException("Database failure");
        await using var context = new SaveContext(_ => throw failure);
        await using var unit = new UnitOfWork(context);
        Assert.Same(failure, await Assert.ThrowsAsync<DbUpdateException>(() => unit.TrySaveChangesAsync()));
    }

    [Fact]
    public async Task TrySaveChanges_Cancellation_IsNotReportedAsConcurrency()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await using var context = new SaveContext(token => Task.FromCanceled<int>(token));
        await using var unit = new UnitOfWork(context);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => unit.TrySaveChangesAsync(cancellation.Token));
    }

    [Fact]
    public void Status_IsConcurrencyToken_WithOriginalPendingValue()
    {
        using var context = new TestHotelBookingDBContext(new DbContextOptionsBuilder<HotelBookingDBContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options);
        var request = new HotelApprovalRequest { Id = 99, Status = "Pending" };
        context.Attach(request);
        request.Status = "Approved";
        context.Update(request);
        var status = context.Entry(request).Property(value => value.Status);
        Assert.True(status.Metadata.IsConcurrencyToken);
        Assert.Equal("Pending", status.OriginalValue);
        Assert.Equal("Approved", status.CurrentValue);
    }

    private sealed class SaveContext(Func<CancellationToken, Task<int>> save) : TestHotelBookingDBContext(
        new DbContextOptionsBuilder<HotelBookingDBContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => save(cancellationToken);
    }
}
