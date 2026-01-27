using System.Text.Json;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IRoomViewService : IStandardManage<RoomViewDTO, RoomViewCreateOrUpdateDTO>
{
    Task<ApiResponse<PagedManageResult<RoomViewDTO>>> GetPagedListAsync(PagingRequest paging);
}

public class RoomViewService : BaseManage<RoomView, IRoomViewRepository, RoomViewDTO, RoomViewCreateOrUpdateDTO>, IRoomViewService
{
    public RoomViewService(IRoomViewRepository repo, IUnitOfWork dbu) : base(repo, dbu)
    {
    }

    protected override RoomViewDTO MapToDto(RoomView entity)
    {
        return new RoomViewDTO
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    protected override RoomView MapToEntity(RoomViewCreateOrUpdateDTO createDto)
    {
        return new RoomView { Name = createDto.Name };
    }

    protected override void MapToEntity(RoomViewCreateOrUpdateDTO updateDto, RoomView entity)
    {
        entity.Name = updateDto.Name;
    }

    // Validation
    protected override async Task<ValidationResult> ValidateAsync(RoomViewCreateOrUpdateDTO dto, int? id = null)
    {
        var basicValidation = ValidateFactory.ValidateFullAsync<RoomView>(
            _repo,
            dto.Name,
            id,
            typeId: null,
            getEntityIsDeletedFunc: x => x.IsDeleted,
            isDeletedSelector: x => x.IsDeleted,
            nameSelector: x => x.Name);
        return await basicValidation;

    }

    public async Task<ApiResponse<List<RoomViewDTO>>> GetAllAsync()
    {
        var rvList = await _repo.WhereAsync(rv => rv.IsDeleted == false);

        if (rvList == null || rvList.Count() == 0)
        {
            return ResponseFactory.Failure<List<RoomViewDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
        }

        try
        {
            var result = rvList.Select(rv => MapToDto(rv)).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<RoomViewDTO>>();
        }
    }

    // --- IMPLEMENT HÀM: LẤY DANH SÁCH PHÂN TRANG ---
    public async Task<ApiResponse<PagedManageResult<RoomViewDTO>>> GetPagedListAsync(PagingRequest paging)
    {
        try
        {
            // 1. Validate tham số phân trang (Trang 1, Size 10...)
            var pagingCheck = ValidateFactory.ValidatePaging(paging);
            if (!pagingCheck.IsValid)
            {
                return ResponseFactory.Failure<PagedManageResult<RoomViewDTO>>(
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
            var result = new PagedManageResult<RoomViewDTO>(
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
            return ResponseFactory.ServerError<PagedManageResult<RoomViewDTO>>();
        }
    }
}