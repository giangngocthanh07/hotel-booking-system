using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.AdminManagement;

public class ServiceServiceTests : BaseServiceTest
{
    private readonly Mock<IServiceTypeRepository> _mockServiceTypeRepo;
    private readonly Mock<IServiceRepository> _mockServiceRepo;
    private readonly Mock<IValidator<ServiceCreateDTO>> _mockCreateValidator;
    private readonly Mock<IValidator<ServiceUpdateDTO>> _mockUpdateValidator;
    private readonly Mock<IValidator<PagingRequest>> _mockPagingValidator;
    private readonly ServiceService _serviceService;

    public ServiceServiceTests()
    {
        _mockServiceTypeRepo = new Mock<IServiceTypeRepository>();
        _mockServiceRepo = new Mock<IServiceRepository>();
        _mockCreateValidator = new Mock<IValidator<ServiceCreateDTO>>();
        _mockUpdateValidator = new Mock<IValidator<ServiceUpdateDTO>>();
        _mockPagingValidator = new Mock<IValidator<PagingRequest>>();
        
        _serviceService = new ServiceService(
            _mockServiceRepo.Object,
            _mockUnitOfWork.Object,
            _mockServiceTypeRepo.Object,
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

        var createDto = new ServiceStandardCreateDTO
        {
            Name = "Service 1",
            Description = "Description 1",
            Price = 100
        };

        // 2. Act
        var result = await _serviceService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.CREATE_SUCCESSFULLY);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<ServiceCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IServiceRepository, Service>(_mockServiceRepo);
        Verify_Repo_AddAsync<IServiceRepository, Service>(_mockServiceRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsConflict()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess(isDuplicate: true);

        var createDto = new ServiceStandardCreateDTO
        {
            Name = "Duplicate Service",
            Description = "Description",
            Price = 200
        };

        // 2. Act
        var result = await _serviceService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.AdminManagement.Service.NAME_ALREADY_EXISTS);

        // Verify steps
        Verify_Repo_Never_AddAsync<IServiceRepository, Service>(_mockServiceRepo);
        Verify_Never_Saved();
    }
    [Fact]
    public async Task CreateAsync_InvalidRequest_ReturnsBadRequest()
    {
        var createDto = new ServiceStandardCreateDTO
        {
            Name = ""
        };
        var validationFailure = new List<ValidationFailure> { new ValidationFailure("Name", MessageResponse.AdminManagement.Service.EMPTY_NAME) };
        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<ServiceStandardCreateDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));
        var result = await _serviceService.CreateAsync(createDto);
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(validationFailure.First().ErrorMessage);
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<ServiceStandardCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_Never_AddAsync<IServiceRepository, Service>(_mockServiceRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtValidateCreateLogicAsync_ReturnsServerError()
    {
        MockCreateValidationSuccess();
        var createDto = new ServiceStandardCreateDTO { Name = "Test" };
        _mockServiceRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Service, bool>>>())).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _serviceService.CreateAsync(createDto);
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Repo_Never_AddAsync<IServiceRepository, Service>(_mockServiceRepo);
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtAddAsync_ReturnsServerError()
    {
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();
        var createDto = new ServiceStandardCreateDTO { Name = "Test" };
        _mockServiceRepo.Setup(x => x.AddAsync(It.IsAny<Service>())).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _serviceService.CreateAsync(createDto);
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task CreateAsync_SystemThrowException_AtSaveChangesAsync_ReturnsServerError()
    {
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess();
        var createDto = new ServiceStandardCreateDTO { Name = "Test" };
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _serviceService.CreateAsync(createDto);
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Repo_AddAsync<IServiceRepository, Service>(_mockServiceRepo);
    }
    #endregion
    
    #region UpdateAsync
    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var serviceId = 1;
        var updateDTO = new ServiceStandardUpdateDTO
        {
            Name = "Updated Service",
            Description = "Updated Description",
            Price = 150
        };

        MockUpdate_EntityFound(new Service { Id = serviceId, Name = "Old Service", TypeId = 1, IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // 2. Act
        var result = await _serviceService.UpdateAsync(serviceId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.UPDATE_SUCCESSFULLY);

        // Verify
        _mockServiceRepo.Verify(x => x.GetByIdAsync(serviceId), Times.Once());
        Verify_Repo_UpdateAsync<IServiceRepository, Service>(_mockServiceRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task UpdateAsync_IdNotFound_ReturnsNotFound()
    {
        // 1. Arrange
        var serviceId = 99;
        var updateDTO = new ServiceStandardUpdateDTO
        {
            Name = "Updated Service",
            Description = "Description",
            Price = 150
        };

        _mockServiceRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Service)null!);

        // 2. Act
        var result = await _serviceService.UpdateAsync(serviceId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.Common.NOT_FOUND);

        Verify_Repo_Never_UpdateAsync<IServiceRepository, Service>(_mockServiceRepo);
        Verify_Never_Saved();
    }
    #endregion

    #region GetTypeDataAsync
    [Fact]
    public async Task GetTypeDataAsync_ReturnsSuccess_WhenTypesExist()
    {
        // 1. Arrange
        var mockTypes = new List<ServiceType>
        {
            new ServiceType { Id = 1, Name = "Type 1", IsDeleted = false },
            new ServiceType { Id = 2, Name = "Type 2", IsDeleted = false }
        };

        _mockServiceTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<ServiceType, bool>>>()))
            .ReturnsAsync(mockTypes.AsQueryable());

        // 2. Act
        var result = await _serviceService.GetTypeDataAsync();

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().HaveCount(2);
        result.Content!.First().Name.Should().Be("Type 1");
    }

    [Fact]
    public async Task GetTypeDataAsync_ReturnsNotFound_WhenTypesListEmpty()
    {
        // 1. Arrange
        _mockServiceTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<ServiceType, bool>>>()))
            .ReturnsAsync(new List<ServiceType>().AsQueryable());

        // 2. Act
        var result = await _serviceService.GetTypeDataAsync();

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
    }
    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsBadRequest()
    {
        var result = await _serviceService.UpdateAsync(-1, new ServiceStandardUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.AdminManagement.Service.INVALID_ID);
        Verify_Repo_Never_UpdateAsync<IServiceRepository, Service>(_mockServiceRepo);
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ReturnsBadRequest()
    {
        var id = 1;
        MockUpdate_EntityFound(new Service { Id = id, IsDeleted = false });
        var validationFailure = new List<ValidationFailure> { new ValidationFailure("Name", "Error") };
        _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<ServiceStandardUpdateDTO>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailure));
        var result = await _serviceService.UpdateAsync(id, new ServiceStandardUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        Verify_Repo_Never_UpdateAsync<IServiceRepository, Service>(_mockServiceRepo);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ReturnsConflict()
    {
        var id = 1;
        MockUpdate_EntityFound(new Service { Id = id, IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(true);
        var result = await _serviceService.UpdateAsync(id, new ServiceStandardUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.AdminManagement.Service.NAME_ALREADY_EXISTS);
        Verify_Repo_Never_UpdateAsync<IServiceRepository, Service>(_mockServiceRepo);
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtUpdateAsync_ReturnsServerError()
    {
        var id = 1;
        _mockServiceRepo.Setup(x => x.GetByIdAsync(id)).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _serviceService.UpdateAsync(id, new ServiceStandardUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        Verify_Repo_Never_UpdateAsync<IServiceRepository, Service>(_mockServiceRepo);
    }

    [Fact]
    public async Task UpdateAsync_SystemThrowException_AtSaveChangesAsync_ReturnsServerError()
    {
        var id = 1;
        MockUpdate_EntityFound(new Service { Id = id, IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);
        _mockUnitOfWork.Setup(dbu => dbu.SaveChangesAsync()).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
        var result = await _serviceService.UpdateAsync(id, new ServiceStandardUpdateDTO());
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
    }
    #endregion
    
    #region GetServicesByTypeAsync
    [Fact]
    public async Task GetServicesByTypeAsync_ValidTypeId_ReturnsSuccess()
    {
        // 1. Arrange
        int typeId = 1;
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };

        _mockPagingValidator.Setup(x => x.ValidateAsync(paging, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));

        _mockServiceTypeRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<ServiceType, bool>>>()))
            .ReturnsAsync(true);

        var mockServices = new List<Service>
        {
            new Service { Id = 1, Name = "Service 1", TypeId = typeId, IsDeleted = false, Additional = "{}" }
        };

        _mockServiceRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<Service, bool>>>(),
            paging.PageIndex.Value,
            paging.PageSize.Value,
            It.IsAny<Func<IQueryable<Service>, IOrderedQueryable<Service>>>()))
            .ReturnsAsync((mockServices, 1));

        // 2. Act
        var result = await _serviceService.GetServicesByTypeAsync(typeId, paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().NotBeNull();
        result.Content!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetServicesByTypeAsync_InvalidPaging_ReturnsBadRequest()
    {
        // 1. Arrange
        int typeId = 1;
        var paging = new PagingRequest { PageIndex = -1, PageSize = 10 };

        var validationFailure = new List<ValidationFailure> { new ValidationFailure("PageIndex", "Invalid") };
        _mockPagingValidator.Setup(x => x.ValidateAsync(paging, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult(validationFailure)));

        // 2. Act
        var result = await _serviceService.GetServicesByTypeAsync(typeId, paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be("Invalid");
    }
    #endregion

    #region HELPERS
    private void MockCreateValidationSuccess()
    {
        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<ServiceCreateDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
    }

    private void MockCreateLogicValidationSuccess(bool isDuplicate = false)
    {
        _mockServiceRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Service, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }

    private void MockUpdate_EntityFound(Service entity)
    {
        _mockServiceRepo.Setup(x => x.GetByIdAsync(entity.Id))
            .ReturnsAsync(entity);
    }

    private void MockUpdateValidation_Success()
    {
        _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<ServiceUpdateDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
    }

    private void MockUpdate_BusinessLogic_DuplicateCheck(bool isDuplicate)
    {
        _mockServiceRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Service, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }
    #endregion
}
