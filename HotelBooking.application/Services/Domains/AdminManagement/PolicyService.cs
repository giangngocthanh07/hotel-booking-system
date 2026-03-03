using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    /// <summary>
    /// Interface for managing Policies — hotel rules and conditions
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

        // --- VALIDATION LOGIC (important) ---

        protected override async Task<ValidationResult> ValidateCreateLogicAsync(PolicyCreateDTO dto)
        {
            // 1. Check for duplicate name within the same TypeId
            bool exists = await _repo.AnyAsync(x => x.Name == dto.Name && x.TypeId == dto.TypeId);
            if (exists) return ValidationResult.Fail(MessageResponse.AdminManagement.Policy.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

            // 2. Business rule validation (check-in < check-out, etc.)
            // For Create, dto.TypeId is available directly
            // return PolicyHelper.ValidateBusinessRules(dto.TypeId, dto);
            // (Requires a helper that accepts a common DTO or maps to a mock entity for validation)

            return ValidationResult.Success();
        }

        protected override async Task<ValidationResult> ValidateUpdateLogicAsync(PolicyUpdateDTO dto, int id)
        {
            // 1. Fetch the original entity to retrieve TypeId (DTO does not include TypeId)
            var currentEntity = await _repo.GetByIdAsync(id);
            if (currentEntity == null || currentEntity.IsDeleted == true) return ValidationResult.Fail(
                    MessageResponse.Common.NOT_FOUND,
                    StatusCodeResponse.NotFound
                ); // Let the main handler process the 404

            // 2. Check for duplicate name (using TypeId from DB)
            bool exists = await _repo.AnyAsync(x =>
                x.Name == dto.Name &&
                x.TypeId == currentEntity.TypeId && // Use original TypeId
                x.IsDeleted == false &&
                x.Id != id
            );
            if (exists) return ValidationResult.Fail(MessageResponse.AdminManagement.Policy.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

            // 3. Determine expected TypeId from the DTO type
            int? expectedTypeId = PolicyHelper.GetTypeIdFromUpdateDto(dto);

            // 4. Cross-check: if the DB Policy belongs to a different type than the calling endpoint (DTO)
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
            // 1. Validate pagination
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

                // Logic 1: get default ID — query PolicyType table, take first non-deleted
                getDefaultIdFunc: async () =>
                {
                    var firstType = (await _policyTypeRepo.WhereAsync(x => x.IsDeleted != true)).FirstOrDefault();
                    return firstType?.Id;
                },

                // Logic 2: check if ID exists — query PolicyType table
                checkTypeExistsFunc: async (id) =>
                {
                    var exists = (await _policyTypeRepo.WhereAsync(x => x.Id == id && x.IsDeleted != true)).Any();
                    return exists;
                },

                // Logic 3: Fetch entities from DB
                getPagedItemsFunc: async (id, pageIndex, pageSize) =>
                {
                    return await _repo.GetPagedAsync(
                        filter: x => x.TypeId == id && x.IsDeleted == false,
                        pageIndex: pageIndex,
                        pageSize: pageSize,
                        orderBy: q => q.OrderBy(x => x.Id)
                    );
                },

                // Logic 4: Map to DTO (reuses the existing MapToDto method)
                mapToDtoFunc: MapToDto
            );
        }
    }
}
