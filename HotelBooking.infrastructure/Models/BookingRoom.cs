using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class BookingRoom
{
    public int BookingId { get; set; }

    public int RoomId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;
}
