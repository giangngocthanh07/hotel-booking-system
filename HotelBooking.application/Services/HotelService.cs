using System.Text.Json;
using HotelBooking.application.Interfaces;
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
    public Task<ApiResponse<List<PolicyTypeDTO>>> GetAllPolicyTypesAsync();
    public Task<ApiResponse<List<PolicyDTO>>> GetAllPoliciesByTypeAsync(int typeId);
    public Task<ApiResponse<List<CityDTO>>> GetAllCitiesAsync();
    public Task<ApiResponse<CreateHotelResponseDTO>> PostHotelAsync(CreateHotelDTO newHotel, int ownerId);

    public Task<ApiResponse<UploadResultDTO>> TestUploadImageToCloudinaryAsync(UploadFileDTO file, int userId);
}

public class HotelService : IHotelService
{
    public HotelBookingDBContext _context;
    private readonly IHotelRepository _hotelRepository;
    private readonly IAmenityRepository _amenityRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IPolicyTypeRepository _policyTypeRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IImageHelper _imageHelper;
    private readonly IPhotoService _photoService;
    public IUnitOfWork _dbu;

    public HotelService(HotelBookingDBContext context, IHotelRepository hotelRepository, IAmenityRepository amenityRepository, ICityRepository cityRepository, IPolicyTypeRepository policyTypeRepository, IPolicyRepository policyRepository, IImageHelper imageHelper, IPhotoService photoService, IUnitOfWork dbu)
    {
        _context = context;
        _hotelRepository = hotelRepository;
        _amenityRepository = amenityRepository;
        _cityRepository = cityRepository;
        _policyTypeRepository = policyTypeRepository;
        _policyRepository = policyRepository;
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
                Message = MessageResponse.UPDATE_SUCCESSFULLY,
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
            var nameExists = await _amenityRepository.AnyAsync(a => a.Id != id && a.Name.ToLower() == amenity.Name.ToLower());
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

    #region 
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
                _context.HotelImages.Add(new HotelImage
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
                    _context.HotelImages.Add(new HotelImage
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
                _context.HotelAmenities.Add(new HotelAmenity
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
                    _context.HotelPolicies.Add(new HotelPolicy
                    {
                        HotelId = hotel.Id,
                        PolicyId = policyId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Bước 5: Update lại Hotel + SaveChanges
            _context.Hotels.Update(hotel);
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

    #region MANAGE POLICIES
    // =============== ĐỌC, THÊM, SỬA, XÓA CHÍNH SÁCH CHO KHÁCH SẠN ================

    public async Task<ApiResponse<List<PolicyTypeDTO>>> GetAllPolicyTypesAsync()
    {
        try
        {
            var policyTypes = await _policyTypeRepository.WhereAsync(pt => pt.IsDeleted == false);

            if (policyTypes == null || !policyTypes.Any())
            {
                return new ApiResponse<List<PolicyTypeDTO>>
                {
                    StatusCode = StatusCodeResponse.NotFound,
                    Message = MessageResponse.EMPTY_LIST,
                    Content = null
                };
            }

            var result = policyTypes.Select(pt => new PolicyTypeDTO
            {
                Id = pt.Id,
                Name = pt.TypeName,
                IsDeleted = pt.IsDeleted
            }).ToList();

            return new ApiResponse<List<PolicyTypeDTO>>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = MessageResponse.UPDATE_SUCCESSFULLY,
                Content = result
            };

        }
        catch (Exception)
        {
            return new ApiResponse<List<PolicyTypeDTO>>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = MessageResponse.ERROR_IN_SERVER,
                Content = null
            };
        }
    }

    public async Task<ApiResponse<List<PolicyDTO>>> GetAllPoliciesByTypeAsync(int typeId)
    {
        try
        {
            var policies = await _policyRepository.WhereAsync(p => p.PolicyTypeId == typeId && p.IsDeleted == false);

            if (policies == null || !policies.Any())
            {
                return new ApiResponse<List<PolicyDTO>>
                {
                    StatusCode = StatusCodeResponse.NotFound,
                    Message = MessageResponse.EMPTY_LIST,
                    Content = null
                };
            }

            var result = policies.Select(p => new PolicyDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsDeleted = p.IsDeleted,
                PolicyTypeId = p.PolicyTypeId
            }).ToList();

            return new ApiResponse<List<PolicyDTO>>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = MessageResponse.UPDATE_SUCCESSFULLY,
                Content = result
            };
        }
        catch (Exception)
        {
            return new ApiResponse<List<PolicyDTO>>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = MessageResponse.ERROR_IN_SERVER,
                Content = null
            };
        }
    }

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


#endregion



// #region CREATE, UPDATE, DELETE HOTEL
// // =============== TẠO, SỬA, XÓA KHÁCH SẠN ================
// public async Task<ApiResponse<int>> CreateHotelAsync(CreateHotelDTO dto, int userId)
// {
//     await using var transaction = await _context.Database.BeginTransactionAsync();

//     try
//     {
//         // 🏨 Bước 1: Tạo Hotel trước
//         var hotel = new Hotel
//         {
//             Name = dto.Name,
//             Address = dto.Address,
//             Description = dto.Description,
//             CoverImageUrl = null,
//             OwnerId = userId,
//             CreatedAt = DateTime.UtcNow,
//             IsVerified = true,  // mặc định là true, sau này có thể thêm chức năng verify
//             Status = "Active",
//             IsDeleted = false,
//             CityId = dto.CityId,
//             CountryId = null
//         };

//         _context.Hotels.Add(hotel);
//         await _context.SaveChangesAsync(); // sinh HotelId

//         // 🏨 Bước 2: Upload ảnh
//         // Cover image (ảnh bìa) vẫn lưu trực tiếp vào Hotels
//         if (dto.CoverFile != null)
//         {
//             var coverUrl = await _imageHelper.UploadAsync(dto.CoverFile, userId, hotel.Id, "cover");
//             hotel.CoverImageUrl = coverUrl;
//         }

//         if (dto.MainFile != null)
//             var mainUrl = await _imageHelper.UploadAsync(dto.MainFile, userId, hotel.Id, "main");
//         _context.HotelImages.Add(new HotelImage
//         {
//             HotelId = hotel.Id,
//             ImageUrl = mainUrl,
//             IsDeleted = false
//         });
//         if (dto.SubFiles != null)
//         {
//             int index = 1;
//             foreach (var file in dto.SubFiles)
//             {
//                 var subUrl = await _imageHelper.UploadAsync(file, userId, hotel.Id, $"sub{index}");
//                 _context.HotelImages.Add(new HotelImage
//                 {
//                     HotelId = hotel.Id,
//                     ImageUrl = subUrl,
//                     IsDeleted = false
//                 });
//                 index++;
//             }
//         }

//         // 🏨 Bước 3: Lưu Policies
//         if (dto.PolicyIds != null && dto.PolicyIds.Any())
//         {
//             foreach (var policyId in dto.PolicyIds)
//             {
//                 _context.HotelPolicies.Add(new HotelPolicy
//                 {
//                     HotelId = hotel.Id,
//                     PolicyId = policyId,
//                     CreatedAt = DateTime.UtcNow
//                 });
//             }
//         }

//         // 🏨 Bước 4: Lưu Amenities
//         foreach (var amenityId in dto.AmenityIds)
//         {
//             _context.HotelAmenities.Add(new HotelAmenity
//             {
//                 HotelId = hotel.Id,
//                 AmenityId = amenityId
//             });
//         }

//         // 🏨 Bước 5: Update lại Hotel + SaveChanges
//         _context.Hotels.Update(hotel);
//         await _context.SaveChangesAsync();

//         // Commit transaction nếu tất cả đều thành công
//         await transaction.CommitAsync();
//         return new ApiResponse<int>
//         {
//             StatusCode = StatusCodeResponse.Success,
//             Message = MessageResponse.CREATE_SUCCESSFULLY,
//             Content = hotel.Id
//         };
//     }
//     catch (Exception ex)
//     {
//         // Rollback transaction nếu có lỗi
//         await transaction.RollbackAsync();
//         return new ApiResponse<int>
//         {
//             StatusCode = StatusCodeResponse.Error,
//             Message = MessageResponse.ERROR_IN_SERVER,
//             Content = 0
//         };
//     }

// }
// #endregion

// DTO cho tiện ích khi tạo/sửa khách sạn
// public class AmenityCreateOrUpdateDTO
// {
//     public string Name { get; set; } = "";
//     public string? Description { get; set; }
//     public string? IconClass { get; set; }
//     public string? IconColor { get; set; }
// }

// // DTO cho chính sách khi tạo/sửa khách sạn
// public class PolicyCreateOrUpdateDTO
// {
//     public string CheckIn { get; set; } = "";
//     public string CheckOut { get; set; } = "";
//     public string CancellationPolicy { get; set; } = "";
// }

// // DTO cho ảnh khách sạn khi tạo/sửa khách sạn
// public class HotelImageCreateDTO
// {
//     public string ImageUrl { get; set; } = "";
//     public bool IsCover { get; set; } = false;
// }

// // DTO tổng hợp cho việc tạo/sửa khách sạn
// public class HotelCreateOrUpdateDTO
// {
//     public string Name { get; set; } = "";
//     public string Address { get; set; } = "";
//     public string City { get; set; } = "";
//     public string Country { get; set; } = "";
//     public string Description { get; set; } = "";
//     public int StarRating { get; set; }
//     public List<AmenityCreateOrUpdateDTO> Amenities { get; set; } = new();
//     public PolicyCreateOrUpdateDTO? Policy { get; set; }
//     public List<HotelImageCreateDTO> Images { get; set; } = new();
// }
