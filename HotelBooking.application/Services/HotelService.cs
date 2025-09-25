using System.Text.Json;
using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IHotelService
{
    public Task<string> GetOwnerDashBoard(int ownerId);
    public Task<List<SearchHotelResultDTO>> GetSearchOptionsAsync(string cityName, DateTime? checkIn, DateTime? checkOut,
    int? adults, int? children, int? rooms);
    public Task<ApiResponse<List<AmenityDTO>>> GetAllAmenitiesAsync();
    public Task<ApiResponse<AmenityDTO>> CreateAmenityAsync(AmenityCreateOrUpdateDTO newAmenity);
    public Task<ApiResponse<AmenityDTO>> UpdateAmenityAsync(int id, AmenityCreateOrUpdateDTO amenity);
    public Task<ApiResponse<bool>> DeleteAmenityAsync(int id);
}

public class HotelService : IHotelService
{
    public HotelBookingContext _context;
    private readonly IHotelRepository _hotelRepository;
    private readonly IAmenityRepository _amenityRepository;
    public IUnitOfWork _dbu;

    public HotelService(HotelBookingContext context, IHotelRepository hotelRepository, IAmenityRepository amenityRepository, IUnitOfWork dbu)
    {
        _context = context;
        _hotelRepository = hotelRepository;
        _amenityRepository = amenityRepository;
        _dbu = dbu;
    }

    public async Task<string> GetOwnerDashBoard(int ownerId)
    {
        return await Task.FromResult($"Owner Dashboard for Owner ID: {ownerId}");
    }

    // ================= TÌM KIẾM KHÁCH SẠN THEO FILTER SearchForm.razor ================
    public async Task<List<SearchHotelResultDTO>> GetSearchOptionsAsync(string cityName, DateTime? checkIn, DateTime? checkOut,
    int? adults, int? children, int? rooms)
    {
        var hotels = await _context.Database
    .SqlQueryRaw<SearchHotelResultDTO>(
        "EXEC sp_SearchHotels @CityName={0}, @CheckIn={1}, @CheckOut={2}, @Adults={3}, @Children={4}, @Rooms={5}",
        cityName, checkIn, checkOut, adults, children, rooms)
    .ToListAsync();
        return hotels;
    }

    #region MANAGE AMENITIES
    // =============== ĐỌC, THÊM, SỬA, XÓA TIỆN ÍCH CHO KHÁCH SẠN ================
    public async Task<ApiResponse<List<AmenityDTO>>> GetAllAmenitiesAsync()
    {
        try
        {
            List<AmenityDTO> result = new List<AmenityDTO>();

            var amenities = await _amenityRepository.WhereAsync(a => a.IsDeleted == false);

            if (amenities == null || amenities.Count() == 0)
            {
                return new ApiResponse<List<AmenityDTO>>
                {
                    StatusCode = StatusCodeResponse.NotFound,
                    Message = MessageResponse.EMPTY_LIST,
                    Content = null
                };
            }

            foreach (var amenity in amenities)
            {
                var additional = JsonSerializer.Deserialize<Dictionary<string, string>>(amenity.Additional ?? "{}");

                result.Add(new AmenityDTO
                {
                    Id = amenity.Id,
                    Name = amenity.Name,
                    Description = additional.GetValueOrDefault("Description", null),
                    IconClass = additional.GetValueOrDefault("IconClass", ""),
                    IconColor = additional.GetValueOrDefault("IconColor", "blue"),
                });
            }

            return new ApiResponse<List<AmenityDTO>>
            {
                StatusCode = StatusCodeResponse.Success,
                Content = result
            };
        }
        catch (Exception)
        {
            return new ApiResponse<List<AmenityDTO>>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = MessageResponse.ERROR_IN_SERVER,
                Content = null
            };
        }

    }
    public async Task<ApiResponse<AmenityDTO>> CreateAmenityAsync(AmenityCreateOrUpdateDTO newAmenity)
    {
        try
        {
            // Người dùng mà tạo trùng tên tiện ích thì không cho phép
            var exists = await _amenityRepository.AnyAsync(a => a.Name.ToLower() == newAmenity.Name.ToLower() && a.IsDeleted == false);
            if (exists) return new ApiResponse<AmenityDTO>
            {
                StatusCode = StatusCodeResponse.Conflict,
                Message = MessageResponse.NAME_ALREADY_EXISTS,
                Content = null
            };

            var additional = JsonSerializer.Serialize(new
            { Description = newAmenity.Description, IconClass = newAmenity.IconClass, IconColor = string.IsNullOrWhiteSpace(newAmenity.IconColor) ? "blue" : newAmenity.IconColor });

            var amenity = new Amenity
            {
                Name = newAmenity.Name,
                IsDeleted = false,
                Additional = additional
            };

            // Map entity sang DTO để trả về cho FE
            var resultDTO = new AmenityDTO
            {
                Id = amenity.Id,
                Name = amenity.Name,
                Description = newAmenity.Description,
                IconClass = newAmenity.IconClass,
                IconColor = string.IsNullOrWhiteSpace(newAmenity.IconColor) ? "blue" : newAmenity.IconColor
            };

            await _amenityRepository.AddAsync(amenity);
            await _dbu.SaveChangesAsync();

            return new ApiResponse<AmenityDTO>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = MessageResponse.CREATE_SUCCESSFULLY,
                Content = resultDTO
            };
        }
        catch (Exception)
        {
            return new ApiResponse<AmenityDTO>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = MessageResponse.ERROR_IN_SERVER,
                Content = null
            };
        }
    }

    public async Task<ApiResponse<AmenityDTO>> UpdateAmenityAsync(int id, AmenityCreateOrUpdateDTO amenity)
    {
        try
        {
            var existingAmenity = await _amenityRepository.GetByIdAsync(id);
            if (existingAmenity == null)
                return new ApiResponse<AmenityDTO>
                {
                    StatusCode = StatusCodeResponse.BadRequest,
                    Message = MessageResponse.UPDATE_FAILED,
                    Content = null
                };

            // Người dùng mà đổi tên trùng với tên của 1 tiện ích khác thì cũng không cho phép
            var nameExists = await _amenityRepository.AnyAsync(a => a.Id != id && a.Name.ToLower() == amenity.Name.ToLower() && a.IsDeleted == false);
            if (nameExists) return new ApiResponse<AmenityDTO>
            {
                StatusCode = StatusCodeResponse.Conflict,
                Message = MessageResponse.NAME_ALREADY_EXISTS,
                Content = null
            };

            existingAmenity.Name = amenity.Name;
            existingAmenity.Additional = JsonSerializer.Serialize(new
            {
                Description = string.IsNullOrWhiteSpace(amenity.Description) ? null : amenity.Description,
                IconClass = amenity.IconClass,
                IconColor = string.IsNullOrWhiteSpace(amenity.IconColor) ? "blue" : amenity.IconColor
            });

            var resultDTO = new AmenityDTO
            {
                Id = existingAmenity.Id,
                Name = existingAmenity.Name,
                Description = amenity.Description,
                IconClass = amenity.IconClass,
                IconColor = string.IsNullOrWhiteSpace(amenity.IconColor) ? "blue" : amenity.IconColor
            };

            await _amenityRepository.UpdateAsync(existingAmenity);
            await _dbu.SaveChangesAsync();

            return new ApiResponse<AmenityDTO>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = MessageResponse.UPDATE_SUCCESSFULLY,
                Content = resultDTO
            }; ;
        }
        catch (Exception)
        {
            return new ApiResponse<AmenityDTO>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = MessageResponse.ERROR_IN_SERVER,
                Content = null
            }; ;
        }
    }

    public async Task<ApiResponse<bool>> DeleteAmenityAsync(int id)
    {
        var amenity = await _amenityRepository.GetByIdAsync(id);

        if (amenity == null) return new ApiResponse<bool>
        {
            StatusCode = StatusCodeResponse.NotFound,
            Message = MessageResponse.NOT_FOUND,
            Content = false
        };

        try
        {
            amenity.IsDeleted = true;
            await _amenityRepository.UpdateAsync(amenity);
            await _dbu.SaveChangesAsync(); // EF Core tự track thay đổi
            return new ApiResponse<bool>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = MessageResponse.DELETE_SUCCESSFULLY,
                Content = true
            };
        }
        catch (Exception)
        {
            return new ApiResponse<bool>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = MessageResponse.ERROR_IN_SERVER,
                Content = false
            };
        }
    }
    #endregion

    
}