using System.Text.Json;
using HotelBooking.application.Services.Domains.Media;
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
        private readonly IHotelRepository _hotelRepository;
        private readonly IHotelImageRepository _hotelImageRepository;
        private readonly IHotelAmenityRepository _hotelAmenityRepository;
        private readonly IHotelPolicyRepository _hotelPolicyRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IImageHelper _imageHelper;
        private readonly IPhotoService _photoService;
        public IUnitOfWork _dbu;

        public HotelService(IHotelRepository hotelRepository, IHotelImageRepository hotelImageRepository, IHotelAmenityRepository hotelAmenityRepository, IHotelPolicyRepository hotelPolicyRepository, ICountryRepository countryRepository, ICityRepository cityRepository, IImageHelper imageHelper, IPhotoService photoService, IUnitOfWork dbu)
        {
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

        // ================= SEARCH HOTELS BY FILTER (SearchForm.razor) =================
        public async Task<List<SearchHotelResultDTO>> GetSearchOptionsAsync(string cityName, DateTime? checkIn, DateTime? checkOut,
        int? adults, int? children, int? rooms)
        {
            var results = await _hotelRepository.GetSearchHotelsAsync(cityName, checkIn, checkOut, adults, children, rooms);

            return results.Select(r => new SearchHotelResultDTO
            {
                Id = r.Id,
                Name = r.Name,
                Address = r.Address,
                Description = string.Empty, // Stored Procedure does not return Description
                CityName = r.CityName,
                CountryName = r.CountryName,
                CoverImageUrl = r.CoverImageUrl ?? string.Empty,
                PriceFrom = r.PriceFrom,
                MaxAdultCapacity = r.MaxAdultCapacity,
                MaxChildCapacity = r.MaxChildCapacity,
                AvgRating = r.AvgRating,
                ReviewCount = r.ReviewCount,
                AvailableRooms = r.AvailableRooms
            }).ToList();
        }

        #region 
        [Obsolete]
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

        [Obsolete]
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
        // =============== UPLOAD NEW HOTEL ================
        public async Task<ApiResponse<CreateHotelResponseDTO>> PostHotelAsync(CreateHotelDTO newHotel, int ownerId)
        {
            await _dbu.BeginTransactionAsync();

            try
            {
                // ================= STEP 1: CREATE HOTEL =================
                var hotel = new Hotel
                {
                    Name = newHotel.Name,
                    Address = newHotel.Address,
                    Description = newHotel.Description,
                    CoverImageUrl = null,
                    OwnerId = ownerId,
                    CreatedAt = DateTime.UtcNow,
                    IsVerified = true,  // default true; verification flow can be added later
                    Status = "Active",
                    IsDeleted = false,
                    CityId = newHotel.CityId,
                    CountryId = null
                };

                await _hotelRepository.AddAsync(hotel);
                await _dbu.SaveChangesAsync();

                int hotelId = hotel.Id;

                // ================= STEP 2: UPLOAD IMAGES =================
                if (newHotel.CoverFile != null)
                {
                    var coverUrl = await _photoService.UploadHotelCoverImageAsync(newHotel.CoverFile, ownerId, hotelId);
                    hotel.CoverImageUrl = coverUrl;
                }

                // Main image
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

                // Up to 4 sub-images
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

                // ================= STEP 3: AMENITIES & POLICIES =================
                foreach (var amenityId in newHotel.AmenityIds)
                {
                    await _hotelAmenityRepository.AddAsync(new HotelAmenity
                    {
                        HotelId = hotel.Id,
                        AmenityId = amenityId
                    });
                }

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

                // ================= STEP 4: FINAL UPDATE & COMMIT =================
                await _hotelRepository.UpdateAsync(hotel);
                await _dbu.SaveChangesAsync();

                await _dbu.CommitTransactionAsync();

                return new ApiResponse<CreateHotelResponseDTO>
                {
                    StatusCode = StatusCodeResponse.Success,
                    Message = MessageResponse.Common.CREATE_SUCCESSFULLY,
                    Content = new CreateHotelResponseDTO
                    {
                        HotelId = hotel.Id,
                        Name = hotel.Name
                    }
                };
            }
            catch (Exception)
            {
                await _dbu.RollBackTransactionAsync();

                return new ApiResponse<CreateHotelResponseDTO>
                {
                    StatusCode = StatusCodeResponse.Error,
                    Message = MessageResponse.Common.ERROR_IN_SERVER,
                    Content = null
                };
            }
        }

        #endregion

        // Test: Upload image to Cloudinary inside a folder named by userId
        [Obsolete]
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

