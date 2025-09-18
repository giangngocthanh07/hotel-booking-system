using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
