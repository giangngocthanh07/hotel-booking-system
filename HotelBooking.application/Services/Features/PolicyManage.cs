using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;
public interface IPolicyManage : ITypedManage<PolicyDTO, PolicyCreateOrUpdateDTO>
{
    Task<ApiResponse<List<PolicyTypeDTO>>> GetPolicyTypesAsync();
    Task<ApiResponse<MangagePolicyDTO>> GetManagePolicyDataAsync(int? selectedTypeId = null);
}
public class PolicyManage : BaseManage<Policy, IPolicyRepository, PolicyDTO, PolicyCreateOrUpdateDTO>, IPolicyManage
{
    private readonly IPolicyTypeRepository _poliTypeRepo;
    public PolicyManage(IPolicyRepository repository, IUnitOfWork dbo, IPolicyTypeRepository poliTypeRepo) : base(repository, dbo)
    {
        _poliTypeRepo = poliTypeRepo;
    }

    protected override PolicyDTO MapToDto(Policy entity)
    {
        return new PolicyDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsDeleted = entity.IsDeleted,
            PolicyTypeId = entity.PolicyTypeId
        };
    }

    protected override void MapToEntity(PolicyCreateOrUpdateDTO updateDto, Policy entity)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
    }

    protected override Policy MapToEntity(PolicyCreateOrUpdateDTO createDto)
    {
        return new Policy
        {
            Name = createDto.Name,
            Description = createDto.Description,
            PolicyTypeId = createDto.PolicyTypeId

        };
    }

    // Validation
    protected override async Task<ValidationResult> ValidateAsync(PolicyCreateOrUpdateDTO dto, int? id = null)
    {
        // Cấu trúc chuỗi kiểm tra (Chain of Responsibility)
        // Nó sẽ chạy từ trái sang phải, gặp cái nào lỗi (khác null) là return ngay lập tức.

        var basicCheck = ValidateUtils.RequireNotEmpty(dto.Name, MessageResponse.EMPTY_NAME, StatusCodeResponse.BadRequest) ??
                        ValidateUtils.Require(dto.PolicyTypeId > 0, MessageResponse.BAD_REQUEST, StatusCodeResponse.BadRequest);

        // Nếu các check cơ bản đã có lỗi -> Return luôn, khỏi cần check DB cho tốn thời gian
        if (basicCheck != null) return basicCheck;

        // --- CHECK DB (Logic Check trùng) ---
        // Phần Async ta nên tách ra check riêng sau khi check cơ bản đã OK

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
            if (existingEntity?.PolicyTypeId != dto.PolicyTypeId)
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

    public async Task<ApiResponse<List<PolicyDTO>>> GetAllByTypeAsync(int typeId)
    {
        try
        {
            var policies = await _repo.WhereAsync(p => p.PolicyTypeId == typeId && p.IsDeleted == false);

            if (policies == null || !policies.Any())
            {
                return ResponseFactory.Failure<List<PolicyDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
            }

            var result = policies.Select(p => MapToDto(p)).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<PolicyDTO>>();
        }
    }

    public async Task<ApiResponse<MangagePolicyDTO>> GetManagePolicyDataAsync(int? selectedTypeId = null)
    {
        try
        {
            // 1. Lấy danh sách Loại Policy (PolicyTypes)
            var types = await _poliTypeRepo.GetAllAsync();

            var typeDtos = types.Select(t => new PolicyTypeDTO
            {
                Id = t.Id,
                Name = t.TypeName,
                IsDeleted = t.IsDeleted
            }).ToList();

            // 2. Xác định Type nào đang được chọn
            // Nếu user không truyền vào -> Lấy cái đầu tiên trong danh sách
            int currentTypeId = selectedTypeId ?? (typeDtos.FirstOrDefault()?.Id ?? 0);
            string? currentTypeName = typeDtos.FirstOrDefault(t => t.Id == currentTypeId)?.Name;

            // 3. Lấy danh sách Policy theo Type đã chọn
            // Tận dụng hàm Where của Repo chính
            var policyEntities = await _repo.WhereAsync(p => p.PolicyTypeId == currentTypeId);
            var policyDtos = policyEntities.Select(p => MapToDto(p)).ToList();

            // 4. Đóng gói vào ViewModel (MangagePolicyDTO)
            var viewModel = new MangagePolicyDTO
            {
                PolicyTypes = typeDtos,
                SelectedTypeId = currentTypeId,
                SelectedTypeName = currentTypeName,
                Policies = policyDtos
            };

            return ResponseFactory.Success(viewModel, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<MangagePolicyDTO>();
        }
    }

    public async Task<ApiResponse<List<PolicyTypeDTO>>> GetPolicyTypesAsync()
    {
        try
        {
            var policyTypes = await _poliTypeRepo.WhereAsync(pt => pt.IsDeleted == false);

            if (policyTypes == null || !policyTypes.Any())
            {
                return ResponseFactory.Failure<List<PolicyTypeDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
            }

            var result = policyTypes.Select(pt => new PolicyTypeDTO
            {
                Id = pt.Id,
                Name = pt.TypeName,
                IsDeleted = pt.IsDeleted
            }).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<PolicyTypeDTO>>();
        }
    }
}


