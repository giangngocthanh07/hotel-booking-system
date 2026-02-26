using System.Text.Json;
using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IUnitTypeService : IStandardManage<UnitTypeDTO, UnitTypeCreateDTO, UnitTypeUpdateDTO>
{
    Task<ApiResponse<PagedManageResult<UnitTypeDTO>>> GetPagedListAsync(PagingRequest paging);
}

public class UnitTypeService : BaseManage<UnitType, IUnitTypeRepository, UnitTypeDTO, UnitTypeCreateDTO, UnitTypeUpdateDTO>, IUnitTypeService
{
    private readonly IValidator<PagingRequest> _pagingValidator;

    public UnitTypeService(IUnitTypeRepository repo, IUnitOfWork dbu, IValidator<UnitTypeCreateDTO> createVal, IValidator<UnitTypeUpdateDTO> updateVal, IValidator<PagingRequest> pagingValidator) : base(repo, dbu, createVal, updateVal)
    {
        _pagingValidator = pagingValidator;
    }

    // Map Entity -> DTO (Hiển thị ra UI)
    protected override UnitTypeDTO MapToDto(UnitType entity)
    {
        return new UnitTypeDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsDeleted = entity.IsDeleted,
            // Field riêng của UnitType
            IsEntirePlace = entity.IsEntirePlace
        };
    }

    // Map CreateDTO -> Entity (Tạo mới)
    protected override UnitType MapCreateToEntity(UnitTypeCreateDTO createDto)
    {
        return new UnitType
        {
            Name = createDto.Name,
            Description = createDto.Description,
            IsDeleted = false,
            IsEntirePlace = createDto.IsEntirePlace
        };
    }

    // Map UpdateDTO -> Entity (Cập nhật)
    protected override void MapUpdateToEntity(UnitTypeUpdateDTO updateDto, UnitType entity)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
        entity.IsEntirePlace = updateDto.IsEntirePlace;
    }

    // Validation
    protected override async Task<ValidationResult> ValidateCreateLogicAsync(UnitTypeCreateDTO dto)
    {
        bool exists = await _repo.AnyAsync(x =>
            x.Name == dto.Name);

        if (exists) return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.UnitType.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    protected override async Task<ValidationResult> ValidateUpdateLogicAsync(UnitTypeUpdateDTO dto, int id)
    {
        bool exists = await _repo.AnyAsync(x =>
            x.Name == dto.Name &&
            x.Id != id &&
            x.IsDeleted == false);

        if (exists)
            return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.UnitType.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    public async Task<ApiResponse<List<UnitTypeDTO>>> GetAllAsync()
    {
        var utList = await _repo.WhereAsync(ut => ut.IsDeleted == false);

        if (utList == null || utList.Count() == 0)
        {
            return ResponseFactory.Failure<List<UnitTypeDTO>>(StatusCodeResponse.NotFound, MessageResponse.Common.EMPTY_LIST);
        }

        try
        {
            var result = utList.Select(ut => MapToDto(ut)).ToList();
            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
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
            // [BƯỚC 1] Dùng FluentValidation
            // ValidateAsync check cả null, > 0, max size... cực sạch sẽ
            var validationResult = await _pagingValidator.ValidateAsync(paging);

            if (!validationResult.IsValid)
            {
                // Lấy lỗi đầu tiên trả về
                return ResponseFactory.Failure<PagedManageResult<UnitTypeDTO>>(
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
            var result = new PagedManageResult<UnitTypeDTO>(
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
            return ResponseFactory.ServerError<PagedManageResult<UnitTypeDTO>>();
        }
    }
}