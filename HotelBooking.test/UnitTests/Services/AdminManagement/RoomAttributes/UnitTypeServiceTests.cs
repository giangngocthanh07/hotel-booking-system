using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.AdminManagement.RoomAttributes;

public class UnitTypeServiceTests : BaseServiceTest<UnitTypeService>
{
    private readonly Mock<IUnitTypeRepository> _mockUnitTypeRepo;
    private readonly Mock<IValidator<UnitTypeCreateDTO>> _mockCreateValidator;
    private readonly Mock<IValidator<UnitTypeUpdateDTO>> _mockUpdateValidator;
    private readonly Mock<IValidator<PagingRequest>> _mockPagingValidator;
    private readonly UnitTypeService _unitTypeService;

    public UnitTypeServiceTests()
    {
        _mockUnitTypeRepo = new Mock<IUnitTypeRepository>();
        _mockCreateValidator = new Mock<IValidator<UnitTypeCreateDTO>>();
        _mockUpdateValidator = new Mock<IValidator<UnitTypeUpdateDTO>>();
        _mockPagingValidator = new Mock<IValidator<PagingRequest>>();

        _unitTypeService = new UnitTypeService(
            _mockUnitTypeRepo.Object,
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

        var createDto = new UnitTypeCreateDTO
        {
            Name = "Square Meters",
            Description = "Square meters unit"
        };

        // 2. Act
        var result = await _unitTypeService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.CREATE_SUCCESSFULLY);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<UnitTypeCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Repo_AddAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsConflict()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess(isDuplicate: true);

        var createDto = new UnitTypeCreateDTO
        {
            Name = "Duplicate Square Meters",
            Description = "Description"
        };

        // 2. Act
        var result = await _unitTypeService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.AdminManagement.RoomAttribute.UnitType.NAME_ALREADY_EXISTS);

        // Verify steps
        Verify_Repo_Never_AddAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Never_Saved();
    }
    [Fact]
    public async Task CreateAsync_InvalidRequest_ReturnsBadRequest()
    {
        var createDto = new UnitTypeCreateDTO
        {
            Name = "",
            Description = "Description"
        };
        var validationFailure = new List<ValidationFailure> { new ValidationFailure("Name", MessageResponse.AdminManagement.RoomAttribute.UnitType.EMPTY_NAME) };
        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<UnitTypeCreateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));
        var result = await _unitTypeService.CreateAsync(createDto);
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(validationFailure.First().ErrorMessage);
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<UnitTypeCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_Never_AddAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtValidateCreateLogicAsync_ReturnsServerError()
    {
        MockCreateValidationSuccess();
        var createDto = new UnitTypeCreateDTO { Name = "Test" };
        _mockUnitTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<UnitType, bool>>>())).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _unitTypeService.CreateAsync(createDto);
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Repo_Never_AddAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtAddAsync_ReturnsServerError()
    {
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();
        var createDto = new UnitTypeCreateDTO { Name = "Test" };
        _mockUnitTypeRepo.Setup(x => x.AddAsync(It.IsAny<UnitType>())).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _unitTypeService.CreateAsync(createDto);
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtSaveChangesAsync_ReturnsServerError()
    {
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();
        var createDto = new UnitTypeCreateDTO { Name = "Test" };
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _unitTypeService.CreateAsync(createDto);
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Repo_AddAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        VerifyLogErrorOnce();
    }
    #endregion
    #region UpdateAsync
    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var unitTypeId = 1;
        var updateDTO = new UnitTypeUpdateDTO
        {
            Name = "Updated Square Meters",
            Description = "Updated Description"
        };

        MockUpdate_EntityFound(new UnitType { Id = unitTypeId, Name = "Old Square Meters", IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // 2. Act
        var result = await _unitTypeService.UpdateAsync(unitTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.UPDATE_SUCCESSFULLY);

        // Verify
        _mockUnitTypeRepo.Verify(x => x.GetByIdAsync(unitTypeId), Times.Once());
        Verify_Repo_UpdateAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task UpdateAsync_IdNotFound_ReturnsNotFound()
    {
        // 1. Arrange
        var unitTypeId = 99;
        var updateDTO = new UnitTypeUpdateDTO
        {
            Name = "Updated Unit",
            Description = "Description"
        };

        _mockUnitTypeRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((UnitType)null!);

        // 2. Act
        var result = await _unitTypeService.UpdateAsync(unitTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.Common.NOT_FOUND);

        Verify_Repo_Never_UpdateAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ReturnsBadRequest()
    {
        var id = 1;
        MockUpdate_EntityFound(new UnitType { Id = id, IsDeleted = false });
        var validationFailure = new List<ValidationFailure> { new ValidationFailure("Name", "Error") };
        _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<UnitTypeUpdateDTO>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));
        var result = await _unitTypeService.UpdateAsync(id, new UnitTypeUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        Verify_Repo_Never_UpdateAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ReturnsConflict()
    {
        var id = 1;
        MockUpdate_EntityFound(new UnitType { Id = id, IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(true);
        var result = await _unitTypeService.UpdateAsync(id, new UnitTypeUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.AdminManagement.RoomAttribute.UnitType.NAME_ALREADY_EXISTS);
        Verify_Repo_Never_UpdateAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtGetByIdAsync_ReturnsServerError()
    {
        // 1. Arrange
        var unitTypeId = 1;
        var updateDTO = new UnitTypeUpdateDTO { Name = "Square Meters", Description = "Description" };

        // Mock GetByIdAsync throw Exception
        _mockUnitTypeRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _unitTypeService.UpdateAsync(unitTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUnitTypeRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<UnitTypeUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Never());
        Verify_Repo_Never_AnyAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Repo_Never_UpdateAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtValidateUpdateLogicAsync_ReturnsServerError()
    {
        // 1. Arrange
        var unitTypeId = 1;
        var updateDTO = new UnitTypeUpdateDTO { Name = "Square Meters", Description = "Description" };

        MockUpdate_EntityFound(new UnitType { Id = unitTypeId, Name = "Old Square Meters", IsDeleted = false });
        MockUpdateValidation_Success();

        // Mock AnyAsync throw Exception
        _mockUnitTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<UnitType, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _unitTypeService.UpdateAsync(unitTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUnitTypeRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<UnitTypeUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Repo_Never_UpdateAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtAnyAsync_ReturnsServerError()
    {
        // 1. Arrange
        var unitTypeId = 1;
        var updateDTO = new UnitTypeUpdateDTO { Name = "Square Meters", Description = "Description" };

        MockUpdate_EntityFound(new UnitType { Id = unitTypeId, Name = "Old Square Meters", IsDeleted = false });
        MockUpdateValidation_Success();

        // Mock AnyAsync throw Exception
        _mockUnitTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<UnitType, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _unitTypeService.UpdateAsync(unitTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUnitTypeRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<UnitTypeUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Repo_Never_UpdateAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtUpdateRepositoryAsync_ReturnsServerError()
    {
        // 1. Arrange
        var unitTypeId = 1;
        var updateDTO = new UnitTypeUpdateDTO { Name = "Square Meters", Description = "Description" };

        MockUpdate_EntityFound(new UnitType { Id = unitTypeId, Name = "Old Square Meters", IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // Mock UpdateAsync throw Exception
        _mockUnitTypeRepo.Setup(x => x.UpdateAsync(It.IsAny<UnitType>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _unitTypeService.UpdateAsync(unitTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUnitTypeRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<UnitTypeUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Repo_UpdateAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtSaveChangesAsync_ReturnsServerError()
    {
        // 1. Arrange
        var unitTypeId = 1;
        var updateDTO = new UnitTypeUpdateDTO { Name = "Square Meters", Description = "Description" };

        MockUpdate_EntityFound(new UnitType { Id = unitTypeId, Name = "Square Meters", IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // Mock SaveChangesAsync throw Exception
        _mockUnitOfWork.Setup(dbu => dbu.SaveChangesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _unitTypeService.UpdateAsync(unitTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUnitTypeRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<UnitTypeUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
        Verify_Repo_UpdateAsync<IUnitTypeRepository, UnitType>(_mockUnitTypeRepo);
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

        var mockUnitTypes = new List<UnitType>
        {
            new UnitType { Id = 1, Name = "Square Meters", IsDeleted = false, Additional = "{}" }
        };

        _mockUnitTypeRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<UnitType, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<UnitType>, IOrderedQueryable<UnitType>>>()))
            .ReturnsAsync((mockUnitTypes, 1));

        // 2. Act
        var result = await _unitTypeService.GetPagedListAsync(paging);

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
        var result = await _unitTypeService.GetPagedListAsync(paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_INDEX);

        // Verify
        _mockUnitTypeRepo.Verify(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<UnitType, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<UnitType>, IOrderedQueryable<UnitType>>>()), Times.Never());
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
        var result = await _unitTypeService.GetPagedListAsync(paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_SIZE);

        // Verify
        _mockUnitTypeRepo.Verify(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<UnitType, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<UnitType>, IOrderedQueryable<UnitType>>>()), Times.Never());
    }

    [Fact]
    public async Task GetPagedListAsync_SystemThrowException_AtGetPagedAsync_ReturnsServerError()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };

        _mockPagingValidator.Setup(x => x.ValidateAsync(paging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUnitTypeRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<UnitType, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<UnitType>, IOrderedQueryable<UnitType>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _unitTypeService.GetPagedListAsync(paging);

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
        var mockUnitTypes = new List<UnitType>
        {
            new UnitType { Id = 1, Name = "Square Meters", IsDeleted = false, Additional = "{}" },
            new UnitType { Id = 2, Name = "Square Feet", IsDeleted = false, Additional = "{}" }
        };

        _mockUnitTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<UnitType, bool>>>()))
            .ReturnsAsync(mockUnitTypes.AsQueryable());

        // 2. Act
        var result = await _unitTypeService.GetAllAsync();

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().HaveCount(2);
    }
    #endregion

    #region HELPERS
    private void MockCreateValidationSuccess()
    {
        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<UnitTypeCreateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void MockCreateLogicValidationSuccess(bool isDuplicate = false)
    {
        _mockUnitTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<UnitType, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }

    private void MockUpdate_EntityFound(UnitType entity)
    {
        _mockUnitTypeRepo.Setup(x => x.GetByIdAsync(entity.Id))
            .ReturnsAsync(entity);
    }

    private void MockUpdateValidation_Success()
    {
        _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<UnitTypeUpdateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void MockUpdate_BusinessLogic_DuplicateCheck(bool isDuplicate)
    {
        _mockUnitTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<UnitType, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }
    #endregion
}
