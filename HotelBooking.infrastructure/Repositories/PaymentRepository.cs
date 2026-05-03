using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IPaymentRepository : IRepository<Payment> { }
public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
