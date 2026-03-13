using Moq;
using FluentAssertions;
using FluentValidation;

// 1. Using DTOs and Services from Application layer
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.application.Helpers;

// 2. Using Entities and Repo Interfaces from Infrastructure layer
using HotelBooking.infrastructure.Models;

namespace HotelBooking.Tests.Services.AdminManagement
{
    public class ManagementAdminServiceTest : BaseServiceTest
    {
        private readonly Mock<IAmenityTypeRepository> _mockAmenityTypeRepo;
        private readonly Mock<IPolicyTypeRepository> _mockPolicyTypeRepo;
        private readonly Mock<IServiceTypeRepository> _mockServiceTypeRepo;
        private readonly Mock<IRoomQualityGroupRepository> _mockRoomQualityTypeRepo;
        private readonly Mock<IValidator<ManageMenuRequest>> _mockValidator;
        private readonly ManagementAdminService _managementAdminService;

        public ManagementAdminServiceTest()
        {
            _mockAmenityTypeRepo = new Mock<IAmenityTypeRepository>();
            _mockServiceTypeRepo = new Mock<IServiceTypeRepository>();
            _mockPolicyTypeRepo = new Mock<IPolicyTypeRepository>();
            _mockRoomQualityTypeRepo = new Mock<IRoomQualityGroupRepository>();
            _mockValidator = new Mock<IValidator<ManageMenuRequest>>();
            _managementAdminService = new ManagementAdminService(
                _mockAmenityTypeRepo.Object,
                _mockServiceTypeRepo.Object,
                _mockPolicyTypeRepo.Object,
                _mockRoomQualityTypeRepo.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task GetManageMenuAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new ManageMenuRequest
            {
                Module = ManageModuleEnum.Service
            };

            _mockValidator.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _mockServiceTypeRepo.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<ServiceType>
            {
                new ServiceType { Id = 1, Name = "Service 1" },
                new ServiceType { Id = 2, Name = "Service 2" }
            });

            // Act
            var result = await _managementAdminService.GetManageMenuAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.Success);
            result.Content.Should().NotBeNull();
            result.Content.Types.Should().NotBeNull();
            result.Content.Types.Should().HaveCount(2);
            result.Content.Types.Should().BeEquivalentTo(new List<ServiceTypeDTO>
            {
                new ServiceTypeDTO { Id = 1, Name = "Service 1" },
                new ServiceTypeDTO { Id = 2, Name = "Service 2" }
            });
        }
    }
}