using System.Linq.Expressions;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.RequestManagement;

[Trait("Requirement", "FR-ONB-013")]
[Trait("Requirement", "FR-ONB-014")]
public class HotelRequestDecisionMapperTests
{
    [Theory]
    [InlineData("Pending")]
    [InlineData("Cancelled")]
    public async Task UndecidedRequests_DoNotExposeAdminNotesOrLoadProcessorNames(string status)
    {
        // An update timestamp or stale internal remark is not evidence of an Admin decision.
        var request = new HotelApprovalRequest { Status = status, AdminId = 42,
            AdminRemark = "Internal note", UpdatedAt = DateTime.UtcNow };
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        var names = await HotelRequestDecisionMapper.LoadNamesAsync([request], users.Object);
        var dto = new HotelRegistrationDetailDTO();
        HotelRequestDecisionMapper.Apply(dto, request, names);
        Assert.Null(dto.AdminRemark);
        Assert.Null(dto.ProcessedAt);
        Assert.Null(dto.ProcessedBy);
        Assert.Null(dto.ProcessedByName);
        users.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("Approved", null)]
    [InlineData("Rejected", "Incomplete business license.")]
    public void DecidedRequests_MapReasonOnlyForRejection(string status, string? expectedRemark)
    {
        var request = new HotelApprovalRequest { Status = status, AdminId = 42,
            AdminRemark = "Incomplete business license.", UpdatedAt = DateTime.UtcNow };
        var dto = new HotelRegistrationDetailDTO();
        HotelRequestDecisionMapper.Apply(dto, request, new Dictionary<int, string?> { [42] = "Admin" });
        Assert.Equal(expectedRemark, dto.AdminRemark);
        Assert.Equal(42, dto.ProcessedBy);
        Assert.Equal("Admin", dto.ProcessedByName);
        Assert.Equal(request.UpdatedAt, dto.ProcessedAt);
    }

    [Fact]
    public async Task MissingProcessorName_KeepsRecordedDecisionWithoutInventingAName()
    {
        // Legacy records may have no resolvable account; retain the ID, time and reason.
        var users = new Mock<IUserRepository>();
        users.Setup(repo => repo.WhereAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync([]);
        var request = new HotelApprovalRequest { Status = "Rejected", AdminId = 42,
            AdminRemark = "Incomplete business license.", UpdatedAt = DateTime.UtcNow };
        var names = await HotelRequestDecisionMapper.LoadNamesAsync([request], users.Object);
        var dto = new HotelRegistrationDetailDTO();
        HotelRequestDecisionMapper.Apply(dto, request, names);
        Assert.Null(dto.ProcessedByName);
        Assert.Equal(42, dto.ProcessedBy);
        Assert.Equal(request.AdminRemark, dto.AdminRemark);
        Assert.Equal(request.UpdatedAt, dto.ProcessedAt);
    }

    [Fact]
    public async Task ProcessorLookup_IsLimitedToAdminIdsOfDecidedRequests()
    {
        var users = new Mock<IUserRepository>();
        var accounts = new[] { new User { Id = 42, FullName = "Reviewer" }, new User { Id = 7, FullName = "Owner" } };
        users.Setup(repo => repo.WhereAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((Expression<Func<User, bool>> predicate) => accounts.Where(predicate.Compile()));
        var names = await HotelRequestDecisionMapper.LoadNamesAsync([
            new HotelApprovalRequest { Status = "Rejected", AdminId = 42 },
            new HotelApprovalRequest { Status = "Rejected", AdminId = 42 },
            new HotelApprovalRequest { Status = "Pending", AdminId = 7 }
        ], users.Object);
        Assert.Equal("Reviewer", Assert.Single(names).Value);
        users.Verify(repo => repo.WhereAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
    }
}
