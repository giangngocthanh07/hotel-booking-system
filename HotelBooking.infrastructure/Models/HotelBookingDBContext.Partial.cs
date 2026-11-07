using Microsoft.EntityFrameworkCore;

namespace HotelBooking.infrastructure.Models;

public partial class HotelBookingDBContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchHotelResult>().HasNoKey();
        // FR-ONB-019: compare the original status on UPDATE so competing decisions
        // cannot overwrite a saved terminal state. No database column is added.
        modelBuilder.Entity<HotelApprovalRequest>().Property(request => request.Status).IsConcurrencyToken();
    }
}
