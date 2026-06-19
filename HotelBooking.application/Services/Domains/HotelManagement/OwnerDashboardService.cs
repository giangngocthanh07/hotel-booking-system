using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Interfaces;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace HotelBooking.application.Services.Domains.HotelManagement;

public class OwnerDashboardService : IOwnerDashboardService
{
    private readonly IHotelRepository _hotelRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IRoomTypeRepository _roomTypeRepo;
    private readonly ILogger<OwnerDashboardService> _logger;

    public OwnerDashboardService(
        IHotelRepository hotelRepo,
        IBookingRepository bookingRepo,
        IPaymentRepository paymentRepo,
        IRoomRepository roomRepo,
        IRoomTypeRepository roomTypeRepo,
        ILogger<OwnerDashboardService> logger)
    {
        _hotelRepo = hotelRepo;
        _bookingRepo = bookingRepo;
        _paymentRepo = paymentRepo;
        _roomRepo = roomRepo;
        _roomTypeRepo = roomTypeRepo;
        _logger = logger;
    }

    public async Task<ApiResponse<OwnerDashboardDTO>> GetOwnerDashboardStatsAsync(int ownerId)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // 1. Get all hotels owned by this user
            var ownedHotels = await _hotelRepo.WhereAsync(h => h.OwnerId == ownerId && h.IsDeleted != true);
            var hotelIds = ownedHotels.Select(h => h.Id).ToList();

            if (!hotelIds.Any())
            {
                return ResponseFactory.Success(new OwnerDashboardDTO(), "No hotels found for this owner.");
            }

            // 2. Get operational stats (Arrivals, Departures, Staying)
            var bookings = await _bookingRepo.GetBookingsByHotelsAsync(hotelIds);
            
            var arrivals = bookings.Count(b => b.CheckInDate == today);
            var departures = bookings.Count(b => b.CheckOutDate == today);
            var staying = bookings.Count(b => b.CheckInDate <= today && b.CheckOutDate > today);

            // 3. Get Revenue
            var ownerPayments = await _paymentRepo.GetPaymentsByHotelsAsync(hotelIds);
            var totalRevenue = ownerPayments.Sum(p => p.Amount);

            // 4. Calculate Occupancy Rate
            var allRooms = await _roomRepo.GetRoomsByHotelsAsync(hotelIds);
            var totalRoomCount = allRooms.Count();
            double occupancyRate = totalRoomCount > 0 ? (double)staying / totalRoomCount * 100 : 0;

            // 5. Room Availability Summary
            var roomTypes = await _roomTypeRepo.WhereAsync(rt => hotelIds.Contains(rt.HotelId) && rt.IsDeleted != true);
            var availabilitySummary = new List<RoomAvailabilitySummaryDTO>();
            foreach (var rt in roomTypes)
            {
                var total = allRooms.Count(r => r.RoomTypeId == rt.Id);
                var bookedCount = bookings.Count(b => b.RoomTypeId == rt.Id && b.CheckInDate <= today && b.CheckOutDate > today);
                availabilitySummary.Add(new RoomAvailabilitySummaryDTO
                {
                    RoomTypeName = rt.Name,
                    TotalRooms = total,
                    AvailableRooms = total - bookedCount
                });
            }

            // 6. Revenue Trend (Last 30 days)
            var revenueTrend = new List<RevenueTrendDTO>();
            for (int i = 29; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                var amount = ownerPayments
                    .Where(p => p.PaidAt.HasValue && p.PaidAt.Value.Date == date.Date)
                    .Sum(p => p.Amount);
                revenueTrend.Add(new RevenueTrendDTO { DateLabel = date.ToString("dd/MM"), Amount = amount });
            }

            var dashboard = new OwnerDashboardDTO
            {
                TodayArrivals = arrivals,
                TodayDepartures = departures,
                TotalStaying = staying,
                TotalRevenue = totalRevenue,
                OccupancyRate = Math.Round(occupancyRate, 2),
                RecentBookings = bookings.OrderByDescending(b => b.CreatedAt).Take(5).Select(b => new OwnerRecentBookingDTO
                {
                    Id = b.Id,
                    CustomerName = b.Customer?.FullName ?? ("Customer ID: " + b.CustomerId),
                    RoomTypeName = b.RoomType?.Name ?? ("Room Type ID: " + b.RoomTypeId),
                    CheckIn = b.CheckInDate.ToDateTime(TimeOnly.MinValue),
                    CheckOut = b.CheckOutDate.ToDateTime(TimeOnly.MinValue),
                    TotalPrice = b.TotalPrice,
                    Status = b.Status ?? "Unknown"
                }).ToList(),
                RoomAvailability = availabilitySummary,
                DailyRevenueTrend = revenueTrend
            };

            return ResponseFactory.Success(dashboard, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating owner dashboard stats for owner {OwnerId}", ownerId);
            return ResponseFactory.ServerError<OwnerDashboardDTO>();
        }
    }
}
