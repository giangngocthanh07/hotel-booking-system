using HotelBooking.application.Helpers;

public interface IRoomAttributeFacade
{
    // Gom nhóm 1: Các thuộc tính cơ bản
    IUnitTypeManage UnitTypeManage { get; }
    IBedTypeManage BedTypeManage { get; }
    IRoomViewManage RoomViewManage { get; }
    IRoomQualityManage RoomQualityManage { get; }

    // Lấy List dựa vào enum
    Task<ApiResponse<IEnumerable<RoomAttributeDTO>>> GetAllByTypeAsync(RoomAttributeType type, int? typeId = null);
}

public class RoomAttributeFacade : IRoomAttributeFacade
{
    public IUnitTypeManage UnitTypeManage { get; private set; }
    public IBedTypeManage BedTypeManage { get; private set; }
    public IRoomViewManage RoomViewManage { get; private set; }
    public IRoomQualityManage RoomQualityManage { get; private set; }

    public RoomAttributeFacade(IUnitTypeManage unitTypeManage, IBedTypeManage bedTypeManage, IRoomViewManage roomViewManage, IRoomQualityManage roomQualityManage)
    {
        UnitTypeManage = unitTypeManage;
        BedTypeManage = bedTypeManage;
        RoomViewManage = roomViewManage;
        RoomQualityManage = roomQualityManage;
    }

    public async Task<ApiResponse<IEnumerable<RoomAttributeDTO>>> GetAllByTypeAsync(RoomAttributeType type, int? typeId = null)
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
            return ResponseFactory.Failure<IEnumerable<RoomAttributeDTO>>(
                validationError.StatusCode,
                validationError.Message
            );
        }

        try
        {
            IEnumerable<RoomAttributeDTO>? result = null;
            switch (type)
            {
                case RoomAttributeType.UnitType:
                    var r1 = await UnitTypeManage.GetAllAsync();
                    if (r1.StatusCode != StatusCodeResponse.Success)
                        return ResponseFactory.Failure<IEnumerable<RoomAttributeDTO>>(r1.StatusCode, r1.Message);

                    result = r1?.Content?.Cast<RoomAttributeDTO>(); // Ép kiểu về cha
                    break;
                case RoomAttributeType.BedType:
                    var r2 = await BedTypeManage.GetAllAsync();
                    if (r2.StatusCode != StatusCodeResponse.Success)
                        return ResponseFactory.Failure<IEnumerable<RoomAttributeDTO>>(r2.StatusCode, r2.Message);

                    result = r2?.Content?.Cast<RoomAttributeDTO>();
                    break;
                case RoomAttributeType.RoomView:
                    var r3 = await RoomViewManage.GetAllAsync();
                    if (r3.StatusCode != StatusCodeResponse.Success)
                        return ResponseFactory.Failure<IEnumerable<RoomAttributeDTO>>(r3.StatusCode, r3.Message);

                    result = r3?.Content?.Cast<RoomAttributeDTO>();
                    break;
                case RoomAttributeType.RoomQuality:
                    var r4 = await RoomQualityManage.GetAllByTypeAsync(typeId);
                    if (r4.StatusCode != StatusCodeResponse.Success)
                        return ResponseFactory.Failure<IEnumerable<RoomAttributeDTO>>(r4.StatusCode, r4.Message);

                    result = r4?.Content?.Cast<RoomAttributeDTO>();
                    break;
                default:
                    return ResponseFactory.Failure<IEnumerable<RoomAttributeDTO>>(StatusCodeResponse.BadRequest, MessageResponse.BAD_REQUEST);
            }
            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<IEnumerable<RoomAttributeDTO>>();
        }
    }
}