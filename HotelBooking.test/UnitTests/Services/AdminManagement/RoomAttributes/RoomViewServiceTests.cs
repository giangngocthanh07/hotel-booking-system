using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.AdminManagement.RoomAttributes;

public class RoomViewServiceTests : BaseServiceTest<RoomViewService>
{
    private readonly Mock<IRoomViewRepository> _mockRoomViewRepo;
    private readonly Mock<IValidator<RoomViewCreateDTO>> _mockCreateValidator;
    private readonly Mock<IValidator<RoomViewUpdateDTO>> _mockUpdateValidator;
    private readonly Mock<IValidator<PagingRequest>> _mockPagingValidator;
    private readonly RoomViewService _roomViewService;

    public RoomViewServiceTests()
    {
        _mockRoomViewRepo = new Mock<IRoomViewRepository>();
        _mockCreateValidator = new Mock<IValidator<RoomViewCreateDTO>>();
        _mockUpdateValidator = new Mock<IValidator<RoomViewUpdateDTO>>();
        _mockPagingValidator = new Mock<IValidator<PagingRequest>>();

        _roomViewService = new RoomViewService(
            _mockRoomViewRepo.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockPagingValidator.Object
        );
    }

    #region CreateAsync
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();

        var createDto = new RoomViewCreateDTO
        {
            Name = "City View",
            Description = "View of the city"
        };

        // 2. Act
        var result = await _roomViewService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.CREATE_SUCCESSFULLY);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<RoomViewCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Repo_AddAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsConflict()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess(isDuplicate: true);

        var createDto = new RoomViewCreateDTO
        {
            Name = "Duplicate City View",
            Description = "Description"
        };

        // 2. Act
        var result = await _roomViewService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.AdminManagement.RoomAttribute.RoomView.NAME_ALREADY_EXISTS);

        // Verify steps
        Verify_Repo_Never_AddAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Never_Saved();
    }
    [Fact]
    public async Task CreateAsync_InvalidRequest_ReturnsBadRequest()
    {
        var createDto = new RoomViewCreateDTO
        {
            Name = "",
            Description = "Description"
        };
        var validationFailure = new List<ValidationFailure> { new ValidationFailure("Name", MessageResponse.AdminManagement.RoomAttribute.RoomView.EMPTY_NAME) };
        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<RoomViewCreateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));
        var result = await _roomViewService.CreateAsync(createDto);
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(validationFailure.First().ErrorMessage);
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<RoomViewCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_Never_AddAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtValidateCreateLogicAsync_ReturnsServerError()
    {
        MockCreateValidationSuccess();
        var createDto = new RoomViewCreateDTO { Name = "Test" };
        _mockRoomViewRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<RoomView, bool>>>())).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _roomViewService.CreateAsync(createDto);
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Repo_Never_AddAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtAddAsync_ReturnsServerError()
    {
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();
        var createDto = new RoomViewCreateDTO { Name = "Test" };
        _mockRoomViewRepo.Setup(x => x.AddAsync(It.IsAny<RoomView>())).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _roomViewService.CreateAsync(createDto);
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtSaveChangesAsync_ReturnsServerError()
    {
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();
        var createDto = new RoomViewCreateDTO { Name = "Test" };
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _roomViewService.CreateAsync(createDto);
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Repo_AddAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        VerifyLogErrorOnce();
    }
    #endregion
    
    #region UpdateAsync
    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var roomViewId = 1;
        var updateDTO = new RoomViewUpdateDTO
        {
            Name = "Updated City View",
            Description = "Updated Description"
        };

        MockUpdate_EntityFound(new RoomView { Id = roomViewId, Name = "Old City View", IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // 2. Act
        var result = await _roomViewService.UpdateAsync(roomViewId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.UPDATE_SUCCESSFULLY);

        // Verify
        _mockRoomViewRepo.Verify(x => x.GetByIdAsync(roomViewId), Times.Once());
        Verify_Repo_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task UpdateAsync_IdNotFound_ReturnsNotFound()
    {
        // 1. Arrange
        var roomViewId = 99;
        var updateDTO = new RoomViewUpdateDTO
        {
            Name = "Updated View",
            Description = "Description"
        };

        _mockRoomViewRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((RoomView)null!);

        // 2. Act
        var result = await _roomViewService.UpdateAsync(roomViewId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.Common.NOT_FOUND);

        Verify_Repo_Never_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Never_Saved();
    }
    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsBadRequest()
    {
        var result = await _roomViewService.UpdateAsync(-1, new RoomViewUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.AdminManagement.RoomAttribute.RoomView.INVALID_ID);
        Verify_Repo_Never_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ReturnsBadRequest()
    {
        var id = 1;
        MockUpdate_EntityFound(new RoomView { Id = id, IsDeleted = false });
        var validationFailure = new List<ValidationFailure> { new ValidationFailure("Name", "Error") };
        _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<RoomViewUpdateDTO>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));
        var result = await _roomViewService.UpdateAsync(id, new RoomViewUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        Verify_Repo_Never_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ReturnsConflict()
    {
        var id = 1;
        MockUpdate_EntityFound(new RoomView { Id = id, IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(true);
        var result = await _roomViewService.UpdateAsync(id, new RoomViewUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.AdminManagement.RoomAttribute.RoomView.NAME_ALREADY_EXISTS);
        Verify_Repo_Never_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtUpdateAsync_ReturnsServerError()
    {
        var id = 1;
        _mockRoomViewRepo.Setup(x => x.GetByIdAsync(id)).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _roomViewService.UpdateAsync(id, new RoomViewUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Repo_Never_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtGetByIdAsync_ReturnsServerError()
    {
        // 1. Arrange
        var roomViewId = 1;
        var updateDTO = new RoomViewUpdateDTO { Name = "City View", Description = "Description" };

        // Mock GetByIdAsync throw Exception
        _mockRoomViewRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _roomViewService.UpdateAsync(roomViewId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockRoomViewRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<RoomViewUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Never());
        Verify_Repo_Never_AnyAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Repo_Never_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtValidateUpdateLogicAsync_ReturnsServerError()
    {
        // 1. Arrange
        var roomViewId = 1;
        var updateDTO = new RoomViewUpdateDTO { Name = "City View", Description = "Description" };

        MockUpdate_EntityFound(new RoomView { Id = roomViewId, Name = "Old City View", IsDeleted = false });
        MockUpdateValidation_Success();

        // Mock AnyAsync throw Exception
        _mockRoomViewRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<RoomView, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _roomViewService.UpdateAsync(roomViewId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockRoomViewRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<RoomViewUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Repo_Never_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtAnyAsync_ReturnsServerError()
    {
        // 1. Arrange
        var roomViewId = 1;
        var updateDTO = new RoomViewUpdateDTO { Name = "City View", Description = "Description" };

        MockUpdate_EntityFound(new RoomView { Id = roomViewId, Name = "Old City View", IsDeleted = false });
        MockUpdateValidation_Success();

        // Mock AnyAsync throw Exception
        _mockRoomViewRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<RoomView, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _roomViewService.UpdateAsync(roomViewId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockRoomViewRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<RoomViewUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Repo_Never_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtUpdateRepositoryAsync_ReturnsServerError()
    {
        // 1. Arrange
        var roomViewId = 1;
        var updateDTO = new RoomViewUpdateDTO { Name = "City View", Description = "Description" };

        MockUpdate_EntityFound(new RoomView { Id = roomViewId, Name = "Old City View", IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // Mock UpdateAsync throw Exception
        _mockRoomViewRepo.Setup(x => x.UpdateAsync(It.IsAny<RoomView>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _roomViewService.UpdateAsync(roomViewId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockRoomViewRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<RoomViewUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Repo_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtSaveChangesAsync_ReturnsServerError()
    {
        var id = 1;
        MockUpdate_EntityFound(new RoomView { Id = id, IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // Mock SaveChangesAsync throw Exception
        _mockUnitOfWork.Setup(dbu => dbu.SaveChangesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        var result = await _roomViewService.UpdateAsync(id, new RoomViewUpdateDTO());

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockRoomViewRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<RoomViewUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Repo_UpdateAsync<IRoomViewRepository, RoomView>(_mockRoomViewRepo);
        Verify_Saved(1);
        VerifyLogErrorOnce();
    }
    #endregion

    #region GetPagedListAsync
    [Fact]
    public async Task GetPagedListAsync_ValidPaging_ReturnsSuccess()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };

        _mockPagingValidator.Setup(x => x.ValidateAsync(paging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var mockRoomViews = new List<RoomView>
        {
            new RoomView { Id = 1, Name = "City View", IsDeleted = false, Additional = "{}" }
        };

        _mockRoomViewRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<RoomView, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<RoomView>, IOrderedQueryable<RoomView>>>()))
            .ReturnsAsync((mockRoomViews, 1));

        // 2. Act
        var result = await _roomViewService.GetPagedListAsync(paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().NotBeNull();
        result.Content!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedListAsync_InvalidPageIndex_ReturnsBadRequest()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = -1, PageSize = 10 };

        var validationFailure = new List<ValidationFailure>
        {
            new ValidationFailure("PageIndex", MessageResponse.Pagination.INVALID_PAGE_INDEX)
        };
        _mockPagingValidator.Setup(x => x.ValidateAsync(paging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));

        // 2. Act
        var result = await _roomViewService.GetPagedListAsync(paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_INDEX);

        // Verify
        _mockRoomViewRepo.Verify(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<RoomView, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<RoomView>, IOrderedQueryable<RoomView>>>()), Times.Never());
    }

    [Fact]
    public async Task GetPagedListAsync_InvalidPageSize_ReturnsBadRequest()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = -1 };

        var validationFailure = new List<ValidationFailure>
        {
            new ValidationFailure("PageSize", MessageResponse.Pagination.INVALID_PAGE_SIZE)
        };
        _mockPagingValidator.Setup(x => x.ValidateAsync(paging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));

        // 2. Act
        var result = await _roomViewService.GetPagedListAsync(paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_SIZE);

        // Verify
        _mockRoomViewRepo.Verify(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<RoomView, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<RoomView>, IOrderedQueryable<RoomView>>>()), Times.Never());
    }

    [Fact]
    public async Task GetPagedListAsync_SystemThrowException_AtGetPagedAsync_ReturnsServerError()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };

        _mockPagingValidator.Setup(x => x.ValidateAsync(paging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockRoomViewRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<RoomView, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<RoomView>, IOrderedQueryable<RoomView>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _roomViewService.GetPagedListAsync(paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
        VerifyLogErrorOnce();
    }
    #endregion

    #region GetAllAsync
    [Fact]
    public async Task GetAllAsync_ReturnsSuccess_WhenDataExists()
    {
        // 1. Arrange
        var mockRoomViews = new List<RoomView>
        {
            new RoomView { Id = 1, Name = "City View", IsDeleted = false, Additional = "{}" },
            new RoomView { Id = 2, Name = "Ocean View", IsDeleted = false, Additional = "{}" }
        };

        _mockRoomViewRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<RoomView, bool>>>()))
            .ReturnsAsync(mockRoomViews.AsQueryable());

        // 2. Act
        var result = await _roomViewService.GetAllAsync();

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().HaveCount(2);
    }
    #endregion

    #region HELPERS
    private void MockCreateValidationSuccess()
    {
        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<RoomViewCreateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void MockCreateLogicValidationSuccess(bool isDuplicate = false)
    {
        _mockRoomViewRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<RoomView, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }

    private void MockUpdate_EntityFound(RoomView entity)
    {
        _mockRoomViewRepo.Setup(x => x.GetByIdAsync(entity.Id))
            .ReturnsAsync(entity);
    }

    private void MockUpdateValidation_Success()
    {
        _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<RoomViewUpdateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void MockUpdate_BusinessLogic_DuplicateCheck(bool isDuplicate)
    {
        _mockRoomViewRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<RoomView, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }
    #endregion
}
