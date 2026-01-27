using System.Text.Json;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    /// <summary>
    /// Interface cho quản lý Amenity - các tiện nghi của khách sạn
    /// </summary>
    public interface IAmenityService : ITypedManage<AmenityDTO, AmenityTypeDTO, AmenityCreateOrUpdateDTO>
    {
        Task<ApiResponse<PagedManageResult<AmenityDTO>>> GetAmenitiesByTypeAsync(int? typeId, PagingRequest paging);
    }

    public class AmenityService : BaseManage<Amenity, IAmenityRepository, AmenityDTO, AmenityCreateOrUpdateDTO>, IAmenityService
    {
        private readonly IAmenityTypeRepository _amenityTypeRepo;

        public AmenityService(
            IAmenityRepository repository,
            IAmenityTypeRepository amenityTypeRepository,
            IUnitOfWork unitOfWork)
            : base(repository, unitOfWork)
        {
            _amenityTypeRepo = amenityTypeRepository;
        }

        protected override AmenityDTO MapToDto(Amenity entity)
        {
            var additional = JsonSerializer.Deserialize<Dictionary<string, string?>>(entity.Additional ?? "{}")
                ?? new Dictionary<string, string?>();

            return new AmenityDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = additional.GetValueOrDefault("Description", null),
                IsDeleted = entity.IsDeleted,
                TypeId = entity.TypeId
            };
        }

        protected override void MapToEntity(AmenityCreateOrUpdateDTO updateDto, Amenity entity)
        {
            entity.Name = updateDto.Name;
            entity.Additional = JsonSerializer.Serialize(new
            {
                Description = string.IsNullOrWhiteSpace(updateDto.Description) ? null : updateDto.Description,
            });
        }

        protected override Amenity MapToEntity(AmenityCreateOrUpdateDTO createDto)
        {
            var additional = JsonSerializer.Serialize(new
            { Description = createDto.Description });

            return new Amenity
            {
                Name = createDto.Name,
                Additional = additional,
                IsDeleted = false,
                TypeId = createDto.TypeId
            };
        }

        protected override async Task<ValidationResult> ValidateAsync(AmenityCreateOrUpdateDTO dto, int? id = null)
        {
            var basicValidation = ValidateFactory.ValidateFullAsync<Amenity>(
                _repo,
                dto.Name,
                id,
                dto.TypeId,
                getEntityIsDeletedFunc: x => x.IsDeleted,
                isDeletedSelector: x => x.IsDeleted,
                nameSelector: x => x.Name);
            return await basicValidation;
        }

        public async Task<ApiResponse<List<AmenityTypeDTO>>> GetTypeDataAsync()
        {
            try
            {
                var amenityTypes = await _amenityTypeRepo.WhereAsync(a => a.IsDeleted == false);

                if (amenityTypes == null || !amenityTypes.Any())
                {
                    return ResponseFactory.Failure<List<AmenityTypeDTO>>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.EMPTY_LIST);
                }

                var result = amenityTypes.Select(a =>
                {
                    var additionalData = string.IsNullOrWhiteSpace(a.Additional)
                        ? new Dictionary<string, string?>()
                        : JsonSerializer.Deserialize<Dictionary<string, string?>>(a.Additional)
                          ?? new Dictionary<string, string?>();

                    return new AmenityTypeDTO
                    {
                        Id = a.Id,
                        Name = a.Name,
                        IsDeleted = a.IsDeleted,
                        IconClass = additionalData.GetValueOrDefault("IconClass"),
                        IconColor = additionalData.GetValueOrDefault("IconColor")
                    };
                }).ToList();

                return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<List<AmenityTypeDTO>>();
            }
        }

        public async Task<ApiResponse<PagedManageResult<AmenityDTO>>> GetAmenitiesByTypeAsync(int? typeId, PagingRequest paging)
        {
            return await ManagementAdminHelper.GetDataByTypeAsync<Amenity, AmenityDTO>(
                typeId,
                paging,
                getDefaultIdFunc: async () =>
                {
                    var firstType = (await _amenityTypeRepo.WhereAsync(x => x.IsDeleted != true)).FirstOrDefault();
                    return firstType?.Id;
                },
                checkTypeExistsFunc: async (id) =>
                {
                    var exists = (await _amenityTypeRepo.WhereAsync(x => x.Id == id && x.IsDeleted != true)).Any();
                    return exists;
                },
                getPagedItemsFunc: async (id, page, size) =>
                    await _repo.GetPagedAsync(
                        x => x.TypeId == id && x.IsDeleted == false,
                        page,
                        size,
                        q => q.OrderByDescending(x => x.Id)),
                mapToDtoFunc: MapToDto
            );
        }
    }
}
