using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.infrastructure.Models;

// FR-ONB-013/014: keep decision data consistent across Admin detail/list and Owner history.
public static class HotelRequestDecisionMapper
{
    public static async Task<Dictionary<int, string?>> LoadNamesAsync(
        IEnumerable<HotelApprovalRequest> requests, IUserRepository users)
    {
        // Resolve all processors in one query instead of one query per item in the list.
        var ids = requests.Where(IsDecision).Where(request => request.AdminId.HasValue)
            .Select(request => request.AdminId!.Value).Distinct().ToList();
        if (ids.Count == 0) return new();
        var processors = await users.WhereAsync(user => ids.Contains(user.Id));
        return processors.ToDictionary(user => user.Id, user => user.FullName);
    }

    public static void Apply(HotelRegistrationDetailDTO dto, HotelApprovalRequest request,
        IReadOnlyDictionary<int, string?> names)
    {
        // Pending/cancelled updates are not Admin decisions and must not appear as such.
        if (!IsDecision(request)) return;
        dto.ProcessedAt = request.UpdatedAt;
        dto.ProcessedBy = request.AdminId;
        dto.ProcessedByName = request.AdminId.HasValue && names.TryGetValue(request.AdminId.Value, out var name) ? name : null;
        dto.AdminRemark = request.Status == RequestStatusConst.Rejected ? request.AdminRemark : null;
    }

    private static bool IsDecision(HotelApprovalRequest request) =>
        request.Status is RequestStatusConst.Approved or RequestStatusConst.Rejected;
}
