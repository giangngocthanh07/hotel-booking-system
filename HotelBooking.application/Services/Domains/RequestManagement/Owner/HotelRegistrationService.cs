
using System.Text.Json;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement.Base;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.RequestManagement.Owner
{
    public interface IHotelRegistrationService : IBaseUserRequestService<HotelRegistrationDetailDTO, HotelRegistrationDTO>
    {
    }

    public class HotelRegistrationService : IHotelRegistrationService
    {
        private readonly IHotelApprovalRequestRepository _approvalRepo;
        private readonly IHotelRepository _hotelRepo;
        private readonly IValidator<HotelRegistrationDTO> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HotelRegistrationService> _logger;


        public HotelRegistrationService(IHotelApprovalRequestRepository approvalRepo, IHotelRepository hotelRepo, IValidator<HotelRegistrationDTO> validator, IUnitOfWork unitOfWork, ILogger<HotelRegistrationService> logger)
        {
            _approvalRepo = approvalRepo;
            _hotelRepo = hotelRepo;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<HotelRegistrationDetailDTO>> CreateRequestAsync(int ownerId, HotelRegistrationDTO request)
        {
            try
            {
                // Check ownerId
                if (ownerId <= 0)
                {
                    return ResponseFactory.Failure<HotelRegistrationDetailDTO>(StatusCodeResponse.Unauthorized, MessageResponse.RequestManagement.HotelApproval.OWNER_ID_INVALID);
                }

                var validation = await _validator.ValidateAsync(request);
                if (!validation.IsValid)
                {
                    return ResponseFactory.Failure<HotelRegistrationDetailDTO>(
                        StatusCodeResponse.BadRequest,
                        validation.Errors.First().ErrorMessage);
                }

                // Check duplicate name
                var hasDuplicateName = await _hotelRepo.AnyAsync(h => h.Name == request.Name);
                if (hasDuplicateName)
                {
                    return ResponseFactory.Failure<HotelRegistrationDetailDTO>(
                        StatusCodeResponse.Conflict,
                        MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_NAME_ALREADY_EXISTS);
                }

                // Add Hotel Request to Hotel with Pending Status
                var additionalData = new HotelAdditionalInfoForm
                {
                    Description = request.Description,
                    StarRating = request.StarRating,
                    PublicPhone = request.PublicPhone,
                    PublicEmail = request.PublicEmail,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    PropType = new PropertyTypeDTO
                    {
                        Id = request.PropertyTypeId,
                        Name = request.PropertyTypeName
                    },
                    Country = new CountryDTO
                    {
                        Id = request.CountryId,
                        Name = request.CountryName
                    },
                    Province = new ProvinceDTO
                    {
                        Id = request.ProvinceId,
                        Name = request.ProvinceName
                    },
                    Ward = new WardDTO
                    {
                        Id = request.WardId,
                        Name = request.WardName
                    }
                };

                var hotel = new HotelApprovalRequest
                {
                    OwnerId = ownerId,
                    Name = request.Name,
                    Address = request.Address,
                    TaxCode = request.TaxCode,
                    BusinessLicenseUrl = request.BusinessLicenseUrl,
                    CreatedAt = DateTime.Now,
                    Status = RequestStatusConst.Pending,
                    Additional = JsonSerializer.Serialize(additionalData)
                };

                await _approvalRepo.AddAsync(hotel);
                var result = await _unitOfWork.SaveChangesAsync();

                var dto = new HotelRegistrationDetailDTO
                {
                    RequestId = hotel.Id,
                    OwnerId = hotel.OwnerId,
                    Name = hotel.Name,
                    Address = hotel.Address,
                    Description = additionalData.Description,
                    PropertyTypeId = additionalData.PropType.Id,
                    PropertyTypeName = additionalData.PropType.Name,
                    CountryId = additionalData.Country.Id,
                    CountryName = additionalData.Country.Name,
                    ProvinceId = additionalData.Province.Id,
                    ProvinceName = additionalData.Province.Name,
                    WardId = additionalData.Ward.Id,
                    WardName = additionalData.Ward.Name,
                    StarRating = additionalData.StarRating,
                    Status = hotel.Status,
                    PublicPhone = additionalData.PublicPhone,
                    PublicEmail = additionalData.PublicEmail,
                    Latitude = additionalData.Latitude,
                    Longitude = additionalData.Longitude,
                    TaxCode = hotel.TaxCode,
                    BusinessLicenseUrl = hotel.BusinessLicenseUrl
                };

                return ResponseFactory.Success(dto, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CREATED_SUCCESS);
            }
            catch (Exception ex)
            {
                _logger.LogError("HotelRegistrationService.CreateRequestAsync: {ErrorMessage}", ex.Message);
                return ResponseFactory.ServerError<HotelRegistrationDetailDTO>();
            }
        }

        public async Task<ApiResponse<bool>> CancelRequestAsync(int userId, int requestId)
        {
            try
            {
                // Validate userId
                if (userId <= 0)
                {
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.HotelApproval.OWNER_ID_INVALID);
                }

                if (requestId <= 0)
                {
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);
                }

                var pendingRequests = await _approvalRepo.GetPendingByIdAsync(userId);
                var request = pendingRequests.FirstOrDefault(r => r.Id == requestId);

                if (request == null)
                {
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);
                }

                request.Status = RequestStatusConst.Cancelled;
                await _approvalRepo.UpdateAsync(request);

                var saved = await _unitOfWork.SaveChangesAsync() > 0;
                return saved ? ResponseFactory.Success(true, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CANCELLED_SUCCESS) : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CANCEL_FAILED);
            }
            catch (Exception ex)
            {
                _logger.LogError("HotelRegistrationService.CancelRequestAsync: {ErrorMessage}", ex.Message);
                return ResponseFactory.ServerError<bool>();
            }
        }

        public async Task<ApiResponse<List<HotelRegistrationDetailDTO>>> GetMyRequestsAsync(int userId)
        {
            try
            {
                // Validate userId
                if (userId <= 0)
                {
                    return ResponseFactory.Failure<List<HotelRegistrationDetailDTO>>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.HotelApproval.OWNER_ID_INVALID);
                }

                var requests = await _approvalRepo.GetByUserIdAsync(userId);

                if (!requests.Any())
                {
                    return ResponseFactory.Success(new List<HotelRegistrationDetailDTO>(), MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_NO_REQUEST_FOUND);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var rawHotels = requests.ToList();

                var dtoItems = new List<HotelRegistrationDetailDTO>();

                foreach (var i in rawHotels)
                {
                    HotelAdditionalInfo additionalInfo = !string.IsNullOrWhiteSpace(i.Additional)
                    ? JsonSerializer.Deserialize<HotelAdditionalInfo>(i.Additional, options) ?? new HotelAdditionalInfo()
                    : new HotelAdditionalInfo();

                    var dto = new HotelRegistrationDetailDTO();

                    dto.RequestId = i.Id;
                    dto.OwnerId = i.OwnerId;
                    dto.Name = i.Name;
                    dto.Address = i.Address;
                    dto.Description = additionalInfo.Description;
                    dto.PropertyTypeId = additionalInfo.PropType.Id;
                    dto.PropertyTypeName = additionalInfo.PropType.Name;
                    dto.CountryId = additionalInfo.Country.Id;
                    dto.CountryName = additionalInfo.Country.Name;
                    dto.ProvinceId = additionalInfo.Province.Id;
                    dto.ProvinceName = additionalInfo.Province.Name;
                    dto.WardId = additionalInfo.Ward.Id;
                    dto.WardName = additionalInfo.Ward.Name;

                    dto.Latitude = additionalInfo?.Latitude;
                    dto.Longitude = additionalInfo?.Longitude;

                    dto.StarRating = additionalInfo?.StarRating;
                    dto.PublicPhone = additionalInfo?.PublicPhone ?? string.Empty;
                    dto.PublicEmail = additionalInfo?.PublicEmail ?? string.Empty;
                    dto.TaxCode = i.TaxCode;
                    dto.BusinessLicenseUrl = i.BusinessLicenseUrl;
                    dto.Status = i.Status ?? RequestStatusConst.None;

                    dtoItems.Add(dto);
                }
                return ResponseFactory.Success(dtoItems, MessageResponse.RequestManagement.HotelApproval.HOTELS_RETRIEVED);
            }
            catch (Exception ex)
            {
                _logger.LogError("HotelRegistrationService.GetMyRequestsAsync: {ErrorMessage}", ex.Message);
                return ResponseFactory.ServerError<List<HotelRegistrationDetailDTO>>();
            }
        }
    }
}