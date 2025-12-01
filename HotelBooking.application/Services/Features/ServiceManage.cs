using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;
public interface IServiceManage : ITypedManage<ServiceBaseDTO, ServiceCreateOrUpdateDTO>
{
    Task<ApiResponse<ManageServiceDTO>> GetManageServiceDataAsync(int? selectedTypeId = null);
}
public class ServiceManage : BaseManage<Service, IServiceRepository, ServiceBaseDTO, ServiceCreateOrUpdateDTO>, IServiceManage
{
    IServiceTypeRepository _svTypeRepo;
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
            ServiceTypeId = createDto.TargetTypeId,
            Additional = ServiceHelper.MapToAdditionalJson(createDto)

        };
    }

    public async Task<ApiResponse<List<ServiceBaseDTO>>> GetAllByTypeAsync(int typeId)
    {
        try
        {
            var services = await _repo.WhereAsync(sv => sv.ServiceTypeId == typeId && sv.IsDeleted == false);

            if (services == null || !services.Any())
            {
                return ResponseFactory.Failure<List<ServiceBaseDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
            }

            var result = services.Select(p => MapToDto(p)).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<ServiceBaseDTO>>();
        }
    }

    public override async Task<ApiResponse<ServiceBaseDTO>> CreateAsync(ServiceCreateOrUpdateDTO createDto)
    {
        var exists = await _repo.AnyAsync(sv => sv.Name.ToLower() == createDto.Name.ToLower() && sv.IsDeleted == false);

        if (exists) return ResponseFactory.Failure<ServiceBaseDTO>(StatusCodeResponse.Conflict, MessageResponse.NAME_ALREADY_EXISTS);

        return await base.CreateAsync(createDto);
    }

    public override async Task<ApiResponse<ServiceBaseDTO>> UpdateAsync(int id, ServiceCreateOrUpdateDTO updateDto)
    {
        // 1. Lấy entity hiện tại từ DB lên để kiểm tra
        var existingEntity = await _repo.GetByIdAsync(id);

        // Nếu không tìm thấy ID -> Trả về NotFound ngay (hoặc để base xử lý, nhưng check ở đây luôn cho tiện luồng)
        if (existingEntity == null || existingEntity.IsDeleted == true)
        {
            return ResponseFactory.Failure<ServiceBaseDTO>(StatusCodeResponse.NotFound, MessageResponse.NOT_FOUND);
        }

        // So sánh trực tiếp ID trong DB với ID mà DTO tự khai báo
        if (existingEntity.ServiceTypeId != updateDto.TargetTypeId)
        {
            return ResponseFactory.Failure<ServiceBaseDTO>(StatusCodeResponse.BadRequest, MessageResponse.BAD_REQUEST);
        }

        var exists = await _repo.AnyAsync(sv => sv.Name.ToLower() == updateDto.Name.ToLower() && sv.IsDeleted == false);

        if (exists) return ResponseFactory.Failure<ServiceBaseDTO>(StatusCodeResponse.Conflict, MessageResponse.NAME_ALREADY_EXISTS);

        return await base.UpdateAsync(id, updateDto);
    }

    public async Task<ApiResponse<ManageServiceDTO>> GetManageServiceDataAsync(int? selectedTypeId = null)
    {
        try
        {
            var types = await _svTypeRepo.GetAllAsync();

            var typeDtos = types.Select(t => new ServiceTypeDTO
            {
                Id = t.Id,
                Name = t.TypeName,
                IsDeleted = t.IsDeleted
            }).ToList();

            // 2. Xác định Type nào đang được chọn
            // Nếu user không truyền vào -> Lấy cái đầu tiên trong danh sách
            int currentTypeId = selectedTypeId ?? (typeDtos.FirstOrDefault()?.Id ?? 0);
            string? currentTypeName = typeDtos.FirstOrDefault(t => t.Id == currentTypeId)?.Name;

            // 3. Lấy danh sách Service theo Type đã chọn
            // Tận dụng hàm Where của Repo chính
            var serviceEntities = await _repo.WhereAsync(sv => sv.ServiceTypeId == currentTypeId && sv.IsDeleted == false);
            var serviceDtos = serviceEntities.Select(sv => ServiceHelper.MapToServiceDTO(sv)).ToList();

            // 4. Đóng gói vào ViewModel (MangagePolicyDTO)
            var viewModel = new ManageServiceDTO
            {
                ServiceTypes = typeDtos,
                SelectedTypeId = currentTypeId,
                SelectedTypeName = currentTypeName,
                Services = serviceDtos
            };

            return ResponseFactory.Success(viewModel, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<ManageServiceDTO>();
        }
    }
}
