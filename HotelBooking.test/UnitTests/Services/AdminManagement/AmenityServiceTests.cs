
using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.infrastructure.Models;
using Moq;
using Org.BouncyCastle.Asn1.Cms;

namespace HotelBooking.test.UnitTests.Services.AdminManagement;

public class AmenityServiceTests : BaseServiceTest
{
    private readonly Mock<IAmenityTypeRepository> _mockAmenityTypeRepo;
    private readonly Mock<IAmenityRepository> _mockAmenityRepo;
    private readonly Mock<IValidator<AmenityCreateDTO>> _mockCreateValidator;
    private readonly Mock<IValidator<AmenityUpdateDTO>> _mockUpdateValidator;
    private readonly AmenityService _amenityService;

    public AmenityServiceTests()
    {
        _mockAmenityTypeRepo = new Mock<IAmenityTypeRepository>();
        _mockAmenityRepo = new Mock<IAmenityRepository>();
        _mockCreateValidator = new Mock<IValidator<AmenityCreateDTO>>();
        _mockUpdateValidator = new Mock<IValidator<AmenityUpdateDTO>>();
        _amenityService = new AmenityService(
            _mockAmenityRepo.Object,
            _mockAmenityTypeRepo.Object,
            _mockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object
        );
    }

    #region CreateAsync
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();

        var createDto = new AmenityCreateDTO
        {
            Name = "Amenity 1",
            TypeId = 1,
            Description = "Description 1"
        };

        // AddAsync and SaveChangesAsync are automatically success

        // 2. Act
        var result = await _amenityService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.CREATE_SUCCESSFULLY);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<AmenityCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());

        Verify_Repo_AnyAsync<IAmenityRepository, Amenity>(_mockAmenityRepo);
        Verify_Repo_AddAsync<IAmenityRepository, Amenity>(_mockAmenityRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDto = new AmenityCreateDTO
        {
            Name = "",
            TypeId = 1,
            Description = "Description 1"
        };

        // Mock validation failure
        var validationFailure = new List<ValidationFailure>
        {
            new ValidationFailure("Name", MessageResponse.AdminManagement.Amenity.EMPTY_NAME)
        };

        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<AmenityCreateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));

        // 2. Act
        var result = await _amenityService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(validationFailure.First().ErrorMessage);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<AmenityCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_Never_AnyAsync<IAmenityRepository, Amenity>(_mockAmenityRepo);
        Verify_Repo_Never_AddAsync<IAmenityRepository, Amenity>(_mockAmenityRepo);
        Verify_Never_Saved();
    }


    [Fact]
    public async Task CreateAsync_SystemThrowException_AtValidateCreateLogicAsync_ReturnsServerError()
    {
        // 1. Arrange
        MockCreateValidationSuccess();

        var createDTO = new AmenityCreateDTO
        {
            Name = "Amenity 1",
            TypeId = 1,
            Description = "Description 1"
        };

        // Mock AnyAsync throw Exception
        _mockAmenityRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Amenity, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _amenityService.CreateAsync(createDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<AmenityCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once);

        Verify_Repo_AnyAsync<IAmenityRepository, Amenity>(_mockAmenityRepo);
        Verify_Repo_Never_AddAsync<IAmenityRepository, Amenity>(_mockAmenityRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtAddAsync_ReturnsServerError()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();

        var createDTO = new AmenityCreateDTO
        {
            Name = "Amenity 1",
            TypeId = 1,
            Description = "Description 1"
        };

        // Mock AddAsync throw Exception
        _mockAmenityRepo.Setup(x => x.AddAsync(It.IsAny<Amenity>()))
        .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _amenityService.CreateAsync(createDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<AmenityCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once);

        Verify_Repo_AnyAsync<IAmenityRepository, Amenity>(_mockAmenityRepo);
        Verify_Repo_AddAsync<IAmenityRepository, Amenity>(_mockAmenityRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtSaveChangesAsync_ReturnsServerError()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();
        var createDTO = new AmenityCreateDTO
        {
            Name = "Amenity 1",
            TypeId = 1,
            Description = "Description 1"
        };

        // Mock SaveChangesAsync throw Exception
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _amenityService.CreateAsync(createDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<AmenityCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once);
        Verify_Repo_AnyAsync<IAmenityRepository, Amenity>(_mockAmenityRepo);
        Verify_Repo_AddAsync<IAmenityRepository, Amenity>(_mockAmenityRepo);
        Verify_Saved(1);
    }
    #endregion

    #region UpdateAsync
    #endregion

    #region GetAmenitiesByTypeAsync
    // First HAPPY PATH
    [Fact]
    public async Task GetAmenitiesByTypeAsync_WithValidTypeId_ReturnsSuccess()
    {
        // 1. Arrange
        int TypeId = 1;
        PagingRequest paging = new PagingRequest { PageIndex = 1, PageSize = 10 };

        // Mock checkTypeExistFunc: return 1 list contain TypeId = 1 to let Any() return true
        var mockType = new AmenityType { Id = TypeId, Name = "General", IsDeleted = false };
        _mockAmenityTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()))
            .ReturnsAsync(new List<AmenityType> { mockType }.AsQueryable());

        // Mock getPagedItemsFunc: return List of Amenities (Entity)
        var mockAmenities = new List<Amenity>
        {
            new Amenity { Id = 1, Name = "Amenity 1", TypeId = TypeId, IsDeleted = false },
            new Amenity { Id = 2, Name = "Amenity 2", TypeId = TypeId, IsDeleted = false }
        };
        int totalCount = 2;

        _mockAmenityRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<Amenity, bool>>>(),
            paging.PageIndex.Value,
            paging.PageSize.Value,
            It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()))
            .ReturnsAsync((mockAmenities, totalCount));

        // 2. Act
        var result = await _amenityService.GetAmenitiesByTypeAsync(TypeId, paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.GET_SUCCESSFULLY);

        result.Content.Should().NotBeNull();
        result.Content.TotalCount.Should().Be(2);
        result.Content.SelectedTypeId.Should().Be(TypeId);
        result.Content.Items.Should().HaveCount(2);
        result.Content.Items.First().Name.Should().Be("Amenity 1");

        // Verify
        _mockAmenityTypeRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()), Times.Once());
        _mockAmenityRepo.Verify(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(),
            paging.PageIndex.Value,
            paging.PageSize.Value,
            It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()), Times.Once());
    }

    // Second HAPPY PATH
    [Fact]
    public async Task GetAmenitiesByTypeAsync_WithNullTypeId_ReturnsBadRequest()
    {
        // 1. Arrange
        int? TypeId = null!;
        PagingRequest pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

        int defaultTypeId = 99;

        // Mock getDefaultIdFunc: return the list, helper will take the FirstOrDefault()
        var mockType = new AmenityType
        { Id = defaultTypeId, Name = "General", IsDeleted = false };

        _mockAmenityTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()))
            .ReturnsAsync(new List<AmenityType> { mockType }.AsQueryable());

        // Mock getPagedItemsFunc: return List of Amenities (Entity)
        var mockAmenities = new List<Amenity>
        {
            new Amenity { Id = 1, Name = "Amenity 1", TypeId = defaultTypeId, IsDeleted = false },
        };
        int totalCount = 1;

        _mockAmenityRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<Amenity, bool>>>(),
            pagingRequest.PageIndex.Value,
            pagingRequest.PageSize.Value,
            It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()))
            .ReturnsAsync((mockAmenities, totalCount));

        // 2. Act
        var result = await _amenityService.GetAmenitiesByTypeAsync(TypeId, pagingRequest);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);

        result.Content.Should().NotBeNull();
        result.Content.SelectedTypeId.Should().Be(defaultTypeId);
        result.Content.Items.Should().HaveCount(1);

        // Verify steps
        _mockAmenityTypeRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()), Times.Once());
        _mockAmenityRepo.Verify(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(),
            pagingRequest.PageIndex.Value,
            pagingRequest.PageSize.Value,
            It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()), Times.Once());

    }

    [Fact]
    public async Task GetAmenitiesByTypeAsync_InvalidTypeId_ReturnsBadRequest()
    {
        // 1. Arrange
        int TypeId = -1;
        PagingRequest pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

        // 2. Act
        var result = await _amenityService.GetAmenitiesByTypeAsync(TypeId, pagingRequest);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Validation.INVALID_TYPE_ID);

        // Verify steps
        _mockAmenityTypeRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()), Times.Never());
        _mockAmenityRepo.Verify(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(),
        pagingRequest.PageIndex.Value,
        pagingRequest.PageSize.Value,
        It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()), Times.Never());
    }

    [Fact]
    public async Task GetAmenitiesByTypeAsync_TypeIdNotFound_ReturnsNotFound()
    {
        // 1. Arrange
        int TypeId = 999;
        PagingRequest pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

        // Mock checkTypeExistFunc: return empty list to let Any() return false
        _mockAmenityTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()))
            .ReturnsAsync(new List<AmenityType>().AsQueryable());

        // 2. Act
        var result = await _amenityService.GetAmenitiesByTypeAsync(TypeId, pagingRequest);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.Common.NOT_FOUND);

        // Verify steps
        _mockAmenityTypeRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()), Times.Once());
        _mockAmenityRepo.Verify(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(),
        pagingRequest.PageIndex.Value,
        pagingRequest.PageSize.Value,
        It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()), Times.Never());
    }

    [Fact]
    public async Task GetAmenitiesByTypeAsync_InvalidPageIndex_ReturnsBadRequest()
    {
        // 1. Arrange
        int TypeId = 1;
        PagingRequest pagingRequest = new PagingRequest { PageIndex = -1, PageSize = 10 };

        // 2. Act
        var result = await _amenityService.GetAmenitiesByTypeAsync(TypeId, pagingRequest);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_INDEX);

        // Verify steps
        _mockAmenityTypeRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()), Times.Never());
        _mockAmenityRepo.Verify(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(),
        pagingRequest.PageIndex.Value,
        pagingRequest.PageSize.Value,
        It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()), Times.Never());
    }

    [Fact]
    public async Task GetAmenitiesByTypeAsync_InvalidPageSize_ReturnsBadRequest()
    {
        // 1. Arrange
        int TypeId = 1;
        PagingRequest pagingRequest = new PagingRequest { PageIndex = 1, PageSize = -1 };

        // 2. Act
        var result = await _amenityService.GetAmenitiesByTypeAsync(TypeId, pagingRequest);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_SIZE);

        // Verify steps
        _mockAmenityTypeRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()), Times.Never());
        _mockAmenityRepo.Verify(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(),
        pagingRequest.PageIndex.Value,
        pagingRequest.PageSize.Value,
        It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()), Times.Never());
    }

    [Fact]
    public async Task GetAmenitiesByTypeAsync_SystemThrowException_AtWhereAsyncGetDefaultIdFunc_ReturnsServerError()
    {
        // 1. Arrange
        int? TypeId = null!;
        PagingRequest pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

        // Mock getDefaultIdFunc: throw exception
        _mockAmenityTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _amenityService.GetAmenitiesByTypeAsync(TypeId, pagingRequest);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify steps
        _mockAmenityTypeRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()), Times.Once());
        _mockAmenityRepo.Verify(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(),
        pagingRequest.PageIndex.Value,
        pagingRequest.PageSize.Value,
        It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()), Times.Never());
    }

    [Fact]
    public async Task GetAmenitiesByTypeAsync_SystemThrowException_AtWhereAsyncCheckTypeExistsFunc_ReturnsServerError()
    {
        // 1. Arrange
        int TypeId = 1;
        PagingRequest pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

        // Mock checkTypeExistsFunc throw exception
        _mockAmenityTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _amenityService.GetAmenitiesByTypeAsync(TypeId, pagingRequest);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify steps
        _mockAmenityTypeRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()), Times.Once());
        _mockAmenityRepo.Verify(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(),
            pagingRequest.PageIndex.Value,
            pagingRequest.PageSize.Value,
            It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()), Times.Never());
    }

    [Fact]
    public async Task GetAmenitiesByTypeAsync_SystemThrowException_AtGetPagedItemsFuncWithValidTypeId_ReturnsServerError()
    {
        // 1. Arrange
        int TypeId = 1;
        PagingRequest pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

        // Mock checkTypeExistsFunc: return a valid amenity type ID
        var mockType = new AmenityType
        { Id = TypeId, Name = "General", IsDeleted = false };

        _mockAmenityTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()))
            .ReturnsAsync(new List<AmenityType> { mockType }.AsQueryable());

        // Mock GetPageAsync throw Exception
        _mockAmenityRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<Amenity, bool>>>(),
            pagingRequest.PageIndex.Value,
            pagingRequest.PageSize.Value,
            It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _amenityService.GetAmenitiesByTypeAsync(TypeId, pagingRequest);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockAmenityTypeRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()), Times.Once());
        _mockAmenityRepo.Verify(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(),
            pagingRequest.PageIndex.Value,
            pagingRequest.PageSize.Value,
            It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()), Times.Once());
    }

    [Fact]
    public async Task GetAmenitiesByTypeAsync_SystemThrowException_AtGetPagedItemsFuncWithNullTypeId_ReturnsServerError()
    {
        // 1. Arrange
        int? TypeId = null!;
        PagingRequest pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

        // Mock getDefaultIdFunc return a list of amenity type, helper will FirstOrDefault
        var mockType = new AmenityType { Id = 99, Name = "General", IsDeleted = false };
        _mockAmenityTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()))
            .ReturnsAsync(new List<AmenityType> { mockType }.AsQueryable());

        // Mock GetPagedAsync throw Exception
        _mockAmenityRepo.Setup(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(),
            pagingRequest.PageIndex.Value,
            pagingRequest.PageSize.Value,
            It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _amenityService.GetAmenitiesByTypeAsync(TypeId, pagingRequest);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify steps
        _mockAmenityTypeRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<AmenityType, bool>>>()
            ), Times.Once());
        _mockAmenityRepo.Verify(x => x.GetPagedAsync(It.IsAny<Expression<Func<Amenity, bool>>>(), pagingRequest.PageIndex.Value, pagingRequest.PageSize.Value, It.IsAny<Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>>()), Times.Once());
    }

    #endregion

    #region HELPERS
    private void MockCreateValidationSuccess()
    {
        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<AmenityCreateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void MockCreateLogicValidationSuccess(bool isDuplicate = false)
    {
        _mockAmenityRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Amenity, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }

    private void MockUpdateValidationSuccess()
    {
        _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<AmenityUpdateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void MockUpdateLogicValidationSuccess(Amenity? existingAmenity = null, bool isDuplicate = false)
    {
        var amenity = existingAmenity ?? new Amenity { Id = 1, Name = "Amenity 1", TypeId = 1, IsDeleted = false };

        _mockAmenityRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(amenity);

        // Duplicate
        _mockAmenityRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Amenity, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }
    #endregion
}