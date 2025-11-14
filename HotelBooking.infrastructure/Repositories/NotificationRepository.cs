using HotelBooking.infrastructure.Models;

public interface INotificationRepository : IRepository<Notification> { }
public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(HotelBookingDBContext context) : base(context) { }
}
