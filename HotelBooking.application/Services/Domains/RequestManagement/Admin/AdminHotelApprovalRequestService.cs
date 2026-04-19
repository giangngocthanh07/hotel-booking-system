
using System.Linq.Expressions;
using System.Text.Json;
using CloudinaryDotNet;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement.Base;
using HotelBooking.infrastructure.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.application.Services.Domains.RequestManagement.Admin;

public interface IAdminHotelApprovalRequestService : IBaseAdminRequestService<HotelRegistrationDetailDTO>
{
}

public class AdminHotelApprovalRequestService : IAdminHotelApprovalRequestService
{
    private readonly IHotelRepository _hotelRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<PagingRequest> _pagingValidator;

    public AdminHotelApprovalRequestService(IHotelRepository hotelRepo, IUnitOfWork unitOfWork, IValidator<PagingRequest> pagingValidator)
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
                return ResponseFactory.Failure<HotelRegistrationDetailDTO>(StatusCodeResponse.BadRequest, MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);
            }

            var hotelRequest = await _hotelRepo.GetByIdWithOwnerAsync(requestId);

            if (hotelRequest == null)
                return ResponseFactory.Failure<HotelRegistrationDetailDTO>(StatusCodeResponse.NotFound, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);

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
                    StatusCodeResponse.BadRequest, MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);
            }

            if (hotelRequest.Owner == null)
                return ResponseFactory.Failure<HotelRegistrationDetailDTO>(StatusCodeResponse.NotFound, MessageResponse.RequestManagement.HotelApproval.HOTEL_OWNER_NOT_FOUND);

            var additionalInfo = !string.IsNullOrEmpty(hotelRequest.Additional)
                ? JsonSerializer.Deserialize<HotelAdditionalInfo>(hotelRequest.Additional)
                : new HotelAdditionalInfo();

            var dto = new HotelRegistrationDetailDTO();

            dto.RequestId = hotelRequest.Id;
            dto.HotelId = hotelRequest.Id;
            dto.Name = hotelRequest.Name;
            dto.OwnerId = hotelRequest.OwnerId;
            dto.OwnerFullName = hotelRequest.Owner.FullName ?? string.Empty;
            dto.OwnerEmail = hotelRequest.Owner.Email;
            dto.OwnerPhoneNumber = hotelRequest.Owner.PhoneNumber;
            dto.OwnerAddress = hotelRequest.Owner.Address ?? string.Empty;
            dto.Address = hotelRequest.Address;
            dto.Description = hotelRequest.Description;
            dto.PropertyTypeId = hotelRequest.PropertyTypeId;
            dto.CountryId = hotelRequest.CountryId;
            dto.ProvinceId = hotelRequest.ProvinceId;
            dto.WardId = hotelRequest.WardId;
            dto.StarRating = additionalInfo?.StarRating;
            dto.PublicPhone = additionalInfo?.PublicPhone ?? string.Empty;
            dto.PublicEmail = additionalInfo?.PublicEmail ?? string.Empty;
            dto.Longitude = additionalInfo?.Longitude;
            dto.Latitude = additionalInfo?.Latitude;
            dto.TaxCode = additionalInfo?.TaxCode ?? string.Empty;
            dto.BusinessLicenseUrl = additionalInfo?.BusinessLicenseUrl ?? string.Empty;
            dto.Status = hotelRequest.Status ?? RequestStatusConst.None;
            dto.RequestedAt = hotelRequest.CreatedAt ?? DateTime.Now;

            return ResponseFactory.Success(dto, MessageResponse.RequestManagement.HotelApproval.HOTELS_RETRIEVED);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<HotelRegistrationDetailDTO>();
        }
    }

    public async Task<ApiResponse<PagedResult<HotelRegistrationDetailDTO>>> GetPagedRequestsAsync(PagingRequest pagingRequest, string? status = null)
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

            // 1. Validate pagination params
            if (!string.IsNullOrEmpty(status))
            {
                if (!validRequestStatuses.Contains(status))
                {
                    return ResponseFactory.Failure<PagedResult<HotelRegistrationDetailDTO>>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);
                }
            }

            // 2. Build filter expression by status
            Expression<Func<Hotel, bool>>? filter;
            if (!string.IsNullOrEmpty(status))
            {
                filter = r => r.Status == status;
            }
            else
            {
                filter = r => r.Status != null && validRequestStatuses.Contains(r.Status);
            }

            // 3. Call Repository with pagination
            var (items, totalCount) = await _hotelRepo.GetPagedWithUserAsync(filter, pagingRequest.PageIndex ?? 1, pagingRequest.PageSize ?? 10);

            // 4. Convert: Map to DTO
            var rawHotels = items.ToList();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

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
                dto.Latitude = additionalInfo?.Latitude;
                dto.Longitude = additionalInfo?.Longitude;
                dto.StarRating = additionalInfo?.StarRating;
                dto.PublicPhone = additionalInfo?.PublicPhone ?? string.Empty;
                dto.PublicEmail = additionalInfo?.PublicEmail ?? string.Empty;
                dto.TaxCode = additionalInfo?.TaxCode ?? string.Empty;
                dto.BusinessLicenseUrl = additionalInfo?.BusinessLicenseUrl ?? string.Empty;
                dto.Status = i.Status ?? RequestStatusConst.None;

                dto.OwnerId = i.OwnerId;
                dto.OwnerFullName = i.Owner.FullName ?? string.Empty;
                dto.OwnerEmail = i.Owner.Email;
                dto.OwnerPhoneNumber = i.Owner.PhoneNumber;

                dtoItems.Add(dto);
            }

            PagedResult<HotelRegistrationDetailDTO> pagedResult = new PagedResult<HotelRegistrationDetailDTO>(
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
    { return ResponseFactory.ServerError<bool>(); }

    public async Task<ApiResponse<bool>> RejectRequestAsync(int requestId, int adminId) { return ResponseFactory.ServerError<bool>(); }
}