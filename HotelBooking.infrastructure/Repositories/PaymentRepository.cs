using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

using Microsoft.EntityFrameworkCore;

public interface IPaymentRepository : IRepository<Payment> 
{
    Task<List<Payment>> GetPaymentsByHotelsAsync(List<int> hotelIds);
}
public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }

    public async Task<List<Payment>> GetPaymentsByHotelsAsync(List<int> hotelIds)
    {
        return await _dbSet
            .Include(p => p.Booking)
            .Where(p => p.Status == "Success" && hotelIds.Contains(p.Booking.HotelId))
            .ToListAsync(_cancellationToken);
    }
}
