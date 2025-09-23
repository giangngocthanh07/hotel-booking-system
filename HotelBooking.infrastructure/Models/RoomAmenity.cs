using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class RoomAmenity
{
    public int RoomTypeId { get; set; }

    public int AmenityId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Amenity Amenity { get; set; } = null!;

    public virtual RoomType RoomType { get; set; } = null!;
}
