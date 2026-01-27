using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    /// <summary>
    /// Interface cho AdminDashboard - Quản lý menu và cấu trúc dữ liệu admin
    /// </summary>
    public interface IManagementAdminService
    {
        /// <summary>
        /// Lấy cấu trúc Menu (Modules + Types) cho từng module quản lý
        /// </summary>
        Task<ApiResponse<ManageMenuResult>> GetManageMenuAsync(ManageModuleEnum module);
    }

    public class ManagementAdminService : IManagementAdminService
    {
        private readonly IAmenityTypeRepository _amenityTypeRepo;
        private readonly IServiceTypeRepository _serviceTypeRepo;
        private readonly IPolicyTypeRepository _policyTypeRepo;
        private readonly IRoomQualityGroupRepository _roomQualityRepo;

        // Tối ưu Validation: Dùng Static HashSet để tra cứu cực nhanh (O(1))
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
            IRoomQualityGroupRepository roomQualityRepo)
        {
            _amenityTypeRepo = amenityTypeRepo;
            _serviceTypeRepo = serviceTypeRepo;
            _policyTypeRepo = policyTypeRepo;
            _roomQualityRepo = roomQualityRepo;
        }

        public async Task<ApiResponse<ManageMenuResult>> GetManageMenuAsync(ManageModuleEnum module)
        {
            // --- A. VALIDATION ĐẦU VÀO ---

            // Sử dụng chuỗi check liên hoàn. Nếu cái đầu null (OK) thì check cái sau.
            // Nếu có lỗi, biến validationError sẽ giữ lỗi đó.
            var validationError = ValidateFactory.BasicCheck(
                ValidateFactory.Require(module, x => Enum.IsDefined(typeof(ManageModuleEnum), x),
                    MessageResponse.BAD_REQUEST,
                    StatusCodeResponse.BadRequest)
            );

            // Nếu phát hiện lỗi -> Return ngay lập tức
            if (!validationError.IsValid)
            {
                return ResponseFactory.Failure<ManageMenuResult>(
                    validationError.StatusCode,
                    validationError.Message
                );
            }

            try
            {
                // Switch Case để chọn đúng Repo cho từng Module
                switch (module)
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

                    // RoomQuality đặc biệt: Nó tự là Type của chính nó
                    case ManageModuleEnum.RoomQuality:
                        return await ManagementAdminHelper.GetTypesForMenuAsync<RoomQualityGroup, IRoomQualityGroupRepository>(
                            _roomQualityRepo,
                            x => x.IsDeleted != true,
                            x => x.Id,
                            x => x.Name
                        );

                    // Các loại không có Type con (Flat Modules)
                    case ManageModuleEnum.UnitType:
                    case ManageModuleEnum.BedType:
                    case ManageModuleEnum.RoomView:
                        return ResponseFactory.Success(new ManageMenuResult(), null);

                    default:
                        return ResponseFactory.Failure<ManageMenuResult>(StatusCodeResponse.BadRequest, MessageResponse.BAD_REQUEST);
                }
            }
            catch (Exception)
            {
                return ResponseFactory.Failure<ManageMenuResult>(
                    StatusCodeResponse.Error,
                    MessageResponse.ERROR_IN_SERVER
                );
            }
        }
    }
}
