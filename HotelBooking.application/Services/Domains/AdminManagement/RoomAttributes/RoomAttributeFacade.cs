using FluentValidation;
using HotelBooking.application.Helpers;

public interface IRoomAttributeFacade
{
    // Gom nhóm 1: Các thuộc tính cơ bản
    IUnitTypeService UnitTypeService { get; }
    IBedTypeService BedTypeService { get; }
    IRoomViewService RoomViewService { get; }
    IRoomQualityService RoomQualityService { get; }

    // --- HÀM: LẤY DANH SÁCH PHÂN TRANG THEO ENUM ---
    Task<ApiResponse<PagedManageResult<RoomAttributeDTO>>> GetPagedByTypeAsync(
        GetRoomAttributeRequest request);
}


public class RoomAttributeFacade : IRoomAttributeFacade
{
    public IUnitTypeService UnitTypeService { get; private set; }
    public IBedTypeService BedTypeService { get; private set; }
    public IRoomViewService RoomViewService { get; private set; }
    public IRoomQualityService RoomQualityService { get; private set; }

    private readonly IValidator<GetRoomAttributeRequest> _validator;

    public RoomAttributeFacade(IUnitTypeService unitTypeService, IBedTypeService bedTypeService, IRoomViewService roomViewService, IRoomQualityService roomQualityService, IValidator<GetRoomAttributeRequest> validator)
    {
        UnitTypeService = unitTypeService;
        BedTypeService = bedTypeService;
        RoomViewService = roomViewService;
        RoomQualityService = roomQualityService;

        _validator = validator;
    }

    public async Task<ApiResponse<PagedManageResult<RoomAttributeDTO>>> GetPagedByTypeAsync(
        GetRoomAttributeRequest request)
    {
        // --- BƯỚC 1: VALIDATION ĐẦU VÀO ---
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
                    // Gọi Manager con
                    var r1 = await UnitTypeService.GetPagedListAsync(request.Paging);
                    // Convert kết quả con sang kết quả cha (xem hàm Helper bên dưới)
                    return ConvertToBasePagedResult(r1);

                case RoomAttributeType.BedType:
                    var r2 = await BedTypeService.GetPagedListAsync(request.Paging);
                    return ConvertToBasePagedResult(r2);

                case RoomAttributeType.RoomView:
                    var r3 = await RoomViewService.GetPagedListAsync(request.Paging);
                    return ConvertToBasePagedResult(r3);

                case RoomAttributeType.RoomQuality:
                    // RoomQuality cần TypeId
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

    // --- HELPER CHUYỂN ĐỔI KẾT QUẢ TỪ CON SANG CHA ---
    // T: Kiểu dữ liệu con (VD: UnitTypeDTO)
    // Hàm này giúp Facade trả về kiểu chung RoomAttributeDTO
    private ApiResponse<PagedManageResult<RoomAttributeDTO>> ConvertToBasePagedResult<T>(
        ApiResponse<PagedManageResult<T>> sourceResponse) where T : RoomAttributeDTO
    {
        // Nếu API con thất bại, trả về lỗi y hệt
        if (sourceResponse.StatusCode != StatusCodeResponse.Success || sourceResponse.Content == null)
        {
            return ResponseFactory.Failure<PagedManageResult<RoomAttributeDTO>>(
                sourceResponse.StatusCode,
                sourceResponse.Message);
        }

        var sourceContent = sourceResponse.Content;

        // Sử dụng Constructor thay vì Object Initializer
        var baseResult = new PagedManageResult<RoomAttributeDTO>(
            // 1. Items (Cast từ con sang cha)
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

        // Lưu ý: Không cần truyền TotalPages, vì Constructor sẽ tự tính dựa trên Count và Size.
        return ResponseFactory.Success(baseResult, sourceResponse.Message);
    }
}