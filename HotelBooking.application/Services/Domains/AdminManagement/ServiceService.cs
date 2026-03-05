using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;
using HotelBooking.application.Interfaces;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    /// <summary>
    /// Interface for managing Services — hotel service offerings
    /// </summary>
    public interface IServiceService : ITypedManage<ServiceDTO, ServiceTypeDTO, ServiceCreateDTO, ServiceUpdateDTO>
    {
        Task<ApiResponse<PagedManageResult<ServiceDTO>>> GetServicesByTypeAsync(int? typeId, PagingRequest paging);
    }

    public class ServiceService : BaseManage<Service, IServiceRepository, ServiceDTO, ServiceCreateDTO, ServiceUpdateDTO>, IServiceService
    {
        private readonly IServiceTypeRepository _serviceTypeRepo;
        private readonly IValidator<PagingRequest> _pagingValidator;

        public ServiceService(
            IServiceRepository repository,
            IUnitOfWork unitOfWork,
            IServiceTypeRepository serviceTypeRepo,
            IValidator<ServiceCreateDTO> createVal,
            IValidator<ServiceUpdateDTO> updateVal,
            IValidator<PagingRequest> pagingValidator)
            : base(repository, unitOfWork, createVal, updateVal)
        {
            _serviceTypeRepo = serviceTypeRepo;
            _pagingValidator = pagingValidator;
        }

        protected override ServiceDTO MapToDto(Service entity)
        {
            var dto = ServiceHelper.MapToServiceDTO(entity);
            return dto!;
        }

        protected override void MapUpdateToEntity(ServiceUpdateDTO updateDto, Service entity)
        {
            entity.Name = updateDto.Name;
            entity.Description = updateDto.Description;
            entity.TypeId = entity.TypeId; // Preserve TypeId (do not allow type change)

            // [LOGIC NGHIỆP VỤ GIÁ TIỀN]
            entity.Price = updateDto.Price;

            if (updateDto is ServiceAirportUpdateDTO airDto)
            {
                // If Admin unchecks "One-Way Fee" -> Price becomes 0
                entity.Price = airDto.IsOneWayPaid ? airDto.Price : 0;
            }

            // Use Helper overload for Update
            entity.Additional = ServiceHelper.MapToAdditionalJson(updateDto);

            // NOTE: Never map TypeId here to prevent changing the service type
        }

        protected override Service MapCreateToEntity(ServiceCreateDTO createDto)
        {
            var entity = new Service
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Price = createDto.Price,
                TypeId = createDto.TargetTypeId,
                IsDeleted = false,
                Additional = ServiceHelper.MapToAdditionalJson(createDto)
            };

            // Side logic: if Airport service and one-way fee not applied -> Price = 0
            if (createDto is ServiceAirportCreateDTO airDto && !airDto.IsOneWayPaid)
            {
                entity.Price = 0;
            }

            return entity;
        }

        // --- 2. VALIDATION (separated Create/Update) ---
        protected override async Task<ValidationResult> ValidateCreateLogicAsync(ServiceCreateDTO dto)
        {
            // Check for duplicate name within the same type (TargetTypeId)
            bool isDuplicateName = await _repo.AnyAsync(x =>
                x.Name == dto.Name);

            if (isDuplicateName) return ValidationResult.Fail(MessageResponse.AdminManagement.Service.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

            return ValidationResult.Success();
        }

        protected override async Task<ValidationResult> ValidateUpdateLogicAsync(ServiceUpdateDTO dto, int id)
        {
            // UpdateDTO does not include TypeId -> must fetch from DB to determine scope for duplicate check
            // 1. Fetch current entity from DB
            var currentEntity = await _repo.GetByIdAsync(id);
            // If entity is null or IsDeleted == true, return NotFound
            if (currentEntity == null || currentEntity.IsDeleted == true)
            {
                return ValidationResult.Fail(
                    MessageResponse.Common.NOT_FOUND,
                    StatusCodeResponse.NotFound
                );
            }

            // 2. Determine expected TypeId from the DTO type
            int? expectedTypeId = ServiceHelper.GetTypeIdFromUpdateDto(dto);

            // 3. Cross-check: if the DB Service belongs to a different type than the calling endpoint (DTO)
            if (expectedTypeId.HasValue && currentEntity.TypeId != expectedTypeId.Value)
            {
                return ValidationResult.Fail(
                    MessageResponse.AdminManagement.Service.INVALID_ID_BY_TYPE,
                    StatusCodeResponse.BadRequest
                );
            }

            // 4. Check for duplicate name (still required to prevent naming conflicts within the same type)
            bool isDuplicate = await _repo.AnyAsync(x =>
                x.Name == dto.Name &&
                x.TypeId == currentEntity.TypeId && // Check against original TypeId
                x.Id != id &&
                x.IsDeleted == false);

            if (isDuplicate)
                return ValidationResult.Fail(MessageResponse.AdminManagement.Service.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

            return ValidationResult.Success();
        }

        public async Task<ApiResponse<List<ServiceTypeDTO>>> GetTypeDataAsync()
        {
            try
            {
                var svTypes = await _serviceTypeRepo.WhereAsync(sv => sv.IsDeleted == false);

                if (svTypes == null || !svTypes.Any())
                {
                    return ResponseFactory.Failure<List<ServiceTypeDTO>>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.Common.EMPTY_LIST);
                }

                var result = svTypes.Select(sv => new ServiceTypeDTO
                {
                    Id = sv.Id,
                    Name = sv.Name,
                    IsDeleted = sv.IsDeleted
                }).ToList();

                return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<List<ServiceTypeDTO>>();
            }
        }

        public async Task<ApiResponse<PagedManageResult<ServiceDTO>>> GetServicesByTypeAsync(int? typeId, PagingRequest paging)
        {
            // 1. Validate pagination
            var pagingValidation = await _pagingValidator.ValidateAsync(paging);
            if (!pagingValidation.IsValid)
            {
                return ResponseFactory.Failure<PagedManageResult<ServiceDTO>>(
                    StatusCodeResponse.BadRequest,
                    pagingValidation.Errors[0].ErrorMessage);
            }

            // 2. If valid, fetch data by typeId using the shared helper
            return await ManagementAdminHelper.GetDataByTypeAsync<Service, ServiceDTO>(
                typeId,
                paging,

                // Logic 1: get default ID — query ServiceType table, take first non-deleted
                getDefaultIdFunc: async () =>
                {
                    var firstType = await _serviceTypeRepo.FirstOrDefaultAsync(x => x.IsDeleted != true);
                    return firstType?.Id;
                },

                // Logic 2: check if ID exists — query ServiceType table
                checkTypeExistsFunc: async (id) =>
                {
                    var exists = await _serviceTypeRepo.AnyAsync(x => x.Id == id && x.IsDeleted != true);
                    return exists;
                },

                // Logic 3: Fetch entities from DB
                getPagedItemsFunc: async (id, page, size) =>
                    await _repo.GetPagedAsync(
                        x => x.TypeId == id && x.IsDeleted == false,
                        page,
                        size,
                        q => q.OrderByDescending(x => x.Id)),

                // Logic 4: Map to DTO (reuses the existing MapToDto method)
                mapToDtoFunc: MapToDto
            );
        }
    }
}
