
using System.Linq.Expressions;
using System.Text.Json;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement.Base;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.RequestManagement.Admin;

public interface IAdminHotelApprovalRequestService : IBaseAdminRequestService<HotelRegistrationDetailDTO>
{
}

public class AdminHotelApprovalRequestService : IAdminHotelApprovalRequestService
{
    private readonly IHotelRepository _hotelRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<PagingRequest> _pagingValidator;

    public AdminHotelApprovalRequestService(
        IHotelRepository hotelRepo,
        IUnitOfWork unitOfWork,
        IValidator<PagingRequest> pagingValidator)
    {
        _hotelRepo = hotelRepo;
        _unitOfWork = unitOfWork;
        _pagingValidator = pagingValidator;
    }

    public async Task<ApiResponse<List<string>>> GetAllStatusesAsync()
    {
        try
        {
            var statuses = await _hotelRepo.GetDistinctStatusesAsync();
            return ResponseFactory.Success(statuses, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<List<string>>();
        }
    }

    public async Task<ApiResponse<HotelRegistrationDetailDTO>> GetByRequestIdAsync(int requestId)
    {
        try
        {
            if (requestId <= 0)
            {
                return ResponseFactory.Failure<HotelRegistrationDetailDTO>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);
            }

            var hotelRequest = await _hotelRepo.GetByIdWithOwnerAsync(requestId);

            if (hotelRequest == null)
                return ResponseFactory.Failure<HotelRegistrationDetailDTO>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);

            var validRequestStatuses = new HashSet<string>
            {
                RequestStatusConst.Pending,
                RequestStatusConst.Approved,
                RequestStatusConst.Rejected,
                RequestStatusConst.Cancelled,
                RequestStatusConst.None
            };

            if (string.IsNullOrEmpty(hotelRequest.Status) || !validRequestStatuses.Contains(hotelRequest.Status))
            {
                return ResponseFactory.Failure<HotelRegistrationDetailDTO>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);
            }

            if (hotelRequest.Owner == null)
                return ResponseFactory.Failure<HotelRegistrationDetailDTO>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_OWNER_NOT_FOUND);

            var additionalInfo = !string.IsNullOrEmpty(hotelRequest.Additional)
                ? JsonSerializer.Deserialize<HotelAdditionalInfo>(hotelRequest.Additional)
                : new HotelAdditionalInfo();

            var dto = new HotelRegistrationDetailDTO
            {
                RequestId    = hotelRequest.Id,
                HotelId      = hotelRequest.Id,
                Name         = hotelRequest.Name,
                OwnerId      = hotelRequest.OwnerId,
                OwnerFullName    = hotelRequest.Owner.FullName ?? string.Empty,
                OwnerEmail       = hotelRequest.Owner.Email,
                OwnerPhoneNumber = hotelRequest.Owner.PhoneNumber,
                OwnerAddress     = hotelRequest.Owner.Address ?? string.Empty,
                Address          = hotelRequest.Address,
                Description      = hotelRequest.Description,
                PropertyTypeId   = hotelRequest.PropertyTypeId,
                PropertyTypeName = hotelRequest.PropertyType?.Name ?? string.Empty,
                CountryId        = hotelRequest.CountryId,
                CountryName      = hotelRequest.Country?.Name ?? string.Empty,
                ProvinceId       = hotelRequest.ProvinceId,
                ProvinceName     = hotelRequest.Province?.Name ?? string.Empty,
                WardId           = hotelRequest.WardId,
                WardName         = hotelRequest.Ward?.Name ?? string.Empty,
                StarRating       = additionalInfo?.StarRating,
                PublicPhone      = additionalInfo?.PublicPhone ?? string.Empty,
                PublicEmail      = additionalInfo?.PublicEmail ?? string.Empty,
                Longitude        = additionalInfo?.Longitude,
                Latitude         = additionalInfo?.Latitude,
                TaxCode          = additionalInfo?.TaxCode ?? string.Empty,
                BusinessLicenseUrl = additionalInfo?.BusinessLicenseUrl ?? string.Empty,
                Status           = hotelRequest.Status ?? RequestStatusConst.None,
                RequestedAt      = hotelRequest.CreatedAt ?? DateTime.Now
            };

            return ResponseFactory.Success(dto, MessageResponse.RequestManagement.HotelApproval.HOTELS_RETRIEVED);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<HotelRegistrationDetailDTO>();
        }
    }

    public async Task<ApiResponse<PagedResult<HotelRegistrationDetailDTO>>> GetPagedRequestsAsync(
        PagingRequest pagingRequest,
        string? status = null)
    {
        try
        {
            var validation = await _pagingValidator.ValidateAsync(pagingRequest, default);
            if (!validation.IsValid)
            {
                return ResponseFactory.Failure<PagedResult<HotelRegistrationDetailDTO>>(
                    StatusCodeResponse.BadRequest,
                    validation.Errors[0].ErrorMessage);
            }

            var validRequestStatuses = new List<string>
            {
                RequestStatusConst.Pending,
                RequestStatusConst.Approved,
                RequestStatusConst.Rejected,
                RequestStatusConst.Cancelled,
                RequestStatusConst.None
            };

            if (!string.IsNullOrEmpty(status))
            {
                if (!validRequestStatuses.Contains(status))
                {
                    return ResponseFactory.Failure<PagedResult<HotelRegistrationDetailDTO>>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);
                }
            }

            // Build filter expression by status
            Expression<Func<Hotel, bool>>? filter;
            if (!string.IsNullOrEmpty(status))
            {
                filter = r => r.Status == status;
            }
            else
            {
                filter = r => r.Status != null && validRequestStatuses.Contains(r.Status);
            }

            var (items, totalCount) = await _hotelRepo.GetPagedWithUserAsync(
                filter,
                pagingRequest.PageIndex ?? 1,
                pagingRequest.PageSize ?? 10);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var dtoItems = new List<HotelRegistrationDetailDTO>();

            foreach (var i in items)
            {
                HotelAdditionalInfo additionalInfo = !string.IsNullOrWhiteSpace(i.Additional)
                    ? JsonSerializer.Deserialize<HotelAdditionalInfo>(i.Additional, options) ?? new HotelAdditionalInfo()
                    : new HotelAdditionalInfo();

                var dto = new HotelRegistrationDetailDTO
                {
                    RequestId        = i.Id,
                    HotelId          = i.Id,
                    OwnerId          = i.OwnerId,
                    Name             = i.Name,
                    Address          = i.Address,
                    Description      = i.Description,
                    PropertyTypeId   = i.PropertyTypeId,
                    PropertyTypeName = i.PropertyType?.Name ?? string.Empty,
                    CountryId        = i.CountryId,
                    ProvinceId       = i.ProvinceId,
                    WardId           = i.WardId,
                    Latitude         = additionalInfo?.Latitude,
                    Longitude        = additionalInfo?.Longitude,
                    StarRating       = additionalInfo?.StarRating,
                    PublicPhone      = additionalInfo?.PublicPhone ?? string.Empty,
                    PublicEmail      = additionalInfo?.PublicEmail ?? string.Empty,
                    TaxCode          = additionalInfo?.TaxCode ?? string.Empty,
                    BusinessLicenseUrl = additionalInfo?.BusinessLicenseUrl ?? string.Empty,
                    Status           = i.Status ?? RequestStatusConst.None,
                    OwnerFullName    = i.Owner?.FullName ?? string.Empty,
                    OwnerEmail       = i.Owner?.Email ?? string.Empty,
                    OwnerPhoneNumber = i.Owner?.PhoneNumber ?? string.Empty
                };
                dtoItems.Add(dto);
            }

            var pagedResult = new PagedResult<HotelRegistrationDetailDTO>(
                dtoItems,
                totalCount,
                pagingRequest.PageIndex,
                pagingRequest.PageSize);

            return ResponseFactory.Success(pagedResult, MessageResponse.RequestManagement.HotelApproval.HOTELS_RETRIEVED);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<PagedResult<HotelRegistrationDetailDTO>>();
        }
    }

    public async Task<ApiResponse<bool>> ApproveRequestAsync(int requestId, int adminId)
    {
        try
        {
            if (requestId <= 0)
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

            // Load the hotel (tracked – not AsNoTracking)
            var hotel = await _hotelRepo.GetByIdAsync(requestId);

            if (hotel == null)
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);

            if (hotel.Status != RequestStatusConst.Pending)
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

            hotel.Status     = RequestStatusConst.Approved;

            await _hotelRepo.UpdateAsync(hotel);
            var saved = await _unitOfWork.SaveChangesAsync() > 0;

            return saved
                ? ResponseFactory.Success(true, MessageResponse.RequestManagement.HotelApproval.APPROVED_SUCCESS)
                : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.HotelApproval.APPROVE_FAILED);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<bool>();
        }
    }

    public async Task<ApiResponse<bool>> RejectRequestAsync(int requestId, int adminId)
    {
        try
        {
            if (requestId <= 0)
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);

            var hotel = await _hotelRepo.GetByIdAsync(requestId);

            if (hotel == null)
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);

            if (hotel.Status != RequestStatusConst.Pending)
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

            hotel.Status     = RequestStatusConst.Rejected;

            await _hotelRepo.UpdateAsync(hotel);
            var saved = await _unitOfWork.SaveChangesAsync() > 0;

            return saved
                ? ResponseFactory.Success(true, MessageResponse.RequestManagement.HotelApproval.REJECTED_SUCCESS)
                : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.HotelApproval.REJECT_FAILED);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<bool>();
        }
    }
}