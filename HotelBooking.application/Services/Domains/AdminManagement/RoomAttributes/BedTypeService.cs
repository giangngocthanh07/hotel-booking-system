using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;
using HotelBooking.application.Interfaces;


public interface IBedTypeService : IStandardManage<BedTypeDTO, BedTypeCreateDTO, BedTypeUpdateDTO>
{
    Task<ApiResponse<PagedManageResult<BedTypeDTO>>> GetPagedListAsync(PagingRequest paging);
}

public class BedTypeService : BaseManage<BedType, IBedTypeRepository, BedTypeDTO, BedTypeCreateDTO, BedTypeUpdateDTO>, IBedTypeService
{
    private readonly IValidator<PagingRequest> _pagingValidator;

    public BedTypeService(IBedTypeRepository repo, IUnitOfWork dbu, IValidator<BedTypeCreateDTO> createVal,
            IValidator<BedTypeUpdateDTO> updateVal,
            IValidator<PagingRequest> pagingValidator) : base(repo, dbu, createVal, updateVal)
    {
        _pagingValidator = pagingValidator;
    }

    // Map Entity -> DTO (Display to UI) 
    protected override BedTypeDTO MapToDto(BedType entity)
    {
        // Call Helper to extract
        var extraData = BedTypeHelper.MapToAdditionalData(entity.Additional);

        return new BedTypeDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            // Separate field
            DefaultCapacity = entity.DefaultCapacity ?? 1,
            // Map parameters from JSON
            MinWidth = extraData.MinWidth,
            MaxWidth = extraData.MaxWidth
        };
    }

    // Map CreateDTO -> Entity (Create new)
    protected override BedType MapCreateToEntity(BedTypeCreateDTO createDto)
    {
        return new BedType
        {
            Name = createDto.Name,
            IsDeleted = false,
            Description = createDto.Description,
            // Separate field
            DefaultCapacity = createDto.DefaultCapacity,
            // Pack parameters into JSON Additional
            Additional = BedTypeHelper.MapToAdditionalJson(createDto)
        };
    }

    // Map UpdateDTO -> Entity (Update)
    protected override void MapUpdateToEntity(BedTypeUpdateDTO updateDto, BedType entity)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
        // Separate field
        entity.DefaultCapacity = updateDto.DefaultCapacity;
        // Update JSON string
        entity.Additional = BedTypeHelper.MapToAdditionalJson(updateDto);
    }

    // Validation
    protected override async Task<ValidationResult> ValidateCreateLogicAsync(BedTypeCreateDTO dto)
    {
        // Check for duplicate name (Only within BedType group)
        bool isDuplicate = await _repo.AnyAsync(x =>
            x.Name == dto.Name);

        if (isDuplicate) return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.BedType.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    protected override async Task<ValidationResult> ValidateUpdateLogicAsync(BedTypeUpdateDTO dto, int id)
    {
        // Check for duplicate name (Only within BedType group, excluding itself)
        bool isDuplicate = await _repo.AnyAsync(x =>
            x.Name == dto.Name &&
            x.Id != id &&
            x.IsDeleted == false);

        if (isDuplicate)
            return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.BedType.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    public async Task<ApiResponse<List<BedTypeDTO>>> GetAllAsync()
    {
        var btList = await _repo.WhereAsync(bt => bt.IsDeleted == false);

        if (btList == null || btList.Count() == 0)
        {
            return ResponseFactory.Failure<List<BedTypeDTO>>(StatusCodeResponse.NotFound, MessageResponse.Common.EMPTY_LIST);
        }

        try
        {
            var result = btList.Select(bt => MapToDto(bt)).ToList();

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<BedTypeDTO>>();
        }
    }

    // --- IMPLEMENT METHOD: GET PAGED LIST ---
    public async Task<ApiResponse<PagedManageResult<BedTypeDTO>>> GetPagedListAsync(PagingRequest paging)
    {
        try
        {
            // [STEP 1] Use FluentValidation
            // ValidateAsync checks for null, > 0, max size... very cleanly
            var validationResult = await _pagingValidator.ValidateAsync(paging);

            if (!validationResult.IsValid)
            {
                // Get the first error and return
                return ResponseFactory.Failure<PagedManageResult<BedTypeDTO>>(
                    StatusCodeResponse.BadRequest,
                    validationResult.Errors[0].ErrorMessage);
            }

            // [STEP 2] Call Repository to get paged data
            // Filter: Get all that are not deleted (!IsDeleted)
            // OrderBy: Sort by ID descending (Newest first)
            var (items, totalCount) = await _repo.GetPagedAsync(
                pageIndex: paging.PageIndex!.Value,
                pageSize: paging.PageSize!.Value,
                filter: x => x.IsDeleted == false,
                orderBy: q => q.OrderByDescending(x => x.Id)
            );

            // [STEP 3] Map Entity to DTO
            // Use the pre-written MapToDto method in this class
            var dtos = items.Select(MapToDto).ToList();

            // [STEP 4] Package the result
            // [IMPORTANT] Only TotalCount and PageSize need to be passed, TotalPages will be calculated automatically
            var result = new PagedManageResult<BedTypeDTO>(
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
            // Log error if necessary
            return ResponseFactory.ServerError<PagedManageResult<BedTypeDTO>>();
        }
    }
}