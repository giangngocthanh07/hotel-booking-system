using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class RoomImage
{
    public int Id { get; set; }

    public int RoomTypeId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public virtual RoomType RoomType { get; set; } = null!;
}
