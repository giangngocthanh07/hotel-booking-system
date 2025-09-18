using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.infrastructure.Models;

public partial class HotelBookingContext : DbContext
{
    public HotelBookingContext()
    {
    }

    public HotelBookingContext(DbContextOptions<HotelBookingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Amenity> Amenities { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingRoom> BookingRooms { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Hotel> Hotels { get; set; }

    public virtual DbSet<HotelAmenity> HotelAmenities { get; set; }

    public virtual DbSet<HotelPolicy> HotelPolicies { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Policy> Policies { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomAmenity> RoomAmenities { get; set; }

    public virtual DbSet<RoomImage> RoomImages { get; set; }

    public virtual DbSet<RoomService> RoomServices { get; set; }

    public virtual DbSet<RoomType> RoomTypes { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=connectionStringHotelBooking");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Amenitie__3214EC0707B67CC2");

            entity.HasIndex(e => e.Name, "UQ__Amenitie__737584F6222DCDCA").IsUnique();

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bookings__3214EC07E61DF4A8");

            entity.HasIndex(e => e.CustomerId, "IX_Bookings_CustomerId");

            entity.HasIndex(e => e.HotelId, "IX_Bookings_HotelId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("PendingPayment");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Customers");

            entity.HasOne(d => d.Hotel).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.HotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Hotels");

            entity.HasOne(d => d.RoomType).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RoomTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_RoomTypes");
        });

        modelBuilder.Entity<BookingRoom>(entity =>
        {
            entity.HasKey(e => new { e.BookingId, e.RoomId }).HasName("PK__BookingR__F0BD797E38132D46");

            entity.HasIndex(e => e.BookingId, "IX_BookingRooms_BookingId");

            entity.HasIndex(e => e.RoomId, "IX_BookingRooms_RoomId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingRooms)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingRooms_Bookings");

            entity.HasOne(d => d.Room).WithMany(p => p.BookingRooms)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingRooms_Rooms");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cities__3214EC07C04DB18F");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Country).WithMany(p => p.Cities)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("FK__Cities__CountryI__19DFD96B");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Countrie__3214EC07A22038DC");

            entity.HasIndex(e => e.Name, "UQ__Countrie__737584F61DA94F6C").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Hotels__3214EC07A121216E");

            entity.HasIndex(e => e.CityId, "IX_Hotels_CityId");

            entity.HasIndex(e => e.CountryId, "IX_Hotels_CountryId");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("PendingVerification");

            entity.HasOne(d => d.City).WithMany(p => p.Hotels)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK__Hotels__CityId__1AD3FDA4");

            entity.HasOne(d => d.Country).WithMany(p => p.Hotels)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("FK__Hotels__CountryI__1BC821DD");

            entity.HasOne(d => d.Owner).WithMany(p => p.Hotels)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Hotels_Owner");
        });

        modelBuilder.Entity<HotelAmenity>(entity =>
        {
            entity.HasKey(e => new { e.HotelId, e.AmenityId }).HasName("PK__HotelAme__EE40948F26065B25");

            entity.HasIndex(e => e.HotelId, "IX_HotelAmenities_HotelId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Amenity).WithMany(p => p.HotelAmenities)
                .HasForeignKey(d => d.AmenityId)
                .HasConstraintName("FK_HotelAmenities_Amenities");

            entity.HasOne(d => d.Hotel).WithMany(p => p.HotelAmenities)
                .HasForeignKey(d => d.HotelId)
                .HasConstraintName("FK_HotelAmenities_Hotels");
        });

        modelBuilder.Entity<HotelPolicy>(entity =>
        {
            entity.HasKey(e => new { e.HotelId, e.PolicyId }).HasName("PK__HotelPol__14E30845E7C3EC46");

            entity.HasIndex(e => e.HotelId, "IX_HotelPolicies_HotelId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Hotel).WithMany(p => p.HotelPolicies)
                .HasForeignKey(d => d.HotelId)
                .HasConstraintName("FK_HotelPolicies_Hotels");

            entity.HasOne(d => d.Policy).WithMany(p => p.HotelPolicies)
                .HasForeignKey(d => d.PolicyId)
                .HasConstraintName("FK_HotelPolicies_Policies");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Messages__3214EC07F8C61465");

            entity.HasIndex(e => e.BookingId, "IX_Messages_BookingId");

            entity.HasIndex(e => e.HotelId, "IX_Messages_HotelId");

            entity.HasIndex(e => e.ReceiverId, "IX_Messages_ReceiverId");

            entity.HasIndex(e => e.SenderId, "IX_Messages_SenderId");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Booking).WithMany(p => p.Messages)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_Messages_Bookings");

            entity.HasOne(d => d.Hotel).WithMany(p => p.Messages)
                .HasForeignKey(d => d.HotelId)
                .HasConstraintName("FK_Messages_HotelId");

            entity.HasOne(d => d.Receiver).WithMany(p => p.MessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Receiver");

            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Sender");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC075FB21967");

            entity.HasIndex(e => e.UserId, "IX_Notifications_UserId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC0797434055");

            entity.HasIndex(e => e.BookingId, "IX_Payments_BookingId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TransactionId).HasMaxLength(100);

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_Payments_Bookings");
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Policies__3214EC0700D60673");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reviews__3214EC07A386440F");

            entity.HasIndex(e => e.CustomerId, "IX_Reviews_CustomerId");

            entity.HasIndex(e => e.HotelId, "IX_Reviews_HotelId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Customer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Customers");

            entity.HasOne(d => d.Hotel).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.HotelId)
                .HasConstraintName("FK_Reviews_Hotels");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC072A5B7B14");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F68381FAA3").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rooms__3214EC076BBF4588");

            entity.HasIndex(e => e.RoomTypeId, "IX_Rooms_RoomTypeId");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.RoomNumber).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.RoomType).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.RoomTypeId)
                .HasConstraintName("FK_Rooms_RoomTypes");
        });

        modelBuilder.Entity<RoomAmenity>(entity =>
        {
            entity.HasKey(e => new { e.RoomTypeId, e.AmenityId }).HasName("PK__RoomAmen__148A3961C8C36DBD");

            entity.HasIndex(e => e.RoomTypeId, "IX_RoomAmenities_RoomTypeId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Amenity).WithMany(p => p.RoomAmenities)
                .HasForeignKey(d => d.AmenityId)
                .HasConstraintName("FK_RoomAmenities_Amenities");

            entity.HasOne(d => d.RoomType).WithMany(p => p.RoomAmenities)
                .HasForeignKey(d => d.RoomTypeId)
                .HasConstraintName("FK_RoomAmenities_RoomTypes");
        });

        modelBuilder.Entity<RoomImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RoomImag__3214EC077159EC15");

            entity.HasIndex(e => e.RoomTypeId, "IX_RoomImages_RoomTypeId");

            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.RoomType).WithMany(p => p.RoomImages)
                .HasForeignKey(d => d.RoomTypeId)
                .HasConstraintName("FK_RoomImages_RoomTypes");
        });

        modelBuilder.Entity<RoomService>(entity =>
        {
            entity.HasKey(e => new { e.RoomId, e.ServiceId }).HasName("PK__RoomServ__8ED78239812D49AF");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomServices)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__RoomServi__RoomI__1332DBDC");

            entity.HasOne(d => d.Service).WithMany(p => p.RoomServices)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__RoomServi__Servi__14270015");
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RoomType__3214EC072436EC76");

            entity.HasIndex(e => e.HotelId, "IX_RoomTypes_HotelId");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Hotel).WithMany(p => p.RoomTypes)
                .HasForeignKey(d => d.HotelId)
                .HasConstraintName("FK_RoomTypes_Hotels");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Services__3214EC076E85D35A");

            entity.HasIndex(e => e.Name, "UQ__Services__737584F66A96DDF0").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07A7911710");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534800A8CF8").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__Users__C9F284569817A4FB").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__AF2760ADCB5D2FC2");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserRoles_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
