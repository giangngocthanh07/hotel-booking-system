using HotelBooking.application.DTOs.Request.Overview;
using HotelBooking.application.Helpers;

namespace HotelBooking.application.Services.Domains.RequestManagement
{
    public interface IRequestOverviewService
    {
        Task<ApiResponse<RequestStatsDTO>> GetStatsAsync();
        Task<ApiResponse<List<RecentRequestDTO>>> GetRecentRequestsAsync(int count = 10);
    }

    public class RequestOverviewService : IRequestOverviewService
    {
        private readonly IUpgradeRequestRepository _upgradeRequestRepo;
        // private readonly IHotelApprovalRepository _hotelApprovalRepo; // Sau này

        public RequestOverviewService(IUpgradeRequestRepository upgradeRequestRepo)
        {
            _upgradeRequestRepo = upgradeRequestRepo;
        }

        public async Task<ApiResponse<RequestStatsDTO>> GetStatsAsync()
        {
            try
            {
                // Lấy raw stats từ repo
                var rawStats = await _upgradeRequestRepo.GetStatsRawAsync();

                // Mapping sang DTO ở Application layer
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

                // Hotel Approval stats (sau này)
                // var hotelRawStats = await _hotelApprovalRepo.GetStatsRawAsync();
                // var hotelStats = new RequestTypeStatsDTO { ... };

                var stats = new RequestStatsDTO
                {
                    UpgradeRequest = upgradeStats,
                    // HotelApproval = hotelStats, // Sau này
                    TotalPending = upgradeStats.Pending, // + hotelStats.Pending
                    TotalToday = upgradeStats.Today      // + hotelStats.Today
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

                // Get recent upgrade requests
                var upgradeRequests = await _upgradeRequestRepo.GetRecentAsync(count);
                recentRequests.AddRange(upgradeRequests.Select(r => new RecentRequestDTO
                {
                    Id = r.Id,
                    Type = "UpgradeOwner",
                    TypeDisplay = "Nâng cấp Owner",
                    RequesterName = r.User?.FullName ?? r.User?.UserName ?? "",
                    Status = r.Status ?? "Pending",
                    CreatedAt = r.RequestedAt
                }));

                // Get recent hotel approvals (sau này)
                // var hotelApprovals = await _hotelApprovalRepo.GetRecentAsync(count);
                // recentRequests.AddRange(...);

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