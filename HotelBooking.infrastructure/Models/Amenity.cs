using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Amenity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();

    public virtual ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
}
