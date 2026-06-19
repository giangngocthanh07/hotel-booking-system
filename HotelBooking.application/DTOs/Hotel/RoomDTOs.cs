using System.ComponentModel.DataAnnotations;

namespace HotelBooking.application.DTOs.Hotel;

public class BatchAddRoomsRequestDTO
{
    [Required]
    public int HotelId { get; set; }
    
    [Required]
    public int RoomTypeId { get; set; }

    [Required]
    public List<string> RoomNumbers { get; set; } = new();

    public string Status { get; set; } = "Active";
}

public class RoomResponseDTO
{
    public int Id { get; set; }
    public int RoomTypeId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
