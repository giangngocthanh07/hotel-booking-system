using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Additional { get; set; }

    public decimal Price { get; set; }

    public int ServiceTypeId { get; set; }

    public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();

    public virtual ServiceType ServiceType { get; set; } = null!;
}
