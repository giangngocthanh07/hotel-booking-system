using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.AdminManagement;

public class PolicyServiceTests : BaseServiceTest
{
    private readonly Mock<IPolicyTypeRepository> _mockPolicyTypeRepo;
    private readonly Mock<IPolicyRepository> _mockPolicyRepo;
    private readonly Mock<IValidator<PolicyCreateDTO>> _mockCreateValidator;
    private readonly Mock<IValidator<PolicyUpdateDTO>> _mockUpdateValidator;
    private readonly Mock<IValidator<PagingRequest>> _mockPagingValidator;
    private readonly PolicyService _policyService;

    public PolicyServiceTests()
    {
        _mockPolicyTypeRepo = new Mock<IPolicyTypeRepository>();
        _mockPolicyRepo = new Mock<IPolicyRepository>();
        _mockCreateValidator = new Mock<IValidator<PolicyCreateDTO>>();
        _mockUpdateValidator = new Mock<IValidator<PolicyUpdateDTO>>();
        _mockPagingValidator = new Mock<IValidator<PagingRequest>>();
        
        _policyService = new PolicyService(
            _mockPolicyRepo.Object,
            _mockUnitOfWork.Object,
            _mockPolicyTypeRepo.Object,
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

        var createDto = new CheckInOutPolicyCreateDTO
        {
            Name = "Checkin Policy 1",
            TypeId = 1,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _policyService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.CREATE_SUCCESSFULLY);

        // Verify steps
        _mockCreateValidator.Verify(x => x.ValidateAsync(It.IsAny<PolicyCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once());
        Verify_Repo_AnyAsync<IPolicyRepository, Policy>(_mockPolicyRepo);
        Verify_Repo_AddAsync<IPolicyRepository, Policy>(_mockPolicyRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsConflict()
    {
        // 1. Arrange
        MockCreateValidationSuccess();
        MockCreateLogicValidationSuccess(isDuplicate: true);

        var createDto = new CheckInOutPolicyCreateDTO
        {
            Name = "Duplicate Policy 1",
            TypeId = 1,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _policyService.CreateAsync(createDto);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.AdminManagement.Policy.NAME_ALREADY_EXISTS);

        // Verify steps
        Verify_Repo_Never_AddAsync<IPolicyRepository, Policy>(_mockPolicyRepo);
        Verify_Never_Saved();
    }
    #endregion

    #region UpdateAsync
    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var policyId = 1;
        var updateDTO = new CheckInOutPolicyUpdateDTO
        {
            Name = "Updated Policy",
            Description = "Updated Description",
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = new TimeOnly(12, 0)
        };

        MockUpdate_EntityFound(new Policy { Id = policyId, Name = "Old Policy", TypeId = 1, IsDeleted = false });
        MockUpdateValidation_Success();
        MockUpdate_BusinessLogic_DuplicateCheck(false);

        // 2. Act
        var result = await _policyService.UpdateAsync(policyId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.UPDATE_SUCCESSFULLY);

        // Verify
        _mockPolicyRepo.Verify(x => x.GetByIdAsync(policyId), Times.Once());
        Verify_Repo_UpdateAsync<IPolicyRepository, Policy>(_mockPolicyRepo);
        Verify_Saved(1);
    }

    [Fact]
    public async Task UpdateAsync_IdNotFound_ReturnsNotFound()
    {
        // 1. Arrange
        var policyId = 99;
        var updateDTO = new CheckInOutPolicyUpdateDTO
        {
            Name = "Updated Policy",
            Description = "Description"
        };

        _mockPolicyRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Policy)null!);

        // 2. Act
        var result = await _policyService.UpdateAsync(policyId, updateDTO);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.Common.NOT_FOUND);

        Verify_Repo_Never_UpdateAsync<IPolicyRepository, Policy>(_mockPolicyRepo);
        Verify_Never_Saved();
    }
    #endregion

    #region GetTypeDataAsync
    [Fact]
    public async Task GetTypeDataAsync_ReturnsSuccess_WhenTypesExist()
    {
        // 1. Arrange
        var mockTypes = new List<PolicyType>
        {
            new PolicyType { Id = 1, Name = "Type 1", IsDeleted = false },
            new PolicyType { Id = 2, Name = "Type 2", IsDeleted = false }
        };

        _mockPolicyTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<PolicyType, bool>>>()))
            .ReturnsAsync(mockTypes.AsQueryable());

        // 2. Act
        var result = await _policyService.GetTypeDataAsync();

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
        _mockPolicyTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<PolicyType, bool>>>()))
            .ReturnsAsync(new List<PolicyType>().AsQueryable());

        // 2. Act
        var result = await _policyService.GetTypeDataAsync();

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
    }
    #endregion

    #region GetPoliciesByTypeAsync
    [Fact]
    public async Task GetPoliciesByTypeAsync_ValidTypeId_ReturnsSuccess()
    {
        // 1. Arrange
        int typeId = 1;
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };

        _mockPagingValidator.Setup(x => x.ValidateAsync(paging, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));

        var mockType = new PolicyType { Id = typeId, Name = "General", IsDeleted = false };
        _mockPolicyTypeRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<PolicyType, bool>>>()))
            .ReturnsAsync(new List<PolicyType> { mockType }.AsQueryable());

        var mockPolicies = new List<Policy>
        {
            new Policy { Id = 1, Name = "Policy 1", TypeId = typeId, IsDeleted = false, Additional = "{}" }
        };

        _mockPolicyRepo.Setup(x => x.GetPagedAsync(
            It.IsAny<Expression<Func<Policy, bool>>>(),
            paging.PageIndex.Value,
            paging.PageSize.Value,
            It.IsAny<Func<IQueryable<Policy>, IOrderedQueryable<Policy>>>()))
            .ReturnsAsync((mockPolicies, 1));

        // 2. Act
        var result = await _policyService.GetPoliciesByTypeAsync(typeId, paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().NotBeNull();
        result.Content!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPoliciesByTypeAsync_InvalidPaging_ReturnsBadRequest()
    {
        // 1. Arrange
        int typeId = 1;
        var paging = new PagingRequest { PageIndex = -1, PageSize = 10 };

        var validationFailure = new List<ValidationFailure> { new ValidationFailure("PageIndex", "Invalid") };
        _mockPagingValidator.Setup(x => x.ValidateAsync(paging, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult(validationFailure)));

        // 2. Act
        var result = await _policyService.GetPoliciesByTypeAsync(typeId, paging);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be("Invalid");
    }
    #endregion

    #region HELPERS
    private void MockCreateValidationSuccess()
    {
        _mockCreateValidator.Setup(x => x.ValidateAsync(It.IsAny<PolicyCreateDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
    }

    private void MockCreateLogicValidationSuccess(bool isDuplicate = false)
    {
        _mockPolicyRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Policy, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }

    private void MockUpdate_EntityFound(Policy entity)
    {
        _mockPolicyRepo.Setup(x => x.GetByIdAsync(entity.Id))
            .ReturnsAsync(entity);
    }

    private void MockUpdateValidation_Success()
    {
        _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<PolicyUpdateDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
    }

    private void MockUpdate_BusinessLogic_DuplicateCheck(bool isDuplicate)
    {
        _mockPolicyRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Policy, bool>>>()))
            .ReturnsAsync(isDuplicate);
    }
    #endregion
}
