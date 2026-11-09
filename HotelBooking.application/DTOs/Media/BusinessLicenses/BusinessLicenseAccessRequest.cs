namespace HotelBooking.application.DTOs.Media.BusinessLicenses;

// FR-ONB-013/014: API constructs access context from authenticated claims, never from request JSON.
public sealed record BusinessLicenseAccessRequest(int RequestId, int UserId, bool IsAdmin);
