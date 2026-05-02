using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.AdminManagement.RoomAttributes;

public class BedTypeServiceTests : BaseServiceTest<BedTypeService>
{
    private readonly Mock<IBedTypeRepository> _mockBedTypeRepo;
    private readonly Mock<IValidator<BedTypeCreateDTO>> _mockCreateValidator;
    private readonly Mock<IValidator<BedTypeUpdateDTO>> _mockUpdateValidator;
    private readonly Mock<IValidator<PagingRequest>> _mockPagingValidator;
    private readonly BedTypeService _bedTypeService;

    public BedTypeServiceTests()
    {
        _mockBedTypeRepo = new Mock<IBedTypeRepository>();
        _mockCreateValidator = new Mock<IValidator<BedTypeCreateDTO>>();
        _mockUpdateValidator = new Mock<IValidator<BedTypeUpdateDTO>>();
        _mockPagingValidator = new Mock<IValidator<PagingRequest>>();

        _bedTypeService = new BedTypeService(
            _mockBedTypeRepo.Object,
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

        var createDto = new BedTypeCreateDTO
        {
            Name = "King Bed",
            Description = "King size bed",
            DefaultCapacity = 2,
            MinWidth = 1.8,
            MaxWidth = 2.0
        };

        // 2. Act
        var result = await _bedTypeService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.CREATE_SUCCESSFULLY);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_AddAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDto = new BedTypeCreateDTO
        {
            Name = "",
            Description = "Description",
            DefaultCapacity = 2
        };

        // Mock validation failure
        var validationFailure = new List<ValidationFailure>
        {
            new ValidationFailure("Name", MessageResponse.AdminManagement.RoomAttribute.BedType.EMPTY_NAME)
        };

        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<BedTypeCreateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));

        // 2. Act
        var result = await _bedTypeService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(validationFailure.First().ErrorMessage);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_Never_AddAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsConflict()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess(isDuplicate: true);

        var createDto = new BedTypeCreateDTO
        {
            Name = "Duplicate King Bed",
            Description = "Description",
            DefaultCapacity = 2
        };

        // 2. Act
        var result = await _bedTypeService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.AdminManagement.RoomAttribute.BedType.NAME_ALREADY_EXISTS);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeCreateDTO>(), default), Times.Once);

        Verify_Repo_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_Never_AddAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtValidateCreateLogicAsync_ReturnsServerError()
    {
        // 1. Arrange
        MockCreateValidationSuccess();

        var creatDto = new BedTypeCreateDTO
        {
            Name = "Bed Type 1",
            Description = "Description 1",
            DefaultCapacity = 2
        };

        // Mock AnyAsync throw exception
        _mockBedTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<BedType, bool>>>())).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _bedTypeService.CreateAsync(creatDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeCreateDTO>(), default), Times.Once);

        Verify_Repo_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_Never_AddAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtAddAsync_ReturnsServerError()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();

        var createDTO = new BedTypeCreateDTO
        {
            Name = "Bed Type 1",
            Description = "Description 1",
            DefaultCapacity = 2
        };

        // Mock AddAsync throw exception
        _mockBedTypeRepo.Setup(x => x.AddAsync(It.IsAny<BedType>())).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _bedTypeService.CreateAsync(createDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once);

        Verify_Repo_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_AddAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtSaveChangesAsync_ReturnsServerError()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();

        var createDTO = new BedTypeCreateDTO
        {
            Name = "Bed Type 1",
            Description = "Description 1",
            DefaultCapacity = 2
        };

        // Mock SaveChangesAsync fail
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _bedTypeService.CreateAsync(createDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once);
        Verify_Repo_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_AddAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Saved(1);
        VerifyLogErrorOnce();
    }

    #endregion

    #region UpdateAsync
    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var bedTypeId = 1;
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Updated King Bed",
            Description = "Updated Description",
            DefaultCapacity = 2,
            MinWidth = 1.8,
            MaxWidth = 2.0
        };

        MockUpdate_EntityFound(new BedType { Id = bedTypeId, Name = "Old King Bed", IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // 2. Act
        var result = await _bedTypeService.UpdateAsync(bedTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.UPDATE_SUCCESSFULLY);

        // Verify
        _mockBedTypeRepo.Verify(x => x.GetByIdAsync(bedTypeId), Times.Once());
        Verify_Repo_UpdateAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsBadRequest()
    {
        // 1. Arrange
        var bedTypeId = -1;
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Updated Bed",
            Description = "Description",
            DefaultCapacity = 1
        };

        // 2. Act
        var result = await _bedTypeService.UpdateAsync(bedTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_ID);

        // Verify steps
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeUpdateDTO>(), It.IsAny<CancellationToken>()), Times.Never());
        _mockBedTypeRepo.Verify(x => x.GetByIdAsync(bedTypeId), Times.Never());
        Verify_Repo_Never_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_Never_UpdateAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task UpdateAsync_IdNotFound_ReturnsNotFound()
    {
        // 1. Arrange
        var bedTypeId = 99;
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Updated Bed",
            Description = "Description",
            DefaultCapacity = 1
        };

        _mockBedTypeRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((BedType)null!);

        // 2. Act
        var result = await _bedTypeService.UpdateAsync(bedTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.Common.NOT_FOUND);

        Verify_Repo_Never_UpdateAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ReturnsBadRequest()
    {
        // 1. Arrange
        var bedTypeId = 1;

        // Mock GetByIdAsync is not null
        MockUpdate_EntityFound(new BedType { Id = bedTypeId, Name = "Old Bed", IsDeleted = false });

        // Mock validation failure
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "",
            Description = "Description",
            DefaultCapacity = 1
        };

        var validationFailure = new List<ValidationFailure>
        {
            new ValidationFailure("Name", MessageResponse.AdminManagement.RoomAttribute.BedType.EMPTY_NAME)
        };

        _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<BedTypeUpdateDTO>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));

        // 2. Act
        var result = await _bedTypeService.UpdateAsync(bedTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(validationFailure.First().ErrorMessage);

        // Verify steps
        _mockBedTypeRepo.Verify(x => x.GetByIdAsync(bedTypeId), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeUpdateDTO>(), default), Times.Once());
        Verify_Repo_Never_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_Never_UpdateAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
    }
    [Fact]
    public async Task UpdateAsync_DuplicateName_ReturnsConflict()
    {
        // 1. Arrange
        var bedTypeId = 1;
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Duplicate Bed",
            Description = "Description",
            DefaultCapacity = 1
        };

        // Mock GetByIdAsync --> found
        MockUpdate_EntityFound(new BedType { Id = bedTypeId, Name = "Old Bed", IsDeleted = false });

        // Mock validation success
        MockUpdateValidation_Success();

        // Mock duplicate name
        MockUpdate_BusinessLogic_DuplicateCheck(true);

        // 2. Act
        var result = await _bedTypeService.UpdateAsync(bedTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.AdminManagement.RoomAttribute.BedType.NAME_ALREADY_EXISTS);

        // Verify steps
        Verify_Repo_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_Never_AddAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtValidateUpdateLogicAsync_ReturnsServerError()
    {
        // 1. Arrange
        var bedTypeId = 1;
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            Description = "Description 1",
            DefaultCapacity = 2
        };

        // Mock GetByIdAsync is not null
        MockUpdate_EntityFound(new BedType { Id = bedTypeId, Name = "Old Bed", IsDeleted = false });

        // Mock validation success
        MockUpdateValidation_Success();

        // Mock AnyAsync throw exception
        _mockBedTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<BedType, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _bedTypeService.UpdateAsync(bedTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify steps
        _mockBedTypeRepo.Verify(x => x.GetByIdAsync(bedTypeId), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeUpdateDTO>(), default), Times.Once);
        Verify_Repo_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_Never_UpdateAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }
    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtUpdateRepositoryAsync_ReturnsServerError()
    {
        // 1. Arrange
        var bedTypeId = 1;
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            Description = "Description 1",
            DefaultCapacity = 2
        };

        MockUpdate_EntityFound(new BedType { Id = bedTypeId, Name = "Old Bed", IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // Mock UpdateAsync throw Exception
        _mockBedTypeRepo.Setup(x => x.UpdateAsync(It.IsAny<BedType>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _bedTypeService.UpdateAsync(bedTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify steps
        _mockBedTypeRepo.Verify(x => x.GetByIdAsync(bedTypeId), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeUpdateDTO>(), default), Times.Once);
        Verify_Repo_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_UpdateAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtGetByIdAsync_ReturnsServerError()
    {
        // 1. Arrange
        var bedTypeId = 1;
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            Description = "Description 1",
            DefaultCapacity = 2
        };

        // Mock GetByIdAsync throw exception
        _mockBedTypeRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _bedTypeService.UpdateAsync(bedTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify steps
        _mockBedTypeRepo.Verify(x => x.GetByIdAsync(bedTypeId), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeUpdateDTO>(), default), Times.Never);
        Verify_Repo_Never_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_Never_UpdateAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtAnyAsync_ReturnsServerError()
    {
        // 1. Arrange
        var bedTypeId = 1;
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            Description = "Description 1",
            DefaultCapacity = 2
        };

        // Mock GetByIdAsync is not null
        MockUpdate_EntityFound(new BedType { Id = bedTypeId, Name = "Old Bed", IsDeleted = false });

        // Mock validation success
        MockUpdateValidation_Success();

        // Mock AnyAsync throw exception
        _mockBedTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<BedType, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _bedTypeService.UpdateAsync(bedTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify steps
        _mockBedTypeRepo.Verify(x => x.GetByIdAsync(bedTypeId), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeUpdateDTO>(), default), Times.Once);
        Verify_Repo_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_Never_UpdateAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Never_Saved();
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtSaveChangesAsync_ReturnsServerError()
    {
        // 1. Arrange
        var bedTypeId = 1;
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            Description = "Description 1",
            DefaultCapacity = 2
        };

        // Mock GetByIdAsync is not null
        MockUpdate_EntityFound(new BedType { Id = bedTypeId, Name = "Bed Type 1", IsDeleted = false });

        // Mock validation  
        MockUpdateValidation_Success();

        // Mock AnyAsync is not null
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // Mock SaveChangesAdync throw exception
        _mockUnitOfWork.Setup(dbu => dbu.SaveChangesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _bedTypeService.UpdateAsync(bedTypeId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockBedTypeRepo.Verify(x => x.GetByIdAsync(bedTypeId), Times.Once());
        _mockUpdateValidator.Verify(x => x.ValidateAsync(It.IsAny<BedTypeUpdateDTO>(), default), Times.Once);
        Verify_Repo_AnyAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
        Verify_Repo_UpdateAsync<IBedTypeRepository, BedType>(_mockBedTypeRepo);
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

        var mockBedTypes = new List<BedType>
        {
            new BedType { Id = 1, Name = "King Bed", IsDeleted = false, Additional = "{}" }
        };

        _mockBedTypeRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<BedType, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<BedType>, IOrderedQueryable<BedType>>>()))
            .ReturnsAsync((mockBedTypes, 1));

        // 2. Act
        var result = await _bedTypeService.GetPagedListAsync(paging);

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
        var result = await _bedTypeService.GetPagedListAsync(paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_INDEX);

        // Verify
        _mockBedTypeRepo.Verify(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<BedType, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<BedType>, IOrderedQueryable<BedType>>>()), Times.Never());
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
        var result = await _bedTypeService.GetPagedListAsync(paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_SIZE);

        // Verify
        _mockBedTypeRepo.Verify(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<BedType, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<BedType>, IOrderedQueryable<BedType>>>()), Times.Never());
    }

    [Fact]
    public async Task GetPagedListAsync_SystemThrowException_AtGetPagedAsync_ReturnsServerError()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };

        _mockPagingValidator.Setup(x => x.ValidateAsync(paging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockBedTypeRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<BedType, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<BedType>, IOrderedQueryable<BedType>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _bedTypeService.GetPagedListAsync(paging);

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
        var mockBedTypes = new List<BedType>
        {
            new BedType { Id = 1, Name = "King Bed", IsDeleted = false, Additional = "{}" },
            new BedType { Id = 2, Name = "Queen Bed", IsDeleted = false, Additional = "{}" }
        };

        _mockBedTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<BedType, bool>>>()))
            .ReturnsAsync(mockBedTypes.AsQueryable());

        // 2. Act
        var result = await _bedTypeService.GetAllAsync();

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().HaveCount(2);
    }
    #endregion

    #region HELPERS
    private void MockCreateValidationSuccess()
    {
        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<BedTypeCreateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void MockCreateLogicValidationSuccess(bool isDuplicate = false)
    {
        _mockBedTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<BedType, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }

    private void MockUpdate_EntityFound(BedType entity)
    {
        _mockBedTypeRepo.Setup(x => x.GetByIdAsync(entity.Id))
            .ReturnsAsync(entity);
    }

    private void MockUpdateValidation_Success()
    {
        _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<BedTypeUpdateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void MockUpdate_BusinessLogic_DuplicateCheck(bool isDuplicate)
    {
        _mockBedTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<BedType, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }
    #endregion
}
