
using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.UpgradeRequest;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.RequestManagement.Customer;
using HotelBooking.infrastructure.Models;
using Moq;

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

            // Mock User
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync(new User());

            // Mock UserRole (Customer) --> true, (Check Owner) --> false
            _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                     .ReturnsAsync(true)
                     .ReturnsAsync(false);

            // Mock Pending Request
            _mockUpgradeRequestRepo.Setup(r => r.GetPendingByIdAsync(userId)).ReturnsAsync(new List<UpgradeRequest>() { });


            // 2. Act
            var result = await _service.CreateRequestAsync(userId, request);


            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CREATED_SUCCESS);
            result.StatusCode.Should().Be(StatusCodeResponse.Success);

            Verify_Repo_AddAsync<IUpgradeRequestRepository, UpgradeRequest>(_mockUpgradeRequestRepo, 1);
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

            // Mock validation failure
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("TaxCode", MessageResponse.RequestManagement.UpgradeRequest.TAX_CODE_INVALID)
            };


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
            _mockUserRepo.Setup(u => u.GetByIdAsync(userId))
                 .ReturnsAsync(new User());

            // Mock UserRole is Customer and not is Owner
            _mockUserRoleRepo.SetupSequence(ur => ur.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>()))
                     .ReturnsAsync(true)
                     .ReturnsAsync(false);

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
        }

        private void MockValidationSuccess()
        {
            _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateUpgradeRequestDTO>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        }
    }
}