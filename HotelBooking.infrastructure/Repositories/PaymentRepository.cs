using HotelBooking.infrastructure.Models;

public interface IPaymentRepository : IRepository<Payment> { }
public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(HotelBookingDBContext context) : base(context) { }
}
