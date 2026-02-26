using FluentValidation;
using HotelBooking.application.Helpers;

public interface IBaseManage<TEntity, TRepo, TDto, TCreateDTO, TUpdateDTO> : ICommonManage<TDto, TCreateDTO, TUpdateDTO>
{
}

public abstract class BaseManage<TEntity, TRepo, TDto, TCreateDTO, TUpdateDTO> : IBaseManage<TEntity, TRepo, TDto, TCreateDTO, TUpdateDTO>
    where TEntity : class
    where TRepo : IRepository<TEntity>
    where TCreateDTO : class
    where TUpdateDTO : class
{
    // Implementation of base management functionalities
    protected readonly TRepo _repo;
    protected readonly IUnitOfWork _dbu;

    // Inject Validator (Cho phép null - Optional)
    // Inject 2 Validator riêng biệt (Có thể null)
    protected readonly IValidator<TCreateDTO>? _createValidator;
    protected readonly IValidator<TUpdateDTO>? _updateValidator;

    public BaseManage(TRepo repo, IUnitOfWork dbu, IValidator<TCreateDTO>? createValidator = null,
        IValidator<TUpdateDTO>? updateValidator = null)
    {
        _repo = repo;
        _dbu = dbu;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // --- ĐỊNH NGHĨA 3 HÀM BẮT BUỘC CON PHẢI LÀM ---

    // Các factory method để map giữa Entity, DTO, CreateDTO, UpdateDTO

    // 1. Map từ Entity -> DTO (Dùng cho GetAll, GetById, return Create)
    protected abstract TDto MapToDto(TEntity entity);

    // 2. Map từ CreateDTO -> Entity (Dùng cho Create)
    protected abstract TEntity MapCreateToEntity(TCreateDTO createDto);

    // 3. Map từ UpdateDTO vào Entity CÓ SẴN (Dùng cho Update, Deleted)
    protected abstract void MapUpdateToEntity(TUpdateDTO updateDto, TEntity entity);

    // 4. --- VALIDATION METHOD ---

    // Validate Business Logic cho Create (VD: Check trùng tên khi tạo mới)
    protected virtual async Task<ValidationResult> ValidateCreateLogicAsync(TCreateDTO dto)
    {
        return new ValidationResult(); // Mặc định hợp lệ
    }

    // Validate Business Logic cho Update (VD: Check trùng tên nhưng trừ ID hiện tại)
    protected virtual async Task<ValidationResult> ValidateUpdateLogicAsync(TUpdateDTO dto, int id)
    {
        return new ValidationResult(); // Mặc định hợp lệ
    }

    public virtual async Task<ApiResponse<TDto>> GetByIdAsync(int id)
    {
        try
        {
            var entity = await _repo.GetByIdAsync(id);

            // 1.Check null trước
            if (entity == null)
            {
                return ResponseFactory.Failure<TDto>(StatusCodeResponse.NotFound, MessageResponse.Common.NOT_FOUND);
            }

            // --- DÙNG REFLECTION GIỐNG HÀM ADD CỦA BẠN ---
            var prop = typeof(TEntity).GetProperty("IsDeleted");
            if (prop != null && (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?)))
            {
                // Thay vì SetValue (gán), ta dùng GetValue (đọc)
                var value = prop.GetValue(entity);

                // Nếu giá trị đang là true, nghĩa là đã bị xóa mềm
                if (value != null && (bool)value == true)
                {
                    return ResponseFactory.Failure<TDto>(StatusCodeResponse.NotFound, MessageResponse.Common.NOT_FOUND);
                }
            }
            // --------------------------------------------

            // Mapping logic from TEntity to TDto should be implemented here
            TDto dto = MapToDto(entity);

            return ResponseFactory.Success(dto, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<TDto>();
        }
    }

    public virtual async Task<ApiResponse<TDto>> CreateAsync(TCreateDTO createDto)
    {
        try
        {
            // [BƯỚC 1] Gọi Validate
            // A. Chạy FluentValidation (Check format tĩnh)
            // A. Fluent Validation (Tĩnh) -> Dùng _createValidator
            if (_createValidator != null)
            {
                var valResult = await _createValidator.ValidateAsync(createDto);
                if (!valResult.IsValid)
                    return ResponseFactory.Failure<TDto>(StatusCodeResponse.BadRequest, valResult.Errors[0].ErrorMessage);
            }

            // B. Chạy Business Logic (Check DB động)
            var logicResult = await ValidateCreateLogicAsync(createDto);
            if (!logicResult.IsValid)
            {
                return ResponseFactory.Failure<TDto>(logicResult.StatusCode, logicResult.Message);
            }

            // [BƯỚC 2] Logic lưu DB bình thường
            var entity = MapCreateToEntity(createDto);
            await _repo.AddAsync(entity);
            await _dbu.SaveChangesAsync();

            // Mapping logic from TEntity to TDto should be implemented here
            TDto dto = MapToDto(entity);

            return ResponseFactory.Success(dto, MessageResponse.Common.CREATE_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            // Log ex ở đây
            return ResponseFactory.Failure<TDto>(StatusCodeResponse.Error, ex.InnerException?.Message ?? ex.Message);
        }
    }

    public virtual async Task<ApiResponse<TDto>> UpdateAsync(int id, TUpdateDTO updateDto)
    {
        try
        {
            // A. Check Tồn tại
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return ResponseFactory.Failure<TDto>(StatusCodeResponse.NotFound, MessageResponse.Common.NOT_FOUND);

            // --- DÙNG REFLECTION ĐỂ KIỂM TRA ISDELETED ---
            var prop = typeof(TEntity).GetProperty("IsDeleted");
            if (prop != null && (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?)))
            {
                var value = prop.GetValue(entity);
                if (value != null && (bool)value == true)
                {
                    return ResponseFactory.Failure<TDto>(StatusCodeResponse.NotFound, MessageResponse.Common.NOT_FOUND);
                }
            }

            // B. Fluent Validation (Tĩnh) -> Dùng _updateValidator [Lưu ý dùng updateDto]
            if (_updateValidator != null)
            {
                var valResult = await _updateValidator.ValidateAsync(updateDto);
                if (!valResult.IsValid)
                    return ResponseFactory.Failure<TDto>(StatusCodeResponse.BadRequest, valResult.Errors[0].ErrorMessage);
            }

            // C. Business Logic
            var logicResult = await ValidateUpdateLogicAsync(updateDto, id);
            if (!logicResult.IsValid)
                return ResponseFactory.Failure<TDto>(logicResult.StatusCode, logicResult.Message);

            // [BƯỚC 2] Update logic bình thường
            MapUpdateToEntity(updateDto, entity);
            await _repo.UpdateAsync(entity);
            await _dbu.SaveChangesAsync();

            // Mapping logic from TEntity to TDto should be implemented here
            TDto dto = MapToDto(entity);

            return ResponseFactory.Success(dto, MessageResponse.Common.UPDATE_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<TDto>();
        }
    }

    public virtual async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
            {
                return ResponseFactory.Failure<bool>(StatusCodeResponse.NotFound, MessageResponse.Common.NOT_FOUND);
            }

            // Check xem entity có phải là ISoftDelete không
            // --- XÓA MỀM (Cực nhanh, ko cần Reflection) ---
            // Dùng Reflection để tìm cột IsDeleted

            var isDeletedProp = typeof(TEntity).GetProperty("IsDeleted");

            // Kiểm tra xem có cột IsDeleted hay không (hỗ trợ cả bool và bool?)
            if (isDeletedProp != null &&
               (isDeletedProp.PropertyType == typeof(bool) || isDeletedProp.PropertyType == typeof(bool?)))
            {
                // --- LOGIC XÓA MỀM ---
                // Set giá trị là true (Dù là bool? thì gán true vẫn hợp lệ)
                isDeletedProp.SetValue(entity, true);

                await _repo.UpdateAsync(entity);
                await _dbu.SaveChangesAsync();

                return ResponseFactory.Success(true, MessageResponse.Common.DELETE_SUCCESSFULLY);
            }
            else
            {
                // --- KHÔNG XÓA CỨNG ---
                // Nếu bảng không có cột IsDeleted, ta trả về lỗi báo cho Developer biết
                // để tránh việc xóa nhầm dữ liệu quan trọng.
                return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.Common.DELETE_FAILED);
            }
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<bool>();
        }
    }
}