using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Interfaces;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.RoomManagement;

public class RoomService(
    IRoomRepository roomRepository,
    IInventoryAccessService inventoryAccess,
    IValidator<BatchAddRoomsRequestDTO> batchValidator,
    IValidator<int> listValidator,
    IUnitOfWork unitOfWork,
    ILogger<RoomService> logger) : IRoomService
{
    public async Task<ApiResponse<IEnumerable<RoomResponseDTO>>> BatchAddRoomsAsync(
        BatchAddRoomsRequestDTO request, int ownerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request == null) return Failure(StatusCodeResponse.BadRequest, MessageResponse.Common.REQUEST_CANNOT_BE_NULL);
        var validation = await batchValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return Failure(StatusCodeResponse.BadRequest, validation.Errors[0].ErrorMessage);
        if (!await inventoryAccess.CanAccessRoomTypeAsync(request.RoomTypeId, ownerId, request.HotelId, cancellationToken))
            return Failure(StatusCodeResponse.Forbidden, InventoryAccessService.AccessDeniedMessage);
        var duplicate = await FindDuplicateAsync(request, cancellationToken);
        if (duplicate != null) return Failure(StatusCodeResponse.Conflict, $"Room number '{duplicate}' already exists in this hotel.");
        return await CreateRoomsAsync(request, cancellationToken);
    }

    public async Task<ApiResponse<IEnumerable<RoomResponseDTO>>> GetRoomsByRoomTypeAsync(
        int roomTypeId, int ownerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validation = await listValidator.ValidateAsync(roomTypeId, cancellationToken);
        if (!validation.IsValid) return Failure(StatusCodeResponse.BadRequest, validation.Errors[0].ErrorMessage);
        if (!await inventoryAccess.CanAccessRoomTypeAsync(roomTypeId, ownerId, cancellationToken: cancellationToken))
            return Failure(StatusCodeResponse.Forbidden, InventoryAccessService.AccessDeniedMessage);
        cancellationToken.ThrowIfCancellationRequested();
        var rooms = await roomRepository.WhereAsync(room => room.RoomTypeId == roomTypeId && room.IsDeleted != true);
        return ResponseFactory.Success(rooms.Select(ToResponse), "Rooms retrieved successfully.");
    }

    private async Task<string?> FindDuplicateAsync(BatchAddRoomsRequestDTO request, CancellationToken cancellationToken)
    {
        foreach (var roomNumber in request.RoomNumbers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await roomRepository.AnyAsync(room => room.RoomType.HotelId == request.HotelId
                && room.RoomNumber == roomNumber && room.IsDeleted != true)) return roomNumber;
        }
        return null;
    }

    private async Task<ApiResponse<IEnumerable<RoomResponseDTO>>> CreateRoomsAsync(
        BatchAddRoomsRequestDTO request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var rooms = await AddRoomsAsync(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await unitOfWork.SaveChangesAsync();
            cancellationToken.ThrowIfCancellationRequested();
            await unitOfWork.CommitTransactionAsync();
            return ResponseFactory.Success(rooms.Select(ToResponse), "Rooms added successfully.");
        }
        catch (OperationCanceledException)
        {
            await unitOfWork.RollBackTransactionAsync();
            throw;
        }
        catch (Exception exception)
        {
            await unitOfWork.RollBackTransactionAsync();
            logger.LogError(exception, "Error batch adding rooms");
            return Failure(StatusCodeResponse.Error, "An error occurred while adding rooms.");
        }
    }

    private async Task<List<Room>> AddRoomsAsync(BatchAddRoomsRequestDTO request, CancellationToken cancellationToken)
    {
        var rooms = new List<Room>();
        foreach (var number in request.RoomNumbers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var room = new Room
            {
                RoomTypeId = request.RoomTypeId,
                RoomNumber = number,
                Status = request.Status,
                IsDeleted = false
            };
            await roomRepository.AddAsync(room);
            rooms.Add(room);
        }
        return rooms;
    }

    private static RoomResponseDTO ToResponse(Room room) => new()
    {
        Id = room.Id,
        RoomTypeId = room.RoomTypeId,
        RoomNumber = room.RoomNumber,
        Status = room.Status ?? "Active"
    };

    private static ApiResponse<IEnumerable<RoomResponseDTO>> Failure(string status, string message) =>
        ResponseFactory.Failure<IEnumerable<RoomResponseDTO>>(status, message);
}
