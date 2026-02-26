using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    /// <summary>
    /// Interface cho quản lý Service - các dịch vụ của khách sạn
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
            entity.TypeId = entity.TypeId; // Giữ nguyên TypeId

            // [LOGIC NGHIỆP VỤ GIÁ TIỀN]
            entity.Price = updateDto.Price;

            if (updateDto is ServiceAirportUpdateDTO airDto)
            {
                // Nếu Admin bỏ tick "Phí 1 chiều" -> Giá về 0
                entity.Price = airDto.IsOneWayPaid ? airDto.Price : 0;
            }

            // Dùng Helper overload cho Update
            entity.Additional = ServiceHelper.MapToAdditionalJson(updateDto);

            // LƯU Ý: Tuyệt đối KHÔNG map TypeId ở đây để tránh đổi loại dịch vụ
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

            // Logic phụ: Nếu là Airport và không tính phí 1 chiều -> Price = 0
            if (createDto is ServiceAirportCreateDTO airDto && !airDto.IsOneWayPaid)
            {
                entity.Price = 0;
            }

            return entity;
        }

        // --- 2. VALIDATION (Tách biệt Create/Update) ---
        protected override async Task<ValidationResult> ValidateCreateLogicAsync(ServiceCreateDTO dto)
        {
            // Check trùng tên trong cùng loại (TargetTypeId)
            bool isDuplicateName = await _repo.AnyAsync(x =>
                x.Name == dto.Name);

            if (isDuplicateName) return ValidationResult.Fail(MessageResponse.AdminManagement.Service.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

            return ValidationResult.Success();
        }

        protected override async Task<ValidationResult> ValidateUpdateLogicAsync(ServiceUpdateDTO dto, int id)
        {
            // UpdateDTO không có TypeId -> Phải lấy từ DB ra để biết scope check trùng
            // 1. Lấy dữ liệu hiện tại trong DB
            var currentEntity = await _repo.GetByIdAsync(id);
            // Nếu entity null hoặc đã bị IsDeleted = true thì báo NotFound
            if (currentEntity == null || currentEntity.IsDeleted == true)
            {
                return ValidationResult.Fail(
                    MessageResponse.Common.NOT_FOUND,
                    StatusCodeResponse.NotFound
                );
            }

            // 2. Lấy TypeId mong muốn dựa vào kiểu của DTO truyền lên
            int? expectedTypeId = ServiceHelper.GetTypeIdFromUpdateDto(dto);

            // 3. Kiểm tra chéo: Nếu ID Service trong DB thuộc loại khác với Endpoint đang gọi (DTO)
            if (expectedTypeId.HasValue && currentEntity.TypeId != expectedTypeId.Value)
            {
                return ValidationResult.Fail(
                    MessageResponse.AdminManagement.Service.INVALID_ID_BY_TYPE,
                    StatusCodeResponse.BadRequest
                );
            }

            // 4. Check trùng tên (vẫn phải check để tránh trùng tên trong cùng 1 loại)
            bool isDuplicate = await _repo.AnyAsync(x =>
                x.Name == dto.Name &&
                x.TypeId == currentEntity.TypeId && // Check theo TypeId gốc
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
            // 1. Validate phân trang
            var pagingValidation = await _pagingValidator.ValidateAsync(paging);
            if (!pagingValidation.IsValid)
            {
                return ResponseFactory.Failure<PagedManageResult<ServiceDTO>>(
                    StatusCodeResponse.BadRequest,
                    pagingValidation.Errors[0].ErrorMessage);
            }

            // 2. Nếu không có lỗi, lấy dữ liệu theo typeId với helper chung
            return await ManagementAdminHelper.GetDataByTypeAsync<Service, ServiceDTO>(
                typeId,
                paging,

                // Logic 1: lấy ID mặc định: Query bảng ServiceType, lấy thằng đầu tiên chưa xóa
                getDefaultIdFunc: async () =>
                {
                    var firstType = await _serviceTypeRepo.FirstOrDefaultAsync(x => x.IsDeleted != true);
                    return firstType?.Id;
                },

                // Logic 2: kiểm tra ID tồn tại: Query bảng ServiceType
                checkTypeExistsFunc: async (id) =>
                {
                    var exists = await _serviceTypeRepo.AnyAsync(x => x.Id == id && x.IsDeleted != true);
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
}
