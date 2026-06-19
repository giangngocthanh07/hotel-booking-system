using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Interfaces;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace HotelBooking.application.Services.Domains.RoomManagement;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepo;
    private readonly IRoomTypeRepository _roomTypeRepo;
    private readonly IUnitOfWork _dbu;
    private readonly ILogger<RoomService> _logger;

    public RoomService(IRoomRepository roomRepo, IRoomTypeRepository roomTypeRepo, IUnitOfWork dbu, ILogger<RoomService> logger)
    {
        _roomRepo = roomRepo;
        _roomTypeRepo = roomTypeRepo;
        _dbu = dbu;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<RoomResponseDTO>>> BatchAddRoomsAsync(BatchAddRoomsRequestDTO request)
    {
        await _dbu.BeginTransactionAsync();
        try
        {
            // 1. Validate RoomType exists and belongs to Hotel
            var roomType = await _roomTypeRepo.GetByIdAsync(request.RoomTypeId);
            if (roomType == null || roomType.HotelId != request.HotelId)
            {
                await _dbu.RollBackTransactionAsync();
                return ResponseFactory.Failure<IEnumerable<RoomResponseDTO>>(StatusCodeResponse.NotFound, "Room type not found or does not belong to this hotel.");
            }

            // 2. Check for duplicate room numbers within the hotel
            foreach (var roomNumber in request.RoomNumbers)
            {
                var exists = await _roomRepo.AnyAsync(r => r.RoomType.HotelId == request.HotelId && r.RoomNumber == roomNumber && r.IsDeleted != true);
                if (exists)
                {
                    await _dbu.RollBackTransactionAsync();
                    return ResponseFactory.Failure<IEnumerable<RoomResponseDTO>>(StatusCodeResponse.Conflict, $"Room number '{roomNumber}' already exists in this hotel.");
                }
            }

            // 3. Create Rooms
            var rooms = new List<Room>();
            foreach (var roomNumber in request.RoomNumbers)
            {
                var room = new Room
                {
                    RoomTypeId = request.RoomTypeId,
                    RoomNumber = roomNumber,
                    Status = request.Status,
                    IsDeleted = false
                };
                await _roomRepo.AddAsync(room);
                rooms.Add(room);
            }

            await _dbu.SaveChangesAsync();
            await _dbu.CommitTransactionAsync();

            var content = rooms.Select(r => new RoomResponseDTO
            {
                Id = r.Id,
                RoomTypeId = r.RoomTypeId,
                RoomNumber = r.RoomNumber,
                Status = r.Status ?? "Active"
            });

            return ResponseFactory.Success(content, "Rooms added successfully.");
        }
        catch (Exception ex)
        {
            await _dbu.RollBackTransactionAsync();
            _logger.LogError(ex, "Error batch adding rooms");
            return ResponseFactory.Failure<IEnumerable<RoomResponseDTO>>(StatusCodeResponse.Error, "An error occurred while adding rooms.");
        }
    }

    public async Task<ApiResponse<IEnumerable<RoomResponseDTO>>> GetRoomsByRoomTypeAsync(int roomTypeId)
    {
        try
        {
            var rooms = await _roomRepo.WhereAsync(r => r.RoomTypeId == roomTypeId && r.IsDeleted != true);
            var content = rooms.Select(r => new RoomResponseDTO
            {
                Id = r.Id,
                RoomTypeId = r.RoomTypeId,
                RoomNumber = r.RoomNumber,
                Status = r.Status ?? "Active"
            });

            return ResponseFactory.Success(content, "Rooms retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rooms for room type {RoomTypeId}", roomTypeId);
            return ResponseFactory.Failure<IEnumerable<RoomResponseDTO>>(StatusCodeResponse.Error, "An error occurred while retrieving rooms.");
        }
    }
}
