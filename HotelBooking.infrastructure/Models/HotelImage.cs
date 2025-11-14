using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class HotelImage
{
    public int Id { get; set; }

    public int HotelId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public virtual Hotel Hotel { get; set; } = null!;
}
