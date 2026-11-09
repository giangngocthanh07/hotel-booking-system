
using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Services.Domains.RequestManagement.Owner;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.RequestManagement.Owner;

public partial class HotelRegistrationServiceTests : BaseServiceTest<HotelRegistrationService>
{
    private readonly Mock<IHotelApprovalRequestRepository> _mockApprovalRepo;
    private readonly Mock<IHotelRepository> _mockHotelRepo;
    private readonly Mock<IPropertyTypeRepository> _mockPropertyTypeRepo;
    private readonly Mock<IProvinceRepository> _mockProvinceRepo;
    private readonly Mock<IWardRepository> _mockWardRepo;
    private readonly Mock<IValidator<HotelRegistrationDTO>> _mockValidator;
    private readonly IHotelRegistrationService _service;
    private readonly Mock<IUserRepository> _mockUserRepo = new();
    private readonly Mock<IBusinessLicenseUploadRepository> _mockUploads = new();


    public HotelRegistrationServiceTests()
    {
        _mockUnitOfWork.Setup(unit => unit.TrySaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new SaveChangesResult(1, false));
        _mockUploads.Setup(repo => repo.GetOwnedActiveAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, int ownerId, CancellationToken _) => new BusinessLicenseUpload
                { Id = id, OwnerId = ownerId, StoredUrl = "https://example.com/license.pdf" });
        _mockApprovalRepo = new Mock<IHotelApprovalRequestRepository>();
        _mockHotelRepo = new Mock<IHotelRepository>();
        _mockValidator = new Mock<IValidator<HotelRegistrationDTO>>();
        _mockPropertyTypeRepo = new Mock<IPropertyTypeRepository>();
        _mockProvinceRepo = new Mock<IProvinceRepository>();
        _mockWardRepo = new Mock<IWardRepository>();
        
        _mockPropertyTypeRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new PropertyType { Id = 1, IsActive = true });
        _mockProvinceRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Province { Id = 1 });
        _mockWardRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Ward { Id = 1, ProvinceId = 1 });

        _service = new HotelRegistrationService(_mockApprovalRepo.Object, _mockHotelRepo.Object, _mockPropertyTypeRepo.Object, _mockProvinceRepo.Object, _mockWardRepo.Object, _mockValidator.Object, _mockUnitOfWork.Object, _mockLogger.Object, _mockUserRepo.Object, _mockUploads.Object);
    }

    [Fact]
    [Trait("Requirement", "FR-ONB-019")]
    public async Task Cancel_ConcurrentSave_ReturnsConflictWithoutRetry()
    {
        var request = new HotelApprovalRequest { Id = 99, OwnerId = 7, Status = "Pending" };
        _mockApprovalRepo.Setup(repo => repo.GetPendingByIdAsync(7)).ReturnsAsync([request]);
        _mockUnitOfWork.Setup(unit => unit.TrySaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SaveChangesResult(0, true));
        var result = await _service.CancelRequestAsync(7, 99);
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        _mockUnitOfWork.Verify(unit => unit.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Never);
        _mockUnitOfWork.Verify(unit => unit.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    [Trait("Requirement", "FR-ONB-018")]
    public async Task CancelPendingRequest_RecordsUtcTimeAndSavesOnce()
    {
        var request = new HotelApprovalRequest { Id = 99, OwnerId = 7, Status = "Pending" };
        _mockApprovalRepo.Setup(repo => repo.GetPendingByIdAsync(7)).ReturnsAsync([request]);
        _mockUnitOfWork.Setup(unit => unit.TrySaveChangesAsync(default)).ReturnsAsync(new SaveChangesResult(1, false));
        var before = DateTime.UtcNow;
        var result = await _service.CancelRequestAsync(7, 99);
        var after = DateTime.UtcNow;
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        request.Status.Should().Be("Cancelled");
        Assert.NotNull(request.UpdatedAt);
        Assert.Equal(DateTimeKind.Utc, request.UpdatedAt.Value.Kind);
        Assert.InRange(request.UpdatedAt.Value, before, after);
        _mockApprovalRepo.Verify(repo => repo.UpdateAsync(request), Times.Once);
        VerifyCancellationSaved(1);
    }

    [Theory]
    [InlineData(8, "Pending")]
    [InlineData(7, "Approved")]
    [InlineData(7, "Rejected")]
    [InlineData(7, "Cancelled")]
    [Trait("Requirement", "FR-ONB-018")]
    public async Task CancelIneligibleRequest_DoesNotChangeOrSave(int ownerId, string status)
    {
        // Defense in depth: even unexpected repository results cannot bypass the use-case rule.
        var request = new HotelApprovalRequest { Id = 99, OwnerId = ownerId, Status = status };
        _mockApprovalRepo.Setup(repo => repo.GetPendingByIdAsync(7)).ReturnsAsync([request]);
        var result = await _service.CancelRequestAsync(7, 99);
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        request.Status.Should().Be(status);
        request.UpdatedAt.Should().BeNull();
        _mockApprovalRepo.Verify(repo => repo.UpdateAsync(It.IsAny<HotelApprovalRequest>()), Times.Never);
        VerifyCancellationSaved(0);
    }

    [Theory]
    [InlineData(0, 99)]
    [InlineData(-1, 99)]
    [InlineData(7, 0)]
    [InlineData(7, -1)]
    [Trait("Requirement", "FR-ONB-018")]
    public async Task CancelInvalidIds_DoesNotQueryOrSave(int ownerId, int requestId)
    {
        var result = await _service.CancelRequestAsync(ownerId, requestId);
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        _mockApprovalRepo.Verify(repo => repo.GetPendingByIdAsync(It.IsAny<int>()), Times.Never);
        VerifyCancellationSaved(0);
    }

    [Fact]
    [Trait("Requirement", "FR-ONB-018")]
    public async Task CancelMissingRequest_DoesNotSave()
    {
        _mockApprovalRepo.Setup(repo => repo.GetPendingByIdAsync(7)).ReturnsAsync([]);
        var result = await _service.CancelRequestAsync(7, 99);
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        VerifyCancellationSaved(0);
    }

    [Fact]
    [Trait("Requirement", "FR-ONB-018")]
    public async Task CancelSaveFailure_DoesNotReportSuccess()
    {
        var request = new HotelApprovalRequest { Id = 99, OwnerId = 7, Status = "Pending" };
        _mockApprovalRepo.Setup(repo => repo.GetPendingByIdAsync(7)).ReturnsAsync([request]);
        _mockUnitOfWork.Setup(unit => unit.TrySaveChangesAsync(default)).ReturnsAsync(new SaveChangesResult(0, false));
        var result = await _service.CancelRequestAsync(7, 99);
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        VerifyCancellationSaved(1);
    }

    [Fact]
    [Trait("Requirement", "FR-ONB-013")]
    public async Task GetMyRequests_RejectedRequest_ExposesDecisionWithoutWriting()
    {
        // Enrich only the history fetched for this Owner; reading history must never save data.
        var request = new HotelApprovalRequest { Id = 99, OwnerId = 7, Status = "Rejected",
            AdminRemark = "Business license is incomplete.", AdminId = 42, UpdatedAt = DateTime.UtcNow };
        _mockApprovalRepo.Setup(repo => repo.GetByUserIdAsync(7)).ReturnsAsync([request]);
        _mockUserRepo.Setup(repo => repo.WhereAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync([new User { Id = 42, FullName = "Review Admin" }]);
        var result = await _service.GetMyRequestsAsync(7);
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        var detail = Assert.Single(result.Content!);
        detail.AdminRemark.Should().Be(request.AdminRemark);
        detail.ProcessedAt.Should().Be(request.UpdatedAt);
        detail.ProcessedBy.Should().Be(42);
        detail.ProcessedByName.Should().Be("Review Admin");
        _mockApprovalRepo.Verify(repo => repo.GetByUserIdAsync(7), Times.Once);
        VerifyRegistrationSaved(0);
    }

    [Fact]
    public async Task HotelRegistration_ValidRequest_ShouldReturnTrue()
    {
        // 1. Arrange
        int ownerId = 1;
        var request = CreateValidRequest();

        // Mock validate success
        MockValidationSuccess();

        // Mock Name is not duplicate
        _mockHotelRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(false);

        // 2. Act
        var result = await _service.CreateRequestAsync(ownerId, request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CREATED_SUCCESS);

        _mockApprovalRepo.Verify(r => r.AddAsync(It.IsAny<HotelApprovalRequest>()), Times.Once);
        VerifyRegistrationSaved(1);
    }

    [Fact]
    [Trait("Requirement", "FR-ONB-012")]
    public async Task CreateRequest_RecordsSubmissionTimeInUtc()
    {
        MockValidationSuccess();
        HotelApprovalRequest? saved = null;
        _mockApprovalRepo.Setup(repo => repo.AddAsync(It.IsAny<HotelApprovalRequest>()))
            .Callback<HotelApprovalRequest>(request => saved = request).Returns(Task.CompletedTask);
        var before = DateTime.UtcNow;
        var result = await _service.CreateRequestAsync(1, CreateValidRequest());
        var after = DateTime.UtcNow;
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        Assert.NotNull(saved);
        Assert.Equal(DateTimeKind.Utc, saved.CreatedAt.Kind);
        Assert.InRange(saved.CreatedAt, before, after);
        VerifyRegistrationSaved(1);
    }

    [Fact]
    public async Task HotelRegistration_InvalidOwnerId_ShouldReturnUnauthorized()
    {
        // 1. Arrange
        int ownerId = -1;
        var request = CreateValidRequest();

        // 2. Act
        var result = await _service.CreateRequestAsync(ownerId, request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Unauthorized);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.OWNER_ID_INVALID);

        // Verify steps
        _mockValidator.Verify(r => r.ValidateAsync(It.IsAny<HotelRegistrationDTO>(), default), Times.Never);
        Verify_Repo_Never_AnyAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        Verify_Repo_Never_AddAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        VerifyRegistrationSaved(0);
    }

    [Fact]
    public async Task HotelRegistration_InvalidRequest_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int ownerId = 1;
        var request = CreateValidRequest();

        // Mock validate failure
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new FluentValidation.Results.ValidationFailure("Name", MessageResponse.Validation.EMPTY_NAME)
        };

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<HotelRegistrationDTO>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

        // 2. Act
        var result = await _service.CreateRequestAsync(ownerId, request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(validationFailures.First().ErrorMessage);

        Verify_Repo_Never_AnyAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        Verify_Repo_Never_AddAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        VerifyRegistrationSaved(0);
    }

    [Fact]
    public async Task HotelRegistration_DuplicateName_ShouldReturnConflict()
    {
        // 1. Arrange
        int ownerId = 1;
        var request = CreateValidRequest();

        MockValidationSuccess();

        // Mock Name is duplicate
        _mockHotelRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(true);

        // 2. Act
        var result = await _service.CreateRequestAsync(ownerId, request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_NAME_ALREADY_EXISTS);

        // Verify
        Verify_Repo_AnyAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Repo_Never_AddAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        VerifyRegistrationSaved(0);
    }

    [Fact]
    public async Task HotelRegistration_SystemThrowsExceptionAtAnyAsync_ShouldReturnServerError()
    {
        // 1. Arrange
        int ownerId = 1;
        var request = CreateValidRequest();

        MockValidationSuccess();

        // Mock throw exception at AnyAsync
        _mockHotelRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.CreateRequestAsync(ownerId, request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        Verify_Repo_AnyAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Repo_Never_AddAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        VerifyRegistrationSaved(0);
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task HotelRegistration_SystemThrowsExceptionAtAddAsync_ShouldReturnServerError()
    {
        // 1. Arrange
        int ownerId = 1;
        var request = CreateValidRequest();

        MockValidationSuccess();

        // Mock Name is not duplicate
        _mockHotelRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(false);

        // Mock throw exception at AddAsync
        _mockApprovalRepo.Setup(x => x.AddAsync(It.IsAny<HotelApprovalRequest>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.CreateRequestAsync(ownerId, request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        Verify_Repo_AnyAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Repo_AddAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        VerifyRegistrationSaved(0);
        VerifyLogErrorOnce();
    }

    [Fact]
    public async Task HotelRegistration_SaveDbFails_ShouldReturnError()
    {
        // 1. Arrange
        int ownerId = 1;
        var request = CreateValidRequest();

        MockValidationSuccess();

        // Mock Name is not duplicate
        _mockHotelRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(false);

        // Mock save db fails
        _mockUnitOfWork.Setup(x => x.TrySaveChangesAsync(default)).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.CreateRequestAsync(ownerId, request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        Verify_Repo_AnyAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Repo_AddAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        VerifyRegistrationSaved(1);
        VerifyLogErrorOnce();
    }



    private HotelRegistrationDTO CreateValidRequest()
    {
        return new HotelRegistrationDTO
        {
            Name = "Test Hotel",
            Address = "123 Test Street",
            Description = "Description 1",
            PropertyTypeId = 1,
            StarRating = 3,
            PublicPhone = "0123456789",
            PublicEmail = "testhotel@gmail.com",
            CountryId = 4,
            ProvinceId = 1,
            WardId = 1,
            Latitude = 10.0,
            Longitude = 20.0,
            TaxCode = "1234567890",
            BusinessLicenseUploadId = Guid.NewGuid(),
            BusinessLicenseUrl = "https://example.com/license.pdf"
        };
    }

    private void MockValidationSuccess()
    {
        _mockValidator.Setup(m => m.ValidateAsync(It.IsAny<HotelRegistrationDTO>(), default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());

    }


    private void VerifyCancellationSaved(int count)
    {
        _mockUnitOfWork.Verify(unit => unit.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(count));
        _mockUnitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }
    private void VerifyRegistrationSaved(int count)
    {
        _mockUnitOfWork.Verify(unit => unit.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(count));
        _mockUnitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }
}
