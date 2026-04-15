using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Ward
{
    public int Id { get; set; }

    public int ProvinceId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();

    public virtual Province Province { get; set; } = null!;
}
