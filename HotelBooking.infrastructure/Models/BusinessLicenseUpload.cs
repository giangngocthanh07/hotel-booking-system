using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class BusinessLicenseUpload
{
    public Guid Id { get; set; }

    public int OwnerId { get; set; }

    public string PublicId { get; set; } = null!;

    public string ResourceType { get; set; } = null!;

    public string DeliveryType { get; set; } = null!;

    public string StoredUrl { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long SizeBytes { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<HotelApprovalRequest> HotelApprovalRequests { get; set; } = new List<HotelApprovalRequest>();

    public virtual User Owner { get; set; } = null!;
}
