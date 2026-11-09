using HotelBooking.application.DTOs.Hotel;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration.RequestManagement;

[Trait("Requirement", "FR-ONB-019")]
[Trait("Requirement", "FR-CAT-006")]
[Trait("Category", "SqlIntegration")]
public class HotelRequestConcurrencySqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Theory]
    [InlineData("Approved", "Approved")]
    [InlineData("Approved", "Rejected")]
    [InlineData("Rejected", "Approved")]
    [InlineData("Approved", "Cancelled")]
    [InlineData("Cancelled", "Approved")]
    [InlineData("Rejected", "Cancelled")]
    [InlineData("Cancelled", "Rejected")]
    [InlineData("Cancelled", "Cancelled")]
    [InlineData("Rejected", "Rejected")]
    public async Task FirstSavedActionWins_WithoutDuplicateOrOrphanHotel(string winner, string loser)
    {
        var seed = await fixture.SeedAsync();
        await using var first = new HotelRequestRaceSession(fixture.Options);
        await using var second = new HotelRequestRaceSession(fixture.Options);
        var firstAction = first.ExecuteAsync(winner, seed, seed.AdminId);
        var secondAction = second.ExecuteAsync(loser, seed, seed.OtherAdminId);
        try
        {
            await AssertReachedSaveAsync(first, firstAction);
            await AssertReachedSaveAsync(second, secondAction);
            first.Release();
            Assert.Equal(StatusCodeResponse.Success, (await firstAction.WaitAsync(TimeSpan.FromSeconds(20))).StatusCode);
            second.Release();
            Assert.Equal(StatusCodeResponse.Conflict, (await secondAction.WaitAsync(TimeSpan.FromSeconds(20))).StatusCode);
            await AssertStoredWinnerAsync(seed, winner);
        }
        finally { first.Release(); second.Release(); }
    }

    private static async Task AssertReachedSaveAsync(HotelRequestRaceSession session, Task<ApiResponse<bool>> action)
    {
        var reached = await Task.WhenAny(session.Ready, action).WaitAsync(TimeSpan.FromSeconds(20));
        Assert.True(reached == session.Ready,
            action.IsCompletedSuccessfully ? $"Action stopped before saving: {action.Result.StatusCode}: {action.Result.Message}" : "Action failed before saving.");
    }

    private async Task AssertStoredWinnerAsync(HotelRequestSeed seed, string winner)
    {
        // Fresh context verifies durable SQL state after the losing transaction has rolled back.
        await using var verify = fixture.CreateContext();
        var request = await verify.HotelApprovalRequests.SingleAsync(value => value.Id == seed.RequestId);
        Assert.Equal(winner, request.Status);
        Assert.NotNull(request.UpdatedAt);
        Assert.Equal(winner == "Approved" ? 1 : 0, await verify.Hotels.CountAsync(hotel => hotel.Name == seed.Name));
        Assert.Equal(winner == "Cancelled" ? (int?)null : seed.AdminId, request.AdminId);
        Assert.Equal(winner == "Rejected" ? "The submitted business license is incomplete." : null, request.AdminRemark);
        if (winner == "Approved") await AssertApprovedHotelIsInactiveAsync(seed);
    }

    private async Task AssertApprovedHotelIsInactiveAsync(HotelRequestSeed seed)
    {
        // FR-ONB-016 / FR-CAT-006: approval commits ownership, but publication awaits the Owner's activation.
        await using var verify = fixture.CreateContext();
        var hotel = await verify.Hotels.SingleAsync(value => value.Name == seed.Name);
        Assert.Equal(HotelStatus.Inactive, hotel.Status);
        Assert.True(hotel.IsVerified);
        Assert.False(hotel.IsDeleted);
        Assert.Equal(seed.OwnerId, hotel.OwnerId);
    }
}
