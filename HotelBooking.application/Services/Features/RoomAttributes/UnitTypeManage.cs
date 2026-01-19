using System.Text.Json;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IUnitTypeManage : IStandardManage<UnitTypeDTO, UnitTypeCreateOrUpdateDTO>
{
    Task<ApiResponse<PagedManageResult<UnitTypeDTO>>> GetPagedListAsync(PagingRequest paging);
}

public class UnitTypeManage : BaseManage<UnitType, IUnitTypeRepository, UnitTypeDTO, UnitTypeCreateOrUpdateDTO>, IUnitTypeManage
{
    public UnitTypeManage(IUnitTypeRepository repo, IUnitOfWork dbu) : base(repo, dbu)
    {
    }

    // Map Entity -> DTO (Hiển thị ra UI)
    protected override UnitTypeDTO MapToDto(UnitType entity)
    {
        return new UnitTypeDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            // Field riêng của UnitType
            IsEntirePlace = entity.IsEntirePlace
        };
    }

    // Map CreateDTO -> Entity (Tạo mới)
    protected override UnitType MapToEntity(UnitTypeCreateOrUpdateDTO createDto)
    {
        return new UnitType
        {
            Name = createDto.Name,
            IsDeleted = false,
            IsEntirePlace = createDto.IsEntirePlace
        };
    }

    // Map UpdateDTO -> Entity (Cập nhật)
    protected override void MapToEntity(UnitTypeCreateOrUpdateDTO updateDto, UnitType entity)
    {
        entity.Name = updateDto.Name;
        entity.IsEntirePlace = updateDto.IsEntirePlace;
    }

    // Validation
    protected override async Task<ValidationResult> ValidateAsync(UnitTypeCreateOrUpdateDTO dto, int? id = null)
    {
        var basicValidation = ValidateFactory.ValidateFullAsync<UnitType>(
            _repo,
            dto.Name,
            id,
            typeId: null,
            getEntityIsDeletedFunc: x => x.IsDeleted,
            isDeletedSelector: x => x.IsDeleted,
            nameSelector: x => x.Name);
        return await basicValidation;
    }

    public async Task<ApiResponse<List<UnitTypeDTO>>> GetAllAsync()
    {
        var utList = await _repo.WhereAsync(ut => ut.IsDeleted == false);

        if (utList == null || utList.Count() == 0)
        {
            return ResponseFactory.Failure<List<UnitTypeDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
        }

        try
        {
            var result = utList.Select(ut => MapToDto(ut)).ToList();
            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<UnitTypeDTO>>();
        }
    }

    // --- IMPLEMENT HÀM: LẤY DANH SÁCH PHÂN TRANG ---
    public async Task<ApiResponse<PagedManageResult<UnitTypeDTO>>> GetPagedListAsync(PagingRequest paging)
    {
        try
        {
            // 1. Validate tham số phân trang (Trang 1, Size 10...)
            var pagingCheck = ValidateFactory.ValidatePaging(paging);
            if (!pagingCheck.IsValid)
            {
                return ResponseFactory.Failure<PagedManageResult<UnitTypeDTO>>(
                    pagingCheck.StatusCode,
                    pagingCheck.Message);
            }

            // 2. Gọi Repository lấy dữ liệu phân trang
            // Filter: Lấy tất cả cái chưa xóa (!IsDeleted)
            // OrderBy: Sắp xếp theo ID giảm dần (Mới nhất lên đầu)
            var (items, totalCount) = await _repo.GetPagedAsync(
                pageIndex: paging.PageIndex!.Value,
                pageSize: paging.PageSize!.Value,
                filter: x => x.IsDeleted == false,
                orderBy: q => q.OrderByDescending(x => x.Id)
            );

            // 3. Map Entity sang DTO
            // Sử dụng hàm MapToDto đã viết sẵn trong class này
            var dtos = items.Select(MapToDto).ToList();

            // 4. Đóng gói kết quả
            // [QUAN TRỌNG] Chỉ cần truyền TotalCount và PageSize, TotalPages sẽ tự động được tính
            var result = new PagedManageResult<UnitTypeDTO>(
                dtos,
                totalCount,
                paging.PageIndex.Value,
                paging.PageSize.Value,
                null // SelectedTypeId = null
            );

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            // Log lỗi nếu cần thiết
            return ResponseFactory.ServerError<PagedManageResult<UnitTypeDTO>>();
        }
    }
}