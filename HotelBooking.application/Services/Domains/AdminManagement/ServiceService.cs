using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    /// <summary>
    /// Interface cho quản lý Service - các dịch vụ của khách sạn
    /// </summary>
    public interface IServiceService : ITypedManage<ServiceBaseDTO, ServiceTypeDTO, ServiceCreateOrUpdateDTO>
    {
        Task<ApiResponse<PagedManageResult<ServiceBaseDTO>>> GetServicesByTypeAsync(int? typeId, PagingRequest paging);
    }

    public class ServiceService : BaseManage<Service, IServiceRepository, ServiceBaseDTO, ServiceCreateOrUpdateDTO>, IServiceService
    {
        private readonly IServiceTypeRepository _serviceTypeRepo;

        public ServiceService(
            IServiceRepository repository,
            IUnitOfWork unitOfWork,
            IServiceTypeRepository serviceTypeRepo)
            : base(repository, unitOfWork)
        {
            _serviceTypeRepo = serviceTypeRepo;
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
                var svTypes = await _serviceTypeRepo.WhereAsync(sv => sv.IsDeleted == false);

                if (svTypes == null || !svTypes.Any())
                {
                    return ResponseFactory.Failure<List<ServiceTypeDTO>>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.EMPTY_LIST);
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
