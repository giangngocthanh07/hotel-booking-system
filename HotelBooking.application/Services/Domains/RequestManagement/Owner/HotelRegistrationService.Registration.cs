using System.Text.Json;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.RequestManagement.Owner;

public partial class HotelRegistrationService
{
    public Task<ApiResponse<HotelRegistrationDetailDTO>> CreateRequestAsync(int ownerId, HotelRegistrationDTO request) =>
        CreateRequestAsync(ownerId, request, default);

    public async Task<ApiResponse<HotelRegistrationDetailDTO>> CreateRequestAsync(int ownerId,
        HotelRegistrationDTO request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ownerId <= 0) return RegistrationFailure(StatusCodeResponse.Unauthorized,
            MessageResponse.RequestManagement.HotelApproval.OWNER_ID_INVALID);
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return RegistrationFailure(StatusCodeResponse.BadRequest, validation.Errors[0].ErrorMessage);
        try
        {
            // FR-ONB-009/013: missing, foreign and deleted receipts share one response.
            var license = await _uploads.GetOwnedActiveAsync(request.BusinessLicenseUploadId.GetValueOrDefault(), ownerId, cancellationToken);
            if (license == null) return RegistrationFailure(StatusCodeResponse.BadRequest, "Select a business license uploaded by your account.");
            var failure = await ValidateRegistrationReferencesAsync(request);
            return failure ?? await SaveRegistrationAsync(ownerId, request, license, cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Hotel registration failed for Owner {OwnerId}.", ownerId);
            return ResponseFactory.ServerError<HotelRegistrationDetailDTO>();
        }
    }

    private async Task<ApiResponse<HotelRegistrationDetailDTO>?> ValidateRegistrationReferencesAsync(HotelRegistrationDTO request)
    {
        if (await _hotelRepo.AnyAsync(hotel => hotel.Name == request.Name && hotel.IsDeleted != true))
            return RegistrationFailure(StatusCodeResponse.Conflict, MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_NAME_ALREADY_EXISTS);
        if (await _approvalRepo.AnyAsync(approval => approval.Name == request.Name && approval.Status == RequestStatusConst.Pending))
            return RegistrationFailure(StatusCodeResponse.Conflict, "Một yêu cầu đăng ký cho khách sạn này đang chờ xử lý.");
        return await ValidateRegistrationLocationAsync(request);
    }

    private async Task<ApiResponse<HotelRegistrationDetailDTO>?> ValidateRegistrationLocationAsync(HotelRegistrationDTO request)
    {
        var propertyType = await _propertyTypeRepo.GetByIdAsync(request.PropertyTypeId);
        if (propertyType == null || propertyType.IsActive != true)
            return RegistrationFailure(StatusCodeResponse.BadRequest, "Loại hình lưu trữ (Property Type) không tồn tại hoặc đã ngừng hoạt động.");
        var province = await _provinceRepo.GetByIdAsync(request.ProvinceId);
        if (province == null) return RegistrationFailure(StatusCodeResponse.BadRequest, "Tỉnh/Thành phố không tồn tại.");
        var ward = await _wardRepo.GetByIdAsync(request.WardId);
        if (ward == null || ward.ProvinceId != request.ProvinceId)
            return RegistrationFailure(StatusCodeResponse.BadRequest, "Phường/Xã không tồn tại hoặc không thuộc Tỉnh/Thành phố đã chọn.");
        return null;
    }

    private async Task<ApiResponse<HotelRegistrationDetailDTO>> SaveRegistrationAsync(int ownerId, HotelRegistrationDTO input,
        BusinessLicenseUpload license, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var additional = RegistrationAdditional(input);
            var request = PendingRegistration(ownerId, input, license, additional);
            await _approvalRepo.AddAsync(request);
            var saved = await _unitOfWork.TrySaveChangesAsync(cancellationToken);
            if (saved.HasConcurrencyConflict || saved.AffectedRows < 1)
            {
                await _unitOfWork.RollBackTransactionAsync();
                return RegistrationFailure(StatusCodeResponse.Error, "Không thể tạo yêu cầu đăng ký.");
            }
            var detail = CreatedRegistrationDetail(request, additional);
            await _unitOfWork.CommitTransactionAsync();
            return ResponseFactory.Success(detail, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CREATED_SUCCESS);
        }
        catch { await _unitOfWork.RollBackTransactionAsync(); throw; }
    }

    private static HotelApprovalRequest PendingRegistration(int ownerId, HotelRegistrationDTO input,
        BusinessLicenseUpload license, HotelAdditionalInfoForm additional) => new()
    {
        OwnerId = ownerId, Name = input.Name, Address = input.Address, TaxCode = input.TaxCode,
        // FR-ONB-013: the registry supplies both the link and URL; never use client URL input.
        BusinessLicenseUploadId = license.Id, BusinessLicenseUrl = license.StoredUrl,
        CreatedAt = DateTime.UtcNow, Status = RequestStatusConst.Pending, Additional = JsonSerializer.Serialize(additional),
    };

    private static HotelAdditionalInfoForm RegistrationAdditional(HotelRegistrationDTO input) => new()
    {
        Description = input.Description, StarRating = input.StarRating,
        PublicPhone = input.PublicPhone, PublicEmail = input.PublicEmail, Latitude = input.Latitude, Longitude = input.Longitude,
        PropType = new PropertyTypeDTO { Id = input.PropertyTypeId, Name = input.PropertyTypeName },
        Country = new CountryDTO { Id = input.CountryId, Name = input.CountryName },
        Province = new ProvinceDTO { Id = input.ProvinceId, Name = input.ProvinceName },
        Ward = new WardDTO { Id = input.WardId, Name = input.WardName },
    };

    private static HotelRegistrationDetailDTO CreatedRegistrationDetail(HotelApprovalRequest request, HotelAdditionalInfoForm additional)
    {
        var detail = new HotelRegistrationDetailDTO
        {
            RequestId = request.Id, OwnerId = request.OwnerId, Name = request.Name, Address = request.Address,
            TaxCode = request.TaxCode, BusinessLicenseUrl = request.BusinessLicenseUrl, Status = request.Status ?? RequestStatusConst.Pending,
        };
        ApplyRegistrationAdditional(detail, additional);
        return detail;
    }

    private static void ApplyRegistrationAdditional(HotelRegistrationDetailDTO detail, HotelAdditionalInfoForm additional)
    {
        detail.Description = additional.Description;
        detail.StarRating = additional.StarRating;
        detail.PublicPhone = additional.PublicPhone;
        detail.PublicEmail = additional.PublicEmail;
        detail.Latitude = additional.Latitude;
        detail.Longitude = additional.Longitude;
        detail.PropertyTypeId = additional.PropType.Id;
        detail.PropertyTypeName = additional.PropType.Name;
        detail.CountryId = additional.Country.Id;
        detail.CountryName = additional.Country.Name;
        detail.ProvinceId = additional.Province.Id;
        detail.ProvinceName = additional.Province.Name;
        detail.WardId = additional.Ward.Id;
        detail.WardName = additional.Ward.Name;
    }

    private static ApiResponse<HotelRegistrationDetailDTO> RegistrationFailure(string status, string message) =>
        ResponseFactory.Failure<HotelRegistrationDetailDTO>(status, message);
}
