using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Country
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
