
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
        private readonly IHotelRepository _hotelRepo;
        private readonly IValidator<HotelRegistrationDTO> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public HotelRegistrationService(IHotelRepository hotelRepo, IValidator<HotelRegistrationDTO> validator, IUnitOfWork unitOfWork)
        {
            _hotelRepo = hotelRepo;
            _validator = validator;
            _unitOfWork = unitOfWork;
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
                var additionalData = new HotelAdditionalInfo
                {
                    StarRating = request.StarRating,
                    PublicPhone = request.PublicPhone,
                    PublicEmail = request.PublicEmail,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    TaxCode = request.TaxCode,
                    BusinessLicenseUrl = request.BusinessLicenseUrl
                };

                var hotel = new Hotel
                {
                    OwnerId = ownerId,
                    Name = request.Name,
                    Address = request.Address,
                    Description = request.Description,
                    PropertyTypeId = request.PropertyTypeId,
                    CountryId = request.CountryId,
                    ProvinceId = request.ProvinceId,
                    WardId = request.WardId,
                    Status = RequestStatusConst.Pending,
                    Additional = JsonSerializer.Serialize(additionalData)
                };

                await _hotelRepo.AddAsync(hotel);
                var result = await _unitOfWork.SaveChangesAsync();

                var dto = new HotelRegistrationDetailDTO
                {
                    RequestId = hotel.Id,
                    OwnerId = hotel.OwnerId,
                    Name = hotel.Name,
                    Address = hotel.Address,
                    Description = hotel.Description,
                    PropertyTypeId = hotel.PropertyTypeId,
                    CountryId = hotel.CountryId,
                    ProvinceId = hotel.ProvinceId,
                    WardId = hotel.WardId,
                    ProvinceName = hotel.Province?.Name ?? string.Empty,
                    WardName = hotel.Ward?.Name ?? string.Empty,
                    CountryName = hotel.Country?.Name ?? string.Empty,
                    Status = hotel.Status,
                    StarRating = additionalData.StarRating,
                    PublicPhone = additionalData.PublicPhone,
                    PublicEmail = additionalData.PublicEmail,
                    Latitude = additionalData.Latitude,
                    Longitude = additionalData.Longitude,
                    TaxCode = additionalData.TaxCode,
                    BusinessLicenseUrl = additionalData.BusinessLicenseUrl
                };



                return ResponseFactory.Success(dto, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CREATED_SUCCESS);
            }
            catch (Exception)
            {
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

                var pendingRequests = await _hotelRepo.GetPendingByIdAsync(userId);
                var request = pendingRequests.FirstOrDefault(r => r.Id == requestId);

                if (request == null)
                {
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);
                }

                request.Status = RequestStatusConst.Cancelled;
                await _hotelRepo.UpdateAsync(request);

                var saved = await _unitOfWork.SaveChangesAsync() > 0;
                return saved ? ResponseFactory.Success(true, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CANCELLED_SUCCESS) : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CANCEL_FAILED);
            }
            catch (Exception)
            {
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

                var requests = await _hotelRepo.GetByUserIdAsync(userId);

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
                    dto.Description = i.Description;
                    dto.PropertyTypeId = i.PropertyTypeId;
                    dto.CountryId = i.CountryId;
                    dto.ProvinceId = i.ProvinceId;
                    dto.WardId = i.WardId;
                    dto.ProvinceName = i.Province?.Name ?? string.Empty;
                    dto.WardName = i.Ward?.Name ?? string.Empty;
                    dto.CountryName = i.Country?.Name ?? string.Empty;
                    dto.Latitude = additionalInfo?.Latitude;
                    dto.Longitude = additionalInfo?.Longitude;
                    dto.StarRating = additionalInfo?.StarRating;
                    dto.PublicPhone = additionalInfo?.PublicPhone ?? string.Empty;
                    dto.PublicEmail = additionalInfo?.PublicEmail ?? string.Empty;
                    dto.TaxCode = additionalInfo?.TaxCode ?? string.Empty;
                    dto.BusinessLicenseUrl = additionalInfo?.BusinessLicenseUrl ?? string.Empty;
                    dto.Status = i.Status ?? RequestStatusConst.None;

                    dtoItems.Add(dto);
                }
                return ResponseFactory.Success(dtoItems, MessageResponse.RequestManagement.HotelApproval.HOTELS_RETRIEVED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<List<HotelRegistrationDetailDTO>>();
            }
        }
    }
}