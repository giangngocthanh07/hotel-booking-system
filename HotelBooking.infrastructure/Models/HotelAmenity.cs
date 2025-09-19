using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class HotelAmenity
{
    public int HotelId { get; set; }

    public int AmenityId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Amenity Amenity { get; set; } = null!;

    public virtual Hotel Hotel { get; set; } = null!;
}
