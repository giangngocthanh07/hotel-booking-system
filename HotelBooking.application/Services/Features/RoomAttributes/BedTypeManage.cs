using System.Text.Json;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;


public interface IBedTypeManage : IStandardManage<BedTypeDTO, BedTypeCreateOrUpdateDTO>
{
    Task<ApiResponse<PagedManageResult<BedTypeDTO>>> GetPagedListAsync(PagingRequest paging);
}

public class BedTypeManage : BaseManage<BedType, IBedTypeRepository, BedTypeDTO, BedTypeCreateOrUpdateDTO>, IBedTypeManage
{
    public BedTypeManage(IBedTypeRepository repo, IUnitOfWork dbu) : base(repo, dbu)
    {
    }

    // Map Entity -> DTO (Hiển thị ra UI)
    protected override BedTypeDTO MapToDto(BedType entity)
    {
        return new BedTypeDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            // Field riêng
            DefaultCapacity = entity.DefaultCapacity ?? 1
        };
    }

    // Map CreateDTO -> Entity (Tạo mới)
    protected override BedType MapToEntity(BedTypeCreateOrUpdateDTO createDto)
    {
        return new BedType
        {
            Name = createDto.Name,
            IsDeleted = false,
            Description = createDto.Description,
            // Field riêng
            DefaultCapacity = createDto.DefaultCapacity
        };
    }

    // Map UpdateDTO -> Entity (Cập nhật)
    protected override void MapToEntity(BedTypeCreateOrUpdateDTO updateDto, BedType entity)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
        // Field riêng
        entity.DefaultCapacity = updateDto.DefaultCapacity;
    }

    // Validation
    protected override async Task<ValidationResult> ValidateAsync(BedTypeCreateOrUpdateDTO dto, int? id = null)
    {
        var basicValidation = ValidateFactory.ValidateFullAsync<BedType>(
            _repo,
            dto.Name,
            id,
            typeId: null,
            getEntityIsDeletedFunc: x => x.IsDeleted,
            isDeletedSelector: x => x.IsDeleted,
            nameSelector: x => x.Name);
        return await basicValidation;
    }

    public async Task<ApiResponse<List<BedTypeDTO>>> GetAllAsync()
    {
        var btList = await _repo.WhereAsync(bt => bt.IsDeleted == false);

        if (btList == null || btList.Count() == 0)
        {
            return ResponseFactory.Failure<List<BedTypeDTO>>(StatusCodeResponse.NotFound, MessageResponse.EMPTY_LIST);
        }

        try
        {
            var result = btList.Select(bt => MapToDto(bt)).ToList();

            return ResponseFactory.Success(result, MessageResponse.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<BedTypeDTO>>();
        }
    }

    // --- IMPLEMENT HÀM: LẤY DANH SÁCH PHÂN TRANG ---
    public async Task<ApiResponse<PagedManageResult<BedTypeDTO>>> GetPagedListAsync(PagingRequest paging)
    {
        try
        {
            // 1. Validate tham số phân trang (Trang 1, Size 10...)
            var pagingCheck = ValidateFactory.ValidatePaging(paging);
            if (!pagingCheck.IsValid)
            {
                return ResponseFactory.Failure<PagedManageResult<BedTypeDTO>>(
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
            var result = new PagedManageResult<BedTypeDTO>(
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
            return ResponseFactory.ServerError<PagedManageResult<BedTypeDTO>>();
        }
    }
}