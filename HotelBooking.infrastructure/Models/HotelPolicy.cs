using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class HotelPolicy
{
    public int HotelId { get; set; }

    public int PolicyId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Hotel Hotel { get; set; } = null!;

    public virtual Policy Policy { get; set; } = null!;
}
