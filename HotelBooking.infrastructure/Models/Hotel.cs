using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class Hotel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Description { get; set; }

    public string? CoverImageUrl { get; set; }

    public int OwnerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsVerified { get; set; }

    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public int? CityId { get; set; }

    public int? CountryId { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual City? City { get; set; }

    public virtual Country? Country { get; set; }

    public virtual ICollection<HotelAmenity> HotelAmenities { get; set; } = new List<HotelAmenity>();

    public virtual ICollection<HotelImage> HotelImages { get; set; } = new List<HotelImage>();

    public virtual ICollection<HotelPolicy> HotelPolicies { get; set; } = new List<HotelPolicy>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
}
