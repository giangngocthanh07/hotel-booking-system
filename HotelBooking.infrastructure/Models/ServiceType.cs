using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class ServiceType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
