using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Helpers;

public interface IRoomAttributeFacade
{

    // --- METHOD: GET PAGED LIST BY ENUM ---
    Task<ApiResponse<PagedManageResult<RoomAttributeDTO>>> GetPagedByTypeAsync(
        GetRoomAttributeRequest request);

    // 
    Task<bool> IsUnitTypeExistedAsync(int id);
    Task<bool> IsRoomQualityExistedAsync(int id);
    Task<bool> IsRoomViewExistedAsync(int id);
    Task<bool> IsBedTypeExistedAsync(int id);

    // 
    Task<RoomAttributeNamesDTO> GetRoomAttributeNamesAsync(RoomNameSuggestionRequest request);

}

public class RoomAttributeFacade : IRoomAttributeFacade
{
    public IUnitTypeService UnitTypeService { get; private set; }
    public IBedTypeService BedTypeService { get; private set; }
    public IRoomViewService RoomViewService { get; private set; }
    public IRoomQualityService RoomQualityService { get; private set; }
    private readonly IBedTypeRepository _bedTypeRepo;

    private readonly IValidator<GetRoomAttributeRequest> _validator;

    public RoomAttributeFacade(IUnitTypeService unitTypeService, IBedTypeService bedTypeService, IRoomViewService roomViewService, IRoomQualityService roomQualityService, IBedTypeRepository bedTypeRepo, IValidator<GetRoomAttributeRequest> validator)
    {
        UnitTypeService = unitTypeService;
        BedTypeService = bedTypeService;
        RoomViewService = roomViewService;
        RoomQualityService = roomQualityService;
        _bedTypeRepo = bedTypeRepo;

        _validator = validator;
    }

    public async Task<ApiResponse<PagedManageResult<RoomAttributeDTO>>> GetPagedByTypeAsync(
        GetRoomAttributeRequest request)
    {
        // --- STEP 1: VALIDATION INPUT ---
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return ResponseFactory.Failure<PagedManageResult<RoomAttributeDTO>>(
                StatusCodeResponse.BadRequest,
                validationResult.Errors[0].ErrorMessage
            );
        }

        try
        {
            switch (request.Type)
            {
                case RoomAttributeType.UnitType:
                    // Call child manager
                    var r1 = await UnitTypeService.GetPagedListAsync(request.Paging);
                    // Convert child result to parent result (see helper function below)
                    return ConvertToBasePagedResult(r1);

                case RoomAttributeType.BedType:
                    var r2 = await BedTypeService.GetPagedListAsync(request.Paging);
                    return ConvertToBasePagedResult(r2);

                case RoomAttributeType.RoomView:
                    var r3 = await RoomViewService.GetPagedListAsync(request.Paging);
                    return ConvertToBasePagedResult(r3);

                case RoomAttributeType.RoomQuality:
                    // RoomQuality needs TypeId
                    var r4 = await RoomQualityService.GetRoomQualitiesByTypeAsync(request.TypeId, request.Paging);
                    return ConvertToBasePagedResult(r4);

                default:
                    return ResponseFactory.Failure<PagedManageResult<RoomAttributeDTO>>(
                       StatusCodeResponse.BadRequest,
                       MessageResponse.Common.BAD_REQUEST);
            }
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<PagedManageResult<RoomAttributeDTO>>();
        }
    }

    // --- IMPLEMENTATION: CHECK EXISTENCE ---
    // Note: These methods are used by RoomTypeService to validate foreign keys (UnitTypeId, QualityId, RoomViewId, BedTypeId)
    public async Task<bool> IsUnitTypeExistedAsync(int id)
    {
        var response = await UnitTypeService.UnitTypeExistsAsync(id);
        return response.Content;
    }

    public async Task<bool> IsRoomQualityExistedAsync(int id)
    {
        var response = await RoomQualityService.RoomQualityExistsAsync(id);
        return response.Content;
    }

    public async Task<bool> IsRoomViewExistedAsync(int id)
    {
        var response = await RoomViewService.RoomViewExistsAsync(id);
        return response.Content;
    }

    public async Task<bool> IsBedTypeExistedAsync(int id)
    {
        var response = await BedTypeService.BedTypeExistsAsync(id);
        return response.Content;
    }

    public async Task<RoomAttributeNamesDTO> GetRoomAttributeNamesAsync(RoomNameSuggestionRequest request)
    {
        try
        {
            var unitTypeNameTask = UnitTypeService.GetUnitTypeNameByIdAsync(request.UnitTypeId);
            var roomQualityNameTask = request.QualityId.HasValue
                ? RoomQualityService.GetRoomQualityNameByIdAsync(request.QualityId.Value)
                : Task.FromResult(ResponseFactory.Failure<string>(StatusCodeResponse.NotFound, string.Empty));
            var roomViewNameTask = request.RoomViewId.HasValue ? RoomViewService.GetRoomViewNameByIdAsync(request.RoomViewId.Value) : Task.FromResult(ResponseFactory.Failure<string>(StatusCodeResponse.NotFound, string.Empty));

            var bedTypeTasks = request.BedTypes
            .Select(b => BedTypeService.GetBedTypeNameByIdAsync(b.BedTypeId))
            .ToList();


            await Task.WhenAll(unitTypeNameTask, roomQualityNameTask, roomViewNameTask, Task.WhenAll(bedTypeTasks));

            var bedTypeNames = bedTypeTasks
            .Select((task, index) => new BedTypeNameDTO
            {
                Name = task.Result.Content ?? string.Empty,
                Quantity = request.BedTypes[index].Quantity
            })
            .ToList();

            return new RoomAttributeNamesDTO
            {
                UnitTypeName = unitTypeNameTask.Result.Content ?? string.Empty,
                QualityName = roomQualityNameTask.Result?.Content,
                RoomViewName = roomViewNameTask.Result?.Content,
                BedTypeNames = bedTypeNames
            };
        }
        catch (Exception)
        {
            return new RoomAttributeNamesDTO
            {
                UnitTypeName = string.Empty,
                QualityName = string.Empty,
                RoomViewName = string.Empty,
                BedTypeNames = new List<BedTypeNameDTO>()
            };
        }
    }

    // --- HELPER: CONVERT CHILD RESULT TO PARENT RESULT ---
    // T: Child data type (e.g: UnitTypeDTO)
    // This function helps Facade return a common type: RoomAttributeDTO
    private ApiResponse<PagedManageResult<RoomAttributeDTO>> ConvertToBasePagedResult<T>(
        ApiResponse<PagedManageResult<T>> sourceResponse) where T : RoomAttributeDTO
    {
        // If child API fails, return the same error
        if (sourceResponse.StatusCode != StatusCodeResponse.Success || sourceResponse.Content == null)
        {
            return ResponseFactory.Failure<PagedManageResult<RoomAttributeDTO>>(
                sourceResponse.StatusCode,
                sourceResponse.Message);
        }

        var sourceContent = sourceResponse.Content;

        // Use Constructor instead of Object Initializer
        var baseResult = new PagedManageResult<RoomAttributeDTO>(
            // 1. Items (Cast from child to parent)
            sourceContent.Items.Cast<RoomAttributeDTO>().ToList(),

            // 2. TotalCount
            sourceContent.TotalCount,

            // 3. PageIndex
            sourceContent.PageIndex,

            // 4. PageSize
            sourceContent.PageSize,

            // 5. SelectedTypeId
            sourceContent.SelectedTypeId
        );

        // Note: No need to pass TotalPages, because Constructor will calculate automatically based on Count and Size.
        return ResponseFactory.Success(baseResult, sourceResponse.Message);
    }
}