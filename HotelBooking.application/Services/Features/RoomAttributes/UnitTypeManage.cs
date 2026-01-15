using System.Text.Json;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IUnitTypeManage : IStandardManage<UnitTypeDTO, UnitTypeCreateOrUpdateDTO>;

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
}