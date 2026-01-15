using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class RoomTypeBedConfig
{
    public int Id { get; set; }

    public int RoomTypeId { get; set; }

    public int BedTypeId { get; set; }

    public int Quantity { get; set; }

    public virtual BedType BedType { get; set; } = null!;

    public virtual RoomType RoomType { get; set; } = null!;
}
