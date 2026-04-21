
using System.Linq.Expressions;
using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement.Admin;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.RequestManagement.Admin;

public class AdminHotelApprovalRequestServiceTests : BaseServiceTest
{
    private readonly Mock<IHotelRepository> _mockHotelRepo;
    private readonly Mock<IValidator<PagingRequest>> _mockPagingValidator;
    private readonly IAdminHotelApprovalRequestService _service;

    // ─── Shared test data ───────────────────────────────────────────────────────
    private const int ValidRequestId  = 99;
    private const int ValidAdminId    = 1;
    private const int ValidOwnerId    = 5;

    private static User BuildOwner() => new User
    {
        Id          = ValidOwnerId,
        UserName    = "owner_user",
        FullName    = "John Owner",
        Email       = "owner@hotel.com",
        PhoneNumber = "0909123456",
        Address     = "123 Owner Street"
    };

    private static Province BuildProvince() => new Province { Id = 1, Name = "Ha Noi" };
    private static Ward     BuildWard()     => new Ward     { Id = 2, Name = "Quan Hoan Kiem" };
    private static Country  BuildCountry()  => new Country  { Id = 1, Name = "Vietnam" };
    private static PropertyType BuildPropertyType() => new PropertyType { Id = 3, Name = "Hotel" };

    /// <summary>Builds a minimal valid <see cref="Hotel"/> entity in Pending state.</summary>
    private static Hotel BuildPendingHotel(
        User?         owner        = null,
        Province?     province     = null,
        Ward?         ward         = null,
        Country?      country      = null,
        PropertyType? propertyType = null,
        string?       additional   = null)
    {
        var additionalJson = additional ?? JsonSerializer.Serialize(new HotelAdditionalInfo
        {
            StarRating         = 3,
            PublicPhone        = "0281234567",
            PublicEmail        = "info@hotel.com",
            TaxCode            = "1234567890",
            BusinessLicenseUrl = "https://license.com/hotel.pdf"
        });

        return new Hotel
        {
            Id             = ValidRequestId,
            Name           = "Test Hotel",
            Address        = "456 Hotel Avenue",
            OwnerId        = ValidOwnerId,
            Status         = RequestStatusConst.Pending,
            PropertyTypeId = 3,
            CountryId      = 1,
            ProvinceId     = 1,
            WardId         = 2,
            Additional     = additionalJson,
            CreatedAt      = DateTime.UtcNow,
            Owner          = owner        ?? BuildOwner(),
            Province       = province     ?? BuildProvince(),
            Ward           = ward         ?? BuildWard(),
            Country        = country      ?? BuildCountry(),
            PropertyType   = propertyType ?? BuildPropertyType()
        };
    }

    public AdminHotelApprovalRequestServiceTests()
    {
        _mockHotelRepo      = new Mock<IHotelRepository>();
        _mockPagingValidator = new Mock<IValidator<PagingRequest>>();

        _service = new AdminHotelApprovalRequestService(
            _mockHotelRepo.Object,
            _mockUnitOfWork.Object,
            _mockPagingValidator.Object);
    }

    // ════════════════════════════════════════════════════════════════════════════
    // 1. GET ALL STATUSES
    // ════════════════════════════════════════════════════════════════════════════
    #region GET ALL STATUSES

    [Fact]
    public async Task GetAllStatuses_HappyPath_ShouldReturnStatusList()
    {
        // 1. Arrange
        var statuses = new List<string>
        {
            RequestStatusConst.Pending,
            RequestStatusConst.Approved,
            RequestStatusConst.Rejected
        };

        _mockHotelRepo.Setup(r => r.GetDistinctStatusesAsync())
            .ReturnsAsync(statuses);

        // 2. Act
        var result = await _service.GetAllStatusesAsync();

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.GET_SUCCESSFULLY);

        result.Content.Should().NotBeNull();
        result.Content.Should().BeEquivalentTo(statuses);

        _mockHotelRepo.Verify(r => r.GetDistinctStatusesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllStatuses_SystemThrowsException_ShouldReturnServerError()
    {
        // 1. Arrange
        _mockHotelRepo.Setup(r => r.GetDistinctStatusesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.GetAllStatusesAsync();

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        result.Content.Should().BeNull();

        _mockHotelRepo.Verify(r => r.GetDistinctStatusesAsync(), Times.Once);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    // 2. GET BY REQUEST ID
    // ════════════════════════════════════════════════════════════════════════════
    #region GET BY REQUEST ID

    [Fact]
    public async Task GetByRequestId_ValidRequest_ShouldReturnMappedDTO()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();

        // Build the expected DTO that the service should produce from the hotel entity
        var additionalInfo = JsonSerializer.Deserialize<HotelAdditionalInfo>(hotel.Additional!);
        var expectedDTO = new HotelRegistrationDetailDTO
        {
            RequestId        = hotel.Id,
            HotelId          = hotel.Id,
            Name             = hotel.Name,
            OwnerId          = hotel.OwnerId,
            OwnerFullName    = hotel.Owner.FullName ?? string.Empty,
            OwnerEmail       = hotel.Owner.Email,
            OwnerPhoneNumber = hotel.Owner.PhoneNumber,
            OwnerAddress     = hotel.Owner.Address ?? string.Empty,
            Address          = hotel.Address,
            Description      = hotel.Description,
            PropertyTypeId   = hotel.PropertyTypeId,
            PropertyTypeName = hotel.PropertyType?.Name ?? string.Empty,
            CountryId        = hotel.CountryId,
            CountryName      = hotel.Country?.Name ?? string.Empty,
            ProvinceId       = hotel.ProvinceId,
            ProvinceName     = hotel.Province?.Name ?? string.Empty,
            WardId           = hotel.WardId,
            WardName         = hotel.Ward?.Name ?? string.Empty,
            StarRating       = additionalInfo?.StarRating,
            PublicPhone      = additionalInfo?.PublicPhone ?? string.Empty,
            PublicEmail      = additionalInfo?.PublicEmail ?? string.Empty,
            Longitude        = additionalInfo?.Longitude,
            Latitude         = additionalInfo?.Latitude,
            TaxCode          = additionalInfo?.TaxCode ?? string.Empty,
            BusinessLicenseUrl = additionalInfo?.BusinessLicenseUrl ?? string.Empty,
            Status           = hotel.Status ?? RequestStatusConst.None,
            RequestedAt      = hotel.CreatedAt ?? DateTime.Now
        };

        _mockHotelRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync(hotel);

        // 2. Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTELS_RETRIEVED);

        result.Content.Should().NotBeNull();
        result.Content.Should().BeEquivalentTo(expectedDTO);

        _mockHotelRepo.Verify(r => r.GetByIdWithOwnerAsync(ValidRequestId), Times.Once);
    }

    [Fact]
    public async Task GetByRequestId_InvalidRequestId_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int invalidId = 0;

        // 2. Act
        var result = await _service.GetByRequestIdAsync(invalidId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

        result.Content.Should().BeNull();

        // Repo never called
        _mockHotelRepo.Verify(r => r.GetByIdWithOwnerAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetByRequestId_NegativeRequestId_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int negativeId = -5;

        // 2. Act
        var result = await _service.GetByRequestIdAsync(negativeId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

        result.Content.Should().BeNull();

        _mockHotelRepo.Verify(r => r.GetByIdWithOwnerAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetByRequestId_HotelNotFound_ShouldReturnNotFound()
    {
        // 1. Arrange
        _mockHotelRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync((Hotel)null!);

        // 2. Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);

        result.Content.Should().BeNull();

        _mockHotelRepo.Verify(r => r.GetByIdWithOwnerAsync(ValidRequestId), Times.Once);
    }

    [Fact]
    public async Task GetByRequestId_InvalidStatus_ShouldReturnBadRequest()
    {
        // 1. Arrange – hotel exists but has an unrecognized status
        var hotel = BuildPendingHotel();
        hotel.Status = "UnknownStatus";

        _mockHotelRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync(hotel);

        // 2. Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);

        result.Content.Should().BeNull();
    }

    [Fact]
    public async Task GetByRequestId_NullStatus_ShouldReturnBadRequest()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();
        hotel.Status = null;

        _mockHotelRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync(hotel);

        // 2. Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);

        result.Content.Should().BeNull();
    }

    [Fact]
    public async Task GetByRequestId_OwnerNotFound_ShouldReturnNotFound()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();
        hotel.Owner = null!;

        _mockHotelRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync(hotel);

        // 2. Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_OWNER_NOT_FOUND);

        result.Content.Should().BeNull();
    }

    [Fact]
    public async Task GetByRequestId_NullAdditionalInfo_ShouldReturnSuccessWithDefaults()
    {
        // 1. Arrange – null Additional JSON → should not crash, uses empty defaults
        var hotel = BuildPendingHotel(additional: null);
        hotel.Additional = null;

        _mockHotelRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync(hotel);

        // 2. Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);

        result.Content.Should().NotBeNull();
        result.Content!.TaxCode.Should().BeEmpty();
        result.Content.PublicPhone.Should().BeEmpty();
        result.Content.StarRating.Should().BeNull();
    }

    [Fact]
    public async Task GetByRequestId_SystemThrowsException_ShouldReturnServerError()
    {
        // 1. Arrange
        _mockHotelRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        result.Content.Should().BeNull();
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    // 3. GET PAGED REQUESTS
    // ════════════════════════════════════════════════════════════════════════════
    #region GET PAGED REQUESTS

    [Fact]
    public async Task GetPagedRequests_ValidRequest_ShouldReturnPagedResult()
    {
        // 1. Arrange
        int pageIndex = 1;
        int pageSize  = 10;
        var paging    = new PagingRequest { PageIndex = pageIndex, PageSize = pageSize };

        var hotels = new List<Hotel>
        {
            BuildPendingHotel(),
            BuildPendingHotel()
        };
        hotels[1].Id   = 100;
        hotels[1].Name = "Another Hotel";

        MockPagingValidationSuccess();

        _mockHotelRepo.Setup(r => r.GetPagedWithUserAsync(
                It.IsAny<Expression<Func<Hotel, bool>>>(),
                pageIndex,
                pageSize))
            .ReturnsAsync((hotels, 2));

        // 2. Act
        var result = await _service.GetPagedRequestsAsync(paging, RequestStatusConst.Pending);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTELS_RETRIEVED);

        result.Content.Should().NotBeNull();
        result.Content!.TotalCount.Should().Be(2);
        result.Content.PageIndex.Should().Be(pageIndex);
        result.Content.PageSize.Should().Be(pageSize);
        result.Content.Items.Should().HaveCount(2);
        result.Content.Items.First().Name.Should().Be("Test Hotel");

        _mockPagingValidator.Verify(v => v.ValidateAsync(It.IsAny<PagingRequest>(), default), Times.Once);
        _mockHotelRepo.Verify(r => r.GetPagedWithUserAsync(
            It.IsAny<Expression<Func<Hotel, bool>>>(), pageIndex, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetPagedRequests_NoStatusFilter_ShouldReturnAll()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };

        MockPagingValidationSuccess();
        _mockHotelRepo.Setup(r => r.GetPagedWithUserAsync(
                It.IsAny<Expression<Func<Hotel, bool>>>(),
                1, 10))
            .ReturnsAsync((new List<Hotel> { BuildPendingHotel() }, 1));

        // 2. Act
        var result = await _service.GetPagedRequestsAsync(paging, null);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().NotBeNull();
        result.Content!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPagedRequests_InvalidPageIndex_ShouldReturnBadRequest()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = 0, PageSize = 10 };
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new(nameof(paging.PageIndex), MessageResponse.Pagination.INVALID_PAGE_INDEX)
        };

        _mockPagingValidator.Setup(v => v.ValidateAsync(It.IsAny<PagingRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(failures));

        // 2. Act
        var result = await _service.GetPagedRequestsAsync(paging, null);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_INDEX);
        result.Content.Should().BeNull();

        _mockPagingValidator.Verify(v => v.ValidateAsync(It.IsAny<PagingRequest>(), default), Times.Once);
        _mockHotelRepo.Verify(r => r.GetPagedWithUserAsync(
            It.IsAny<Expression<Func<Hotel, bool>>>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPagedRequests_InvalidPageSize_ShouldReturnBadRequest()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 0 };
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new(nameof(paging.PageSize), MessageResponse.Pagination.INVALID_PAGE_SIZE)
        };

        _mockPagingValidator.Setup(v => v.ValidateAsync(It.IsAny<PagingRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(failures));

        // 2. Act
        var result = await _service.GetPagedRequestsAsync(paging, null);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_SIZE);
        result.Content.Should().BeNull();

        _mockPagingValidator.Verify(v => v.ValidateAsync(It.IsAny<PagingRequest>(), default), Times.Once);
        _mockHotelRepo.Verify(r => r.GetPagedWithUserAsync(
            It.IsAny<Expression<Func<Hotel, bool>>>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPagedRequests_InvalidStatus_ShouldReturnBadRequest()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };
        MockPagingValidationSuccess();

        // 2. Act
        var result = await _service.GetPagedRequestsAsync(paging, "InvalidStatus");

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);
        result.Content.Should().BeNull();

        _mockHotelRepo.Verify(r => r.GetPagedWithUserAsync(
            It.IsAny<Expression<Func<Hotel, bool>>>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPagedRequests_SystemThrowsExceptionAtRepo_ShouldReturnServerError()
    {
        // 1. Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };
        MockPagingValidationSuccess();

        _mockHotelRepo.Setup(r => r.GetPagedWithUserAsync(
                It.IsAny<Expression<Func<Hotel, bool>>>(), 1, 10))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.GetPagedRequestsAsync(paging, RequestStatusConst.Pending);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
        result.Content.Should().BeNull();

        _mockPagingValidator.Verify(v => v.ValidateAsync(It.IsAny<PagingRequest>(), default), Times.Once);
        _mockHotelRepo.Verify(r => r.GetPagedWithUserAsync(
            It.IsAny<Expression<Func<Hotel, bool>>>(), 1, 10), Times.Once);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    // 4. APPROVE REQUEST
    // ════════════════════════════════════════════════════════════════════════════
    #region APPROVE REQUEST

    [Fact]
    public async Task ApproveRequest_ValidRequest_ShouldReturnSuccess()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // 2. Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.APPROVED_SUCCESS);
        result.Content.Should().BeTrue();

        // Verify hotel status was changed and persisted
        _mockHotelRepo.Verify(r => r.UpdateAsync(It.Is<Hotel>(h =>
            h.Id == ValidRequestId &&
            h.Status == RequestStatusConst.Approved
        )), Times.Once);

        Verify_Saved(1);
    }

    [Fact]
    public async Task ApproveRequest_InvalidRequestId_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int invalidId = 0;

        // 2. Act
        var result = await _service.ApproveRequestAsync(invalidId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

        // Verify no DB interaction
        _mockHotelRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        Verify_Repo_Never_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_NegativeRequestId_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int negativeId = -1;

        // 2. Act
        var result = await _service.ApproveRequestAsync(negativeId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_HotelNotFound_ShouldReturnNotFound()
    {
        // 1. Arrange
        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId))
            .ReturnsAsync((Hotel)null!);

        // 2. Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_HotelNotInPendingStatus_ShouldReturnBadRequest()
    {
        // 1. Arrange – hotel is already Approved → cannot approve again
        var hotel = BuildPendingHotel();
        hotel.Status = RequestStatusConst.Approved;

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);

        // 2. Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_HotelInRejectedStatus_ShouldReturnBadRequest()
    {
        // 1. Arrange – hotel is Rejected → cannot approve
        var hotel = BuildPendingHotel();
        hotel.Status = RequestStatusConst.Rejected;

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);

        // 2. Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

        Verify_Repo_Never_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_SaveDbFails_ShouldReturnError()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0); // 0 rows → failure

        // 2. Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.APPROVE_FAILED);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo, 1);
        Verify_Saved(1);
    }

    [Fact]
    public async Task ApproveRequest_SystemThrowsExceptionAtGetByIdAsync_ShouldReturnServerError()
    {
        // 1. Arrange
        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_SystemThrowsExceptionAtUpdateAsync_ShouldReturnServerError()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);
        _mockHotelRepo.Setup(r => r.UpdateAsync(It.IsAny<Hotel>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo, 1);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_SystemThrowsExceptionAtSaveChanges_ShouldReturnServerError()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        Verify_Repo_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo, 1);
        Verify_Saved(1);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    // 5. REJECT REQUEST
    // ════════════════════════════════════════════════════════════════════════════
    #region REJECT REQUEST

    [Fact]
    public async Task RejectRequest_ValidRequest_ShouldReturnSuccess()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // 2. Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.REJECTED_SUCCESS);
        result.Content.Should().BeTrue();

        _mockHotelRepo.Verify(r => r.UpdateAsync(It.Is<Hotel>(h =>
            h.Id == ValidRequestId &&
            h.Status == RequestStatusConst.Rejected
        )), Times.Once);

        Verify_Saved(1);
    }

    [Fact]
    public async Task RejectRequest_InvalidRequestId_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int invalidId = 0;

        // 2. Act
        var result = await _service.RejectRequestAsync(invalidId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        Verify_Repo_Never_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_NegativeRequestId_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int negativeId = -99;

        // 2. Act
        var result = await _service.RejectRequestAsync(negativeId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_HotelNotFound_ShouldReturnNotFound()
    {
        // 1. Arrange
        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId))
            .ReturnsAsync((Hotel)null!);

        // 2. Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_HotelNotInPendingStatus_ShouldReturnBadRequest()
    {
        // 1. Arrange – hotel is already Approved → cannot reject
        var hotel = BuildPendingHotel();
        hotel.Status = RequestStatusConst.Approved;

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);

        // 2. Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_HotelAlreadyRejected_ShouldReturnBadRequest()
    {
        // 1. Arrange – hotel already Rejected → idempotency guard
        var hotel = BuildPendingHotel();
        hotel.Status = RequestStatusConst.Rejected;

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);

        // 2. Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

        Verify_Repo_Never_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_SaveDbFails_ShouldReturnError()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0); // 0 rows → failure

        // 2. Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.REJECT_FAILED);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo, 1);
        Verify_Saved(1);
    }

    [Fact]
    public async Task RejectRequest_SystemThrowsExceptionAtGetByIdAsync_ShouldReturnServerError()
    {
        // 1. Arrange
        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_SystemThrowsExceptionAtUpdateAsync_ShouldReturnServerError()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);
        _mockHotelRepo.Setup(r => r.UpdateAsync(It.IsAny<Hotel>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        _mockHotelRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo, 1);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_SystemThrowsExceptionAtSaveChanges_ShouldReturnServerError()
    {
        // 1. Arrange
        var hotel = BuildPendingHotel();

        _mockHotelRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(hotel);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        Verify_Repo_UpdateAsync<IHotelRepository, Hotel>(_mockHotelRepo, 1);
        Verify_Saved(1);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ════════════════════════════════════════════════════════════════════════════
    #region HELPERS

    private void MockPagingValidationSuccess()
    {
        _mockPagingValidator
            .Setup(v => v.ValidateAsync(It.IsAny<PagingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    #endregion
}