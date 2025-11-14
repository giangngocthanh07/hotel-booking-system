using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class BookingService
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int ServiceId { get; set; }

    public int? Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsPaid { get; set; }

    public string? Additional { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
