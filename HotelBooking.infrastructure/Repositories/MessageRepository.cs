using HotelBooking.infrastructure.Models;

public interface IMessageRepository : IRepository<Message> { }
public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(HotelBookingDBContext context) : base(context) { }
}
