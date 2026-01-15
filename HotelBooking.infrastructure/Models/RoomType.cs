using System;
using System.Collections.Generic;

namespace HotelBooking.infrastructure.Models;

public partial class RoomType
{
    public int Id { get; set; }

    public int HotelId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal PricePerNight { get; set; }

    public int? Capacity { get; set; }

    public bool? IsDeleted { get; set; }

    public int? AdultCapacity { get; set; }

    public int? ChildCapacity { get; set; }

    public int? UnitTypeId { get; set; }

    public int? QualityId { get; set; }

    public int? RoomViewId { get; set; }

    public bool IsPrivateBathroom { get; set; }

    public bool HasBalcony { get; set; }

    public bool HasTerrace { get; set; }

    public bool CanAddExtraBed { get; set; }

    public int? MaxExtraBeds { get; set; }

    public double? AreaSqm { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Hotel Hotel { get; set; } = null!;

    public virtual RoomQuality? Quality { get; set; }

    public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();

    public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();

    public virtual ICollection<RoomTypeBedConfig> RoomTypeBedConfigs { get; set; } = new List<RoomTypeBedConfig>();

    public virtual RoomView? RoomView { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual UnitType? UnitType { get; set; }
}
