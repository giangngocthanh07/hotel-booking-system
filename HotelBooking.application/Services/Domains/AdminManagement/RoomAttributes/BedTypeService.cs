using System.Text.Json;
using FluentValidation;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;


public interface IBedTypeService : IStandardManage<BedTypeDTO, BedTypeCreateDTO, BedTypeUpdateDTO>
{
    Task<ApiResponse<PagedManageResult<BedTypeDTO>>> GetPagedListAsync(PagingRequest paging);
}

public class BedTypeService : BaseManage<BedType, IBedTypeRepository, BedTypeDTO, BedTypeCreateDTO, BedTypeUpdateDTO>, IBedTypeService
{
    private readonly IValidator<PagingRequest> _pagingValidator;

    public BedTypeService(IBedTypeRepository repo, IUnitOfWork dbu, IValidator<BedTypeCreateDTO> createVal,
            IValidator<BedTypeUpdateDTO> updateVal,
            IValidator<PagingRequest> pagingValidator) : base(repo, dbu, createVal, updateVal)
    {
        _pagingValidator = pagingValidator;
    }

    // Map Entity -> DTO (Hiển thị ra UI)
    protected override BedTypeDTO MapToDto(BedType entity)
    {
        // Gọi Helper để giải nén
        var extraData = BedTypeHelper.MapToAdditionalData(entity.Additional);

        return new BedTypeDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            // Field riêng
            DefaultCapacity = entity.DefaultCapacity ?? 1,
            // Map thông số từ JSON
            MinWidth = extraData.MinWidth,
            MaxWidth = extraData.MaxWidth
        };
    }

    // Map CreateDTO -> Entity (Tạo mới)
    protected override BedType MapCreateToEntity(BedTypeCreateDTO createDto)
    {
        return new BedType
        {
            Name = createDto.Name,
            IsDeleted = false,
            Description = createDto.Description,
            // Field riêng
            DefaultCapacity = createDto.DefaultCapacity,
            // Đóng gói thông số vào JSON Additional
            Additional = BedTypeHelper.MapToAdditionalJson(createDto)
        };
    }

    // Map UpdateDTO -> Entity (Cập nhật)
    protected override void MapUpdateToEntity(BedTypeUpdateDTO updateDto, BedType entity)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
        // Field riêng
        entity.DefaultCapacity = updateDto.DefaultCapacity;
        // Cập nhật lại chuỗi JSON
        entity.Additional = BedTypeHelper.MapToAdditionalJson(updateDto);
    }

    // Validation
    protected override async Task<ValidationResult> ValidateCreateLogicAsync(BedTypeCreateDTO dto)
    {
        // Check trùng tên (Chỉ trong nhóm BedType)
        bool isDuplicate = await _repo.AnyAsync(x =>
            x.Name == dto.Name);

        if (isDuplicate) return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.BedType.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    protected override async Task<ValidationResult> ValidateUpdateLogicAsync(BedTypeUpdateDTO dto, int id)
    {
        // Check trùng tên (Chỉ trong nhóm BedType, trừ chính nó)
        bool isDuplicate = await _repo.AnyAsync(x =>
            x.Name == dto.Name &&
            x.Id != id &&
            x.IsDeleted == false);

        if (isDuplicate)
            return ValidationResult.Fail(MessageResponse.AdminManagement.RoomAttribute.BedType.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);

        return ValidationResult.Success();
    }

    public async Task<ApiResponse<List<BedTypeDTO>>> GetAllAsync()
    {
        var btList = await _repo.WhereAsync(bt => bt.IsDeleted == false);

        if (btList == null || btList.Count() == 0)
        {
            return ResponseFactory.Failure<List<BedTypeDTO>>(StatusCodeResponse.NotFound, MessageResponse.Common.EMPTY_LIST);
        }

        try
        {
            var result = btList.Select(bt => MapToDto(bt)).ToList();

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
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
            // [BƯỚC 1] Dùng FluentValidation
            // ValidateAsync check cả null, > 0, max size... cực sạch sẽ
            var validationResult = await _pagingValidator.ValidateAsync(paging);

            if (!validationResult.IsValid)
            {
                // Lấy lỗi đầu tiên trả về
                return ResponseFactory.Failure<PagedManageResult<BedTypeDTO>>(
                    StatusCodeResponse.BadRequest,
                    validationResult.Errors[0].ErrorMessage);
            }

            // [BƯỚC 2] Gọi Repository lấy dữ liệu phân trang
            // Filter: Lấy tất cả cái chưa xóa (!IsDeleted)
            // OrderBy: Sắp xếp theo ID giảm dần (Mới nhất lên đầu)
            var (items, totalCount) = await _repo.GetPagedAsync(
                pageIndex: paging.PageIndex!.Value,
                pageSize: paging.PageSize!.Value,
                filter: x => x.IsDeleted == false,
                orderBy: q => q.OrderByDescending(x => x.Id)
            );

            // [Bước 3] Map Entity sang DTO
            // Sử dụng hàm MapToDto đã viết sẵn trong class này
            var dtos = items.Select(MapToDto).ToList();

            // [Bước 4] Đóng gói kết quả
            // [QUAN TRỌNG] Chỉ cần truyền TotalCount và PageSize, TotalPages sẽ tự động được tính
            var result = new PagedManageResult<BedTypeDTO>(
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
            return ResponseFactory.ServerError<PagedManageResult<BedTypeDTO>>();
        }
    }
}