namespace HotelBooking.webapp.Services.Interface;

/// <summary>
/// A shared interface for services requiring token-based authentication.
/// All services that need to invoke authenticated API endpoints must implement this interface.
/// 
/// Objectives:
/// - Compatibility: Allows the generic AdminPageBase<TService> to work seamlessly with any service.
/// - DRY (Don't Repeat Yourself): Token injection is handled automatically within the base class.
/// - Scalability: Easily extendable; new services only need to implement this interface to be authentication-ready.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Configures the JWT token for the HttpClient's Authorization header.
    /// This is automatically invoked by the AdminPageBase after retrieving the token from LocalStorage.
    /// </summary>
    /// <param name="token">The JWT access token used for authentication.</param>
    void SetToken(string token);
}