using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    /// <summary>
    /// Interface for AdminDashboard — manages admin menu and data structure
    /// </summary>
    public interface IManagementAdminService
    {
        /// <summary>
        /// Retrieve menu structure (Modules + Types) for each management module
        /// </summary>
        Task<ApiResponse<ManageMenuResult>> GetManageMenuAsync(ManageMenuRequest request);
    }

    public class ManagementAdminService : IManagementAdminService
    {
        private readonly IAmenityTypeRepository _amenityTypeRepo;
        private readonly IServiceTypeRepository _serviceTypeRepo;
        private readonly IPolicyTypeRepository _policyTypeRepo;
        private readonly IRoomQualityGroupRepository _roomQualityRepo;

        private readonly IValidator<ManageMenuRequest> _validator;

        // Validation optimization: static HashSet for O(1) lookup
        private static readonly HashSet<ManageModuleEnum> _modulesWithTypeId = new()
        {
            ManageModuleEnum.Service,
            ManageModuleEnum.Policy,
            ManageModuleEnum.Amenity,
            ManageModuleEnum.RoomQuality
        };

        public ManagementAdminService(
            IAmenityTypeRepository amenityTypeRepo,
            IServiceTypeRepository serviceTypeRepo,
            IPolicyTypeRepository policyTypeRepo,
            IRoomQualityGroupRepository roomQualityRepo,
            IValidator<ManageMenuRequest> validator)
        {
            _amenityTypeRepo = amenityTypeRepo;
            _serviceTypeRepo = serviceTypeRepo;
            _policyTypeRepo = policyTypeRepo;
            _roomQualityRepo = roomQualityRepo;
            _validator = validator;
        }

        public async Task<ApiResponse<ManageMenuResult>> GetManageMenuAsync(ManageMenuRequest request)
        {
            // --- A. INPUT VALIDATION ---

            // Chained null-coalescing check: if the first is null (OK), check the next.
            // If an error is found, the validationError variable will hold it.
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return ResponseFactory.Failure<ManageMenuResult>(
                    StatusCodeResponse.BadRequest,
                    validationResult.Errors[0].ErrorMessage
                );
            }

            try
            {
                // Switch-case to select the correct Repo for each Module
                switch (request.Module)
                {
                    case ManageModuleEnum.Service:
                        return await ManagementAdminHelper.GetTypesForMenuAsync<ServiceType, IServiceTypeRepository>(
                            _serviceTypeRepo,
                            x => x.IsDeleted != true,
                            x => x.Id,
                            x => x.Name
                        );

                    case ManageModuleEnum.Policy:
                        return await ManagementAdminHelper.GetTypesForMenuAsync<PolicyType, IPolicyTypeRepository>(
                            _policyTypeRepo,
                            x => x.IsDeleted != true,
                            x => x.Id,
                            x => x.Name
                        );

                    case ManageModuleEnum.Amenity:
                        return await ManagementAdminHelper.GetTypesForMenuAsync<AmenityType, IAmenityTypeRepository>(
                            _amenityTypeRepo,
                            x => x.IsDeleted != true,
                            x => x.Id,
                            x => x.Name
                        );

                    // RoomQuality is special: it acts as its own Type
                    case ManageModuleEnum.RoomQuality:
                        return await ManagementAdminHelper.GetTypesForMenuAsync<RoomQualityGroup, IRoomQualityGroupRepository>(
                            _roomQualityRepo,
                            x => x.IsDeleted != true,
                            x => x.Id,
                            x => x.Name
                        );

                    // Modules without sub-types (Flat Modules)
                    case ManageModuleEnum.UnitType:
                    case ManageModuleEnum.BedType:
                    case ManageModuleEnum.RoomView:
                        return ResponseFactory.Success(new ManageMenuResult(), null);

                    default:
                        return ResponseFactory.Failure<ManageMenuResult>(StatusCodeResponse.BadRequest, MessageResponse.Common.BAD_REQUEST);
                }
            }
            catch (Exception)
            {
                return ResponseFactory.Failure<ManageMenuResult>(
                    StatusCodeResponse.Error,
                    MessageResponse.Common.ERROR_IN_SERVER
                );
            }
        }
    }
}
