using System.Text.Json;
using HotelBooking.application.Interfaces;
using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.application.Services.Domains.HotelManagement
{
    public interface IHotelService
    {
        public Task<string> GetOwnerDashBoard(int ownerId);
        public Task<List<SearchHotelResultDTO>> GetSearchOptionsAsync(string cityName, DateTime? checkIn, DateTime? checkOut,
        int? adults, int? children, int? rooms);
        public Task<ApiResponse<List<CityDTO>>> GetAllCitiesAsync();
        public Task<ApiResponse<CreateHotelResponseDTO>> PostHotelAsync(CreateHotelDTO newHotel, int ownerId);


        public Task<ApiResponse<UploadResultDTO>> TestUploadImageToCloudinaryAsync(UploadFileDTO file, int userId);
    }

    public class HotelService : IHotelService
    {
        public HotelBookingDBContext _context;
        private readonly IHotelRepository _hotelRepository;
        private readonly IHotelImageRepository _hotelImageRepository;
        private readonly IHotelAmenityRepository _hotelAmenityRepository;
        private readonly IHotelPolicyRepository _hotelPolicyRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IImageHelper _imageHelper;
        private readonly IPhotoService _photoService;
        public IUnitOfWork _dbu;

        public HotelService(HotelBookingDBContext context, IHotelRepository hotelRepository, IHotelImageRepository hotelImageRepository, IHotelAmenityRepository hotelAmenityRepository, IHotelPolicyRepository hotelPolicyRepository, ICountryRepository countryRepository, ICityRepository cityRepository,   IImageHelper imageHelper, IPhotoService photoService, IUnitOfWork dbu)
        {
            _context = context;
            _hotelRepository = hotelRepository;
            _hotelImageRepository = hotelImageRepository;
            _hotelAmenityRepository = hotelAmenityRepository;
            _hotelPolicyRepository = hotelPolicyRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
            _imageHelper = imageHelper;
            _photoService = photoService;
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

        #region 
        public async Task<ApiResponse<List<CityDTO>>> GetAllCitiesVNAsync()
        {
            try
            {
                var country = await _countryRepository.FirstOrDefaultAsync(c => c.Name.ToLower() == "vietnam");

                List<CityDTO> result = new List<CityDTO>();

                var cities = await _cityRepository.GetAllAsync();

                if (cities == null || cities.Count() == 0)
                {
                    return new ApiResponse<List<CityDTO>>
                    {
                        StatusCode = StatusCodeResponse.NotFound,
                        Message = MessageResponse.EMPTY_LIST,
                        Content = null
                    };
                }

                foreach (var city in cities)
                {
                    var additional = JsonSerializer.Deserialize<Dictionary<string, string>>(city.Additional ?? "{}");

                    result.Add(new CityDTO
                    {
                        Id = city.Id,
                        Name = city.Name,
                    });
                }

                return new ApiResponse<List<CityDTO>>
                {
                    StatusCode = StatusCodeResponse.Success,
                    Message = MessageResponse.UPDATE_SUCCESSFULLY,
                    Content = result
                };
            }
            catch (Exception)
            {
                return new ApiResponse<List<CityDTO>>
                {
                    StatusCode = StatusCodeResponse.Error,
                    Message = MessageResponse.ERROR_IN_SERVER,
                    Content = null
                };
            }
        }

        public async Task<ApiResponse<List<CityDTO>>> GetAllCitiesAsync()
        {
            try
            {
                List<CityDTO> result = new List<CityDTO>();

                var cities = await _cityRepository.GetAllAsync();

                if (cities == null || cities.Count() == 0)
                {
                    return new ApiResponse<List<CityDTO>>
                    {
                        StatusCode = StatusCodeResponse.NotFound,
                        Message = MessageResponse.EMPTY_LIST,
                        Content = null
                    };
                }

                foreach (var city in cities)
                {
                    var additional = JsonSerializer.Deserialize<Dictionary<string, string>>(city.Additional ?? "{}");

                    result.Add(new CityDTO
                    {
                        Id = city.Id,
                        Name = city.Name,
                    });
                }

                return new ApiResponse<List<CityDTO>>
                {
                    StatusCode = StatusCodeResponse.Success,
                    Message = MessageResponse.UPDATE_SUCCESSFULLY,
                    Content = result
                };
            }
            catch (Exception)
            {
                return new ApiResponse<List<CityDTO>>
                {
                    StatusCode = StatusCodeResponse.Error,
                    Message = MessageResponse.ERROR_IN_SERVER,
                    Content = null
                };
            }
        }
        #endregion

        #region POST HOTEL (Basic Info + Amenities + Images)
        // =============== ĐĂNG TẢI KHÁCH SẠN MỚI ================
        public async Task<ApiResponse<CreateHotelResponseDTO>> PostHotelAsync(CreateHotelDTO newHotel, int ownerId)
        {
            try
            {
                // Bước 1: Tạo Hotel trước
                var hotel = new Hotel
                {
                    Name = newHotel.Name,
                    Address = newHotel.Address,
                    Description = newHotel.Description,
                    CoverImageUrl = null,
                    OwnerId = ownerId,
                    CreatedAt = DateTime.UtcNow,
                    IsVerified = true,  // mặc định là true, sau này có thể thêm chức năng verify
                    Status = "Active",
                    IsDeleted = false,
                    CityId = newHotel.CityId,
                    CountryId = null
                };

                await _hotelRepository.AddAsync(hotel);
                await _dbu.SaveChangesAsync();

                int hotelId = hotel.Id;

                // Bước 2: Upload ảnh
                if (newHotel.CoverFile != null)
                {
                    var coverUrl = await _photoService.UploadHotelCoverImageAsync(newHotel.CoverFile, ownerId, hotelId);
                    hotel.CoverImageUrl = coverUrl;
                }

                // Ảnh chính
                if (newHotel.MainFile != null)
                {
                    var mainUrl = await _photoService.UploadHotelMainImageAsync(newHotel.MainFile, ownerId, hotel.Id);
                    await _hotelImageRepository.AddAsync(new HotelImage
                    {
                        HotelId = hotel.Id,
                        ImageUrl = mainUrl,
                        IsDeleted = false
                    });
                }

                // 4 ảnh phụ
                if (newHotel.SubFiles != null)
                {
                    foreach (var file in newHotel.SubFiles)
                    {
                        var subUrl = await _photoService.UploadHotelSubImageAsync(file, ownerId, hotel.Id);
                        await _hotelImageRepository.AddAsync(new HotelImage
                        {
                            HotelId = hotel.Id,
                            ImageUrl = subUrl,
                            IsDeleted = false
                        });
                    }
                }

                // Bước 3: Lưu Amenities
                foreach (var amenityId in newHotel.AmenityIds)
                {
                    await _hotelAmenityRepository.AddAsync(new HotelAmenity
                    {
                        HotelId = hotel.Id,
                        AmenityId = amenityId
                    });
                }

                // Bước 4: Lưu Policies
                if (newHotel.PolicyIds != null && newHotel.PolicyIds.Any())
                {
                    foreach (var policyId in newHotel.PolicyIds)
                    {
                        await _hotelPolicyRepository.AddAsync(new HotelPolicy
                        {
                            HotelId = hotel.Id,
                            PolicyId = policyId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // Bước 5: Update lại Hotel + SaveChanges
                await _hotelRepository.UpdateAsync(hotel);
                await _dbu.SaveChangesAsync();

                return new ApiResponse<CreateHotelResponseDTO>
                {
                    StatusCode = StatusCodeResponse.Success,
                    Message = MessageResponse.CREATE_SUCCESSFULLY,
                    Content = new CreateHotelResponseDTO
                    {
                        HotelId = hotel.Id,
                        Name = hotel.Name
                    }
                };
            }
            catch (Exception)
            {
                return new ApiResponse<CreateHotelResponseDTO>
                {
                    StatusCode = StatusCodeResponse.Error,
                    Message = MessageResponse.ERROR_IN_SERVER,
                    Content = null
                };
            }
        }

        #endregion

        // Test: Up ảnh lên Cloudinary vào folder có mã userId
        public async Task<ApiResponse<UploadResultDTO>> TestUploadImageToCloudinaryAsync(UploadFileDTO file, int userId)
        {
            var uploadResult = new UploadResultDTO();

            var uploadResponse = await _photoService.UploadPhotoAsync(file, userId);
            var storedFileName = uploadResponse;

            if (storedFileName == null)
            {
                uploadResult.Uploaded = false;
                uploadResult.FileName = file.FileName;
                uploadResult.StoredFileName = null;
            }
            else
            {
                uploadResult.Uploaded = true;
                uploadResult.FileName = file.FileName;
                uploadResult.StoredFileName = storedFileName;
            }

            return new ApiResponse<UploadResultDTO>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = uploadResult.Uploaded ? MessageResponse.UPDATE_SUCCESSFULLY : MessageResponse.UPDATE_FAILED,
                Content = uploadResult
            };
        }
    }
}

