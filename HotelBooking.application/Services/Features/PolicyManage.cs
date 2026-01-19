using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;
public interface IPolicyManage : ITypedManage<PolicyDTO, PolicyTypeDTO, PolicyCreateOrUpdateDTO>
{
    Task<ApiResponse<PagedManageResult<PolicyDTO>>> GetPoliciesByTypeAsync(int? typeId, PagingRequest paging);
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
            TypeId = entity.TypeId,

            // Map các cột Generic
            TimeFrom = entity.TimeFrom,
            TimeTo = entity.TimeTo,
            IntValue1 = entity.IntValue1,
            IntValue2 = entity.IntValue2,
            Amount = entity.Amount,
            Percent = entity.Percent,
            BoolValue = entity.BoolValue
        };
    }

    protected override void MapToEntity(PolicyCreateOrUpdateDTO updateDto, Policy entity)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;

        // Cập nhật các cột Generic
        entity.TimeFrom = updateDto.TimeFrom;
        entity.TimeTo = updateDto.TimeTo;
        entity.IntValue1 = updateDto.IntValue1;
        entity.IntValue2 = updateDto.IntValue2;
        entity.Amount = updateDto.Amount;
        entity.Percent = updateDto.Percent;
        entity.BoolValue = updateDto.BoolValue;
    }

    protected override Policy MapToEntity(PolicyCreateOrUpdateDTO createDto)
    {
        var entity = new Policy
        {
            Name = createDto.Name,
            Description = createDto.Description,
            IsDeleted = false,
            TypeId = createDto.TypeId
        };

        // Gọi hàm áp dụng quy tắc cứng
        PolicyHelper.ApplyStrictBusinessRules(entity);

        return entity;
    }

    // Validation
    protected override async Task<ValidationResult> ValidateAsync(PolicyCreateOrUpdateDTO dto, int? id = null)
    {
        var basicValidation = ValidateFactory.ValidateFullAsync<Policy>(
            _repo,
            dto.Name,
            id,
            dto.TypeId,
            getEntityIsDeletedFunc: x => x.IsDeleted,
            isDeletedSelector: x => x.IsDeleted,
            nameSelector: x => x.Name);
        return await basicValidation;

    }

    public async Task<ApiResponse<List<PolicyTypeDTO>>> GetTypeDataAsync()
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
                Name = pt.Name,
                IsDeleted = pt.IsDeleted
            }).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<PolicyTypeDTO>>();
        }
    }

    public async Task<ApiResponse<PagedManageResult<PolicyDTO>>> GetPoliciesByTypeAsync(int? typeId, PagingRequest paging)
    {
        return await ManagementAdminHelper.GetDataByTypeAsync<Policy, PolicyDTO>(
            typeId,
            paging,

            // Logic 1: lấy ID mặc định: Query bảng PolicyType, lấy thằng đầu tiên chưa xóa
            getDefaultIdFunc: async () =>
            {
                // query DB lấy 1 dòng
                var firstType = (await _poliTypeRepo.WhereAsync(x => x.IsDeleted != true)).FirstOrDefault();
                return firstType?.Id;
            },
            // Logic 2: kiểm tra ID tồn tại: Query bảng PolicyType
            checkTypeExistsFunc: async (id) =>
            {
                // Kiểm tra xem có dòng nào có Id này và chưa bị xóa không
                var exists = (await _poliTypeRepo.WhereAsync(x => x.Id == id && x.IsDeleted != true)).Any();
                return exists;
            },
            // Logic 3: Lấy Entity từ DB
            getPagedItemsFunc: async (id, pageIndex, pageSize) =>
            {
                return await _repo.GetPagedAsync(
                    filter: x => x.TypeId == id && x.IsDeleted == false,
                    pageIndex: pageIndex,
                    pageSize: pageSize,
                    orderBy: q => q.OrderBy(x => x.Id) // Hoặc sort theo tên tùy bạn
                );
            },

            // Logic 4: Map sang DTO (Tái sử dụng hàm MapToDto có sẵn)
            mapToDtoFunc: MapToDto
        );
    }
}


