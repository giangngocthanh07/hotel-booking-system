using System.Text.Json;
using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IRoomViewService : IStandardManage<RoomViewDTO, RoomViewCreateDTO, RoomViewUpdateDTO>
{
    Task<ApiResponse<PagedManageResult<RoomViewDTO>>> GetPagedListAsync(PagingRequest paging);
}

public class RoomViewService : BaseManage<RoomView, IRoomViewRepository, RoomViewDTO, RoomViewCreateDTO, RoomViewUpdateDTO>, IRoomViewService
{
    private readonly IValidator<PagingRequest> _pagingValidator;

    public RoomViewService(IRoomViewRepository repo, IUnitOfWork dbu, IValidator<RoomViewCreateDTO> createVal,
            IValidator<RoomViewUpdateDTO> updateVal, IValidator<PagingRequest> pagingValidator) : base(repo, dbu, createVal, updateVal)
    {
        _pagingValidator = pagingValidator;
    }

    protected override RoomViewDTO MapToDto(RoomView entity)
    {
        return new RoomViewDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsDeleted = entity.IsDeleted
        };
    }

    protected override RoomView MapCreateToEntity(RoomViewCreateDTO createDto)
    {
        return new RoomView { Name = createDto.Name, Description = createDto.Description, IsDeleted = false };
    }

    protected override void MapUpdateToEntity(RoomViewUpdateDTO updateDto, RoomView entity)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
    }

    // Validation
    protected override async Task<ValidationResult> ValidateCreateLogicAsync(RoomViewCreateDTO dto)
    {
        bool exists = await _repo.AnyAsync(x =>
            x.Name == dto.Name);

        if (exists) return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.RoomView.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    protected override async Task<ValidationResult> ValidateUpdateLogicAsync(RoomViewUpdateDTO dto, int id)
    {
        bool exists = await _repo.AnyAsync(x =>
            x.Name == dto.Name &&
            x.Id != id &&
            x.IsDeleted == false);

        if (exists)
            return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.RoomView.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    public async Task<ApiResponse<List<RoomViewDTO>>> GetAllAsync()
    {
        var rvList = await _repo.WhereAsync(rv => rv.IsDeleted == false);

        if (rvList == null || rvList.Count() == 0)
        {
            return ResponseFactory.Failure<List<RoomViewDTO>>(StatusCodeResponse.NotFound, MessageResponse.Common.EMPTY_LIST);
        }

        try
        {
            var result = rvList.Select(rv => MapToDto(rv)).ToList();

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
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
            // [BƯỚC 1] Dùng FluentValidation
            // ValidateAsync check cả null, > 0, max size... cực sạch sẽ
            var validationResult = await _pagingValidator.ValidateAsync(paging);

            if (!validationResult.IsValid)
            {
                // Lấy lỗi đầu tiên trả về
                return ResponseFactory.Failure<PagedManageResult<RoomViewDTO>>(
                    StatusCodeResponse.BadRequest,
                    validationResult.Errors[0].ErrorMessage);
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

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            // Log lỗi nếu cần thiết
            return ResponseFactory.ServerError<PagedManageResult<RoomViewDTO>>();
        }
    }
}