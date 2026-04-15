using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Province
{
    public int Id { get; set; }

    public int CountryId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string? Additional { get; set; }

    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();

    public virtual ICollection<Ward> Wards { get; set; } = new List<Ward>();
}
