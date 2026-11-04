using System.Text.Json;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.RoomManagement;

public interface IRoomTypeService
{
    Task<ApiResponse<RoomTypeResponseDTO>> CreateRoomTypeAsync(RoomTypeCreateDTO request, int ownerId, CancellationToken cancellationToken = default);
}

public class RoomTypeService(
    IRoomTypeRepository roomTypeRepository,
    IValidator<RoomTypeCreateDTO> validator,
    IInventoryAccessService inventoryAccess,
    IRoomAttributeFacade attributes,
    IUnitOfWork unitOfWork,
    ILogger<RoomTypeService> logger) : IRoomTypeService
{
    public async Task<ApiResponse<RoomTypeResponseDTO>> CreateRoomTypeAsync(
        RoomTypeCreateDTO request, int ownerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request == null) return Failure(StatusCodeResponse.BadRequest, MessageResponse.Common.REQUEST_CANNOT_BE_NULL);
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return Failure(StatusCodeResponse.BadRequest, validation.Errors[0].ErrorMessage);
        if (!await inventoryAccess.CanAccessHotelAsync(request.HotelId, ownerId, cancellationToken))
            return Failure(StatusCodeResponse.Forbidden, InventoryAccessService.AccessDeniedMessage);
        cancellationToken.ThrowIfCancellationRequested();
        var attributeError = await ValidateAttributesAsync(request, cancellationToken);
        if (attributeError != null) return Failure(StatusCodeResponse.NotFound, attributeError);
        var normalizedName = request.Name.Trim().ToLower();
        if (await roomTypeRepository.AnyAsync(type => type.HotelId == request.HotelId
            && type.Name.ToLower() == normalizedName && type.IsDeleted == false))
            return Failure(StatusCodeResponse.Conflict, MessageResponse.RoomManagement.ROOM_TYPE_ALREADY_EXISTS);
        return await CreateAsync(request, cancellationToken);
    }

    private async Task<ApiResponse<RoomTypeResponseDTO>> CreateAsync(RoomTypeCreateDTO request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var roomType = ToEntity(request);
            await PersistAsync(roomType, cancellationToken);
            return ResponseFactory.Success(ToResponse(roomType, request), MessageResponse.Common.CREATE_SUCCESSFULLY);
        }
        catch (OperationCanceledException)
        {
            await unitOfWork.RollBackTransactionAsync();
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error creating room type");
            await unitOfWork.RollBackTransactionAsync();
            return Failure(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
        }
    }

    private async Task PersistAsync(RoomType roomType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await roomTypeRepository.AddAsync(roomType);
        cancellationToken.ThrowIfCancellationRequested();
        await unitOfWork.SaveChangesAsync();
        cancellationToken.ThrowIfCancellationRequested();
        await unitOfWork.CommitTransactionAsync();
    }

    private async Task<string?> ValidateAttributesAsync(RoomTypeCreateDTO request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!await attributes.IsUnitTypeExistedAsync(request.UnitTypeId)) return MessageResponse.RoomManagement.ROOM_TYPE_UNIT_TYPE_NOT_FOUND;
        if (request.QualityId.HasValue && !await attributes.IsRoomQualityExistedAsync(request.QualityId.Value))
            return MessageResponse.RoomManagement.ROOM_TYPE_QUALITY_NOT_FOUND;
        if (request.RoomViewId.HasValue && !await attributes.IsRoomViewExistedAsync(request.RoomViewId.Value))
            return MessageResponse.RoomManagement.ROOM_TYPE_ROOM_VIEW_NOT_FOUND;
        foreach (var bed in request.BedTypes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await attributes.IsBedTypeExistedAsync(bed.BedTypeId)) return MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPE_NOT_FOUND;
        }
        return null;
    }

    private static RoomType ToEntity(RoomTypeCreateDTO request)
    {
        var roomType = new RoomType
        {
            HotelId = request.HotelId,
            Name = request.Name.Trim(),
            Description = request.Description,
            IsDeleted = false,
            PricePerNight = request.PricePerNight,
            AdultCapacity = request.AdultCapacity,
            ChildCapacity = request.ChildCapacity,
            Capacity = request.AdultCapacity + request.ChildCapacity,
            Additional = JsonSerializer.Serialize(new { request.BedTypes, request.IsSmokingAllowed, request.TotalRooms })
        };
        SetEntityAttributes(roomType, request);
        AddBedConfigurations(roomType, request);
        return roomType;
    }

    private static void SetEntityAttributes(RoomType roomType, RoomTypeCreateDTO request)
    {
        roomType.UnitTypeId = request.UnitTypeId;
        roomType.QualityId = request.QualityId;
        roomType.RoomViewId = request.RoomViewId;
        roomType.IsPrivateBathroom = request.IsPrivateBathroom;
        roomType.HasBalcony = request.HasBalcony;
        roomType.HasTerrace = request.HasTerrace;
        roomType.CanAddExtraBed = request.CanAddExtraBed;
        roomType.MaxExtraBeds = request.MaxExtraBeds;
        roomType.AreaSqm = request.AreaSqm;
    }

    private static void AddBedConfigurations(RoomType roomType, RoomTypeCreateDTO request)
    {
        foreach (var bed in request.BedTypes)
            roomType.RoomTypeBedConfigs.Add(new RoomTypeBedConfig
            {
                RoomType = roomType,
                BedTypeId = bed.BedTypeId,
                Quantity = bed.Quantity
            });
    }

    private static RoomTypeResponseDTO ToResponse(RoomType roomType, RoomTypeCreateDTO request)
    {
        var response = new RoomTypeResponseDTO
        {
            Id = roomType.Id,
            HotelId = roomType.HotelId,
            Name = roomType.Name,
            Description = roomType.Description,
            IsDeleted = roomType.IsDeleted,
            PricePerNight = roomType.PricePerNight,
            AdultCapacity = roomType.AdultCapacity,
            ChildCapacity = roomType.ChildCapacity,
            IsSmokingAllowed = request.IsSmokingAllowed ?? false,
            TotalRooms = request.TotalRooms,
            BedTypes = request.BedTypes
        };
        SetResponseAttributes(response, roomType);
        return response;
    }

    private static void SetResponseAttributes(RoomTypeResponseDTO response, RoomType roomType)
    {
        response.UnitTypeId = roomType.UnitTypeId;
        response.QualityId = roomType.QualityId;
        response.RoomViewId = roomType.RoomViewId;
        response.IsPrivateBathroom = roomType.IsPrivateBathroom;
        response.HasBalcony = roomType.HasBalcony;
        response.HasTerrace = roomType.HasTerrace;
        response.CanAddExtraBed = roomType.CanAddExtraBed;
        response.MaxExtraBeds = roomType.MaxExtraBeds;
        response.AreaSqm = roomType.AreaSqm;
    }

    private static ApiResponse<RoomTypeResponseDTO> Failure(string status, string message) =>
        ResponseFactory.Failure<RoomTypeResponseDTO>(status, message);
}
