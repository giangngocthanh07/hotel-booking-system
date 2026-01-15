using System.Linq.Expressions;
using HotelBooking.application.Helpers;

public interface IBaseManage<TEntity, TRepo, TDto, TCreateOrUpdateDTO> : ICommonManage<TDto, TCreateOrUpdateDTO>
{
}

public abstract class BaseManage<TEntity, TRepo, TDto, TCreateOrUpdateDTO> : IBaseManage<TEntity, TRepo, TDto, TCreateOrUpdateDTO>
    where TEntity : class
    where TRepo : IRepository<TEntity>
{
    // Implementation of base management functionalities
    protected readonly TRepo _repo;
    protected readonly IUnitOfWork _dbu;

    public BaseManage(TRepo repo, IUnitOfWork dbu)
    {
        _repo = repo;
        _dbu = dbu;
    }

    // --- ĐỊNH NGHĨA 3 HÀM BẮT BUỘC CON PHẢI LÀM ---

    // Các factory method để map giữa Entity, DTO, CreateDTO, UpdateDTO

    // 1. Map từ Entity -> DTO (Dùng cho GetAll, GetById, return Create)
    protected abstract TDto MapToDto(TEntity entity);

    // 2. Map từ CreateDTO -> Entity (Dùng cho Create)
    protected abstract TEntity MapToEntity(TCreateOrUpdateDTO createDto);

    // 3. Map từ UpdateDTO vào Entity CÓ SẴN (Dùng cho Update, Deleted)
    protected abstract void MapToEntity(TCreateOrUpdateDTO updateDto, TEntity entity);

    // 4. --- VALIDATION METHOD ---
    // Hook method để Validate dữ liệu trước khi Create/Update
    protected abstract Task<ValidationResult> ValidateAsync(TCreateOrUpdateDTO dto, int? id = null); 

    public virtual async Task<ApiResponse<TDto>> GetByIdAsync(int id)
    {
        try
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
            {
                return ResponseFactory.Failure<TDto>(StatusCodeResponse.NotFound, MessageResponse.NOT_FOUND);
            }
            // Mapping logic from TEntity to TDto should be implemented here
            TDto dto = MapToDto(entity);

            return ResponseFactory.Success(dto, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<TDto>();
        }
    }

    public virtual async Task<ApiResponse<TDto>> CreateAsync(TCreateOrUpdateDTO createDto)
    {
        try
        {
            // [BƯỚC 1] Gọi Validate
            var validateResult = await ValidateAsync(createDto);

            // Nếu toang -> Trả về lỗi ngay lập tức
            if (!validateResult.IsValid)
            {
                return ResponseFactory.Failure<TDto>(validateResult.StatusCode, validateResult.Message);
            }

            // [BƯỚC 2] Logic lưu DB bình thường
            var entity = MapToEntity(createDto);
            await _repo.AddAsync(entity);
            await _dbu.SaveChangesAsync();

            // Mapping logic from TEntity to TDto should be implemented here
            TDto dto = MapToDto(entity);

            return ResponseFactory.Success(dto, MessageResponse.CREATE_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<TDto>();
        }
    }

    public virtual async Task<ApiResponse<TDto>> UpdateAsync(int id, TCreateOrUpdateDTO updateDto)
    {
        try
        {
            var entity = await _repo.GetByIdAsync(id);

            // [BƯỚC 1] Gọi Hook Validate Update (Truyền ID để check logic trùng lặp ngoại trừ chính nó)
            var validateResult = await ValidateAsync(updateDto, id);

            if (!validateResult.IsValid)
            {
                return ResponseFactory.Failure<TDto>(validateResult.StatusCode, validateResult.Message);
            }

            // [BƯỚC 2] Update logic bình thường
            MapToEntity(updateDto, entity);
            await _repo.UpdateAsync(entity);
            await _dbu.SaveChangesAsync();

            // Mapping logic from TEntity to TDto should be implemented here
            TDto dto = MapToDto(entity);

            return ResponseFactory.Success(dto, MessageResponse.UPDATE_SUCCESSFULLY);
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
                return ResponseFactory.Failure<bool>(StatusCodeResponse.NotFound, MessageResponse.NOT_FOUND);
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

                return ResponseFactory.Success(true, MessageResponse.DELETE_SUCCESSFULLY);
            }
            else
            {
                // --- KHÔNG XÓA CỨNG ---
                // Nếu bảng không có cột IsDeleted, ta trả về lỗi báo cho Developer biết
                // để tránh việc xóa nhầm dữ liệu quan trọng.
                return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.DELETE_FAILED);
            }
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<bool>();
        }
    }
}