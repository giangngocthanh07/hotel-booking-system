using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class PolicyType
{
    public int Id { get; set; }

    public string TypeName { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
}
