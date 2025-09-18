using HotelBooking.infrastructure.Models;

public interface IReviewRepository : IRepository<Review> { }
public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(HotelBookingContext context) : base(context) { }
}
