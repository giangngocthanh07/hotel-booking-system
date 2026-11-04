using HotelBooking.application.Services.Domains.RequestManagement.Owner;
using HotelBooking.application.Validators.Common;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.RequestManagement.Owner;

[Trait("Requirement", "FR-ONB-013")]
[Trait("Requirement", "NFR-PERF-008")]
public class HotelRegistrationHistoryServiceTests
{
    private readonly Mock<IHotelApprovalRequestRepository> _requests = new(MockBehavior.Strict);
    private readonly Mock<IUserRepository> _users = new();

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task InvalidPaging_DoesNotQueryHistory(int page, int size)
    {
        var result = await CreateService().GetPageAsync(7, new PagingRequest { PageIndex = page, PageSize = size });
        Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
        _requests.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPage_UsesTrustedOwnerAndReturnsOnlyTheRequestedSlice()
    {
        // Pagination belongs in the repository; never fetch the complete Owner history first.
        using var cancellation = new CancellationTokenSource();
        var record = new HotelApprovalRequest { Id = 99, OwnerId = 7, Name = "Lake Hotel", Status = "Rejected",
            AdminRemark = "License is incomplete.", UpdatedAt = DateTime.UtcNow };
        _requests.Setup(repo => repo.GetPageByOwnerAsync(7, 2, 5, cancellation.Token)).ReturnsAsync(([record], 6));
        var result = await CreateService().GetPageAsync(7, new PagingRequest { PageIndex = 2, PageSize = 5 }, cancellation.Token);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal(6, result.Content!.TotalCount);
        Assert.Equal(2, result.Content.TotalPages);
        Assert.Equal(2, result.Content.PageIndex);
        Assert.Equal("License is incomplete.", Assert.Single(result.Content.Items).AdminRemark);
        _requests.Verify(repo => repo.GetPageByOwnerAsync(7, 2, 5, cancellation.Token), Times.Once);
        _requests.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EmptyOrOutOfRangePage_ReturnsSuccessfulEmptyResult()
    {
        _requests.Setup(repo => repo.GetPageByOwnerAsync(7, 2, 5, default)).ReturnsAsync(([], 0));
        var result = await CreateService().GetPageAsync(7, new PagingRequest { PageIndex = 2, PageSize = 5 });
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Empty(result.Content!.Items);
        Assert.Equal(0, result.Content.TotalCount);
    }

    [Fact]
    public async Task CancelledPage_DoesNotQueryHistory()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateService().GetPageAsync(7, new PagingRequest(), cancellation.Token));
        _requests.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvalidOwner_DoesNotQueryHistory()
    {
        var result = await CreateService().GetPageAsync(0, new PagingRequest { PageIndex = 1, PageSize = 5 });
        Assert.Equal(StatusCodeResponse.Unauthorized, result.StatusCode);
        _requests.VerifyNoOtherCalls();
    }

    private HotelRegistrationHistoryService CreateService() =>
        new(_requests.Object, _users.Object, new PagingRequestValidator());
}
