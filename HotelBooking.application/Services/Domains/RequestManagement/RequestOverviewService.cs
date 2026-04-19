using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.Overview;

namespace HotelBooking.application.Services.Domains.RequestManagement
{
    /// <summary>
    /// Overview service for all request types - Admin Dashboard.
    /// Aggregates statistics and recent requests from various request types.
    /// </summary>
    public interface IRequestOverviewService
    {
        Task<ApiResponse<RequestStatsDTO>> GetStatsAsync();
        Task<ApiResponse<List<RecentRequestDTO>>> GetRecentRequestsAsync(int count = 10);
    }

    public class RequestOverviewService : IRequestOverviewService
    {
        private readonly IUpgradeRequestRepository _upgradeRequestRepo;
        private readonly IHotelRepository _hotelRepo;

        public RequestOverviewService(IUpgradeRequestRepository upgradeRequestRepo, IHotelRepository hotelRepo)
        {
            _upgradeRequestRepo = upgradeRequestRepo;
            _hotelRepo = hotelRepo;
        }

        public async Task<ApiResponse<RequestStatsDTO>> GetStatsAsync()
        {
            try
            {
                // Retrieve raw statistics from repository
                var rawUpgradeStats = await _upgradeRequestRepo.GetStatsRawAsync();
                var rawHotelStats = await _hotelRepo.GetStatsRawAsync();

                // Mapping to DTO at the Application layer
                var upgradeStats = new RequestTypeStatsDTO
                {
                    Total = rawUpgradeStats.Total,
                    Pending = rawUpgradeStats.Pending,
                    Approved = rawUpgradeStats.Approved,
                    Rejected = rawUpgradeStats.Rejected,
                    Cancelled = rawUpgradeStats.Cancelled,
                    Today = rawUpgradeStats.Today,
                    ThisWeek = rawUpgradeStats.ThisWeek,
                    ThisMonth = rawUpgradeStats.ThisMonth
                };

                var hotelStats = new RequestTypeStatsDTO
                {
                    Total = rawHotelStats.Total,
                    Pending = rawHotelStats.Pending,
                    Approved = rawHotelStats.Approved,
                    Rejected = rawHotelStats.Rejected,
                    Cancelled = rawHotelStats.Cancelled,
                    Today = rawHotelStats.Today,
                    ThisWeek = rawHotelStats.ThisWeek,
                    ThisMonth = rawHotelStats.ThisMonth
                };

                var overallStats = new RequestTypeStatsDTO
                {
                    Total = upgradeStats.Total + hotelStats.Total,
                    Pending = upgradeStats.Pending + hotelStats.Pending,
                    Approved = upgradeStats.Approved + hotelStats.Approved,
                    Rejected = upgradeStats.Rejected + hotelStats.Rejected,
                    Cancelled = upgradeStats.Cancelled + hotelStats.Cancelled,
                    Today = upgradeStats.Today + hotelStats.Today,
                    ThisWeek = upgradeStats.ThisWeek + hotelStats.ThisWeek,
                    ThisMonth = upgradeStats.ThisMonth + hotelStats.ThisMonth
                };

                var stats = new RequestStatsDTO
                {
                    Overall = overallStats,
                    UpgradeRequest = upgradeStats,
                    HotelApproval = hotelStats
                };

                return ResponseFactory.Success(stats, MessageResponse.Common.GET_SUCCESSFULLY);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<RequestStatsDTO>();
            }
        }

        public async Task<ApiResponse<List<RecentRequestDTO>>> GetRecentRequestsAsync(int count = 10)
        {
            try
            {
                var recentRequests = new List<RecentRequestDTO>();

                // Get recent upgrade requests - using RequestType enum
                var upgradeRequests = await _upgradeRequestRepo.GetRecentAsync(count);
                var hotelRequests = await _hotelRepo.GetRecentAsync(count);

                // 2. Map Upgrade Requests
                recentRequests.AddRange(upgradeRequests.Select(r => new RecentRequestDTO
                {
                    Id = r.Id,
                    Type = RequestType.UpgradeOwner.ToString(),
                    TypeDisplay = RequestType.UpgradeOwner.GetDisplayNameEn(),
                    RequesterName = r.User?.FullName ?? r.User?.UserName ?? "",
                    Status = r.Status ?? RequestStatusConst.Pending,
                    CreatedAt = r.RequestedAt
                }));

                // 3. Map Hotel Requests
                recentRequests.AddRange(hotelRequests.Select(h => new RecentRequestDTO
                {
                    Id = h.Id,
                    Type = RequestType.HotelApproval.ToString(),
                    TypeDisplay = RequestType.HotelApproval.GetDisplayNameEn(),
                    RequesterName = h.Name,
                    Status = h.Status ?? RequestStatusConst.Pending,
                    CreatedAt = h.CreatedAt ?? DateTime.Now
                }));

                // Sort by date and take top N
                var result = recentRequests
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(count)
                    .ToList();

                return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<List<RecentRequestDTO>>();
            }
        }
    }
}