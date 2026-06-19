namespace HotelBooking.application.DTOs.Hotel;

public class HotelSearchRequestDTO
{
    public string? CityName { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public int Adults { get; set; } = 1;
    public int Children { get; set; } = 0;
    public int Rooms { get; set; } = 1;
}
