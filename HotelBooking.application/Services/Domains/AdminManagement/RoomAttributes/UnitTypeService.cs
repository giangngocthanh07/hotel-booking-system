using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;
using HotelBooking.application.Interfaces;

public interface IUnitTypeService : IStandardManage<UnitTypeDTO, UnitTypeCreateDTO, UnitTypeUpdateDTO>
{
    Task<ApiResponse<PagedManageResult<UnitTypeDTO>>> GetPagedListAsync(PagingRequest paging);
}

public class UnitTypeService : BaseManage<UnitType, IUnitTypeRepository, UnitTypeDTO, UnitTypeCreateDTO, UnitTypeUpdateDTO>, IUnitTypeService
{
    private readonly IValidator<PagingRequest> _pagingValidator;

    public UnitTypeService(IUnitTypeRepository repo, IUnitOfWork dbu, IValidator<UnitTypeCreateDTO> createVal, IValidator<UnitTypeUpdateDTO> updateVal, IValidator<PagingRequest> pagingValidator) : base(repo, dbu, createVal, updateVal)
    {
        _pagingValidator = pagingValidator;
    }

    // Map Entity -> DTO (UI Display) 
    protected override UnitTypeDTO MapToDto(UnitType entity)
    {
        return new UnitTypeDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsDeleted = entity.IsDeleted,
            // UnitType specific field
            IsEntirePlace = entity.IsEntirePlace
        };
    }

    // Map CreateDTO -> Entity (Creation)
    protected override UnitType MapCreateToEntity(UnitTypeCreateDTO createDto)
    {
        return new UnitType
        {
            Name = createDto.Name,
            Description = createDto.Description,
            IsDeleted = false,
            IsEntirePlace = createDto.IsEntirePlace
        };
    }

    // Map UpdateDTO -> Entity (Update)
    protected override void MapUpdateToEntity(UnitTypeUpdateDTO updateDto, UnitType entity)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
        entity.IsEntirePlace = updateDto.IsEntirePlace;
    }

    // Validation Logic
    protected override async Task<ValidationResult> ValidateCreateLogicAsync(UnitTypeCreateDTO dto)
    {
        bool exists = await _repo.AnyAsync(x => x.Name == dto.Name);

        if (exists)
            return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.UnitType.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    protected override async Task<ValidationResult> ValidateUpdateLogicAsync(UnitTypeUpdateDTO dto, int id)
    {
        bool exists = await _repo.AnyAsync(x =>
            x.Name == dto.Name &&
            x.Id != id &&
            x.IsDeleted == false);

        if (exists)
            return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.UnitType.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    public async Task<ApiResponse<List<UnitTypeDTO>>> GetAllAsync()
    {
        var utList = await _repo.WhereAsync(ut => ut.IsDeleted == false);

        if (utList == null || !utList.Any())
        {
            return ResponseFactory.Failure<List<UnitTypeDTO>>(StatusCodeResponse.NotFound, MessageResponse.Common.EMPTY_LIST);
        }

        try
        {
            var result = utList.Select(ut => MapToDto(ut)).ToList();
            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<UnitTypeDTO>>();
        }
    }

    // --- IMPLEMENTATION: GET PAGINATED LIST ---
    public async Task<ApiResponse<PagedManageResult<UnitTypeDTO>>> GetPagedListAsync(PagingRequest paging)
    {
        try
        {
            // [STEP 1] Using FluentValidation
            // ValidateAsync checks for null, > 0, max size, etc.
            var validationResult = await _pagingValidator.ValidateAsync(paging);

            if (!validationResult.IsValid)
            {
                // Return the first error found
                return ResponseFactory.Failure<PagedManageResult<UnitTypeDTO>>(
                    StatusCodeResponse.BadRequest,
                    validationResult.Errors[0].ErrorMessage);
            }

            // [STEP 2] Call Repository to fetch paginated data
            // Filter: Retrieve all active records (!IsDeleted)
            // OrderBy: Sort by ID descending (Newest first)
            var (items, totalCount) = await _repo.GetPagedAsync(
                pageIndex: paging.PageIndex!.Value,
                pageSize: paging.PageSize!.Value,
                filter: x => x.IsDeleted == false,
                orderBy: q => q.OrderByDescending(x => x.Id)
            );

            // [STEP 3] Map Entity to DTO
            // Using the pre-defined MapToDto method
            var dtos = items.Select(MapToDto).ToList();

            // [STEP 4] Wrap the result
            // [IMPORTANT] TotalPages is automatically calculated by providing TotalCount and PageSize
            var result = new PagedManageResult<UnitTypeDTO>(
                dtos,
                totalCount,
                paging.PageIndex.Value,
                paging.PageSize.Value,
                null // SelectedTypeId = null
            );

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            // Log the exception if necessary
            return ResponseFactory.ServerError<PagedManageResult<UnitTypeDTO>>();
        }
    }
}