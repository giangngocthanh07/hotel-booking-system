using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class HotelApprovalRequest
{
    public int Id { get; set; }

    public int OwnerId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string TaxCode { get; set; } = null!;

    public string BusinessLicenseUrl { get; set; } = null!;

    public string? Additional { get; set; }

    public string? Status { get; set; }

    public string? AdminRemark { get; set; }

    public int? AdminId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Owner { get; set; } = null!;
}
