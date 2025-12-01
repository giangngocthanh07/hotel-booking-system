using System.Linq.Expressions;
using HotelBooking.application.Helpers;

public interface IBaseManage<TEntity, TRepo, TDto, TCreateOrUpdateDTO> : ICommonManage<TDto, TCreateOrUpdateDTO>
{
}

public abstract class BaseManage<TEntity, TRepo, TDto, TCreateOrUpdateDTO> : IBaseManage<TEntity, TRepo, TDto, TCreateOrUpdateDTO> where TEntity : class where TRepo : IRepository<TEntity>
{
    // Implementation of base management functionalities
    public readonly TRepo _repo;
    public IUnitOfWork _dbu;

    public BaseManage(TRepo repo, IUnitOfWork dbu)
    {
        _repo = repo;
        _dbu = dbu;
    }

    // --- ĐỊNH NGHĨA 3 HÀM BẮT BUỘC CON PHẢI LÀM ---

    // 1. Map từ Entity -> DTO (Dùng cho GetAll, GetById, return Create)
    protected abstract TDto MapToDto(TEntity entity);

    // 2. Map từ CreateDTO -> Entity (Dùng cho Create)
    protected abstract TEntity MapToEntity(TCreateOrUpdateDTO createDto);

    // 3. Map từ UpdateDTO vào Entity CÓ SẴN (Dùng cho Update, Deleted)
    protected abstract void MapToEntity(TCreateOrUpdateDTO updateDto, TEntity entity);

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

    private Expression<Func<TEntity, bool>> IsNotDeletedPredicate()
    {
        // 1. Tạo tham số x
        // Tương đương: x =>
        var param = Expression.Parameter(typeof(TEntity), "x");

        // 2. Tạo Member Expression
        // Tương đương: x.IsDeleted
        // Expression.Property trả về một đối tượng kiểu MemberExpression
        var prop = Expression.Property(param, "IsDeleted");

        // 3. Tạo hằng số
        // Tương đương: == true
        var trueValue = Expression.Constant(true, typeof(bool?));

        // 4. Xử lý ép kiểu (Không quan trọng, code cho an toàn)
        var propConverted = Expression.Convert(prop, typeof(bool?));

        // 5. Tạo phép so sánh (BinaryExpression)
        // Tương đương: x.IsDeleted != true;
        var notEqual = Expression.NotEqual(propConverted, trueValue);

        // 6. Đóng gói thành Lambda hoàn chỉnh
        return Expression.Lambda<Func<TEntity, bool>>(notEqual, param);
    }

    protected virtual async Task<IEnumerable<TEntity>> GetEntitiesInternalAsync()
    {
        // Dùng IsNotDeletedPredicate (Expression) để lọc
        var predicate = IsNotDeletedPredicate();

        // CÓ cột IsDeleted -> Dùng WhereAsync của Repo (Chạy SQL cực nhanh)
        if (predicate != null)
        {
            return await _repo.WhereAsync(predicate);
        }
        return await _repo.GetAllAsync();
    }
}