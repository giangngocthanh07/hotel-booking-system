using System.Linq.Expressions;
using HotelBooking.application.Helpers;

public static class ManagementAdminHelper
{
    // --- PHẦN 1: HELPER CHO API MENU (Lấy danh sách Types) ---
    public static async Task<ApiResponse<ManageMenuResult>> GetTypesForMenuAsync<TTypeEntity, TTypeRepo>(
        TTypeRepo typeRepo,
        // Filter: VD lấy những type chưa bị xóa
        Expression<Func<TTypeEntity, bool>> activeTypeFilter,
        // Func lấy Id
        Func<TTypeEntity, int> getTypeIdFunc,
        // Func lấy Name
        Func<TTypeEntity, string> getTypeNameFunc)

        where TTypeRepo : IRepository<TTypeEntity>
        where TTypeEntity : class
    {
        try
        {
            // 1. Query DB lấy Types
            var typeEntities = await typeRepo.WhereAsync(activeTypeFilter);

            // 2. Map sang DTO
            var menuResult = new ManageMenuResult
            {
                Types = typeEntities.Select(t => new ManageTypeDTO
                {
                    Id = getTypeIdFunc(t),
                    Name = getTypeNameFunc(t)
                }).ToList()
            };

            // 3. Logic chọn Default (Option)
            // Lấy cái đầu tiên làm gợi ý cho FE (nếu cần)
            menuResult.DefaultSelectedId = menuResult.Types.FirstOrDefault()?.Id;

            return ResponseFactory.Success(menuResult, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<ManageMenuResult>();
        }
    }

    // --- PHẦN 2: HELPER CHO API DATA (Lấy Items theo TypeId) ---
    public static async Task<ApiResponse<ManageDataResult<TDto>>> GetDataByTypeAsync<TEntity, TDto>(
        int? typeId,
        Func<Task<int?>> getDefaultIdFunc,
        // Logic kiểm tra ID có tồn tại trong DB không (Input: int -> Output: bool)
        Func<int, Task<bool>> checkTypeExistsFunc,
        // Logic lấy dữ liệu từ DB (Input: typeId -> Output: List Entity)
        Func<int, Task<IEnumerable<TEntity>>> getItemsByTypeIdFunc,
        // Logic map từ Entity sang DTO
        Func<TEntity, TDto> mapToDtoFunc)

        where TEntity : class
        where TDto : class
    {
        try
        {
            int currentTypeId;

            // --- VALIDATION 1: Check số âm hoặc bằng 0 ---
            if (typeId.HasValue && typeId <= 0)
            {
                return ResponseFactory.Failure<ManageDataResult<TDto>>(
                    StatusCodeResponse.BadRequest,
                    "TypeId must be greater than 0"
                );
            }

            // --- XỬ LÝ CHỌN ID ---
            if (typeId.HasValue)
            {
                // Case A: User truyền ID -> Phải kiểm tra xem ID này có thật không
                bool isExist = await checkTypeExistsFunc(typeId.Value);

                if (!isExist)
                {
                    // --- VALIDATION 2: ID không tồn tại trong DB ---
                    return ResponseFactory.Failure<ManageDataResult<TDto>>(
                        StatusCodeResponse.NotFound,
                        $"Type with Id {typeId} not found."
                    );
                }

                currentTypeId = typeId.Value;
            }
            else
            {
                // Case B: User không truyền (null) -> Tự lấy Default
                var defaultId = await getDefaultIdFunc();

                if (defaultId.HasValue)
                {
                    currentTypeId = defaultId.Value;
                }
                else
                {
                    // Case C: Bảng Type trống trơn -> Trả về list rỗng (Hợp lệ)
                    return ResponseFactory.Success(new ManageDataResult<TDto>
                    {
                        SelectedTypeId = null,
                        Items = new List<TDto>(),
                        TotalCount = 0
                    }, MessageResponse.EMPTY_LIST);
                }
            }

            // 3. Lấy Data
            var entities = await getItemsByTypeIdFunc(currentTypeId);

            var result = new ManageDataResult<TDto>
            {
                SelectedTypeId = currentTypeId,
                Items = entities.Select(e => mapToDtoFunc(e)).ToList(),
                TotalCount = entities.Count()
            };

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<ManageDataResult<TDto>>();
        }
    }
}