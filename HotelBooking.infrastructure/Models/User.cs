using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();

    public virtual ICollection<Message> MessageReceivers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSenders { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<UpgradeRequest> UpgradeRequestApprovedByNavigations { get; set; } = new List<UpgradeRequest>();

    public virtual ICollection<UpgradeRequest> UpgradeRequestUsers { get; set; } = new List<UpgradeRequest>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
