using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Amenity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public string? Additional { get; set; }

    public int TypeId { get; set; }

    public virtual ICollection<HotelAmenity> HotelAmenities { get; set; } = new List<HotelAmenity>();

    public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();

    public virtual AmenityType Type { get; set; } = null!;
}
