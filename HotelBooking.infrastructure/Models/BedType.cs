using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class BedType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? DefaultCapacity { get; set; }

    public string? Additional { get; set; }

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<RoomTypeBedConfig> RoomTypeBedConfigs { get; set; } = new List<RoomTypeBedConfig>();
}
