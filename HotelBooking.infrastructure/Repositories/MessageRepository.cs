using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IMessageRepository : IRepository<Message> { }
public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
