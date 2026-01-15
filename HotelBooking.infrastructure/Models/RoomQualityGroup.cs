using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class RoomQualityGroup
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? SortOrder { get; set; }

    public string? Additional { get; set; }

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<RoomQuality> RoomQualities { get; set; } = new List<RoomQuality>();
}
