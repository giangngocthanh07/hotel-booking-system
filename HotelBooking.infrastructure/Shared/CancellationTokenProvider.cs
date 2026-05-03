namespace HotelBooking.infrastructure.Shared;

public interface ICancellationTokenProvider
{
    CancellationToken Token { get; }
}

public class HttpCancellationTokenProvider : ICancellationTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;


    public HttpCancellationTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CancellationToken Token => _httpContextAccessor.HttpContext?.RequestAborted ?? default;
}