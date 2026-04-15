using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class PropertyType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
}
