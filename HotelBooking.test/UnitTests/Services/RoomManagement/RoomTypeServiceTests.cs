using System.Linq.Expressions;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Services.Domains.RoomManagement;
using HotelBooking.application.Validators.RoomManagement;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.Tests.Services.RoomManagement;

[Trait("Requirement", "FR-IAM-011")]
[Trait("Requirement", "FR-INV-013")]
public class RoomTypeServiceTests : BaseServiceTest<RoomTypeService>
{
    private readonly Mock<IRoomTypeRepository> _types = new();
    private readonly Mock<IInventoryAccessService> _access = new();
    private readonly Mock<IRoomAttributeFacade> _attributes = new();
    private readonly RoomTypeService _service;

    public RoomTypeServiceTests()
    {
        _access.Setup(access => access.CanAccessHotelAsync(1, 7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _attributes.Setup(attributes => attributes.IsUnitTypeExistedAsync(1)).ReturnsAsync(true);
        _attributes.Setup(attributes => attributes.IsBedTypeExistedAsync(2)).ReturnsAsync(true);
        _attributes.Setup(attributes => attributes.IsRoomQualityExistedAsync(3)).ReturnsAsync(true);
        _attributes.Setup(attributes => attributes.IsRoomViewExistedAsync(4)).ReturnsAsync(true);
        _service = new(_types.Object, new RoomTypeCreateValidator(), _access.Object, _attributes.Object,
            _mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Create_ValidOwner_SavesTrackedBedRelationshipsOnce()
    {
        RoomType? added = null;
        _types.Setup(types => types.AddAsync(It.IsAny<RoomType>())).Callback<RoomType>(type => added = type).Returns(Task.CompletedTask);
        var request = ValidRequest();
        var result = await _service.CreateRoomTypeAsync(request, 7);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.NotNull(added);
        Assert.Equal(3, added.Capacity);
        Assert.Same(added, Assert.Single(added.RoomTypeBedConfigs).RoomType);
        Assert.Equal(2, Assert.Single(added.RoomTypeBedConfigs).BedTypeId);
        Assert.NotNull(result.Content);
        Assert.Equal(request.Name.Trim(), result.Content.Name);
        Assert.Equal(request.BedTypes[0].Quantity, result.Content.BedTypes[0].Quantity);
        Verify_Saved();
        _mockUnitOfWork.Verify(unit => unit.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task Create_UnownedHotel_ForbiddenBeforeLookupsOrWrites()
    {
        var result = await _service.CreateRoomTypeAsync(ValidRequest(), 8);
        Assert.Equal(StatusCodeResponse.Forbidden, result.StatusCode);
        Assert.Equal(InventoryAccessService.AccessDeniedMessage, result.Message);
        Assert.Empty(_types.Invocations);
        Assert.Empty(_attributes.Invocations);
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_InvalidOrNullRequest_ValidationPrecedesAccess(bool isNull)
    {
        var result = await _service.CreateRoomTypeAsync(isNull ? null! : new RoomTypeCreateDTO(), 7);
        Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
        Assert.Empty(_access.Invocations);
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    [Theory]
    [InlineData("unit")]
    [InlineData("quality")]
    [InlineData("view")]
    [InlineData("bed")]
    public async Task Create_MissingAttribute_ReturnsNotFoundWithoutTransaction(string attribute)
    {
        if (attribute == "unit") _attributes.Setup(value => value.IsUnitTypeExistedAsync(1)).ReturnsAsync(false);
        if (attribute == "quality") _attributes.Setup(value => value.IsRoomQualityExistedAsync(3)).ReturnsAsync(false);
        if (attribute == "view") _attributes.Setup(value => value.IsRoomViewExistedAsync(4)).ReturnsAsync(false);
        if (attribute == "bed") _attributes.Setup(value => value.IsBedTypeExistedAsync(2)).ReturnsAsync(false);
        var result = await _service.CreateRoomTypeAsync(ValidRequest(), 7);
        Assert.Equal(StatusCodeResponse.NotFound, result.StatusCode);
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsConflictBeforeTransaction()
    {
        _types.Setup(types => types.AnyAsync(It.IsAny<Expression<Func<RoomType, bool>>>())).ReturnsAsync(true);
        var result = await _service.CreateRoomTypeAsync(ValidRequest(), 7);
        Assert.Equal(StatusCodeResponse.Conflict, result.StatusCode);
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_WriteFailure_RollsBackWithoutCommit(bool failAdd)
    {
        if (failAdd) _types.Setup(types => types.AddAsync(It.IsAny<RoomType>())).ThrowsAsync(new Exception("database"));
        else _mockUnitOfWork.Setup(unit => unit.SaveChangesAsync()).ThrowsAsync(new Exception("database"));
        var result = await _service.CreateRoomTypeAsync(ValidRequest(), 7);
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        _mockUnitOfWork.Verify(unit => unit.RollBackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(unit => unit.CommitTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task Create_CancelledRequest_ThrowsWithoutWrites()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.CreateRoomTypeAsync(ValidRequest(), 7, cancellation.Token));
        Assert.Empty(_types.Invocations);
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    [Fact]
    public async Task Create_CancelledDuringAccess_ThrowsBeforeTransaction()
    {
        using var cancellation = new CancellationTokenSource();
        _access.Setup(access => access.CanAccessHotelAsync(1, 7, cancellation.Token))
            .Callback(() => cancellation.Cancel()).ReturnsAsync(true);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.CreateRoomTypeAsync(ValidRequest(), 7, cancellation.Token));
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    private static RoomTypeCreateDTO ValidRequest() => new()
    {
        HotelId = 1, Name = " Suite ", UnitTypeId = 1, PricePerNight = 100, AdultCapacity = 2, ChildCapacity = 1,
        QualityId = 3, RoomViewId = 4, BedTypes = [new() { BedTypeId = 2, Quantity = 1 }]
    };
}
