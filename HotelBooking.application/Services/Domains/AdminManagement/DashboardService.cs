using HotelBooking.application.DTOs.Admin;
using HotelBooking.application.Interfaces;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace HotelBooking.application.Services.Domains.AdminManagement;

public class DashboardService : IDashboardService
{
    private readonly IUserRepository _userRepo;
    private readonly IHotelRepository _hotelRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IHotelApprovalRequestRepository _hotelApprovalRepo;
    private readonly IUpgradeRequestRepository _upgradeRepo;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IUserRepository userRepo,
        IHotelRepository hotelRepo,
        IBookingRepository bookingRepo,
        IPaymentRepository paymentRepo,
        IHotelApprovalRequestRepository hotelApprovalRepo,
        IUpgradeRequestRepository upgradeRepo,
        ILogger<DashboardService> logger)
    {
        _userRepo = userRepo;
        _hotelRepo = hotelRepo;
        _bookingRepo = bookingRepo;
        _paymentRepo = paymentRepo;
        _hotelApprovalRepo = hotelApprovalRepo;
        _upgradeRepo = upgradeRepo;
        _logger = logger;
    }

    public async Task<ApiResponse<AdminDashboardDTO>> GetAdminDashboardStatsAsync()
    {
        try
        {
            // 1. Get Summary Metrics
            var users = await _userRepo.GetAllAsync();
            var hotels = await _hotelRepo.GetAllAsync();
            var bookings = await _bookingRepo.GetAllAsync();
            var payments = await _paymentRepo.GetAllAsync();

            var totalRevenue = payments.Where(p => p.Status == "Success").Sum(p => p.Amount);

            // 2. Get Pending Requests
            var pendingHotels = await _hotelApprovalRepo.WhereAsync(r => r.Status == "Pending");
            var pendingUpgrades = await _upgradeRepo.WhereAsync(r => r.Status == "Pending");

            // 3. Prepare Revenue Trend (Last 6 months)
            var revenueTrend = new List<RevenueTrendDTO>();
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = DateTime.Today.AddMonths(-i);
                var monthName = monthDate.ToString("MMM yyyy", CultureInfo.InvariantCulture);
                var amount = payments
                    .Where(p => p.Status == "Success" && p.PaidAt.HasValue && 
                                p.PaidAt.Value.Month == monthDate.Month && 
                                p.PaidAt.Value.Year == monthDate.Year)
                    .Sum(p => p.Amount);
                
                revenueTrend.Add(new RevenueTrendDTO { MonthName = monthName, Amount = amount });
            }

            var dashboard = new AdminDashboardDTO
            {
                TotalUsers = users.Count(),
                TotalHotels = hotels.Count(),
                TotalBookings = bookings.Count(),
                TotalRevenue = totalRevenue,
                
                PendingHotelRequests = pendingHotels.OrderByDescending(r => r.CreatedAt).Take(5).Select(r => new RecentRequestSummaryDTO
                {
                    Id = r.Id,
                    Title = r.Name,
                    RequesterName = "Owner ID: " + r.OwnerId,
                    CreatedAt = r.CreatedAt
                }).ToList(),

                PendingUpgradeRequests = pendingUpgrades.OrderByDescending(r => r.RequestedAt).Take(5).Select(r => new RecentRequestSummaryDTO
                {
                    Id = r.Id,
                    Title = "Upgrade to Owner",
                    RequesterName = "User ID: " + r.UserId,
                    CreatedAt = r.RequestedAt
                }).ToList(),

                MonthlyRevenueTrend = revenueTrend
            };

            return ResponseFactory.Success(dashboard, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating admin dashboard stats");
            return ResponseFactory.ServerError<AdminDashboardDTO>();
        }
    }
}
