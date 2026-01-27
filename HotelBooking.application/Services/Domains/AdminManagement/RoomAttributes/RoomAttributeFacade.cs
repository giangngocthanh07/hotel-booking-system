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
        RoomAttributeType type,
        PagingRequest paging,
        int? typeId = null);
}


public class RoomAttributeFacade : IRoomAttributeFacade
{
    public IUnitTypeService UnitTypeService { get; private set; }
    public IBedTypeService BedTypeService { get; private set; }
    public IRoomViewService RoomViewService { get; private set; }
    public IRoomQualityService RoomQualityService { get; private set; }

    public RoomAttributeFacade(IUnitTypeService unitTypeService, IBedTypeService bedTypeService, IRoomViewService roomViewService, IRoomQualityService roomQualityService)
    {
        UnitTypeService = unitTypeService;
        BedTypeService = bedTypeService;
        RoomViewService = roomViewService;
        RoomQualityService = roomQualityService;
    }

    public async Task<ApiResponse<PagedManageResult<RoomAttributeDTO>>> GetPagedByTypeAsync(
        RoomAttributeType type,
        PagingRequest paging,
        int? typeId = null)
    {
        // --- BƯỚC 1: VALIDATION ĐẦU VÀO ---
        // Sử dụng chuỗi check liên hoàn. Nếu cái đầu null (OK) thì check cái sau.
        // Nếu có lỗi, biến validationError sẽ giữ lỗi đó.
        var validationError = ValidateFactory.BasicCheck(
            // 1. Check Module có hợp lệ trong Enum không
            ValidateFactory.Require(type, x => Enum.IsDefined(typeof(RoomAttributeType), x),
                MessageResponse.BAD_REQUEST,
                StatusCodeResponse.BadRequest)
            ,
            // 2. Check typeId: Nếu có giá trị thì phải > 0
            ValidateFactory.Require(typeId, x => x == null || x > 0,
                MessageResponse.BAD_REQUEST,
                StatusCodeResponse.BadRequest)
            ,

            // 3. Check RoomAttributeType có typeId hay không
            ValidateFactory.Require(type, x =>
            {
                // Chỉ những loại này mới cần typeId
                var typesRequiringTypeId = new List<RoomAttributeType>
                {
                    RoomAttributeType.RoomQuality
                };
                if (typesRequiringTypeId.Contains(x))
                {
                    return typeId == null || typeId > 0;
                }
                else
                {
                    // TRƯỜNG HỢP 2: Nếu là loại 1, 2, 3 -> BẮT BUỘC typeId phải là NULL
                    // Nếu người dùng nhập typeId vào -> Lỗi
                    return typeId == null;
                }
            },
                MessageResponse.BAD_REQUEST,
                StatusCodeResponse.BadRequest)
        );

        // Nếu phát hiện lỗi -> Return ngay lập tức
        if (!validationError.IsValid)
        {
            return ResponseFactory.Failure<PagedManageResult<RoomAttributeDTO>>(
                validationError.StatusCode,
                validationError.Message
            );
        }

        try
        {
            switch (type)
            {
                case RoomAttributeType.UnitType:
                    // Gọi Manager con
                    var r1 = await UnitTypeService.GetPagedListAsync(paging);
                    // Convert kết quả con sang kết quả cha (xem hàm Helper bên dưới)
                    return ConvertToBasePagedResult(r1);

                case RoomAttributeType.BedType:
                    var r2 = await BedTypeService.GetPagedListAsync(paging);
                    return ConvertToBasePagedResult(r2);

                case RoomAttributeType.RoomView:
                    var r3 = await RoomViewService.GetPagedListAsync(paging);
                    return ConvertToBasePagedResult(r3);

                case RoomAttributeType.RoomQuality:
                    // RoomQuality cần TypeId
                    var r4 = await RoomQualityService.GetRoomQualitiesByTypeAsync(typeId, paging);
                    return ConvertToBasePagedResult(r4);

                default:
                    return ResponseFactory.Failure<PagedManageResult<RoomAttributeDTO>>(
                       StatusCodeResponse.BadRequest,
                       MessageResponse.BAD_REQUEST);
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