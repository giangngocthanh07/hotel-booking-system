using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class UnitType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsEntirePlace { get; set; }

    public string? Additional { get; set; }

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
}
