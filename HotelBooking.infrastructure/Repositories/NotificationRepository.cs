using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface INotificationRepository : IRepository<Notification> { }
public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
