using HotelBooking.infrastructure.Models;

public interface IPaymentRepository : IRepository<Payment> { }
public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(HotelBookingContext context) : base(context) { }
}
