
using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Role;
using HotelBooking.application.Services.Domains.RequestManagement.Admin;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.RequestManagement.Admin;

public class AdminUpgradeRequestServiceTests : BaseServiceTest
{
    private readonly Mock<IUpgradeRequestRepository> _mockUpgradeRequestRepo;
    private readonly Mock<IUserRoleRepository> _mockUserRoleRepo;
    private readonly Mock<IValidator<PagingRequest>> _mockPagingValidator;
    private readonly IAdminUpgradeRequestService _service;

    public AdminUpgradeRequestServiceTests()
    {
        _mockUpgradeRequestRepo = new Mock<IUpgradeRequestRepository>();
        _mockUserRoleRepo = new Mock<IUserRoleRepository>();
        _mockPagingValidator = new Mock<IValidator<PagingRequest>>();
        _service = new AdminUpgradeRequestService(
            _mockUpgradeRequestRepo.Object,
            _mockUserRoleRepo.Object,
            _mockUnitOfWork.Object,
            _mockPagingValidator.Object);
    }

    #region APPROVE REQUEST TESTS
    [Fact]
    public async Task ApproveRequest_ValidRequest_ShouldReturnTrue()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;
        int customerId = 5;

        var validUser = new User
        {
            Id = customerId,
            UserName = "testuser"

        };

        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = customerId,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        MockGetByIdWithUserAsync(validRequest);
        MockValidCustomerRole(customerId);

        // Mock UnitOfWork
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_APPROVED_SUCCESS);

        // Verify 3 repos
        _mockUserRoleRepo.Verify(r => r.AddAsync(It.Is<UserRole>(ur =>
            ur.UserId == customerId &&
            ur.RoleId == RoleTypeConstDTO.Owner
        )), Times.Once);

        _mockUpgradeRequestRepo.Verify(r => r.UpdateAsync(It.Is<UpgradeRequest>(req =>
            req.Id == requestId &&
            req.Status == RequestStatusConst.Approved &&
            req.ApprovedBy == adminId &&
            req.User.Address == "123 Default Street" &&
            req.User.TaxCode == "1234567890"
        )), Times.Once);

        Verify_Saved(1);


    }

    [Fact]
    public async Task ApproveRequest_InvalidRequestId_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int requestId = 0;
        int adminId = 1;

        // Mock InvalidRequestId --> FAIL FAST
        _mockUpgradeRequestRepo.Setup(r => r.GetByIdWithUserAsync(requestId))
            .ReturnsAsync((UpgradeRequest)null!);

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_ID_INVALID);

        // Verify that userRole repo's methods: AnyAsync, AddAsync and upgradeRequestRepo's method: UpdateAsync and SaveChanges of dbu are not called.
        Verify_Repo_Never_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Repo_Never_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_RequestNotFound_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        _mockUpgradeRequestRepo.Setup(r => r.GetByIdWithUserAsync(requestId))
        .ReturnsAsync((UpgradeRequest)null!);

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);

        // Verify that userRole repo's methods: AnyAsync, AddAsync and upgradeRequestRepo's method: UpdateAsync and SaveChanges of dbu are not called.
        Verify_Repo_Never_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Repo_Never_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_InvalidRequestStatus_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        var invalidRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Approved,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        // Mock Invalid Request Status
        MockGetByIdWithUserAsync(invalidRequest);

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);

        // Verify
        // Verify that userRole repo's methods: AnyAsync, AddAsync and upgradeRequestRepo's method: UpdateAsync and SaveChanges of dbu are not called.
        Verify_Repo_Never_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Repo_Never_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_UserNotFound_ShouldReturnNotFound()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        var requestWithNoUser = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            User = null!
        };

        // Mock User --> Not Found
        MockGetByIdWithUserAsync(requestWithNoUser);

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

        // Verify that userRole repo's methods: AnyAsync, AddAsync and upgradeRequestRepo's method: UpdateAsync and SaveChanges of dbu are not called.
        Verify_Repo_Never_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Repo_Never_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Never_Saved();

    }

    [Fact]
    public async Task ApproveRequest_UserNotCustomer_ShouldReturnForbidden()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        var validUser = new User
        {
            Id = 5,
            UserName = "notcustomer"

        };

        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        MockGetByIdWithUserAsync(validRequest);

        // Mock UserRole is not Customer Role
        _mockUserRoleRepo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
            .ReturnsAsync(false);

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Forbidden);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);

        // Verify
        Verify_Repo_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 1);

        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Repo_Never_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Never_Saved();

    }

    [Fact]
    public async Task ApproveRequest_UserAlreadyOwner_ShouldReturnForbidden()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        var validUser = new User
        {
            Id = 5,
            UserName = "owner"

        };

        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        MockGetByIdWithUserAsync(validRequest);

        _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
            .ReturnsAsync(true)
            .ReturnsAsync(true);

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Forbidden);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_ALREADY_OWNER);

        // Verify
        Verify_Repo_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 2);

        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Repo_Never_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Never_Saved();

    }

    [Fact]
    public async Task ApproveRequest_SaveDbFails_ShouldReturnError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        MockGetByIdWithUserAsync(validRequest);
        MockValidCustomerRole(5);

        // Mock UnitOfWork
        _mockUnitOfWork.Setup(dbu => dbu.SaveChangesAsync())
            .ReturnsAsync(0);

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_APPROVE_FAILED);

        // Verify
        _mockUserRoleRepo.Verify(r => r.AddAsync(It.Is<UserRole>(ur =>
            ur.UserId == 5 &&
            ur.RoleId == RoleTypeConstDTO.Owner
        )), Times.Once);

        _mockUpgradeRequestRepo.Verify(r => r.UpdateAsync(It.Is<UpgradeRequest>(req =>
            req.Id == requestId &&
            req.Status == RequestStatusConst.Approved &&
            req.ApprovedBy == adminId &&
            req.User.Address == "123 Default Street" &&
            req.User.TaxCode == "1234567890"
        )), Times.Once);

        Verify_Saved(1);
    }

    [Fact]
    public async Task ApproveRequest_SystemThrowsExceptionAtGetByIdWithUserAsync_ShouldReturnServerError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        // valid User
        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        // valid Request
        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        _mockUpgradeRequestRepo.Setup(r => r.GetByIdWithUserAsync(requestId))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdWithUserAsync(requestId), Times.Once);

        Verify_Repo_Never_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Repo_Never_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_SystemThrowsExceptionAtUserRoleRepoAtStart_ShouldReturnServerError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        // valid User
        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        // valid Request
        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser

        };

        MockGetByIdWithUserAsync(validRequest);

        // Mock UserRole - Fail Fast
        _mockUserRoleRepo.Setup(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>())).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdWithUserAsync(requestId), Times.Once);

        Verify_Repo_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 1);

        Verify_Repo_Never_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task SystemThrowsExceptionAtUserRoleRepoAtEnd_ShouldReturnServerError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        // valid User
        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        // valid Request
        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser

        };

        MockGetByIdWithUserAsync(validRequest);

        // Mock userRole fail at the second time --> FAIL FAST
        _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
            .ReturnsAsync(true)
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdWithUserAsync(requestId), Times.Once);

        Verify_Repo_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 2);

        Verify_Repo_Never_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_SystemThrowsExceptionAtAddAsyncUserRoleRepo_ShouldReturnServerError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        // valid User
        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        MockGetByIdWithUserAsync(validRequest);
        MockValidCustomerRole(5);

        // Mock AddAsync fail --> FAIL FAST
        _mockUserRoleRepo.Setup(r => r.AddAsync(It.Is<UserRole>(ur =>
            ur.UserId == 5 &&
            ur.RoleId == RoleTypeConstDTO.Owner)))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act 
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdWithUserAsync(requestId), Times.Once);

        Verify_Repo_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 2);
        Verify_Repo_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 1);

        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task ApproveRequest_SystemThrowsExceptionAtUpdateAsyncUpgradeRequestRepo_ShouldReturnServerError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        // valid User
        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        var ownerRole = new UserRole
        {
            UserId = 5,
            RoleId = RoleTypeConstDTO.Owner
        };


        MockGetByIdWithUserAsync(validRequest);
        MockValidCustomerRole(5);

        // No need to setup AddAsync success --> automatically passed

        // Mock UpdateAsync fail --> FAIL FAST
        _mockUpgradeRequestRepo.Setup(r => r.UpdateAsync(It.IsAny<UpgradeRequest>()))
        .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdWithUserAsync(requestId), Times.Once);

        Verify_Repo_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 2);
        Verify_Repo_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 1);
        Verify_Repo_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo, 1);

        Verify_Never_Saved();

    }

    [Fact]
    public async Task ApproveRequest_SystemThrowsExceptionAtSaveDb_ShouldReturnServerError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        // valid User
        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        MockGetByIdWithUserAsync(validRequest);
        MockValidCustomerRole(5);

        // AddAsync and UpdateAsync automatically passed --> no need to mock

        // Mock SaveChangesAsync fail at Catch
        _mockUnitOfWork.Setup(dbu => dbu.SaveChangesAsync()).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.ApproveRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdWithUserAsync(requestId), Times.Once);
        Verify_Repo_AnyAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 2);
        Verify_Repo_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 1);
        Verify_Repo_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo, 1);

        Verify_Saved(1);
    }
    #endregion

    #region REJECT REQUEST TESTS
    [Fact]
    public async Task RejectRequest_ValidRequest_ShouldReturnTrue()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"
        };

        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        // Mock GetByIdAsync success
        _mockUpgradeRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(validRequest);

        // UpdateAsync automatically successs -> no need to mock

        // Mock SaveChangesAsync success --> return 1
        _mockUnitOfWork.Setup(dbu => dbu.SaveChangesAsync()).ReturnsAsync(1);

        // 2. Act
        var result = await _service.RejectRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_REJECTED_SUCCESS);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdAsync(requestId), Times.Once);
        _mockUpgradeRequestRepo.Verify(r => r.UpdateAsync(It.Is<UpgradeRequest>(req =>
            req.Id == requestId &&
            req.Status == RequestStatusConst.Rejected &&
            req.ApprovedBy == adminId &&
            req.ApprovedAt != null
        )), Times.Once);

        Verify_Saved(1);
    }

    [Fact]
    public async Task RejectRequest_InvalidRequestId_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int requestId = 0;  // Invalid Request
        int adminId = 1;

        // 2. Act
        var result = await _service.RejectRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_ID_INVALID);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdAsync(requestId), Times.Never);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_RequestNotFound_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        // Mock GetByIdAsync fail --> FAIL FAST
        _mockUpgradeRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((UpgradeRequest)null!);

        // 2. Act
        var result = await _service.RejectRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdAsync(requestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_InvalidRequestStatus_ShouldReturnBadRequest()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"
        };

        var invalidRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Approved,
        };

        // Mock InvalidRequestStatus
        _mockUpgradeRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(invalidRequest);

        // 2. Act
        var result = await _service.RejectRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdAsync(requestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_SaveDbFails_ShouldReturnError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        // Mock GetByIdAsync success
        _mockUpgradeRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(validRequest);

        // UpdateAsync is success --> no need to mock

        // Mock SaveChanges fails at try --> return 0
        _mockUnitOfWork.Setup(dbu => dbu.SaveChangesAsync())
            .ReturnsAsync(0);

        // 2. Act
        var result = await _service.RejectRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_REJECT_FAILED);

        // Verify
        _mockUpgradeRequestRepo.Verify(r => r.GetByIdAsync(requestId), Times.Once);
        Verify_Repo_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo, 1);

        Verify_Saved(1);
    }

    [Fact]
    public async Task RejectRequest_SystemThrowsExceptionAtGetByIdAsyncFails_ShouldReturnServerError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        // Mock fails at GetByIdAsync --> catch --> FAIL FAST
        _mockUpgradeRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.RejectRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        _mockUpgradeRequestRepo.Verify(r => r.GetByIdAsync(requestId), Times.Once);
        Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_SystemThrowsExceptionAtUpdateAsyncFails_ShouldReturnServerError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        // valid user
        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        // valid request
        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        // Mock GetByIdAsync success
        _mockUpgradeRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(validRequest);

        // Mock UpdateAsync fail
        _mockUpgradeRequestRepo.Setup(r => r.UpdateAsync(It.IsAny<UpgradeRequest>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.RejectRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        _mockUpgradeRequestRepo.Verify(r => r.GetByIdAsync(requestId), Times.Once);
        Verify_Repo_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo, 1);

        Verify_Never_Saved();
    }

    [Fact]
    public async Task RejectRequest_SystemThrowsExceptionAtSaveDbFails_ShouldReturnServerError()
    {
        // 1. Arrange
        int requestId = 99;
        int adminId = 1;

        // valid user
        var validUser = new User
        {
            Id = 5,
            UserName = "testuser"

        };

        // valid request
        var validRequest = new UpgradeRequest
        {
            Id = requestId,
            UserId = 5,
            Status = RequestStatusConst.Pending,
            Address = "123 Default Street",
            TaxCode = "1234567890",
            User = validUser
        };

        // Mock GetByIdAsync success
        _mockUpgradeRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(validRequest);

        // UpdateAsync automatically success --> no need to mock

        // Mock SaveChangesAsync fail
        _mockUnitOfWork.Setup(dbu => dbu.SaveChangesAsync())
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.RejectRequestAsync(requestId, adminId);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        _mockUpgradeRequestRepo.Verify(r => r.GetByIdAsync(requestId), Times.Once);
        Verify_Repo_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo, 1);
        Verify_Saved(1);

    }

    #endregion

    // HELPERS
    private void MockGetByIdWithUserAsync(UpgradeRequest returnedRequest)
    {
        _mockUpgradeRequestRepo.Setup(r => r.GetByIdWithUserAsync(returnedRequest.Id))
            .ReturnsAsync(returnedRequest);
    }

    private void MockValidCustomerRole(int userId)
    {
        _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
    }
}


