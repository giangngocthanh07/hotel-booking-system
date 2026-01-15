using System.Text.Json;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;


public interface IBedTypeManage : IStandardManage<BedTypeDTO, BedTypeCreateOrUpdateDTO>;

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
}