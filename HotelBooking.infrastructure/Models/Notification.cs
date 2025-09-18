using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Additional { get; set; }

    public virtual User User { get; set; } = null!;
}
