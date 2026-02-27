using System.Text.Json;
using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    /// <summary>
    /// Interface cho quản lý Amenity - các tiện nghi của khách sạn
    /// </summary>
    public interface IAmenityService : ITypedManage<AmenityDTO, AmenityTypeDTO, AmenityCreateDTO, AmenityUpdateDTO>
    {
        Task<ApiResponse<PagedManageResult<AmenityDTO>>> GetAmenitiesByTypeAsync(int? typeId, PagingRequest paging);
    }

    public class AmenityService : BaseManage<Amenity, IAmenityRepository, AmenityDTO, AmenityCreateDTO, AmenityUpdateDTO>, IAmenityService
    {
        private readonly IAmenityTypeRepository _amenityTypeRepo;

        public AmenityService(
            IAmenityRepository repository,
            IAmenityTypeRepository amenityTypeRepository,
            IUnitOfWork unitOfWork,
            IValidator<AmenityCreateDTO> createValidator,
        IValidator<AmenityUpdateDTO> updateValidator)
            : base(repository, unitOfWork, createValidator, updateValidator)
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

        protected override void MapUpdateToEntity(AmenityUpdateDTO updateDto, Amenity entity)
        {
            entity.Name = updateDto.Name;
            entity.Additional = JsonSerializer.Serialize(new
            {
                Description = string.IsNullOrWhiteSpace(updateDto.Description) ? null : updateDto.Description,
            });
        }

        protected override Amenity MapCreateToEntity(AmenityCreateDTO createDto)
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

        // Override hàm check Logic (Thay vì Expression Tree)
        // Override Logic Create: Check trùng tên (chung chung)
        protected override async Task<ValidationResult> ValidateCreateLogicAsync(AmenityCreateDTO dto)
        {
            bool exists = await _repo.AnyAsync(x => x.Name == dto.Name);
            if (exists) return ValidationResult.Fail(MessageResponse.AdminManagement.Amenity.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

            return ValidationResult.Success();
        }

        // Override Logic Update: Check trùng tên (Trừ ID hiện tại ra)
        protected override async Task<ValidationResult> ValidateUpdateLogicAsync(AmenityUpdateDTO dto, int id)
        {
            {
                // BƯỚC 1: Lấy TypeId gốc từ DB (Vì DTO không có, và ta không tin user)
                // Dùng AsNoTracking để tối ưu vì chỉ cần đọc TypeId
                var currentEntity = await _repo.GetByIdAsync(id);

                if (currentEntity == null) return ValidationResult.Success(); // Để hàm Update chính xử lý lỗi 404 sau

                // BƯỚC 2: Check trùng tên nhưng phải cùng TypeId GỐC
                bool isDuplicate = await _repo.AnyAsync(x =>
                    x.Name == dto.Name &&
                    x.TypeId == currentEntity.TypeId && // Lấy từ DB, không lấy từ DTO
                    x.IsDeleted == false &&
                    x.Id != id
                );

                if (isDuplicate)
                    return ValidationResult.Fail(MessageResponse.AdminManagement.Amenity.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

                return ValidationResult.Success();
            }
        }

        [Obsolete]
        public async Task<ApiResponse<List<AmenityTypeDTO>>> GetTypeDataAsync()
        {
            try
            {
                var amenityTypes = await _amenityTypeRepo.WhereAsync(a => a.IsDeleted == false);

                if (amenityTypes == null || !amenityTypes.Any())
                {
                    return ResponseFactory.Failure<List<AmenityTypeDTO>>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.Common.EMPTY_LIST);
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

                return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
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
