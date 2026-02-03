using FluentValidation;
using System.Linq.Expressions;
using HotelBooking.application.Helpers;
using HotelBooking.application.Validators.Common;

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
    public static async Task<ApiResponse<PagedManageResult<TDto>>> GetDataByTypeAsync<TEntity, TDto>(
        int? typeId,
        PagingRequest paging, // 1. Nhận tham số phân trang

        // Nếu user không truyền typeId -> chạy logic lấy ID mặc định này
        // Logic lấy ID mặc định (nếu typeId == null)
        Func<Task<int?>> getDefaultIdFunc,

        // Nếu user có truyền typeId -> chạy logic kiểm tra tồn tại này
        // Logic kiểm tra ID có tồn tại trong DB không (Input: int -> Output: bool)
        Func<int, Task<bool>> checkTypeExistsFunc,

        // 2. Hàm lấy dữ liệu trả về Tuple (Items, TotalCount)
        Func<int, int, int, Task<(IEnumerable<TEntity> Items, int TotalCount)>> getPagedItemsFunc,

        // Logic map từ Entity sang DTO
        Func<TEntity, TDto> mapToDtoFunc)

        where TEntity : class
        where TDto : class
    {
        try
        {
            // [BƯỚC 1] Validate Phân trang bằng Validator bạn vừa tạo
            // Vì Helper là static, ta khởi tạo Validator trực tiếp (Safe & Fast)
            var pagingValidator = new PagingRequestValidator();
            var validationResult = await pagingValidator.ValidateAsync(paging);

            if (!validationResult.IsValid)
            {
                // Lấy lỗi đầu tiên trả về
                return ResponseFactory.Failure<PagedManageResult<TDto>>(
                    StatusCodeResponse.BadRequest,
                    validationResult.Errors[0].ErrorMessage
                );
            }

            // [BƯỚC 2] Validate TypeId (Logic đơn giản check thủ công cho lẹ)
            if (typeId.HasValue && typeId <= 0)
            {
                return ResponseFactory.Failure<PagedManageResult<TDto>>(
                    StatusCodeResponse.BadRequest,
                    "TypeId phải lớn hơn 0"
                );
            }

            int currentTypeId;

            // --- XỬ LÝ CHỌN ID ---
            if (typeId.HasValue)
            {
                // Case A: User truyền ID -> Phải kiểm tra xem ID này có thật không
                bool isExist = await checkTypeExistsFunc(typeId.Value);

                if (!isExist)
                {
                    // --- VALIDATION 2: ID không tồn tại trong DB ---
                    return ResponseFactory.Failure<PagedManageResult<TDto>>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.NOT_FOUND
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
                    // Trường hợp không có Type nào trong DB -> Trả về rỗng
                    return ResponseFactory.Success(
                        new PagedManageResult<TDto>(new List<TDto>(), 0, paging.PageIndex.Value, paging.PageSize.Value, null),
                        MessageResponse.EMPTY_LIST
                    );
                }
            }

            // 3. LẤY DỮ LIỆU PHÂN TRANG (Gọi Repo)
            var (entities, totalCount) = await getPagedItemsFunc(currentTypeId, paging.PageIndex.Value, paging.PageSize.Value);

            // 4. MAP SANG DTO
            var dtos = entities.Select(e => mapToDtoFunc(e)).ToList();

            // 5. ĐÓNG GÓI KẾT QUẢ (Dùng PagedManageResult)
            // Truyền currentTypeId vào để FE biết đường highlight menu
            var result = new PagedManageResult<TDto>(dtos, totalCount, paging.PageIndex.Value, paging.PageSize.Value, currentTypeId);

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<PagedManageResult<TDto>>();
        }
    }
}