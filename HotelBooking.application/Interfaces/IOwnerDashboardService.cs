using HotelBooking.application.DTOs.Hotel;

namespace HotelBooking.application.Interfaces;

public interface IOwnerDashboardService
{
    Task<ApiResponse<OwnerDashboardDTO>> GetOwnerDashboardStatsAsync(int ownerId);
}
