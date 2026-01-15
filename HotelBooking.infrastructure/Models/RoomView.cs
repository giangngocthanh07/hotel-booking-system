using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class RoomView
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Additional { get; set; }

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
}
