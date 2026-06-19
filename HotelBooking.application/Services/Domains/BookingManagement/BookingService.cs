using HotelBooking.application.DTOs.Booking;
using HotelBooking.application.Interfaces;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace HotelBooking.application.Services.Domains.BookingManagement;

public class BookingService : IBookingService
{
    private readonly IRoomTypeRepository _roomTypeRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IBookingRoomRepository _bookingRoomRepo;
    private readonly IHotelRepository _hotelRepo;
    private readonly IUnitOfWork _dbu;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IRoomTypeRepository roomTypeRepo,
        IRoomRepository roomRepo,
        IBookingRepository bookingRepo,
        IBookingRoomRepository bookingRoomRepo,
        IHotelRepository hotelRepo,
        IUnitOfWork dbu,
        ILogger<BookingService> logger)
    {
        _roomTypeRepo = roomTypeRepo;
        _roomRepo = roomRepo;
        _bookingRepo = bookingRepo;
        _bookingRoomRepo = bookingRoomRepo;
        _hotelRepo = hotelRepo;
        _dbu = dbu;
        _logger = logger;
    }

    public async Task<ApiResponse<BookingResponseDTO>> CreateBookingAsync(BookingRequestDTO request, int userId)
    {
        await _dbu.BeginTransactionAsync();
        try
        {
            // 1. Validate RoomType exists
            var roomType = await _roomTypeRepo.GetByIdAsync(request.RoomTypeId);
            if (roomType == null)
            {
                await _dbu.RollBackTransactionAsync();
                return new ApiResponse<BookingResponseDTO> { StatusCode = StatusCodeResponse.Error, Message = "Room type not found." };
            }

            // 2. Check Availability
            var availableRooms = await _roomRepo.GetAvailableRoomsAsync(request.RoomTypeId, request.CheckInDate, request.CheckOutDate, request.NumberOfRooms);
            if (availableRooms.Count < request.NumberOfRooms)
            {
                await _dbu.RollBackTransactionAsync();
                return new ApiResponse<BookingResponseDTO> { StatusCode = StatusCodeResponse.Error, Message = "Not enough rooms available for the selected dates." };
            }

            // 3. Calculate Price
            int nights = (request.CheckOutDate - request.CheckInDate).Days;
            if (nights <= 0) nights = 1; // Basic safety
            decimal totalPrice = roomType.PricePerNight * nights * request.NumberOfRooms;

            // 4. Create Booking
            var booking = new Booking
            {
                CustomerId = userId,
                HotelId = request.HotelId,
                RoomTypeId = request.RoomTypeId,
                CheckInDate = DateOnly.FromDateTime(request.CheckInDate),
                CheckOutDate = DateOnly.FromDateTime(request.CheckOutDate),
                TotalPrice = totalPrice,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Additional = request.Notes
            };

            await _bookingRepo.AddAsync(booking);
            await _dbu.SaveChangesAsync(); // Project standard: dbu.SaveChangesAsync()

            // 5. Assign Rooms
            foreach (var room in availableRooms)
            {
                await _bookingRoomRepo.AddAsync(new BookingRoom
                {
                    BookingId = booking.Id,
                    RoomId = room.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _dbu.SaveChangesAsync();
            await _dbu.CommitTransactionAsync();

            return new ApiResponse<BookingResponseDTO>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = "Booking created successfully.",
                Content = new BookingResponseDTO
                {
                    BookingId = booking.Id,
                    BookingReference = $"BK-{booking.Id}-{DateTime.Now:yyyyMMdd}",
                    TotalPrice = totalPrice,
                    Status = booking.Status
                }
            };
        }
        catch (Exception ex)
        {
            await _dbu.RollBackTransactionAsync();
            _logger.LogError(ex, "Error creating booking");
            return new ApiResponse<BookingResponseDTO>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = "An error occurred while creating the booking.",
                Content = null
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<BookingHistoryDTO>>> GetGuestBookingsAsync(int userId, string? status)
    {
        try
        {
            var bookings = await _bookingRepo.WhereAsync(b => b.CustomerId == userId);
            
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                bookings = bookings.Where(b => b.Status == status);
            }

            var content = bookings.OrderByDescending(b => b.CreatedAt).Select(b => new BookingHistoryDTO
            {
                Id = b.Id,
                BookingReference = $"BK-{b.Id}",
                HotelId = b.HotelId,
                HotelName = b.Hotel?.Name ?? "Unknown Hotel",
                HotelCoverImageUrl = b.Hotel?.CoverImageUrl,
                RoomTypeId = b.RoomTypeId,
                RoomTypeName = b.RoomType?.Name ?? "Unknown Room Type",
                CheckInDate = b.CheckInDate.ToDateTime(TimeOnly.MinValue),
                CheckOutDate = b.CheckOutDate.ToDateTime(TimeOnly.MinValue),
                TotalPrice = b.TotalPrice,
                Status = b.Status ?? "Unknown",
                CreatedAt = b.CreatedAt ?? DateTime.MinValue
            });

            return ResponseFactory.Success(content, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting guest bookings for user {UserId}", userId);
            return ResponseFactory.ServerError<IEnumerable<BookingHistoryDTO>>();
        }
    }

    public async Task<ApiResponse<IEnumerable<BookingHistoryDTO>>> GetOwnerBookingsAsync(int ownerId, string? status, string? searchTerm)
    {
        try
        {
            // 1. Get owned hotels
            var ownedHotels = await _hotelRepo.WhereAsync(h => h.OwnerId == ownerId && h.IsDeleted != true);
            var hotelIds = ownedHotels.Select(h => h.Id).ToList();

            if (!hotelIds.Any())
            {
                return ResponseFactory.Success(Enumerable.Empty<BookingHistoryDTO>(), "No bookings found.");
            }

            // 2. Get bookings for these hotels
            var bookings = await _bookingRepo.GetBookingsByHotelsAsync(hotelIds);

            // 3. Apply Filters
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                bookings = bookings.Where(b => b.Status == status).ToList();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                bookings = bookings.Where(b => 
                    b.Id.ToString().Contains(searchTerm) || 
                    (b.Customer != null && b.Customer.FullName != null && b.Customer.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            var content = bookings.OrderByDescending(b => b.CreatedAt).Select(b => new BookingHistoryDTO
            {
                Id = b.Id,
                BookingReference = $"BK-{b.Id}",
                HotelId = b.HotelId,
                HotelName = b.Hotel?.Name ?? "Your Hotel",
                RoomTypeId = b.RoomTypeId,
                RoomTypeName = b.RoomType?.Name ?? "Room Type",
                CheckInDate = b.CheckInDate.ToDateTime(TimeOnly.MinValue),
                CheckOutDate = b.CheckOutDate.ToDateTime(TimeOnly.MinValue),
                TotalPrice = b.TotalPrice,
                Status = b.Status ?? "Unknown",
                CreatedAt = b.CreatedAt ?? DateTime.MinValue,
                CustomerId = b.CustomerId,
                CustomerName = b.Customer?.FullName ?? "Unknown Customer"
            });

            return ResponseFactory.Success(content, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting owner bookings for owner {OwnerId}", ownerId);
            return ResponseFactory.ServerError<IEnumerable<BookingHistoryDTO>>();
        }
    }
}
