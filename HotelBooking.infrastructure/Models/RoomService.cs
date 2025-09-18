using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class RoomService
{
    public int RoomId { get; set; }

    public int ServiceId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
