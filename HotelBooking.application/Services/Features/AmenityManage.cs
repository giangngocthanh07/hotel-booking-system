using System.Text.Json;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IAmenityManage : ITypedManage<AmenityDTO, AmenityTypeDTO, AmenityCreateOrUpdateDTO>
{
    Task<ApiResponse<ManageDataResult<AmenityDTO>>> GetAmenitiesByTypeAsync(int? typeId);
}

public class AmenityManage : BaseManage<Amenity, IAmenityRepository, AmenityDTO, AmenityCreateOrUpdateDTO>, IAmenityManage
{
    private readonly IAmenityTypeRepository _amenityTypeRepo;
    public AmenityManage(IAmenityRepository repository, IAmenityTypeRepository amenityTypeRepository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
        _amenityTypeRepo = amenityTypeRepository;
    }


    protected override AmenityDTO MapToDto(Amenity entity)
    {
        var additional = JsonSerializer.Deserialize<Dictionary<string, string?>>(entity.Additional ?? "{}") ?? new Dictionary<string, string?>();

        return new AmenityDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = additional.GetValueOrDefault("Description", null),
            IsDeleted = entity.IsDeleted,
            TypeId = entity.TypeId
        };
    }

    protected override void MapToEntity(AmenityCreateOrUpdateDTO updateDto, Amenity entity)
    {
        entity.Name = updateDto.Name;
        entity.Additional = JsonSerializer.Serialize(new
        {
            Description = string.IsNullOrWhiteSpace(updateDto.Description) ? null : updateDto.Description,
        });
    }

    protected override Amenity MapToEntity(AmenityCreateOrUpdateDTO createDto)
    {
        var additional = JsonSerializer.Serialize(new
        { Description = createDto.Description });

        return new Amenity
        {
            Name = createDto.Name,
            Additional = additional,
            IsDeleted = false,
            TypeId = createDto.TypeId
        };
    }

    // Validation
    protected override async Task<ValidationResult> ValidateAsync(AmenityCreateOrUpdateDTO dto, int? id = null)
    {
        var basicValidation = ValidateFactory.ValidateFullAsync<Amenity>(
            _repo,
            dto.Name,
            id,
            dto.TypeId,
            getEntityIsDeletedFunc: x => x.IsDeleted,
            isDeletedSelector: x => x.IsDeleted,
            nameSelector: x => x.Name);
        return await basicValidation;
    }

    public async Task<ApiResponse<List<AmenityTypeDTO>>> GetTypeDataAsync()
    {
        try
        {
            var amenityTypes = await _amenityTypeRepo.WhereAsync(a => a.IsDeleted == false);

            if (amenityTypes == null || !amenityTypes.Any())
            {
                return ResponseFactory.Failure<List<AmenityTypeDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
            }

            // 2. Map sang DTO và xử lý JSON cho TỪNG item
            var result = amenityTypes.Select(a =>
            {
                // A. Giải mã JSON của từng dòng (Additional)
                // Nếu null hoặc rỗng -> Tạo Dictionary rỗng để tránh lỗi
                var additionalData = string.IsNullOrWhiteSpace(a.Additional)
                    ? new Dictionary<string, string?>()
                    : JsonSerializer.Deserialize<Dictionary<string, string?>>(a.Additional)
                      ?? new Dictionary<string, string?>();

                // B. Trả về DTO đã map dữ liệu
                return new AmenityTypeDTO
                {
                    Id = a.Id,
                    Name = a.Name,
                    IsDeleted = a.IsDeleted,

                    // C. Lấy value từ Dictionary (dùng GetValueOrDefault cho an toàn)
                    IconClass = additionalData.GetValueOrDefault("IconClass"),
                    IconColor = additionalData.GetValueOrDefault("IconColor")
                };
            }).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<AmenityTypeDTO>>();
        }
    }

    public async Task<ApiResponse<ManageDataResult<AmenityDTO>>> GetAmenitiesByTypeAsync(int? typeId)
    {
        return await ManagementAdminHelper.GetDataByTypeAsync<Amenity, AmenityDTO>(
            typeId,
            // Logic 1: lấy ID mặc định: Query bảng ServiceType, lấy thằng đầu tiên chưa xóa
            getDefaultIdFunc: async () =>
            {
                // query DB lấy 1 dòng
                var firstType = (await _amenityTypeRepo.WhereAsync(x => x.IsDeleted != true)).FirstOrDefault();
                return firstType?.Id;
            },
            // Logic 2: kiểm tra ID tồn tại: Query bảng AmenityType
            checkTypeExistsFunc: async (id) =>
            {
                // Kiểm tra xem có dòng nào có Id này và chưa bị xóa không
                var exists = (await _amenityTypeRepo.WhereAsync(x => x.Id == id && x.IsDeleted != true)).Any();
                return exists;
            },
            // Logic 3: Lấy Entity từ DB
            getItemsByTypeIdFunc: async (id) => await _repo.WhereAsync(sv => sv.TypeId == id && sv.IsDeleted == false),
            // Logic 4: Map sang DTO (Tái sử dụng hàm MapToDto có sẵn)
            mapToDtoFunc: MapToDto
        );
    }
}