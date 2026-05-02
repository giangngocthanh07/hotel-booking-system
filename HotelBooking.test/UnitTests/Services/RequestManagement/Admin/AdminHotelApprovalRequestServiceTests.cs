
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
    private readonly Mock<IHotelApprovalRequestRepository> _mockApprovalRepo;
    private readonly Mock<IHotelRepository> _mockHotelRepo;
    private readonly Mock<IValidator<PagingRequest>> _mockPagingValidator;
    private readonly IAdminHotelApprovalRequestService _service;

    private const int ValidRequestId = 99;
    private const int ValidAdminId   = 1;
    private const int ValidOwnerId   = 5;

    private static User BuildOwner() => new User
    {
        Id          = ValidOwnerId,
        UserName    = "owner_user",
        FullName    = "John Owner",
        Email       = "owner@hotel.com",
        PhoneNumber = "0909123456",
        Address     = "123 Owner Street"
    };

    private static string BuildAdditionalJson() =>
        JsonSerializer.Serialize(new HotelAdditionalInfo
        {
            StarRating  = 3,
            PublicPhone = "0281234567",
            PublicEmail = "info@hotel.com",
            PropType    = new PropertyTypeDTO { Id = 3, Name = "Hotel" },
            Country     = new CountryDTO     { Id = 4, Name = "Vietnam" },
            Province    = new ProvinceDTO    { Id = 1, Name = "Ha Noi" },
            Ward        = new WardDTO        { Id = 2, Name = "Quan Hoan Kiem" },
            Latitude    = 21.0,
            Longitude   = 105.0
        });

    private static HotelApprovalRequest BuildPendingRequest(
        User?   owner      = null,
        string? additional = null,
        string? status     = null) => new HotelApprovalRequest
    {
        Id                 = ValidRequestId,
        Name               = "Test Hotel",
        Address            = "456 Hotel Avenue",
        TaxCode            = "1234567890",
        BusinessLicenseUrl = "https://license.com/hotel.pdf",
        OwnerId            = ValidOwnerId,
        Status             = status ?? RequestStatusConst.Pending,
        Additional         = additional ?? BuildAdditionalJson(),
        CreatedAt          = DateTime.UtcNow,
        Owner              = owner ?? BuildOwner()
    };

    public AdminHotelApprovalRequestServiceTests()
    {
        _mockApprovalRepo    = new Mock<IHotelApprovalRequestRepository>();
        _mockHotelRepo       = new Mock<IHotelRepository>();
        _mockPagingValidator = new Mock<IValidator<PagingRequest>>();

        _service = new AdminHotelApprovalRequestService(
            _mockApprovalRepo.Object,
            _mockHotelRepo.Object,
            _mockUnitOfWork.Object,
            _mockPagingValidator.Object);
    }

    // ════════════════════════════════════════════════════════════════════════
    // 1. GET ALL STATUSES
    // ════════════════════════════════════════════════════════════════════════
    #region GET ALL STATUSES

    [Fact]
    public async Task GetAllStatuses_HappyPath_ShouldReturnStatusList()
    {
        // Arrange
        var statuses = new List<string>
        {
            RequestStatusConst.Pending,
            RequestStatusConst.Approved,
            RequestStatusConst.Rejected
        };
        _mockApprovalRepo.Setup(r => r.GetDistinctStatusesAsync()).ReturnsAsync(statuses);

        // Act
        var result = await _service.GetAllStatusesAsync();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.Common.GET_SUCCESSFULLY);
        result.Content.Should().BeEquivalentTo(statuses);
        _mockApprovalRepo.Verify(r => r.GetDistinctStatusesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllStatuses_SystemThrowsException_ShouldReturnServerError()
    {
        // Arrange
        _mockApprovalRepo.Setup(r => r.GetDistinctStatusesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // Act
        var result = await _service.GetAllStatusesAsync();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
        result.Content.Should().BeNull();
        _mockApprovalRepo.Verify(r => r.GetDistinctStatusesAsync(), Times.Once);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // 2. GET BY REQUEST ID
    // ════════════════════════════════════════════════════════════════════════
    #region GET BY REQUEST ID

    [Fact]
    public async Task GetByRequestId_ValidRequest_ShouldReturnMappedDTO()
    {
        // Arrange
        var request        = BuildPendingRequest();
        var additionalInfo = JsonSerializer.Deserialize<HotelAdditionalInfo>(request.Additional!);

        _mockApprovalRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync(request);

        // Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTELS_RETRIEVED);
        result.Content.Should().NotBeNull();
        result.Content!.RequestId.Should().Be(request.Id);
        result.Content.Name.Should().Be(request.Name);
        result.Content.TaxCode.Should().Be(request.TaxCode);
        result.Content.BusinessLicenseUrl.Should().Be(request.BusinessLicenseUrl);
        result.Content.OwnerId.Should().Be(request.OwnerId);
        result.Content.StarRating.Should().Be(additionalInfo!.StarRating);
        result.Content.RequestedAt.Should().Be(request.CreatedAt);
        _mockApprovalRepo.Verify(r => r.GetByIdWithOwnerAsync(ValidRequestId), Times.Once);
    }

    [Fact]
    public async Task GetByRequestId_InvalidRequestId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _service.GetByRequestIdAsync(0);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);
        result.Content.Should().BeNull();
        _mockApprovalRepo.Verify(r => r.GetByIdWithOwnerAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetByRequestId_NegativeRequestId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _service.GetByRequestIdAsync(-5);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);
        result.Content.Should().BeNull();
        _mockApprovalRepo.Verify(r => r.GetByIdWithOwnerAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetByRequestId_RequestNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockApprovalRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync((HotelApprovalRequest)null!);

        // Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);
        result.Content.Should().BeNull();
        _mockApprovalRepo.Verify(r => r.GetByIdWithOwnerAsync(ValidRequestId), Times.Once);
    }

    [Fact]
    public async Task GetByRequestId_InvalidStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var request = BuildPendingRequest(status: "UnknownStatus");
        _mockApprovalRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync(request);

        // Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);
        result.Content.Should().BeNull();
    }

    [Fact]
    public async Task GetByRequestId_NullStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var request = BuildPendingRequest();
        request.Status = null;
        _mockApprovalRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync(request);

        // Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);
        result.Content.Should().BeNull();
    }

    [Fact]
    public async Task GetByRequestId_OwnerNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = BuildPendingRequest();
        request.Owner = null!;
        _mockApprovalRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync(request);

        // Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_OWNER_NOT_FOUND);
        result.Content.Should().BeNull();
    }

    [Fact]
    public async Task GetByRequestId_NullAdditional_ShouldReturnSuccessWithDefaults()
    {
        // Arrange – null Additional JSON should not crash; uses empty defaults
        var request = BuildPendingRequest();
        request.Additional = null;
        _mockApprovalRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ReturnsAsync(request);

        // Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().NotBeNull();
        result.Content!.StarRating.Should().BeNull();
        result.Content.PublicPhone.Should().BeEmpty();
        result.Content.PublicEmail.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRequestId_SystemThrowsException_ShouldReturnServerError()
    {
        // Arrange
        _mockApprovalRepo.Setup(r => r.GetByIdWithOwnerAsync(ValidRequestId))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // Act
        var result = await _service.GetByRequestIdAsync(ValidRequestId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
        result.Content.Should().BeNull();
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // 3. GET PAGED REQUESTS
    // ════════════════════════════════════════════════════════════════════════
    #region GET PAGED REQUESTS

    [Fact]
    public async Task GetPagedRequests_ValidRequest_ShouldReturnPagedResult()
    {
        // Arrange
        var paging   = new PagingRequest { PageIndex = 1, PageSize = 10 };
        var requests = new List<HotelApprovalRequest> { BuildPendingRequest(), BuildPendingRequest() };
        requests[1].Id   = 100;
        requests[1].Name = "Another Hotel";

        MockPagingValidationSuccess();
        _mockApprovalRepo.Setup(r => r.GetPagedWithUserAsync(
                It.IsAny<Expression<Func<HotelApprovalRequest, bool>>>(), 1, 10))
            .ReturnsAsync((requests, 2));

        // Act
        var result = await _service.GetPagedRequestsAsync(paging, RequestStatusConst.Pending);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTELS_RETRIEVED);
        result.Content!.TotalCount.Should().Be(2);
        result.Content.PageIndex.Should().Be(1);
        result.Content.PageSize.Should().Be(10);
        result.Content.Items.Should().HaveCount(2);
        result.Content.Items.First().Name.Should().Be("Test Hotel");

        _mockPagingValidator.Verify(v => v.ValidateAsync(It.IsAny<PagingRequest>(), default), Times.Once);
        _mockApprovalRepo.Verify(r => r.GetPagedWithUserAsync(
            It.IsAny<Expression<Func<HotelApprovalRequest, bool>>>(), 1, 10), Times.Once);
    }

    [Fact]
    public async Task GetPagedRequests_NoStatusFilter_ShouldReturnAll()
    {
        // Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };
        MockPagingValidationSuccess();
        _mockApprovalRepo.Setup(r => r.GetPagedWithUserAsync(
                It.IsAny<Expression<Func<HotelApprovalRequest, bool>>>(), 1, 10))
            .ReturnsAsync((new List<HotelApprovalRequest> { BuildPendingRequest() }, 1));

        // Act
        var result = await _service.GetPagedRequestsAsync(paging, null);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPagedRequests_InvalidPageIndex_ShouldReturnBadRequest()
    {
        // Arrange
        var paging = new PagingRequest { PageIndex = 0, PageSize = 10 };
        MockPagingValidationFailure(nameof(paging.PageIndex), MessageResponse.Pagination.INVALID_PAGE_INDEX);

        // Act
        var result = await _service.GetPagedRequestsAsync(paging, null);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_INDEX);
        result.Content.Should().BeNull();

        _mockPagingValidator.Verify(v => v.ValidateAsync(It.IsAny<PagingRequest>(), default), Times.Once);
        _mockApprovalRepo.Verify(r => r.GetPagedWithUserAsync(
            It.IsAny<Expression<Func<HotelApprovalRequest, bool>>>(),
            It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPagedRequests_InvalidPageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 0 };
        MockPagingValidationFailure(nameof(paging.PageSize), MessageResponse.Pagination.INVALID_PAGE_SIZE);

        // Act
        var result = await _service.GetPagedRequestsAsync(paging, null);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.Pagination.INVALID_PAGE_SIZE);
        result.Content.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedRequests_InvalidStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };
        MockPagingValidationSuccess();

        // Act
        var result = await _service.GetPagedRequestsAsync(paging, "InvalidStatus");

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);
        result.Content.Should().BeNull();

        _mockApprovalRepo.Verify(r => r.GetPagedWithUserAsync(
            It.IsAny<Expression<Func<HotelApprovalRequest, bool>>>(),
            It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPagedRequests_SystemThrowsExceptionAtRepo_ShouldReturnServerError()
    {
        // Arrange
        var paging = new PagingRequest { PageIndex = 1, PageSize = 10 };
        MockPagingValidationSuccess();
        _mockApprovalRepo.Setup(r => r.GetPagedWithUserAsync(
                It.IsAny<Expression<Func<HotelApprovalRequest, bool>>>(), 1, 10))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // Act
        var result = await _service.GetPagedRequestsAsync(paging, RequestStatusConst.Pending);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
        result.Content.Should().BeNull();
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // 4. APPROVE REQUEST
    // ════════════════════════════════════════════════════════════════════════
    #region APPROVE REQUEST

    [Fact]
    public async Task ApproveRequest_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = BuildPendingRequest();
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(request);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.APPROVED_SUCCESS);
        result.Content.Should().BeTrue();

        _mockApprovalRepo.Verify(r => r.UpdateAsync(It.Is<HotelApprovalRequest>(req =>
            req.Status == RequestStatusConst.Approved &&
            req.AdminId == ValidAdminId)), Times.Once);

        _mockHotelRepo.Verify(r => r.AddAsync(It.Is<Hotel>(h =>
            h.Name    == request.Name &&
            h.OwnerId == request.OwnerId &&
            h.IsVerified == true)), Times.Once);

        Verify_Saved(1);
    }

    [Fact]
    public async Task ApproveRequest_InvalidRequestId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _service.ApproveRequestAsync(0, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_NegativeRequestId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _service.ApproveRequestAsync(-1, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_RequestNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId))
            .ReturnsAsync((HotelApprovalRequest)null!);

        // Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_RequestNotInPendingStatus_ShouldReturnBadRequest()
    {
        // Arrange – already Approved → cannot approve again
        var request = BuildPendingRequest(status: RequestStatusConst.Approved);
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(request);

        // Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_RequestInRejectedStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var request = BuildPendingRequest(status: RequestStatusConst.Rejected);
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(request);

        // Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

        Verify_Repo_Never_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_SaveDbFails_ShouldReturnError()
    {
        // Arrange
        var request = BuildPendingRequest();
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(request);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.APPROVE_FAILED);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo, 1);
        Verify_Repo_AddAsync<IHotelRepository, Hotel>(_mockHotelRepo, 1);
        Verify_Saved(1);
    }

    [Fact]
    public async Task ApproveRequest_SystemThrowsAtGetById_ShouldReturnServerError()
    {
        // Arrange
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_SystemThrowsAtSaveChanges_ShouldReturnServerError()
    {
        // Arrange
        var request = BuildPendingRequest();
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(request);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // Act
        var result = await _service.ApproveRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        Verify_Repo_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo, 1);
        Verify_Repo_AddAsync<IHotelRepository, Hotel>(_mockHotelRepo, 1);
        Verify_Saved(1);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // 5. REJECT REQUEST
    // ════════════════════════════════════════════════════════════════════════
    #region REJECT REQUEST

    [Fact]
    public async Task RejectRequest_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = BuildPendingRequest();
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(request);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.REJECTED_SUCCESS);
        result.Content.Should().BeTrue();

        _mockApprovalRepo.Verify(r => r.UpdateAsync(It.Is<HotelApprovalRequest>(req =>
            req.Status  == RequestStatusConst.Rejected &&
            req.AdminId == ValidAdminId)), Times.Once);

        // Hotel repo should NOT be touched on reject
        _mockHotelRepo.Verify(r => r.AddAsync(It.IsAny<Hotel>()), Times.Never);

        Verify_Saved(1);
    }

    [Fact]
    public async Task RejectRequest_InvalidRequestId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _service.RejectRequestAsync(0, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_NegativeRequestId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _service.RejectRequestAsync(-99, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_RequestNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId))
            .ReturnsAsync((HotelApprovalRequest)null!);

        // Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_RequestNotInPendingStatus_ShouldReturnBadRequest()
    {
        // Arrange – already Approved → cannot reject
        var request = BuildPendingRequest(status: RequestStatusConst.Approved);
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(request);

        // Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_AlreadyRejected_ShouldReturnBadRequest()
    {
        // Arrange – idempotency guard
        var request = BuildPendingRequest(status: RequestStatusConst.Rejected);
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(request);

        // Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

        Verify_Repo_Never_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_SaveDbFails_ShouldReturnError()
    {
        // Arrange
        var request = BuildPendingRequest();
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(request);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.REJECT_FAILED);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo, 1);
        Verify_Saved(1);
    }

    [Fact]
    public async Task RejectRequest_SystemThrowsAtGetById_ShouldReturnServerError()
    {
        // Arrange
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        _mockApprovalRepo.Verify(r => r.GetByIdAsync(ValidRequestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_SystemThrowsAtSaveChanges_ShouldReturnServerError()
    {
        // Arrange
        var request = BuildPendingRequest();
        _mockApprovalRepo.Setup(r => r.GetByIdAsync(ValidRequestId)).ReturnsAsync(request);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // Act
        var result = await _service.RejectRequestAsync(ValidRequestId, ValidAdminId);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        Verify_Repo_UpdateAsync<IHotelApprovalRequestRepository, HotelApprovalRequest>(_mockApprovalRepo, 1);
        Verify_Saved(1);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ════════════════════════════════════════════════════════════════════════
    #region HELPERS

    private void MockPagingValidationSuccess()
    {
        _mockPagingValidator
            .Setup(v => v.ValidateAsync(It.IsAny<PagingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void MockPagingValidationFailure(string propertyName, string errorMessage)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new(propertyName, errorMessage)
        };
        _mockPagingValidator
            .Setup(v => v.ValidateAsync(It.IsAny<PagingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(failures));
    }

    #endregion
}