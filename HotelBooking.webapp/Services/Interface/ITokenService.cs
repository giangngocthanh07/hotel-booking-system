namespace HotelBooking.webapp.Services.Interface;

/// <summary>
/// Interface chung cho các service cần xác thực token.
/// Tất cả service cần gọi API có xác thực đều implement interface này.
/// 
/// Mục đích:
/// - Cho phép AdminPageBase&lt;TService&gt; generic hoạt động với mọi service
/// - DRY: SetToken được xử lý tự động trong base class
/// - Dễ mở rộng: Thêm service mới chỉ cần implement interface này
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Set JWT token cho Authorization header của HttpClient.
    /// Được gọi tự động bởi AdminPageBase sau khi lấy token từ LocalStorage.
    /// </summary>
    /// <param name="token">JWT access token</param>
    void SetToken(string token);
}
