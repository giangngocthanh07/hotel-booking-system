
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
        Task<ApiResponse<HotelRegistrationDetailDTO>> CreateRequestAsync(int ownerId, HotelRegistrationDTO request, CancellationToken cancellationToken);
    }

    public partial class HotelRegistrationService : IHotelRegistrationService
    {
        private readonly IHotelApprovalRequestRepository _approvalRepo;
        private readonly IHotelRepository _hotelRepo;
        private readonly IPropertyTypeRepository _propertyTypeRepo;
        private readonly IProvinceRepository _provinceRepo;
        private readonly IWardRepository _wardRepo;
        private readonly IValidator<HotelRegistrationDTO> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HotelRegistrationService> _logger;
        private readonly IUserRepository _userRepo;
        private readonly IBusinessLicenseUploadRepository _uploads;


        public HotelRegistrationService(IHotelApprovalRequestRepository approvalRepo, IHotelRepository hotelRepo, IPropertyTypeRepository propertyTypeRepo, IProvinceRepository provinceRepo, IWardRepository wardRepo, IValidator<HotelRegistrationDTO> validator, IUnitOfWork unitOfWork, ILogger<HotelRegistrationService> logger, IUserRepository userRepo, IBusinessLicenseUploadRepository uploads)
        {
            _approvalRepo = approvalRepo;
            _hotelRepo = hotelRepo;
            _propertyTypeRepo = propertyTypeRepo;
            _provinceRepo = provinceRepo;
            _wardRepo = wardRepo;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userRepo = userRepo;
            _uploads = uploads;
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
                // FR-ONB-018: enforce ownership and Pending status before changing a request.
                var request = pendingRequests.FirstOrDefault(r =>
                    r.Id == requestId && r.OwnerId == userId && r.Status == RequestStatusConst.Pending);

                if (request == null)
                {
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);
                }

                request.Status = RequestStatusConst.Cancelled;
                // FR-ONB-018: UpdatedAt records cancellation time without an Admin decision.
                request.UpdatedAt = DateTime.UtcNow;
                await _approvalRepo.UpdateAsync(request);

                // FR-ONB-019: another saved decision wins over this stale cancellation.
                var saveResult = await _unitOfWork.TrySaveChangesAsync();
                if (saveResult.HasConcurrencyConflict) return HotelRequestSaveResponse.Conflict();
                return saveResult.AffectedRows > 0 ? ResponseFactory.Success(true, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CANCELLED_SUCCESS) : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CANCEL_FAILED);
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
                // FR-ONB-013: enrich only requests already scoped to this Owner by the repository.
                var processorNames = await HotelRequestDecisionMapper.LoadNamesAsync(rawHotels, _userRepo);

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
                    dto.RequestedAt = i.CreatedAt;
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

                    HotelRequestDecisionMapper.Apply(dto, i, processorNames);
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
