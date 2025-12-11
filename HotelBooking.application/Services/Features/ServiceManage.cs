using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;
public interface IServiceManage : ITypedManage<ServiceBaseDTO, ServiceCreateOrUpdateDTO>
{
    Task<ApiResponse<ManageServiceDTO>> GetManageServiceDataAsync(int? selectedTypeId = null);
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
            ServiceTypeId = createDto.TargetTypeId,
            Additional = ServiceHelper.MapToAdditionalJson(createDto)

        };
    }

    // Validation
    protected override async Task<ValidationResult> ValidateAsync(ServiceCreateOrUpdateDTO dto, int? id = null)
    {
        // Cấu trúc chuỗi kiểm tra (Chain of Responsibility)
        // Nó sẽ chạy từ trái sang phải, gặp cái nào lỗi (khác null) là return ngay lập tức.

        var basicCheck =
            ValidateUtils.RequireNotEmpty(dto.Name, MessageResponse.EMPTY_NAME, StatusCodeResponse.BadRequest) ??
            ValidateUtils.Require(dto.TargetTypeId > 0, MessageResponse.BAD_REQUEST, StatusCodeResponse.BadRequest);

        // Nếu các check cơ bản đã có lỗi -> Return luôn, khỏi cần check DB cho tốn thời gian
        if (basicCheck != null) return basicCheck;

        // --- CHECK DB (Logic Check trùng) ---
        // Phần Async ta nên tách ra check riêng sau khi check cơ bản đã OK

        // --- 2. VALIDATE NGHIỆP VỤ (Cần DB) ---

        // A. Xử lý riêng cho trường hợp UPDATE
        if (id.HasValue) // Tương đương: if (id != null)
        {
            // Phải query lấy entity cũ lên để so sánh
            // Lưu ý: EF Core có cơ chế Cache, nên việc query ở đây và query lại ở hàm UpdateAsync 
            // thường không ảnh hưởng đáng kể hiệu năng (nó lấy từ bộ nhớ đệm).
            var existingEntity = await _repo.GetByIdAsync(id.Value);

            // Kiểm tra tồn tại
            // Nếu existingEntity null -> Trả về 404 NotFound ngay lập tức
            var foundCheck = ValidateUtils.RequireFound(existingEntity, MessageResponse.NOT_FOUND, StatusCodeResponse.NotFound);
            if (foundCheck != null) return foundCheck;

            // Nếu entity bị deleted == true -> Trả về lỗi NotFound
            if (existingEntity.IsDeleted == true)
            {
                return ValidationResult.Fail(MessageResponse.NOT_FOUND, StatusCodeResponse.NotFound);
            }

            // [LOGIC BẠN CẦN]: Kiểm tra xem User có cố tình đổi Loại dịch vụ không?
            if (existingEntity?.ServiceTypeId != dto.TargetTypeId)
            {
                return ValidationResult.Fail(MessageResponse.BAD_REQUEST, StatusCodeResponse.BadRequest);
            }
        }

        // B. Xử lý Check trùng tên (Áp dụng cho cả Create và Update)
        bool isDuplicate;
        if (id == null)
            isDuplicate = await _repo.AnyAsync(x => x.Name == dto.Name);
        else
            isDuplicate = await _repo.AnyAsync(x => x.Name == dto.Name && x.Id != id);


        if (isDuplicate)
        {
            return ValidationResult.Fail(MessageResponse.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);
        }
        // Nếu qua hết cửa ải -> Thành công
        return ValidationResult.Success();
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
