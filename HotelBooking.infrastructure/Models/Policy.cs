using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Policy
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<HotelPolicy> HotelPolicies { get; set; } = new List<HotelPolicy>();
}
