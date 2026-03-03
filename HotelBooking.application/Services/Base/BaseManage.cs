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

    // Inject validators (optional — can be null)
    // Inject two separate validators (both nullable)
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

    // --- 3 ABSTRACT METHODS THAT SUBCLASSES MUST IMPLEMENT ---

    // Factory methods to map between Entity, DTO, CreateDTO, UpdateDTO

    // 1. Map Entity -> DTO (used by GetAll, GetById, return Create)
    protected abstract TDto MapToDto(TEntity entity);

    // 2. Map CreateDTO -> Entity (used by Create)
    protected abstract TEntity MapCreateToEntity(TCreateDTO createDto);

    // 3. Map UpdateDTO onto an EXISTING Entity (used by Update, Delete)
    protected abstract void MapUpdateToEntity(TUpdateDTO updateDto, TEntity entity);

    // 4. --- VALIDATION METHODS ---

    // Validate business logic for Create (e.g., check duplicate name on new entry)
    protected virtual async Task<ValidationResult> ValidateCreateLogicAsync(TCreateDTO dto)
    {
        return new ValidationResult(); // Valid by default
    }

    // Validate business logic for Update (e.g., check duplicate name excluding current ID)
    protected virtual async Task<ValidationResult> ValidateUpdateLogicAsync(TUpdateDTO dto, int id)
    {
        return new ValidationResult(); // Valid by default
    }

    public virtual async Task<ApiResponse<TDto>> GetByIdAsync(int id)
    {
        try
        {
            var entity = await _repo.GetByIdAsync(id);

            // 1. Check for null first
            if (entity == null)
            {
                return ResponseFactory.Failure<TDto>(StatusCodeResponse.NotFound, MessageResponse.Common.NOT_FOUND);
            }

            // --- USE REFLECTION (same approach as AddAsync) ---
            var prop = typeof(TEntity).GetProperty("IsDeleted");
            if (prop != null && (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?)))
            {
                // Use GetValue (read) instead of SetValue (write)
                var value = prop.GetValue(entity);

                // If value is true, entity has been soft-deleted
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
            // [STEP 1] Run validation
            // A. Run FluentValidation (static format check)
            // A. Fluent Validation (static) -> uses _createValidator
            if (_createValidator != null)
            {
                var valResult = await _createValidator.ValidateAsync(createDto);
                if (!valResult.IsValid)
                    return ResponseFactory.Failure<TDto>(StatusCodeResponse.BadRequest, valResult.Errors[0].ErrorMessage);
            }

            // B. Run business logic validation (dynamic DB checks)
            var logicResult = await ValidateCreateLogicAsync(createDto);
            if (!logicResult.IsValid)
            {
                return ResponseFactory.Failure<TDto>(logicResult.StatusCode, logicResult.Message);
            }

            // [STEP 2] Normal DB persistence logic
            var entity = MapCreateToEntity(createDto);
            await _repo.AddAsync(entity);
            await _dbu.SaveChangesAsync();

            // Mapping logic from TEntity to TDto should be implemented here
            TDto dto = MapToDto(entity);

            return ResponseFactory.Success(dto, MessageResponse.Common.CREATE_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            // Log ex here if needed
            return ResponseFactory.Failure<TDto>(StatusCodeResponse.Error, ex.InnerException?.Message ?? ex.Message);
        }
    }

    public virtual async Task<ApiResponse<TDto>> UpdateAsync(int id, TUpdateDTO updateDto)
    {
        try
        {
            // A. Check existence
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return ResponseFactory.Failure<TDto>(StatusCodeResponse.NotFound, MessageResponse.Common.NOT_FOUND);

            // --- USE REFLECTION TO CHECK ISDELETED ---
            var prop = typeof(TEntity).GetProperty("IsDeleted");
            if (prop != null && (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?)))
            {
                var value = prop.GetValue(entity);
                if (value != null && (bool)value == true)
                {
                    return ResponseFactory.Failure<TDto>(StatusCodeResponse.NotFound, MessageResponse.Common.NOT_FOUND);
                }
            }

            // B. Fluent Validation (static) -> uses _updateValidator [Note: pass updateDto]
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

            // [STEP 2] Normal update logic
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

            // Check if entity supports soft-delete
            // --- SOFT DELETE (fast, no Reflection needed for the check itself) ---
            // Use Reflection to find the IsDeleted column

            var isDeletedProp = typeof(TEntity).GetProperty("IsDeleted");

            // Check if the IsDeleted column exists (supports both bool and bool?)
            if (isDeletedProp != null &&
               (isDeletedProp.PropertyType == typeof(bool) || isDeletedProp.PropertyType == typeof(bool?)))
            {
                // --- SOFT DELETE LOGIC ---
                // Set value to true (valid for both bool and bool?)
                isDeletedProp.SetValue(entity, true);

                await _repo.UpdateAsync(entity);
                await _dbu.SaveChangesAsync();

                return ResponseFactory.Success(true, MessageResponse.Common.DELETE_SUCCESSFULLY);
            }
            else
            {
                // --- NO HARD DELETE ---
                // If the table has no IsDeleted column, return an error to alert the developer
                // and prevent accidental deletion of important data.
                return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.Common.DELETE_FAILED);
            }
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<bool>();
        }
    }
}