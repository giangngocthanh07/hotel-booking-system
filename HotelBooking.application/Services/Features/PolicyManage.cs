using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;
public interface IPolicyManage : ITypedManage<PolicyDTO, PolicyCreateOrUpdateDTO>
{
    Task<ApiResponse<List<PolicyTypeDTO>>> GetPolicyTypesAsync();
    Task<ApiResponse<MangagePolicyDTO>> GetManagePolicyDataAsync(int? selectedTypeId = null);
}
public class PolicyManage : BaseManage<Policy, IPolicyRepository, PolicyDTO, PolicyCreateOrUpdateDTO>, IPolicyManage
{
    IPolicyTypeRepository _poliTypeRepo;
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

    public override async Task<ApiResponse<PolicyDTO>> CreateAsync(PolicyCreateOrUpdateDTO createDto)
    {
        var exists = await _repo.AnyAsync(sv => sv.Name.ToLower() == createDto.Name.ToLower() && sv.IsDeleted == false);

        if (exists)
            return ResponseFactory.Failure<PolicyDTO>(StatusCodeResponse.Conflict, MessageResponse.NAME_ALREADY_EXISTS);

        return await base.CreateAsync(createDto);
    }

    public override async Task<ApiResponse<PolicyDTO>> UpdateAsync(int id, PolicyCreateOrUpdateDTO updateDto)
    {
        // 1. Lấy entity hiện tại từ DB lên để kiểm tra
        var existingEntity = await _repo.GetByIdAsync(id);

        // Nếu không tìm thấy ID -> Trả về NotFound ngay (hoặc để base xử lý, nhưng check ở đây luôn cho tiện luồng)
        if (existingEntity == null || existingEntity.IsDeleted == true)
        {
            return ResponseFactory.Failure<PolicyDTO>(StatusCodeResponse.NotFound, MessageResponse.NOT_FOUND);
        }

        var exists = await _repo.AnyAsync(p => p.Name.ToLower() == updateDto.Name.ToLower() && p.IsDeleted == false);

        if (exists)
            return ResponseFactory.Failure<PolicyDTO>(StatusCodeResponse.Conflict, MessageResponse.NAME_ALREADY_EXISTS);

        return await base.UpdateAsync(id, updateDto);
    }

}


