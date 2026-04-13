using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.AdminManagement.RoomAttributes;

public class UnitTypeServiceTests : BaseServiceTest
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
