using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IManagementAdmin
{
    // API 1: Lấy cấu trúc Menu (Modules + Types)
    Task<ApiResponse<ManageMenuResult>> GetManageMenuAsync(ManageModuleEnum module);
}

public class ManagementAdmin : IManagementAdmin
{
    private readonly IAmenityTypeRepository _amenityTypeRepo;
    private readonly IServiceTypeRepository _serviceTypeRepo;
    private readonly IPolicyTypeRepository _policyTypeRepo;
    // Thay vì gọi riêng lẻ từng Manage ở các Group non-type như UnitType, BedType, RoomView, ta gọi qua nghiệp vụ RoomAttributeFacade
    private readonly IRoomQualityGroupRepository _roomQualityRepo;

    // 1. Tối ưu Validation: Dùng Static HashSet để tra cứu cực nhanh (O(1)) và không tốn RAM khởi tạo lại
    private static readonly HashSet<ManageModuleEnum> _modulesWithTypeId = new()
    {
        ManageModuleEnum.Service,
        ManageModuleEnum.Policy,
        ManageModuleEnum.Amenity,
        ManageModuleEnum.RoomQuality
    };
    public ManagementAdmin(IAmenityTypeRepository amenityTypeRepo, IServiceTypeRepository serviceTypeRepo, IPolicyTypeRepository policyTypeRepo, IRoomQualityGroupRepository roomQualityRepo)
    {
        _amenityTypeRepo = amenityTypeRepo;
        _serviceTypeRepo = serviceTypeRepo;
        _policyTypeRepo = policyTypeRepo;
        _roomQualityRepo = roomQualityRepo;
    }
    // Các phương thức quản lý chung cho Admin
    public async Task<ApiResponse<ManageMenuResult>> GetManageMenuAsync(ManageModuleEnum module)
    {
        // --- A. VALIDATION ĐẦU VÀO ---
        // Sử dụng chuỗi check liên hoàn. Nếu cái đầu null (OK) thì check cái sau.
        // Nếu có lỗi, biến validationError sẽ giữ lỗi đó.
        var validationError = ValidateFactory.BasicCheck(
            // 1. Check Module có hợp lệ trong Enum không
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
                        x => x.IsDeleted != true, // Filter
                        x => x.Id,         // Cách lấy Id
                        x => x.Name        // Cách lấy Name
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

                case ManageModuleEnum.RoomQuality:
                    // RoomQuality đặc biệt: Nó tự là Type của chính nó
                    return await ManagementAdminHelper.GetTypesForMenuAsync<RoomQualityGroup, IRoomQualityGroupRepository>(
                       _roomQualityRepo,
                       x => x.IsDeleted != true,
                       x => x.Id,
                       x => x.Name
                   );

                // Các loại không có Type con (Flat Modules) -> Trả về Menu rỗng (Chỉ có tên Module)
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