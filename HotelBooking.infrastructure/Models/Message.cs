using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Message
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public int? HotelId { get; set; }

    public int? BookingId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public bool? IsRead { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Hotel? Hotel { get; set; }

    public virtual User Receiver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
