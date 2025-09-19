using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class UpgradeRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RequestedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public int? ApprovedBy { get; set; }

    public string? Additional { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
