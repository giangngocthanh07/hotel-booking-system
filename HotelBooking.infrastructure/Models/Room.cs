using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Room
{
    public int Id { get; set; }

    public int RoomTypeId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Additional { get; set; }

    public virtual RoomType RoomType { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
