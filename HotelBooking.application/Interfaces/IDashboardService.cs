using HotelBooking.application.DTOs.Admin;

namespace HotelBooking.application.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<AdminDashboardDTO>> GetAdminDashboardStatsAsync();
}
