using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.Overview;
using HotelBooking.application.Helpers;

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
        // private readonly IHotelApprovalRepository _hotelApprovalRepo; // For future implementation

        public RequestOverviewService(IUpgradeRequestRepository upgradeRequestRepo)
        {
            _upgradeRequestRepo = upgradeRequestRepo;
        }

        public async Task<ApiResponse<RequestStatsDTO>> GetStatsAsync()
        {
            try
            {
                // Retrieve raw statistics from repository
                var rawStats = await _upgradeRequestRepo.GetStatsRawAsync();

                // Mapping to DTO at the Application layer
                var upgradeStats = new RequestTypeStatsDTO
                {
                    Total = rawStats.Total,
                    Pending = rawStats.Pending,
                    Approved = rawStats.Approved,
                    Rejected = rawStats.Rejected,
                    Cancelled = rawStats.Cancelled,
                    Today = rawStats.Today,
                    ThisWeek = rawStats.ThisWeek,
                    ThisMonth = rawStats.ThisMonth
                };

                // Hotel Approval stats (Future implementation)
                // var hotelRawStats = await _hotelApprovalRepo.GetStatsRawAsync();
                // var hotelStats = new RequestTypeStatsDTO { ... };

                var stats = new RequestStatsDTO
                {
                    UpgradeRequest = upgradeStats,
                    // HotelApproval = hotelStats, // Placeholder for future use
                    TotalPending = upgradeStats.Pending, // Sum of all pending requests
                    TotalToday = upgradeStats.Today      // Sum of all requests today
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
                recentRequests.AddRange(upgradeRequests.Select(r => new RecentRequestDTO
                {
                    Id = r.Id,
                    Type = RequestType.UpgradeOwner.ToString(),
                    TypeDisplay = RequestType.UpgradeOwner.GetDisplayName(),
                    RequesterName = r.User?.FullName ?? r.User?.UserName ?? "",
                    Status = r.Status ?? RequestStatusConst.Pending,
                    CreatedAt = r.RequestedAt
                }));

                // Get recent hotel approvals (Future implementation)
                // var hotelApprovals = await _hotelApprovalRepo.GetRecentAsync(count);
                // recentRequests.AddRange(hotelApprovals.Select(r => new RecentRequestDTO
                // {
                //     Id = r.Id,
                //     Type = RequestType.HotelApproval.ToString(),
                //     TypeDisplay = RequestType.HotelApproval.GetDisplayName(),
                //     ...
                // }));

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