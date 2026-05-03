
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Services.Domains.Media;

namespace HotelBooking.application.Services.Domains.HotelManagement
{
    public interface IHotelService
    {
        public Task<string> GetOwnerDashBoard(int ownerId);
        public Task<List<SearchHotelResultDTO>> GetSearchOptionsAsync(string cityName, DateTime? checkIn, DateTime? checkOut,
        int? adults, int? children, int? rooms);
        public Task<ApiResponse<IEnumerable<PropertyTypeDTO>>> GetPropertyTypesAsync();


        public Task<ApiResponse<UploadResultDTO>> TestUploadImageToCloudinaryAsync(UploadFileDTO file, int userId);
    }

    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IHotelImageRepository _hotelImageRepository;
        private readonly IHotelAmenityRepository _hotelAmenityRepository;
        private readonly IHotelPolicyRepository _hotelPolicyRepository;
        private readonly IPropertyTypeRepository _propTypeRepository;
        private readonly IImageHelper _imageHelper;
        private readonly IPhotoService _photoService;
        public IUnitOfWork _dbu;
        public ILogger<HotelService> _logger;


        public HotelService(IHotelRepository hotelRepository, IHotelImageRepository hotelImageRepository, IHotelAmenityRepository hotelAmenityRepository, IHotelPolicyRepository hotelPolicyRepository, IPropertyTypeRepository propTypeRepository, IImageHelper imageHelper, IPhotoService photoService, IUnitOfWork dbu, ILogger<HotelService> logger)
        {
            _hotelRepository = hotelRepository;
            _hotelImageRepository = hotelImageRepository;
            _hotelAmenityRepository = hotelAmenityRepository;
            _hotelPolicyRepository = hotelPolicyRepository;
            _propTypeRepository = propTypeRepository;
            _imageHelper = imageHelper;
            _photoService = photoService;
            _dbu = dbu;
            _logger = logger;
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

        #region PROPERTY TYPE
        public async Task<ApiResponse<IEnumerable<PropertyTypeDTO>>> GetPropertyTypesAsync()
        {
            try
            {
                var propertyTypes = await _propTypeRepository.GetAllAsync();
                var result = propertyTypes.Select(p => new PropertyTypeDTO
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToList();

                return new ApiResponse<IEnumerable<PropertyTypeDTO>>
                {
                    StatusCode = StatusCodeResponse.Success,
                    Message = MessageResponse.Common.GET_SUCCESSFULLY,
                    Content = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("HotelService.GetPropertyTypesAsync: {ErrorMessage}", ex.Message);
                return new ApiResponse<IEnumerable<PropertyTypeDTO>>
                {
                    StatusCode = StatusCodeResponse.Error,
                    Message = MessageResponse.Common.ERROR_IN_SERVER,
                    Content = null
                };
            }
        }

        #endregion

        #region POST HOTEL (Basic Info + Amenities + Images)
        // =============== UPLOAD NEW HOTEL ================
        // public async Task<ApiResponse<CreateHotelResponseDTO>> PostHotelAsync(HotelRegistrationDTO newHotel, int ownerId)
        // {
        //     await _dbu.BeginTransactionAsync();

        //     try
        //     {
        //         // ================= STEP 1: CREATE HOTEL =================
        //         var hotel = new Hotel
        //         {
        //             Name = newHotel.Name,
        //             Address = newHotel.Address,
        //             Description = newHotel.Description,
        //             CoverImageUrl = null,
        //             OwnerId = ownerId,
        //             CreatedAt = DateTime.UtcNow,
        //             IsVerified = true,  // default true; verification flow can be added later
        //             Status = "Active",
        //             IsDeleted = false,
        //             CountryId = null
        //         };

        //         await _hotelRepository.AddAsync(hotel);
        //         await _dbu.SaveChangesAsync();

        //         int hotelId = hotel.Id;

        //         // ================= STEP 2: UPLOAD IMAGES =================
        //         if (newHotel.CoverFile != null)
        //         {
        //             var coverUrl = await _photoService.UploadHotelCoverImageAsync(newHotel.CoverFile, ownerId, hotelId);
        //             hotel.CoverImageUrl = coverUrl;
        //         }

        //         // Main image
        //         if (newHotel.MainFile != null)
        //         {
        //             var mainUrl = await _photoService.UploadHotelMainImageAsync(newHotel.MainFile, ownerId, hotel.Id);
        //             await _hotelImageRepository.AddAsync(new HotelImage
        //             {
        //                 HotelId = hotel.Id,
        //                 ImageUrl = mainUrl,
        //                 IsDeleted = false
        //             });
        //         }

        //         // Up to 4 sub-images
        //         if (newHotel.SubFiles != null)
        //         {
        //             foreach (var file in newHotel.SubFiles)
        //             {
        //                 var subUrl = await _photoService.UploadHotelSubImageAsync(file, ownerId, hotel.Id);
        //                 await _hotelImageRepository.AddAsync(new HotelImage
        //                 {
        //                     HotelId = hotel.Id,
        //                     ImageUrl = subUrl,
        //                     IsDeleted = false
        //                 });
        //             }
        //         }

        //         // ================= STEP 3: AMENITIES & POLICIES =================
        //         foreach (var amenityId in newHotel.AmenityIds)
        //         {
        //             await _hotelAmenityRepository.AddAsync(new HotelAmenity
        //             {
        //                 HotelId = hotel.Id,
        //                 AmenityId = amenityId
        //             });
        //         }

        //         if (newHotel.PolicyIds != null && newHotel.PolicyIds.Any())
        //         {
        //             foreach (var policyId in newHotel.PolicyIds)
        //             {
        //                 await _hotelPolicyRepository.AddAsync(new HotelPolicy
        //                 {
        //                     HotelId = hotel.Id,
        //                     PolicyId = policyId,
        //                     CreatedAt = DateTime.UtcNow
        //                 });
        //             }
        //         }

        //         // ================= STEP 4: FINAL UPDATE & COMMIT =================
        //         await _hotelRepository.UpdateAsync(hotel);
        //         await _dbu.SaveChangesAsync();

        //         await _dbu.CommitTransactionAsync();

        //         return new ApiResponse<CreateHotelResponseDTO>
        //         {
        //             StatusCode = StatusCodeResponse.Success,
        //             Message = MessageResponse.Common.CREATE_SUCCESSFULLY,
        //             Content = new CreateHotelResponseDTO
        //             {
        //                 HotelId = hotel.Id,
        //                 Name = hotel.Name
        //             }
        //         };
        //     }
        //     catch (Exception)
        //     {
        //         await _dbu.RollBackTransactionAsync();

        //         return new ApiResponse<CreateHotelResponseDTO>
        //         {
        //             StatusCode = StatusCodeResponse.Error,
        //             Message = MessageResponse.Common.ERROR_IN_SERVER,
        //             Content = null
        //         };
        //     }
        // }

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

