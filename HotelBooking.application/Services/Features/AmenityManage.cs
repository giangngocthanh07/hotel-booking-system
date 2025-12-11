using System.Text.Json;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IAmenityManage : IStandardManage<AmenityDTO, AmenityCreateOrUpdateDTO>;

public class AmenityManage : BaseManage<Amenity, IAmenityRepository, AmenityDTO, AmenityCreateOrUpdateDTO>, IAmenityManage
{
    public AmenityManage(IAmenityRepository repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }


    protected override AmenityDTO MapToDto(Amenity entity)
    {
        var additional = JsonSerializer.Deserialize<Dictionary<string, string?>>(entity.Additional ?? "{}") ?? new Dictionary<string, string?>();

        return new AmenityDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = additional.GetValueOrDefault("Description", null),
            IconClass = additional.GetValueOrDefault("IconClass", null) ?? "",
            IconColor = additional.GetValueOrDefault("IconColor", null) ?? "blue",
        };
    }

    protected override void MapToEntity(AmenityCreateOrUpdateDTO updateDto, Amenity entity)
    {
        entity.Name = updateDto.Name;
        entity.Additional = JsonSerializer.Serialize(new
        {
            Description = string.IsNullOrWhiteSpace(updateDto.Description) ? null : updateDto.Description,
            IconClass = updateDto.IconClass,
            IconColor = string.IsNullOrWhiteSpace(updateDto.IconColor) ? "blue" : updateDto.IconColor
        });
    }

    protected override Amenity MapToEntity(AmenityCreateOrUpdateDTO createDto)
    {
        var additional = JsonSerializer.Serialize(new
        { Description = createDto.Description, IconClass = createDto.IconClass, IconColor = string.IsNullOrWhiteSpace(createDto.IconColor) ? "blue" : createDto.IconColor });

        return new Amenity
        {
            Name = createDto.Name,
            Additional = additional,
        };
    }

    // Validation
    protected override async Task<ValidationResult> ValidateAsync(AmenityCreateOrUpdateDTO dto, int? id = null)
    {
        // Cấu trúc chuỗi kiểm tra (Chain of Responsibility)
        // Nó sẽ chạy từ trái sang phải, gặp cái nào lỗi (khác null) là return ngay lập tức.

        var basicCheck = ValidateUtils.RequireNotEmpty(dto.Name, MessageResponse.EMPTY_NAME, StatusCodeResponse.BadRequest);

        // Nếu các check cơ bản đã có lỗi -> Return luôn, khỏi cần check DB cho tốn thời gian
        if (basicCheck != null) return basicCheck;

        // --- CHECK DB (Logic Check trùng) ---
        // Phần Async ta nên tách ra check riêng sau khi check cơ bản đã OK
        // --- 2. VALIDATE NGHIỆP VỤ (Cần DB) ---

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

    public async Task<ApiResponse<List<AmenityDTO>>> GetAllAsync()
    {
        var amenities = await _repo.WhereAsync(a => a.IsDeleted == false);

        if (amenities == null || amenities.Count() == 0)
        {
            return ResponseFactory.Failure<List<AmenityDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
        }

        try
        {
            var result = amenities.Select(a => MapToDto(a)).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<AmenityDTO>>();
        }
    }
}