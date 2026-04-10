
using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.UpgradeRequest;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.RequestManagement.Customer;
using HotelBooking.infrastructure.Models;
using Moq;
using Org.BouncyCastle.Asn1.Cms;

namespace HotelBooking.test.UnitTests.Services.RequestManagement.Customer
{
    public class CustomerUpgradeRequestServiceTests : BaseServiceTest
    {
        private readonly Mock<IUpgradeRequestRepository> _mockUpgradeRequestRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IUserRoleRepository> _mockUserRoleRepo;
        private readonly Mock<IValidator<CreateUpgradeRequestDTO>> _mockCreateValidator;
        private readonly ICustomerUpgradeRequestService _service;

        public CustomerUpgradeRequestServiceTests()
        {
            _mockUpgradeRequestRepo = new Mock<IUpgradeRequestRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUserRoleRepo = new Mock<IUserRoleRepository>();
            _mockCreateValidator = new Mock<IValidator<CreateUpgradeRequestDTO>>();
            _service = new CustomerUpgradeRequestService(_mockUpgradeRequestRepo.Object, _mockUserRepo.Object, _mockUserRoleRepo.Object, _mockUnitOfWork.Object, _mockCreateValidator.Object);
        }

        #region CREATE REQUEST TESTS


        [Fact]
        public async Task CreateRequest_ValidRequest_ShouldReturnTrue()
        {
            // 1. Arrange
            var userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation success
            MockValidationSuccess();

            // Mock User --> Found
            // Mock UserRole (Customer) --> true, (Check Owner) --> false
            MockValidCustomerRole(userId);

            // Mock Pending Request
            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId)).ReturnsAsync(new List<UpgradeRequest>() { });


            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);


            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CREATED_SUCCESS);
            result.StatusCode.Should().Be(StatusCodeResponse.Success);

            _mockUpgradeRequestRepo.Verify(r => r.AddAsync(It.Is<UpgradeRequest>(req =>
                req.UserId == userId &&
                req.Address == request.Address &&
                req.TaxCode == request.TaxCode &&
                req.Status == RequestStatusConst.Pending
            )), Times.Once);
            Verify_Saved(1);
        }

        [Fact]
        public async Task CreateRequest_InvalidRequest_ShouldReturnBadRequest()
        {
            // 1. Arrange
            var userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "123456789"
            };

            // Validation failure
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("TaxCode", MessageResponse.RequestManagement.UpgradeRequest.TAX_CODE_INVALID)
            };

            // Mock validation
            _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateUpgradeRequestDTO>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

            // 2. Act 
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(validationFailures.First().ErrorMessage);
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            Verify_Repo_Never_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();

        }

        [Fact]
        public async Task CreateRequest_InvalidUserId_ShouldReturnBadRequest()
        {
            // 1. Arrange
            var userId = 0;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation
            MockValidationSuccess();

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USERID_INVALID);
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            Verify_Repo_Never_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRequest_UserNotFound_ShouldReturnNotFound()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation
            MockValidationSuccess();

            // Mock User (not found)
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                 .ReturnsAsync((User)null!);

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);

            Verify_Repo_Never_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();

        }

        [Fact]
        public async Task CreateRequest_UserNotCustomer_ShouldReturnForbidden()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation
            MockValidationSuccess();

            // Mock User
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                 .ReturnsAsync(new User());

            // Mock UserRole is not Customer Role
            _mockUserRoleRepo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ReturnsAsync(false);

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);
            result.StatusCode.Should().Be(StatusCodeResponse.Forbidden);

            Verify_Repo_Never_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRequest_UserAlreadyOwner_ShouldReturnForbidden()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation
            MockValidationSuccess();

            // Mock User
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                 .ReturnsAsync(new User());

            // Mock UserRole is not Customer Role
            _mockUserRoleRepo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ReturnsAsync(true);

            // Mock User has already Owner Role
            _mockUserRoleRepo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>())).ReturnsAsync(true);

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_ALREADY_OWNER);
            result.StatusCode.Should().Be(StatusCodeResponse.Forbidden);

            Verify_Repo_Never_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRequest_HasPendingRequest_ShouldReturnConflict()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation
            MockValidationSuccess();

            // Mock User
            // Mock UserRole is Customer and not is Owner
            MockValidCustomerRole(userId);

            // Mock pending request
            var pendingRequests = new List<UpgradeRequest>
            {
                new UpgradeRequest { Id = 99, UserId = userId, Status = RequestStatusConst.Pending }
            };

            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId)).ReturnsAsync(pendingRequests);

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();

            result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.PENDING_REQUEST_EXISTS);

            Verify_Repo_Never_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRequest_SaveDbFails_ShouldReturnError()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation
            MockValidationSuccess();

            // Mock User
            // Mock UserRole is Customer but not is Owner
            MockValidCustomerRole(userId);

            // Mock pending request --> not in pending request
            var pendingRequests = new List<UpgradeRequest>();

            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId)).ReturnsAsync(pendingRequests);

            // Mock save DB fails at try - Logic Error - using If to check
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CREATE_FAILED);

            _mockUpgradeRequestRepo.Verify(r => r.AddAsync(It.Is<UpgradeRequest>(req =>
                req.UserId == userId &&
                req.Address == request.Address &&
                req.TaxCode == request.TaxCode &&
                req.Status == RequestStatusConst.Pending
            )), Times.Once);
            Verify_Saved(1);
        }

        [Fact]
        public async Task CreateRequest_SystemThrowsExceptionAtUserRepo_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock Validation
            MockValidationSuccess();

            // Mock User - Fail Fast --> No interact with DB
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                 .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            Verify_Repo_Never_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRequest_SystemThrowsExceptionAtUserRoleRepoAtStart_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation
            MockValidationSuccess();

            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                 .ReturnsAsync(new User());

            // Mock UserRole - Fail Fast
            _mockUserRoleRepo.Setup(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>())).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            Verify_Repo_Never_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRequest_SystemThrowsExceptionAtUserRoleRepoAtEnd_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation
            MockValidationSuccess();

            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                 .ReturnsAsync(new User());

            // Mock UserRole - Fail Fast at the end
            _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ReturnsAsync(true)
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            Verify_Repo_Never_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRequest_SystemThrowsExceptionAtGettingRequest_ShoudlReturnServerError()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation
            MockValidationSuccess();

            // Mock User
            // Mock UserRole is Customer and not is Owner
            MockValidCustomerRole(userId);

            // Mock pending request --> Fail fast
            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            Verify_Repo_Never_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRequest_SystemThrowsExceptionAtAddAsync_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation 
            MockValidationSuccess();

            // Mock User
            // Mock UserRole is Customer and not is Owner
            MockValidCustomerRole(userId);

            // Mock pending request --> No pending request
            var pendingRequests = new List<UpgradeRequest>();

            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId))
            .ReturnsAsync(pendingRequests);

            // Mock AddAsync --> Fail
            _mockUpgradeRequestRepo.Setup(r => r.AddAsync(It.IsAny<UpgradeRequest>()))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            _mockUpgradeRequestRepo.Verify(r => r.AddAsync(It.Is<UpgradeRequest>(req =>
                req.UserId == userId &&
                req.Address == request.Address &&
                req.TaxCode == request.TaxCode &&
                req.Status == RequestStatusConst.Pending
            )), Times.Once);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRequest_SystemThrowsExceptionAtSaveDb_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;
            var request = new CreateUpgradeRequestDTO
            {
                Address = "123 Main St",
                TaxCode = "1234567890"
            };

            // Mock validation
            MockValidationSuccess();

            // Mock User
            // Mock UserRole is Customer and not is Owner
            MockValidCustomerRole(userId);

            // Mock pending request --> No pending request
            var pendingRequests = new List<UpgradeRequest>();

            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId))
            .ReturnsAsync(pendingRequests);

            // Mock save DB fails at catch - dbu - SaveChangesAsync
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
               .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            _mockUpgradeRequestRepo.Verify(r => r.AddAsync(It.Is<UpgradeRequest>(req =>
                req.UserId == userId &&
                req.Address == request.Address &&
                req.TaxCode == request.TaxCode &&
                req.Status == RequestStatusConst.Pending
            )), Times.Once);
            Verify_Saved(1);
        }

        #endregion

        #region CANCEL REQUEST TESTS
        [Fact]
        public async Task CancelRequest_ValidRequest_ShouldReturnTrue()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            // Mock UserRole is Customer and not is Owner
            MockValidCustomerRole(userId);

            // Mock user's pending request --> Is Pending Request
            var pendingRequests = new List<UpgradeRequest>
            {
                new UpgradeRequest { Id = 99, UserId = userId, Status = RequestStatusConst.Pending }
            };

            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId))
                .ReturnsAsync(pendingRequests);

            // Mock save DB successfully
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Success);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CANCELLED_SUCCESS);

            _mockUpgradeRequestRepo.Verify(r => r.UpdateAsync(It.Is<UpgradeRequest>(req =>
                req.Id == 99 &&
                req.Status == RequestStatusConst.Cancelled
            )), Times.Once);
            Verify_Saved(1);
        }

        [Fact]
        public async Task CancelRequest_InvalidUserId_ShouldReturnBadRequest()
        {
            // 1. Arrange
            int userId = 0;

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USERID_INVALID);
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CancelRequest_UserNotFound_ShouldReturnNotFound()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Not Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                 .ReturnsAsync((User)null!);

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);

            Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CancelRequest_UserNotCustomer_ShouldReturnForbidden()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(new User());

            // Mock UserRole is not Customer Role
            _mockUserRoleRepo.Setup(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ReturnsAsync(false);

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);
            result.StatusCode.Should().Be(StatusCodeResponse.Forbidden);

            Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CancelRequest_UserAlreadyOwner_ShouldReturnForbidden()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(new User());

            // Mock UserRole is already an Owner
            _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ReturnsAsync(true)
                .ReturnsAsync(true);

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_ALREADY_OWNER);
            result.StatusCode.Should().Be(StatusCodeResponse.Forbidden);

            Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();

        }

        [Fact]
        public async Task CancelRequest_NoPendingRequest_ShouldReturnNotFound()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            // Mock UserRole is Customer and not is Owner
            MockValidCustomerRole(userId);

            // Mock user's pending request --> No Pending Request
            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId))
                .ReturnsAsync(new List<UpgradeRequest>());


            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_NOT_FOUND);

            Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CancelRequest_SaveDbFails_ShouldReturnError()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            // Simulate an existing user to pass the initial validation.

            // Mock UserRole is Customer and not is Owner
            // Ensure the user has the 'Customer' role and is not already an 'Owner', 
            // granting them permission to cancel the request.
            MockValidCustomerRole(userId);

            // Mock user's pending request --> Is Pending request
            // Provide a valid, 'Pending' upgrade request belonging to this user 
            // that is eligible for cancellation.
            var pendingRequests = new List<UpgradeRequest>
            {
                new UpgradeRequest { Id = 99, UserId = userId, Status = RequestStatusConst.Pending }
            };

            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId)).ReturnsAsync(pendingRequests);

            // Mock dbu fail at Try - SaveChanges <= 0
            // Force a database commit failure (returns 0 affected rows) 
            // to trigger the database error handling block.
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(0);

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CANCEL_FAILED);

            _mockUpgradeRequestRepo.Verify(r => r.UpdateAsync(It.Is<UpgradeRequest>(req =>
                req.Id == 99 &&
                req.Status == RequestStatusConst.Cancelled
            )), Times.Once);
            Verify_Saved(1);
        }

        [Fact]
        public async Task CancelRequest_SystemThrowsExceptionAtUserRepo_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User - Fail Fast --> No interact with DB
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CancelRequest_SystemThrowsExceptionAtUserRoleRepoAtStart_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(new User());

            // Mock UserRole - Fail Fast
            _mockUserRoleRepo.Setup(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CancelRequest_SystemThrowsExceptionAtUserRoleRepoAtEnd_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(new User());

            // Mock UserRole --> Is Customer but fail at the second time
            _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ReturnsAsync(true)
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CancelRequest_SystemThrowsExceptionAtGettingRequest_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            // Mock UserRole
            MockValidCustomerRole(userId);

            // Mock pending request --> Fail fast
            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            Verify_Repo_Never_UpdateAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CancelRequest_SystemThrowsExceptionAtUpdateAsync_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            // Mock UserRole
            MockValidCustomerRole(userId);

            // Mock pending request --> Is Pending Request
            var pendingRequests = new List<UpgradeRequest>
            {
                new UpgradeRequest { Id = 99, UserId = userId, Status = RequestStatusConst.Pending }
            };

            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId))
                .ReturnsAsync(pendingRequests);

            // Mock UpdateAsync Fail
            _mockUpgradeRequestRepo.Setup(r => r.UpdateAsync(It.IsAny<UpgradeRequest>()))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            _mockUpgradeRequestRepo.Verify(r => r.UpdateAsync(It.Is<UpgradeRequest>(req =>
                req.Id == 99 &&
                req.Status == RequestStatusConst.Cancelled
            )), Times.Once);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CancelRequest_SystemThrowsExceptionAtSaveDb_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            // Mock UserRole
            MockValidCustomerRole(userId);

            // Mock pending request --> Is Pending Request)
            var pendingRequests = new List<UpgradeRequest>()
            {
                new UpgradeRequest { Id = 99, UserId = userId, Status = RequestStatusConst.Pending }
            };
            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId))
                .ReturnsAsync(pendingRequests);

            // Mock save DB fails at catch - dbu - SaveChangesAsync
            _mockUnitOfWork.Setup(dbu => dbu.SaveChangesAsync())
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CancelRequestAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            _mockUpgradeRequestRepo.Verify(r => r.UpdateAsync(It.Is<UpgradeRequest>(req =>
                req.Id == 99 &&
                req.Status == RequestStatusConst.Cancelled
            )), Times.Once);
            Verify_Saved(1);
        }

        #endregion

        #region GET MY REQUEST
        [Fact]
        public async Task GetMyRequest_ValidRequest_ShouldReturnSuccessAndData()
        {
            // 1. Arrange
            int userId = 1;

            MockValidCustomerRole(userId);

            // Mock user's requests
            var requestsFromDb = new List<UpgradeRequest>
            {
                new UpgradeRequest { Id = 1, UserId = userId, Status = RequestStatusConst.Pending, RequestedAt = DateTime.Now.AddDays(-2), User = new User { UserName = "user1" } },
                new UpgradeRequest { Id = 2, UserId = userId, Status = RequestStatusConst.Approved, RequestedAt = DateTime.Now, User = new User { UserName = "user1" } }
            };

            _mockUpgradeRequestRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(requestsFromDb);

            // 2. Act
            var result = await _service.GetMyRequestsAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Success);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUESTS_RETRIEVED);

            result.Content.Should().NotBeNull();
            result.Content.Should().HaveCount(2); // requestsFromDb has 2 records
            result.Content.First().RequestId.Should().Be(2); // OrderByDescending()


        }

        [Fact]
        public async Task GetMyRequest_InvalidUserId_ShouldReturnBadRequest()
        {
            // 1. Arrange
            int userId = 0;

            // 2. Act
            var result = await _service.GetMyRequestsAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USERID_INVALID);

            // Make sure that no query interact with DB
            _mockUserRepo.Verify(u => u.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetMyRequest_UserNotFound_ShouldReturnNotFound()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Not Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync((User)null!);

            // 2. Act
            var result = await _service.GetMyRequestsAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

            // Make sure that no query at userRoleRepo interact with DB
            _mockUserRoleRepo.Verify(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task GetMyRequest_UserNotCustomer_ShouldReturnForbidden()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(new User());

            // Mock UserRole is not Customer Role
            _mockUserRoleRepo.Setup(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ReturnsAsync(false);

            // 2. Act
            var result = await _service.GetMyRequestsAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Forbidden);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);

            // Make sure that no query at upgradeRequestRepo query interact with DB
            _mockUpgradeRequestRepo.Verify(r => r.GetByUserIdAsync(It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public async Task GetMyRequest_UserAlreadyOwner_ShouldReturnForbidden()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(new User());

            // Mock UserRole is already an Owner
            _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ReturnsAsync(true)
                .ReturnsAsync(true);

            // 2. Act
            var result = await _service.GetMyRequestsAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Forbidden);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.USER_ALREADY_OWNER);

            // Make sure that no query at upgradeRequestRepo query interact with DB
            _mockUpgradeRequestRepo.Verify(r => r.GetByUserIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetMyRequest_NoRequest_ShouldReturnSuccessWithEmptyList()
        {
            // 1. Arrange
            int userId = 1;

            // Mock User --> Found and UserRole valid
            MockValidCustomerRole(userId);

            // Mock user's requests --> Empty List
            _mockUpgradeRequestRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(new List<UpgradeRequest>());

            // 2. Act
            var result = await _service.GetMyRequestsAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Success);
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.NO_REQUESTS_FOUND);

            // Make sure that Content is an empty list, not null
            result.Content.Should().NotBeNull();
            result.Content.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMyRequest_SystemThrowsExceptionAtUserRepo_ShouldReturnServerError()
        {
            // 1. Arrange
            int userId = 1;

            // Mock user FAIL FAST --> NO INTERACT WITH DB
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.GetMyRequestsAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            // Make sure that userRole and upgradeRequest's queries no interact with DB
            _mockUserRoleRepo.Verify(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()), Times.Never);
            _mockUpgradeRequestRepo.Verify(r => r.GetByUserIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetMyRequest_SystemThrowsExceptionAtUserRoleRepoAtStart_ShouldReturnServerError()
        {
            // 1. Act
            int userId = 1;

            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(new User());

            // Mock UserRole - Fail Fast
            _mockUserRoleRepo.Setup(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.GetMyRequestsAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            // Make sure that userRole'query is called once only
            _mockUserRoleRepo.Verify(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()), Times.Once);

            // Make sure that upgradeRequest's query no interact with DB
            _mockUpgradeRequestRepo.Verify(r => r.GetByUserIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetMyRequest_SystemThrowsExceptionAtUserRoleRepoAtEnd_ShouldReturnServerError()
        {
            // 1. Act
            int userId = 1;

            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(new User());

            // Mock UserRole --> Is Customer
            _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ReturnsAsync(true)
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.GetMyRequestsAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            // Make sure that userRole'query is called twice
            _mockUserRoleRepo.Verify(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()), Times.Exactly(2));

            // Make sure that upgradeRequest's query not interact with DB
            _mockUpgradeRequestRepo.Verify(u => u.GetByUserIdAsync(It.IsAny<int>()), Times.Never);

        }

        [Fact]
        public async Task GetMyRequest_SystemThrowsExceptionAtGettingRequest_ShouldReturnServerError()
        {
            // 1. Act
            int userId = 1;

            // Mock User --> Found and Mock userRole is valid
            MockValidCustomerRole(userId);

            // Mock pending request --> Fail fast
            _mockUpgradeRequestRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.GetMyRequestsAsync(userId);

            // 3. Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

            // Make sure that upgradeRequest's query is called once
            _mockUpgradeRequestRepo.Verify(r => r.GetByUserIdAsync(It.IsAny<int>()), Times.Once);
        }



        #endregion

        // HELPERS
        private void MockValidationSuccess()
        {
            _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateUpgradeRequestDTO>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        }

        private void MockValidCustomerRole(int userId)
        {
            // Mock User --> Found
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(new User());

            // Mock UserRole --> Is Customer, NOT Owner
            _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
        }
    }
}