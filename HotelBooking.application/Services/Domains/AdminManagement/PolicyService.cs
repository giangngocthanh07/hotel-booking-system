using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    /// <summary>
    /// Interface cho quản lý Policy - các chính sách của khách sạn
    /// </summary>
    public interface IPolicyService : ITypedManage<PolicyDTO, PolicyTypeDTO, PolicyCreateDTO, PolicyUpdateDTO>
    {
        Task<ApiResponse<PagedManageResult<PolicyDTO>>> GetPoliciesByTypeAsync(int? typeId, PagingRequest paging);
    }

    public class PolicyService : BaseManage<Policy, IPolicyRepository, PolicyDTO, PolicyCreateDTO, PolicyUpdateDTO>, IPolicyService
    {
        private readonly IPolicyTypeRepository _policyTypeRepo;
                private readonly IValidator<PagingRequest> _pagingValidator;


        public PolicyService(
            IPolicyRepository repository,
            IUnitOfWork unitOfWork,
            IPolicyTypeRepository policyTypeRepo,
            IValidator<PolicyCreateDTO> createVal,
        IValidator<PolicyUpdateDTO> updateVal,
        IValidator<PagingRequest> pagingValidator)
            : base(repository, unitOfWork, createVal, updateVal)
        {
            _policyTypeRepo = policyTypeRepo;
            _pagingValidator = pagingValidator;
        }

        protected override PolicyDTO MapToDto(Policy entity)
        {
            // Delegate to PolicyHelper for polymorphic mapping
            return PolicyHelper.MapToPolicyDTO(entity)!;
        }

        protected override void MapUpdateToEntity(PolicyUpdateDTO updateDto, Policy entity)
        {
            entity.Name = updateDto.Name;
            entity.Description = updateDto.Description;

            // Map polymorphic data to JSON
            entity.Additional = PolicyHelper.MapToAdditionalJson(updateDto);
        }

        protected override Policy MapCreateToEntity(PolicyCreateDTO createDto)
        {
            return new Policy
            {
                Name = createDto.Name,
                Description = createDto.Description,
                IsDeleted = false,
                TypeId = createDto.TypeId,
                // Map polymorphic data to JSON
                Additional = PolicyHelper.MapToAdditionalJson(createDto)
            };
        }

        // --- VALIDATION LOGIC (Quan trọng) ---

        protected override async Task<ValidationResult> ValidateCreateLogicAsync(PolicyCreateDTO dto)
        {
            // 1. Check trùng tên trong cùng TypeId
            bool exists = await _repo.AnyAsync(x => x.Name == dto.Name && x.TypeId == dto.TypeId);
            if (exists) return ValidationResult.Fail(MessageResponse.AdminManagement.Policy.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

            // 2. Check Logic nghiệp vụ (Giờ check-in < check-out, v.v...)
            // Vì đây là Create, ta có dto.TypeId trực tiếp
            // return PolicyHelper.ValidateBusinessRules(dto.TypeId, dto); 
            // (Cần viết hàm Helper nhận DTO chung hoặc map sang entity giả để check)

            return ValidationResult.Success();
        }

        protected override async Task<ValidationResult> ValidateUpdateLogicAsync(PolicyUpdateDTO dto, int id)
        {
            // 1. Lấy Entity gốc để biết TypeId là gì (Vì DTO không có TypeId)
            var currentEntity = await _repo.GetByIdAsync(id);
            if (currentEntity == null || currentEntity.IsDeleted == true) return ValidationResult.Fail(
                    MessageResponse.Common.NOT_FOUND,
                    StatusCodeResponse.NotFound
                ); // Để hàm chính xử lý 404

            // 2. Check trùng tên (Dùng TypeId từ DB)
            bool exists = await _repo.AnyAsync(x =>
                x.Name == dto.Name &&
                x.TypeId == currentEntity.TypeId && // Lấy TypeId gốc
                x.IsDeleted == false &&
                x.Id != id
            );
            if (exists) return ValidationResult.Fail(MessageResponse.AdminManagement.Policy.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

            // 3. Lấy TypeId mong muốn dựa vào kiểu của DTO truyền lên
            int? expectedTypeId = PolicyHelper.GetTypeIdFromUpdateDto(dto);

            // 4. Kiểm tra chéo: Nếu ID Service trong DB thuộc loại khác với Endpoint đang gọi (DTO)
            if (expectedTypeId.HasValue && currentEntity.TypeId != expectedTypeId.Value)
            {
                return ValidationResult.Fail(
                    MessageResponse.AdminManagement.Policy.INVALID_ID_BY_TYPE,
                    StatusCodeResponse.BadRequest
                );
            }

            return ValidationResult.Success();
        }
        public async Task<ApiResponse<List<PolicyTypeDTO>>> GetTypeDataAsync()
        {
            try
            {
                var policyTypes = await _policyTypeRepo.WhereAsync(pt => pt.IsDeleted == false);

                if (policyTypes == null || !policyTypes.Any())
                {
                    return ResponseFactory.Failure<List<PolicyTypeDTO>>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.Common.EMPTY_LIST);
                }

                var result = policyTypes.Select(pt => new PolicyTypeDTO
                {
                    Id = pt.Id,
                    Name = pt.Name,
                    IsDeleted = pt.IsDeleted
                }).ToList();

                return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<List<PolicyTypeDTO>>();
            }
        }

        public async Task<ApiResponse<PagedManageResult<PolicyDTO>>> GetPoliciesByTypeAsync(int? typeId, PagingRequest paging)
        {
             // 1. Validate phân trang
            var pagingValidation = await _pagingValidator.ValidateAsync(paging);
            if (!pagingValidation.IsValid)
            {
                return ResponseFactory.Failure<PagedManageResult<PolicyDTO>>(
                    StatusCodeResponse.BadRequest,
                    pagingValidation.Errors[0].ErrorMessage);
            }

            return await ManagementAdminHelper.GetDataByTypeAsync<Policy, PolicyDTO>(
                typeId,
                paging,

                // Logic 1: lấy ID mặc định: Query bảng PolicyType, lấy thằng đầu tiên chưa xóa
                getDefaultIdFunc: async () =>
                {
                    var firstType = (await _policyTypeRepo.WhereAsync(x => x.IsDeleted != true)).FirstOrDefault();
                    return firstType?.Id;
                },

                // Logic 2: kiểm tra ID tồn tại: Query bảng PolicyType
                checkTypeExistsFunc: async (id) =>
                {
                    var exists = (await _policyTypeRepo.WhereAsync(x => x.Id == id && x.IsDeleted != true)).Any();
                    return exists;
                },

                // Logic 3: Lấy Entity từ DB
                getPagedItemsFunc: async (id, pageIndex, pageSize) =>
                {
                    return await _repo.GetPagedAsync(
                        filter: x => x.TypeId == id && x.IsDeleted == false,
                        pageIndex: pageIndex,
                        pageSize: pageSize,
                        orderBy: q => q.OrderBy(x => x.Id)
                    );
                },

                // Logic 4: Map sang DTO (Tái sử dụng hàm MapToDto có sẵn)
                mapToDtoFunc: MapToDto
            );
        }
    }
}
