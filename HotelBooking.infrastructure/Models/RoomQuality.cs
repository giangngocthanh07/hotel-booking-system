using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class RoomQuality
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int TypeId { get; set; }

    public string? Additional { get; set; }

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public int? SortOrder { get; set; }

    public virtual ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();

    public virtual RoomQualityGroup Type { get; set; } = null!;
}
