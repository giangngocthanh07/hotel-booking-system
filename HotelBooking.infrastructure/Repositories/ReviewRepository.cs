using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IReviewRepository : IRepository<Review> { }
public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
