using System.Text.Json;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

public interface IRoomViewManage : IStandardManage<RoomViewDTO, RoomViewCreateOrUpdateDTO>;

public class RoomViewManage : BaseManage<RoomView, IRoomViewRepository, RoomViewDTO, RoomViewCreateOrUpdateDTO>, IRoomViewManage
{
    public RoomViewManage(IRoomViewRepository repo, IUnitOfWork dbu) : base(repo, dbu)
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
}