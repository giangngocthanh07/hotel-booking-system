using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IRoomQualityService : ITypedManage<RoomQualityDTO, RoomQualityGroupDTO, RoomQualityCreateDTO, RoomQualityUpdateDTO>
{
    Task<ApiResponse<List<RoomQualityDTO>>> GetAllByTypeAsync(int? typeId = null);
    Task<ApiResponse<PagedManageResult<RoomQualityDTO>>> GetRoomQualitiesByTypeAsync(int? typeId, PagingRequest paging);
}

public class RoomQualityService : BaseManage<RoomQuality, IRoomQualityRepository, RoomQualityDTO, RoomQualityCreateDTO, RoomQualityUpdateDTO>, IRoomQualityService
{
    private readonly IRoomQualityGroupRepository _roomQualityTypeRepo;
    public RoomQualityService(IRoomQualityRepository repository, IUnitOfWork dbo, IRoomQualityGroupRepository roomQualityTypeRepo, IValidator<RoomQualityCreateDTO> createVal,
            IValidator<RoomQualityUpdateDTO> updateVal) : base(repository, dbo, createVal, updateVal)
    {
        _roomQualityTypeRepo = roomQualityTypeRepo;
    }

    protected override RoomQualityDTO MapToDto(RoomQuality entity)
    {
        return new RoomQualityDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder ?? 0,
            IsDeleted = entity.IsDeleted,
            TypeId = entity.TypeId
        };
    }

    protected override void MapUpdateToEntity(RoomQualityUpdateDTO updateDto, RoomQuality entity)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
        entity.SortOrder = updateDto.SortOrder;
    }

    protected override RoomQuality MapCreateToEntity(RoomQualityCreateDTO createDto)
    {
        return new RoomQuality
        {
            Name = createDto.Name,
            Description = createDto.Description,
            SortOrder = createDto.SortOrder,
            IsDeleted = false,
            TypeId = createDto.TypeId
        };
    }

    // Validation
    protected override async Task<ValidationResult> ValidateCreateLogicAsync(RoomQualityCreateDTO dto)
    {
        // Check trùng tên TRONG CÙNG NHÓM (TypeId)
        bool exists = await _repo.AnyAsync(x =>
            x.Name == dto.Name &&
            x.TypeId == dto.TypeId);

        if (exists) return ValidationResult.Fail(MessageResponse.AdminManagement.Amenity.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    protected override async Task<ValidationResult> ValidateUpdateLogicAsync(RoomQualityUpdateDTO dto, int id)
    {
        // Lấy Entity gốc để biết nó đang thuộc nhóm nào
        var currentEntity = await _repo.GetByIdAsync(id);
        if (currentEntity == null) return ValidationResult.Success();

        // Check trùng tên (Dùng TypeId gốc từ DB)
        bool exists = await _repo.AnyAsync(x =>
            x.Name == dto.Name &&
            x.TypeId == currentEntity.TypeId && // Lấy TypeId từ DB
            x.Id != id &&
            x.IsDeleted == false);

        if (exists)
            return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }


    // Hàm này dùng cho RoomAttributeFacade.cs
    public async Task<ApiResponse<List<RoomQualityDTO>>> GetAllByTypeAsync(int? typeId = null)
    {

        try
        {
            if (!typeId.HasValue)
            {
                var types = await _roomQualityTypeRepo.WhereAsync(rq => rq.IsDeleted == false);
                if (types == null || !types.Any())
                {
                    return ResponseFactory.Failure<List<RoomQualityDTO>>(StatusCodeResponse.NotFound, MessageResponse.Common.EMPTY_LIST);
                }

                typeId = types.First().Id;
            }

            var roomQualities = await _repo.WhereAsync(rq => rq.TypeId == typeId && rq.IsDeleted == false);

            if (roomQualities == null || !roomQualities.Any())
            {
                return ResponseFactory.Failure<List<RoomQualityDTO>>(StatusCodeResponse.NotFound, MessageResponse.Common.EMPTY_LIST);
            }

            var result = roomQualities.Select(rq => MapToDto(rq)).ToList();

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<RoomQualityDTO>>();

        }
    }

    public async Task<ApiResponse<List<RoomQualityGroupDTO>>> GetTypeDataAsync()
    {
        try
        {
            var rqTypes = await _roomQualityTypeRepo.WhereAsync(rq => rq.IsDeleted == false);

            if (rqTypes == null || !rqTypes.Any())
            {
                return ResponseFactory.Failure<List<RoomQualityGroupDTO>>(StatusCodeResponse.NotFound, MessageResponse.Common.EMPTY_LIST);
            }

            var result = rqTypes.Select(rq => new RoomQualityGroupDTO
            {
                Id = rq.Id,
                Name = rq.Name,
                SortOrder = rq.SortOrder,
                IsDeleted = rq.IsDeleted
            }).ToList();

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<RoomQualityGroupDTO>>();
        }
    }

    // Hàm này dùng cho ManagementAdmin.cs
    public async Task<ApiResponse<PagedManageResult<RoomQualityDTO>>> GetRoomQualitiesByTypeAsync(int? typeId, PagingRequest paging)
    {
        return await ManagementAdminHelper.GetDataByTypeAsync<RoomQuality, RoomQualityDTO>(
            typeId,
            paging,

            // Logic 1: lấy ID mặc định: Query bảng RoomQualityGroup, lấy thằng đầu tiên chưa xóa
            getDefaultIdFunc: async () =>
            {
                // query DB lấy 1 dòng
                var firstType = (await _roomQualityTypeRepo.WhereAsync(x => x.IsDeleted != true)).FirstOrDefault();
                return firstType?.Id;
            },
            // Logic 2: kiểm tra ID tồn tại: Query bảng RoomQualityGroup
            checkTypeExistsFunc: async (id) =>
            {
                // Kiểm tra xem có dòng nào có Id này và chưa bị xóa không
                var exists = (await _roomQualityTypeRepo.WhereAsync(x => x.Id == id && x.IsDeleted != true)).Any();
                return exists;
            },
            // Logic 3: Lấy Entity từ DB
            getPagedItemsFunc: async (id, page, size) =>
            await _repo.GetPagedAsync(
                x => x.TypeId == id && x.IsDeleted == false,
                page,
                size,
                q => q.OrderByDescending(x => x.Id)),
            // Logic 4: Map sang DTO (Tái sử dụng hàm MapToDto có sẵn)
            mapToDtoFunc: MapToDto
        );
    }


}