using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class AmenityType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Additional { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
}
