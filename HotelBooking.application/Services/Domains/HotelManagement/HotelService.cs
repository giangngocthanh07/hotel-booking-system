
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Services.Domains.Media;
using HotelBooking.application.Validators.RoomManagement;

namespace HotelBooking.application.Services.Domains.HotelManagement
{
    public interface IHotelService
    {
        public Task<string> GetOwnerDashBoard(int ownerId);
        public Task<ApiResponse<IEnumerable<SearchHotelResultDTO>>> SearchHotelsAsync(HotelSearchRequestDTO request);
        public Task<ApiResponse<HotelDetailsDTO>> GetHotelDetailsAsync(int id);
        public Task<ApiResponse<IEnumerable<PropertyTypeDTO>>> GetPropertyTypesAsync();
        public Task<ApiResponse<UploadResultDTO>> TestUploadImageToCloudinaryAsync(UploadFileDTO file, int userId);
    }

    public class HotelService : IHotelService
    {
        // ... (existing implementation)

        public async Task<ApiResponse<HotelDetailsDTO>> GetHotelDetailsAsync(int id)
        {
            try
            {
                var hotel = await _hotelRepository.GetHotelDetailsByIdAsync(id);
                if (hotel == null)
                {
                    return new ApiResponse<HotelDetailsDTO>
                    {
                        StatusCode = StatusCodeResponse.Error,
                        Message = MessageResponse.Common.NOT_FOUND,
                        Content = null
                    };
                }

                var content = new HotelDetailsDTO
                {
                    Id = hotel.Id,
                    Name = hotel.Name,
                    Address = hotel.Address,
                    Description = hotel.Description,
                    CoverImageUrl = hotel.CoverImageUrl,
                    AvgRating = hotel.Reviews.Any() ? (decimal)hotel.Reviews.Average(r => r.Rating ?? 0) : 0,
                    ReviewCount = hotel.Reviews.Count,
                    Gallery = hotel.HotelImages.Select(i => i.ImageUrl).ToList(),
                    Amenities = hotel.HotelAmenities.Select(ha => new AmenityDTO
                    {
                        Id = ha.Amenity.Id,
                        Name = ha.Amenity.Name
                    }).ToList(),
                    RoomTypes = hotel.RoomTypes.Select(rt => new RoomTypeDetailsDTO
                    {
                        Id = rt.Id,
                        Name = rt.Name,
                        Description = rt.Description,
                        PricePerNight = rt.PricePerNight,
                        AdultCapacity = rt.AdultCapacity,
                        ChildCapacity = rt.ChildCapacity,
                        AreaSqm = rt.AreaSqm,
                        Images = rt.RoomImages.Select(ri => ri.ImageUrl).ToList(),
                        Amenities = rt.RoomAmenities.Select(ra => ra.Amenity.Name).ToList()
                    }).ToList(),
                    RecentReviews = hotel.Reviews.OrderByDescending(r => r.CreatedAt).Take(5).Select(r => new ReviewDTO
                    {
                        UserName = r.Customer.FullName ?? "Anonymous",
                        Comment = r.Comment ?? string.Empty,
                        Rating = r.Rating ?? 0,
                        CreatedAt = r.CreatedAt ?? DateTime.UtcNow
                    }).ToList()
                };

                return new ApiResponse<HotelDetailsDTO>
                {
                    StatusCode = StatusCodeResponse.Success,
                    Message = MessageResponse.Common.GET_SUCCESSFULLY,
                    Content = content
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel details for ID {Id}", id);
                return new ApiResponse<HotelDetailsDTO>
                {
                    StatusCode = StatusCodeResponse.Error,
                    Message = MessageResponse.Common.ERROR_IN_SERVER,
                    Content = null
                };
            }
        }
        private readonly IHotelRepository _hotelRepository;
        private readonly IHotelImageRepository _hotelImageRepository;
        private readonly IHotelAmenityRepository _hotelAmenityRepository;
        private readonly IHotelPolicyRepository _hotelPolicyRepository;
        private readonly IPropertyTypeRepository _propTypeRepository;
        private readonly IImageHelper _imageHelper;
        private readonly IPhotoService _photoService;
        private readonly IValidator<HotelSearchRequestDTO> _validator;
        public IUnitOfWork _dbu;
        public ILogger<HotelService> _logger;


        public HotelService(IHotelRepository hotelRepository, IHotelImageRepository hotelImageRepository, IHotelAmenityRepository hotelAmenityRepository, IHotelPolicyRepository hotelPolicyRepository, IPropertyTypeRepository propTypeRepository, IImageHelper imageHelper, IPhotoService photoService, IUnitOfWork dbu, ILogger<HotelService> logger, IValidator<HotelSearchRequestDTO> validator)
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
            _validator = validator;
        }

        public async Task<string> GetOwnerDashBoard(int ownerId)
        {
            return await Task.FromResult($"Owner Dashboard for Owner ID: {ownerId}");
        }

        // ================= SEARCH HOTELS BY FILTER (SearchForm.razor) =================
        public async Task<ApiResponse<IEnumerable<SearchHotelResultDTO>>> SearchHotelsAsync(HotelSearchRequestDTO request)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return new ApiResponse<IEnumerable<SearchHotelResultDTO>>
                    {
                        StatusCode = StatusCodeResponse.Error,
                        Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        Content = Enumerable.Empty<SearchHotelResultDTO>()
                    };
                }

                var results = await _hotelRepository.GetSearchHotelsAsync(
                    request.CityName ?? string.Empty, 
                    request.CheckIn, 
                    request.CheckOut, 
                    request.Adults, 
                    request.Children, 
                    request.Rooms);

                var content = results.Select(r => new SearchHotelResultDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Address = r.Address,
                    Description = string.Empty,
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

                return new ApiResponse<IEnumerable<SearchHotelResultDTO>>
                {
                    StatusCode = StatusCodeResponse.Success,
                    Message = MessageResponse.Common.GET_SUCCESSFULLY,
                    Content = content
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching hotels");
                return new ApiResponse<IEnumerable<SearchHotelResultDTO>>
                {
                    StatusCode = StatusCodeResponse.Error,
                    Message = MessageResponse.Common.ERROR_IN_SERVER,
                    Content = Enumerable.Empty<SearchHotelResultDTO>()
                };
            }
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
            return await _photoService.UploadPhotoAsync(file, userId);
        }

    }
}

