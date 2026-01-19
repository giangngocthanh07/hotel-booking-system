using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Policy
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int TypeId { get; set; }

    public string? Description { get; set; }

    public TimeOnly? TimeFrom { get; set; }

    public TimeOnly? TimeTo { get; set; }

    public int? IntValue1 { get; set; }

    public int? IntValue2 { get; set; }

    public decimal? Amount { get; set; }

    public double? Percent { get; set; }

    public bool? BoolValue { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<HotelPolicy> HotelPolicies { get; set; } = new List<HotelPolicy>();

    public virtual PolicyType Type { get; set; } = null!;
}
