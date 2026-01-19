using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;
public interface IServiceManage : ITypedManage<ServiceBaseDTO, ServiceTypeDTO, ServiceCreateOrUpdateDTO>
{
    Task<ApiResponse<PagedManageResult<ServiceBaseDTO>>> GetServicesByTypeAsync(int? typeId, PagingRequest paging);
}
public class ServiceManage : BaseManage<Service, IServiceRepository, ServiceBaseDTO, ServiceCreateOrUpdateDTO>, IServiceManage
{
    private readonly IServiceTypeRepository _svTypeRepo;
    public ServiceManage(IServiceRepository repository, IUnitOfWork dbo, IServiceTypeRepository svTypeRepo) : base(repository, dbo)
    {
        _svTypeRepo = svTypeRepo;
    }

    protected override ServiceBaseDTO MapToDto(Service entity)
    {
        var dto = ServiceHelper.MapToServiceDTO(entity);
        return dto!;
    }

    protected override void MapToEntity(ServiceCreateOrUpdateDTO updateDto, Service entity)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
        entity.Additional = ServiceHelper.MapToAdditionalJson(updateDto, entity.Additional);
    }

    protected override Service MapToEntity(ServiceCreateOrUpdateDTO createDto)
    {
        return new Service
        {
            Name = createDto.Name,
            Description = createDto.Description,
            TypeId = createDto.TargetTypeId,
            Additional = ServiceHelper.MapToAdditionalJson(createDto)

        };
    }

    // Validation
    protected override async Task<ValidationResult> ValidateAsync(ServiceCreateOrUpdateDTO dto, int? id = null)
    {
        var basicValidation = ValidateFactory.ValidateFullAsync<Service>(
            _repo,
            dto.Name,
            id,
            typeId: dto.TargetTypeId,
            getEntityIsDeletedFunc: x => x.IsDeleted,
            isDeletedSelector: x => x.IsDeleted,
            nameSelector: x => x.Name);
        return await basicValidation;
    }

    public async Task<ApiResponse<List<ServiceTypeDTO>>> GetTypeDataAsync()
    {
        try
        {
            var svTypes = await _svTypeRepo.WhereAsync(sv => sv.IsDeleted == false);

            if (svTypes == null || !svTypes.Any())
            {
                return ResponseFactory.Failure<List<ServiceTypeDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
            }

            var result = svTypes.Select(sv => new ServiceTypeDTO
            {
                Id = sv.Id,
                Name = sv.Name,
                IsDeleted = sv.IsDeleted
            }).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<ServiceTypeDTO>>();
        }
    }

    public async Task<ApiResponse<PagedManageResult<ServiceBaseDTO>>> GetServicesByTypeAsync(int? typeId, PagingRequest paging)
    {
        return await ManagementAdminHelper.GetDataByTypeAsync<Service, ServiceBaseDTO>(
            typeId,
            paging,

            // Logic 1: lấy ID mặc định: Query bảng ServiceType, lấy thằng đầu tiên chưa xóa
            getDefaultIdFunc: async () =>
            {
                // query DB lấy 1 dòng
                var firstType = (await _svTypeRepo.WhereAsync(x => x.IsDeleted != true)).FirstOrDefault();
                return firstType?.Id;
            },
            // Logic 2: kiểm tra ID tồn tại: Query bảng ServiceType
            checkTypeExistsFunc: async (id) =>
            {
                // Kiểm tra xem có dòng nào có Id này và chưa bị xóa không
                var exists = (await _svTypeRepo.WhereAsync(x => x.Id == id && x.IsDeleted != true)).Any();
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
