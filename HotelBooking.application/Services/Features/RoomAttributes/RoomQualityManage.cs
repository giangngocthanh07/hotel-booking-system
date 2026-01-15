using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IRoomQualityManage : ITypedManage<RoomQualityDTO, RoomQualityGroupDTO, RoomQualityCreateOrUpdateDTO>
{
    Task<ApiResponse<List<RoomQualityDTO>>> GetAllByTypeAsync(int? typeId = null);
    Task<ApiResponse<ManageDataResult<RoomQualityDTO>>> GetRoomQualitiesByTypeAsync(int? typeId);
}

public class RoomQualityManage : BaseManage<RoomQuality, IRoomQualityRepository, RoomQualityDTO, RoomQualityCreateOrUpdateDTO>, IRoomQualityManage
{
    private readonly IRoomQualityGroupRepository _roomQualityTypeRepo;
    public RoomQualityManage(IRoomQualityRepository repository, IUnitOfWork dbo, IRoomQualityGroupRepository roomQualityTypeRepo) : base(repository, dbo)
    {
        _roomQualityTypeRepo = roomQualityTypeRepo;
    }

    protected override RoomQualityDTO MapToDto(RoomQuality entity)
    {
        return new RoomQualityDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            SortOrder = entity.SortOrder,
            IsDeleted = entity.IsDeleted,
            TypeId = entity.TypeId
        };
    }

    protected override void MapToEntity(RoomQualityCreateOrUpdateDTO updateDto, RoomQuality entity)
    {
        entity.Name = updateDto.Name;
        entity.SortOrder = updateDto.SortOrder;
    }

    protected override RoomQuality MapToEntity(RoomQualityCreateOrUpdateDTO createDto)
    {
        return new RoomQuality
        {
            Name = createDto.Name,
            SortOrder = createDto.SortOrder,
            IsDeleted = false,
            TypeId = createDto.TypeId

        };
    }

    // Validation
    protected override async Task<ValidationResult> ValidateAsync(RoomQualityCreateOrUpdateDTO dto, int? id = null)
    {
        var basicValidation = ValidateFactory.ValidateFullAsync<RoomQuality>(
            _repo,
            dto.Name,
            id,
            dto.TypeId,
            getEntityIsDeletedFunc: x => x.IsDeleted,
            isDeletedSelector: x => x.IsDeleted,
            nameSelector: x => x.Name);
        return await basicValidation;
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
                    return ResponseFactory.Failure<List<RoomQualityDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
                }

                typeId = types.First().Id;
            }

            var roomQualities = await _repo.WhereAsync(rq => rq.TypeId == typeId && rq.IsDeleted == false);

            if (roomQualities == null || !roomQualities.Any())
            {
                return ResponseFactory.Failure<List<RoomQualityDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
            }

            var result = roomQualities.Select(rq => MapToDto(rq)).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
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
                return ResponseFactory.Failure<List<RoomQualityGroupDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
            }

            var result = rqTypes.Select(rq => new RoomQualityGroupDTO
            {
                Id = rq.Id,
                Name = rq.Name,
                SortOrder = rq.SortOrder,
                IsDeleted = rq.IsDeleted
            }).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<RoomQualityGroupDTO>>();
        }
    }

    // Hàm này dùng cho ManagementAdmin.cs
    public async Task<ApiResponse<ManageDataResult<RoomQualityDTO>>> GetRoomQualitiesByTypeAsync(int? typeId)
    {
        return await ManagementAdminHelper.GetDataByTypeAsync<RoomQuality, RoomQualityDTO>(
            typeId,
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
            getItemsByTypeIdFunc: async (id) => await _repo.WhereAsync(sv => sv.TypeId == id && sv.IsDeleted == false),
            // Logic 4: Map sang DTO (Tái sử dụng hàm MapToDto có sẵn)
            mapToDtoFunc: MapToDto
        );
    }


}