using HotelBooking.application.DTOs.Hotel;

namespace HotelBooking.application.Interfaces;

public interface IRoomService
{
    Task<ApiResponse<IEnumerable<RoomResponseDTO>>> BatchAddRoomsAsync(BatchAddRoomsRequestDTO request);
    Task<ApiResponse<IEnumerable<RoomResponseDTO>>> GetRoomsByRoomTypeAsync(int roomTypeId);
}
