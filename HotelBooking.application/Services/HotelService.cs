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
    public Task<ApiResponse<PolicyDTO>> CreatePolicyAsync(PolicyCreateOrUpdateDTO newPolicy);
    public Task<ApiResponse<PolicyDTO>> UpdatePolicyAsync(int id, PolicyCreateOrUpdateDTO policy);
    public Task<ApiResponse<bool>> DeletePolicyAsync(int id);
    public Task<ApiResponse<List<ServiceTypeDTO>>> GetAllServiceTypesAsync();
    public Task<ApiResponse<List<ServiceBaseDTO>>> GetAllServicesByTypeAsync(int typeId);
    public Task<ApiResponse<ServiceBaseDTO>> CreateServiceByTypeAsync(CreateServiceAdminDTO newService, int typeId);
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
    private readonly IServiceTypeRepository _serviceTypeRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IImageHelper _imageHelper;
    private readonly IPhotoService _photoService;
    public IUnitOfWork _dbu;

    public HotelService(HotelBookingDBContext context, IHotelRepository hotelRepository, IAmenityRepository amenityRepository, ICityRepository cityRepository, IPolicyTypeRepository policyTypeRepository, IPolicyRepository policyRepository, IServiceTypeRepository serviceTypeRepository, IServiceRepository serviceRepository, IImageHelper imageHelper, IPhotoService photoService, IUnitOfWork dbu)
    {
        _context = context;
        _hotelRepository = hotelRepository;
        _amenityRepository = amenityRepository;
        _cityRepository = cityRepository;
        _policyTypeRepository = policyTypeRepository;
        _policyRepository = policyRepository;
        _serviceTypeRepository = serviceTypeRepository;
        _serviceRepository = serviceRepository;
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

    public async Task<ApiResponse<PolicyDTO>> CreatePolicyAsync(PolicyCreateOrUpdateDTO newPolicy)
    {
        try
        {
            // Người dùng mà tạo trùng tên tiện ích thì không cho phép
            var exists = await _policyRepository.AnyAsync(p => p.Name.ToLower() == newPolicy.Name.ToLower() && p.IsDeleted == false);
            if (exists) return new ApiResponse<PolicyDTO>
            {
                StatusCode = StatusCodeResponse.Conflict,
                Message = MessageResponse.NAME_ALREADY_EXISTS,
                Content = null
            };

            // var additional = JsonSerializer.Serialize(new
            // {  });

            var policy = new Policy
            {
                Name = newPolicy.Name,
                Description = newPolicy.Description,
                IsDeleted = false,
                PolicyTypeId = newPolicy.PolicyTypeId
            };

            // Map entity sang DTO để trả về cho FE
            var resultDTO = new PolicyDTO
            {
                Id = policy.Id,
                Name = policy.Name,
                Description = newPolicy.Description,
                PolicyTypeId = newPolicy.PolicyTypeId
            };

            await _policyRepository.AddAsync(policy);
            await _dbu.SaveChangesAsync();

            return new ApiResponse<PolicyDTO>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = MessageResponse.CREATE_SUCCESSFULLY,
                Content = resultDTO
            };
        }
        catch (Exception)
        {
            return new ApiResponse<PolicyDTO>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = MessageResponse.ERROR_IN_SERVER,
                Content = null
            };
        }
    }

    public async Task<ApiResponse<PolicyDTO>> UpdatePolicyAsync(int id, PolicyCreateOrUpdateDTO policy)
    {
        try
        {
            var existingPolicy = await _policyRepository.GetByIdAsync(id);
            if (existingPolicy == null)
                return new ApiResponse<PolicyDTO>
                {
                    StatusCode = StatusCodeResponse.BadRequest,
                    Message = MessageResponse.UPDATE_FAILED,
                    Content = null
                };

            // Người dùng mà đổi tên trùng với tên của 1 tiện ích khác thì cũng không cho phép
            var nameExists = await _policyRepository.AnyAsync(p => p.Id != id && p.Name.ToLower() == policy.Name.ToLower());
            if (nameExists) return new ApiResponse<PolicyDTO>
            {
                StatusCode = StatusCodeResponse.Conflict,
                Message = MessageResponse.NAME_ALREADY_EXISTS,
                Content = null
            };

            existingPolicy.Name = policy.Name;
            existingPolicy.Description = policy.Description;
            // existingPolicy.Additional = JsonSerializer.Serialize(new
            // {

            // });

            var resultDTO = new PolicyDTO
            {
                Id = existingPolicy.Id,
                Name = existingPolicy.Name,
                Description = policy.Description,
                PolicyTypeId = existingPolicy.PolicyTypeId

            };

            await _policyRepository.UpdateAsync(existingPolicy);
            await _dbu.SaveChangesAsync();

            return new ApiResponse<PolicyDTO>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = MessageResponse.UPDATE_SUCCESSFULLY,
                Content = resultDTO
            }; ;
        }
        catch (Exception)
        {
            return new ApiResponse<PolicyDTO>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = MessageResponse.ERROR_IN_SERVER,
                Content = null
            }; ;
        }
    }

    public async Task<ApiResponse<bool>> DeletePolicyAsync(int id)
    {
        var policy = await _policyRepository.GetByIdAsync(id);

        if (policy == null) return new ApiResponse<bool>
        {
            StatusCode = StatusCodeResponse.NotFound,
            Message = MessageResponse.NOT_FOUND,
            Content = false
        };

        try
        {
            policy.IsDeleted = true;
            await _policyRepository.UpdateAsync(policy);
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

    #region MANAGE SERVICES
    public async Task<ApiResponse<List<ServiceTypeDTO>>> GetAllServiceTypesAsync()
    {
        try
        {
            var serviceTypes = await _serviceTypeRepository.WhereAsync(sv => sv.IsDeleted == false);

            if (serviceTypes == null || !serviceTypes.Any())
            {
                return new ApiResponse<List<ServiceTypeDTO>>
                {
                    StatusCode = StatusCodeResponse.NotFound,
                    Message = MessageResponse.EMPTY_LIST,
                    Content = null
                };
            }

            var result = serviceTypes.Select(sv => new ServiceTypeDTO
            {
                Id = sv.Id,
                Name = sv.TypeName,
                IsDeleted = sv.IsDeleted
            }).ToList();

            return new ApiResponse<List<ServiceTypeDTO>>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = MessageResponse.UPDATE_SUCCESSFULLY,
                Content = result
            };

        }
        catch (Exception)
        {
            return new ApiResponse<List<ServiceTypeDTO>>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = MessageResponse.ERROR_IN_SERVER,
                Content = null
            };
        }
    }

    public async Task<ApiResponse<List<ServiceBaseDTO>>> GetAllServicesByTypeAsync(int typeId)
    {
        try
        {
            // Khai báo list DTO abstract
            var resultList = new List<ServiceBaseDTO>();

            // 2. Dùng If-Else để build câu query cho đúng Type

            if (typeId == 1) // Standard
            {
                var services = await _serviceRepository.WhereAsync(sv => sv.ServiceTypeId == 1 && sv.IsDeleted == false);

                if (services == null || !services.Any())
                {
                    return new ApiResponse<List<ServiceBaseDTO>>
                    {
                        StatusCode = StatusCodeResponse.NotFound,
                        Message = MessageResponse.EMPTY_LIST,
                        Content = null
                    };
                }

                foreach (var sv in services)
                {
                    var additional = JsonSerializer.Deserialize<Dictionary<string, string>>(sv.Additional ?? "{}");

                    var standardDTO = new ServiceStandardDTO
                    {
                        Id = sv.Id,
                        Name = sv.Name,
                        Description = sv.Description,
                        Unit = additional?.GetValueOrDefault("Unit", ""),
                        Price = sv.Price,
                        IsDeleted = sv.IsDeleted,
                        ServiceTypeId = sv.ServiceTypeId
                    };

                    resultList.Add(standardDTO);
                }

            }
            else if (typeId == 2) // Airport Transfer
            {
                var services = await _serviceRepository.WhereAsync(sv => sv.ServiceTypeId == 2 && sv.IsDeleted == false);

                if (services == null || !services.Any())
                {
                    return new ApiResponse<List<ServiceBaseDTO>>
                    {
                        StatusCode = StatusCodeResponse.NotFound,
                        Message = MessageResponse.EMPTY_LIST,
                        Content = null
                    };
                }

                // Cấu hình để JsonSerializer không phân biệt chữ hoa/thường
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                foreach (var sv in services)
                {
                    var additional = JsonSerializer.Deserialize<ServiceAdditionalDataAT>(sv.Additional ?? "{}", jsonOptions);

                    var atDTO = new ServiceAirportTransferDTO
                    {
                        Id = sv.Id,
                        Name = sv.Name,
                        Description = sv.Description,
                        Price = sv.Price,
                        IsDeleted = sv.IsDeleted,
                        ServiceTypeId = sv.ServiceTypeId,

                        // Gán trực tiếp từ đối tượng 'additional'
                        MaxPassengers = additional?.MaxPassengers,
                        MaxLuggage = additional?.MaxLuggage,
                        RoundTripPrice = additional?.RoundTripPrice,
                        AdditionalFee = additional?.AdditionalFee,
                        AdditionalFeeStartTime = additional?.AdditionalFeeStartTime,
                        AdditionalFeeEndTime = additional?.AdditionalFeeEndTime
                    };

                    resultList.Add(atDTO);
                }
            }
            else
            {
                return new ApiResponse<List<ServiceBaseDTO>>
                {
                    StatusCode = StatusCodeResponse.NotFound,
                    Message = MessageResponse.NOT_FOUND,
                    Content = null
                };
            }
            return new ApiResponse<List<ServiceBaseDTO>>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = MessageResponse.UPDATE_SUCCESSFULLY,
                Content = resultList
            };
        }
        catch (Exception)
        {

            return new ApiResponse<List<ServiceBaseDTO>>
            {
                StatusCode = StatusCodeResponse.Error,
                Message = MessageResponse.ERROR_IN_SERVER,
                Content = null
            };
        }
    }

    public async Task<ApiResponse<ServiceBaseDTO>> CreateServiceByTypeAsync(CreateServiceAdminDTO newService, int typeId)
    {
        try
        {
            var exists = await _serviceRepository.AnyAsync(sv => sv.Name.ToLower() == newService.Name.ToLower() && sv.IsDeleted == false);
            if (exists) return new ApiResponse<ServiceBaseDTO>
            {
                StatusCode = StatusCodeResponse.Conflict,
                Message = MessageResponse.NAME_ALREADY_EXISTS,
                Content = null
            };

            var entityToAdd = new Service();

            // Các trường chung
            entityToAdd.Name = newService.Name;
            entityToAdd.Description = newService.Description;
            entityToAdd.ServiceTypeId = typeId;


            object? additionalData = null;

            switch (typeId)
            {
                case 1: // Standard
                        // Tạo object chứa các trường của Standard (Giá trị null)
                    if (newService is CreateStandardServiceAdminDTO standard)
                    {
                        additionalData = new { Unit = standard.Unit };
                    }
                    else
                    {
                        return new ApiResponse<ServiceBaseDTO>
                        {
                            StatusCode = StatusCodeResponse.BadRequest,
                            Message = MessageResponse.CREATE_FAILED,
                            Content = null
                        };
                    }
                    break;

                case 2: // Airport Transfer
                        // Tạo object chứa các trường của Airport (Giá trị null)
                    additionalData = new ServiceAdditionalDataAT()
                    {
                        MaxPassengers = 0,
                        MaxLuggage = 0,
                        RoundTripPrice = (decimal?)null,
                        AdditionalFee = (decimal?)null,
                        AdditionalFeeStartTime = (TimeSpan?)null,
                        AdditionalFeeEndTime = (TimeSpan?)null
                    };
                    break;

                default:
                    return new ApiResponse<ServiceBaseDTO>
                    {
                        StatusCode = StatusCodeResponse.BadRequest,
                        Message = MessageResponse.CREATE_FAILED,
                        Content = null
                    };
            }

            if (additionalData != null)
            {
                entityToAdd.Additional = JsonSerializer.Serialize(additionalData);
            }

            await _serviceRepository.AddAsync(entityToAdd);
            await _dbu.SaveChangesAsync();

            // Trả về DTO tương ứng
            ServiceBaseDTO resultDTO = null;
            if (typeId == 1)
            {
                resultDTO = new ServiceStandardDTO
                {
                    Id = entityToAdd.Id,
                    Name = entityToAdd.Name,
                    Description = entityToAdd.Description,
                    Unit = JsonSerializer.Deserialize<ServiceStandardDTO>(entityToAdd.Additional ?? "{}")?.Unit,
                    Price = entityToAdd.Price,
                    ServiceTypeId = entityToAdd.ServiceTypeId
                };
            }
            else if (typeId == 2)
            {
                resultDTO = new ServiceAirportTransferDTO
                {
                    Id = entityToAdd.Id,
                    Name = entityToAdd.Name,
                    Description = entityToAdd.Description,
                    Price = entityToAdd.Price,
                    ServiceTypeId = entityToAdd.ServiceTypeId,
                    MaxPassengers = null,
                    MaxLuggage = null,
                    RoundTripPrice = null,
                    AdditionalFee = null,
                    AdditionalFeeStartTime = null,
                    AdditionalFeeEndTime = null
                };
            }

            return new ApiResponse<ServiceBaseDTO>
            {
                StatusCode = StatusCodeResponse.Success,
                Message = MessageResponse.CREATE_SUCCESSFULLY,
                Content = resultDTO
            };

        }
        catch (Exception)
        {

            return new ApiResponse<ServiceBaseDTO>
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


#endregion

