
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
    private readonly IHotelApprovalRequestRepository _approvalRepo;
    private readonly IHotelRepository _hotelRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<PagingRequest> _pagingValidator;
    private readonly ILogger<AdminHotelApprovalRequestService> _logger;

    public AdminHotelApprovalRequestService(
        IHotelApprovalRequestRepository approvalRepo,
        IHotelRepository hotelRepo,
        IUnitOfWork unitOfWork,
        IValidator<PagingRequest> pagingValidator,
        ILogger<AdminHotelApprovalRequestService> logger)
    {
        _approvalRepo = approvalRepo;
        _hotelRepo = hotelRepo;
        _unitOfWork = unitOfWork;
        _pagingValidator = pagingValidator;
        _logger = logger;
    }

    public async Task<ApiResponse<List<string>>> GetAllStatusesAsync()
    {
        try
        {
            var statuses = await _approvalRepo.GetDistinctStatusesAsync();
            return ResponseFactory.Success(statuses, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AdminHotelApprovalRequestService - GetAllStatusesAsync] Error getting all statuses: {ErrorMessage}", ex.Message);
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

            var approvalRequest = await _approvalRepo.GetByIdWithOwnerAsync(requestId);

            if (approvalRequest == null)
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

            if (string.IsNullOrEmpty(approvalRequest.Status) || !validRequestStatuses.Contains(approvalRequest.Status))
            {
                return ResponseFactory.Failure<HotelRegistrationDetailDTO>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.AdminHotelApprovalRequestService.INVALID_STATUS);
            }

            if (approvalRequest.Owner == null)
                return ResponseFactory.Failure<HotelRegistrationDetailDTO>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_OWNER_NOT_FOUND);

            var additionalInfo = !string.IsNullOrEmpty(approvalRequest.Additional)
                ? JsonSerializer.Deserialize<HotelAdditionalInfo>(approvalRequest.Additional)
                : new HotelAdditionalInfo();

            var dto = new HotelRegistrationDetailDTO
            {
                RequestId = approvalRequest.Id,
                Name = approvalRequest.Name,
                OwnerId = approvalRequest.OwnerId,
                OwnerFullName = approvalRequest.Owner?.FullName ?? string.Empty,
                OwnerEmail = approvalRequest.Owner?.Email ?? string.Empty,
                OwnerPhoneNumber = approvalRequest.Owner?.PhoneNumber ?? string.Empty,
                OwnerAddress = approvalRequest.Owner?.Address ?? string.Empty,
                Address = approvalRequest.Address,
                TaxCode = approvalRequest.TaxCode,
                BusinessLicenseUrl = approvalRequest.BusinessLicenseUrl,
                Status = approvalRequest.Status,
                RequestedAt = approvalRequest.CreatedAt,

                Description = additionalInfo?.Description,

                PropertyTypeId = additionalInfo!.PropType.Id,
                PropertyTypeName = additionalInfo!.PropType.Name,
                CountryId = additionalInfo.Country.Id,
                CountryName = additionalInfo.Country.Name,
                ProvinceId = additionalInfo.Province.Id,
                ProvinceName = additionalInfo.Province.Name,
                WardId = additionalInfo.Ward.Id,
                WardName = additionalInfo.Ward.Name,

                StarRating = additionalInfo?.StarRating,
                PublicPhone = additionalInfo!.PublicPhone,
                PublicEmail = additionalInfo.PublicEmail,

                Longitude = additionalInfo?.Longitude,
                Latitude = additionalInfo?.Latitude,
            };

            return ResponseFactory.Success(dto, MessageResponse.RequestManagement.HotelApproval.HOTELS_RETRIEVED);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AdminHotelApprovalRequestService - GetByRequestIdAsync] Error getting hotel registration request with ID {RequestId}: {ErrorMessage}", requestId, ex.Message);
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
            Expression<Func<HotelApprovalRequest, bool>>? filter;
            if (!string.IsNullOrEmpty(status))
            {
                filter = r => r.Status == status;
            }
            else
            {
                filter = r => r.Status != null && validRequestStatuses.Contains(r.Status);
            }

            var (items, totalCount) = await _approvalRepo.GetPagedWithUserAsync(
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
                    RequestId = i.Id,
                    OwnerId = i.OwnerId,
                    Name = i.Name,
                    Address = i.Address,
                    TaxCode = i.TaxCode,
                    BusinessLicenseUrl = i.BusinessLicenseUrl,
                    Status = i.Status ?? RequestStatusConst.None,
                    RequestedAt = i.CreatedAt,
                    OwnerFullName = i.Owner?.FullName ?? string.Empty,
                    OwnerEmail = i.Owner?.Email ?? string.Empty,
                    OwnerPhoneNumber = i.Owner?.PhoneNumber ?? string.Empty,

                    Description = additionalInfo.Description,

                    PropertyTypeId = additionalInfo.PropType.Id,
                    PropertyTypeName = additionalInfo.PropType.Name,
                    CountryId = additionalInfo.Country.Id,
                    CountryName = additionalInfo.Country.Name,
                    ProvinceId = additionalInfo.Province.Id,
                    ProvinceName = additionalInfo.Province.Name,
                    WardId = additionalInfo.Ward.Id,
                    WardName = additionalInfo.Ward.Name,

                    Latitude = additionalInfo.Latitude,
                    Longitude = additionalInfo.Longitude,

                    StarRating = additionalInfo.StarRating,
                    PublicPhone = additionalInfo.PublicPhone,
                    PublicEmail = additionalInfo.PublicEmail
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AdminHotelApprovalRequestService - GetPagedRequestsAsync] Error getting hotel registration requests: {ErrorMessage}", ex.Message);
            return ResponseFactory.ServerError<PagedResult<HotelRegistrationDetailDTO>>();
        }
    }

    public async Task<ApiResponse<bool>> ApproveRequestAsync(int requestId, int adminId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (requestId <= 0)
            {
                await _unitOfWork.RollBackTransactionAsync();
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_INVALID_REQUEST_ID);
            }

            // Load the hotel (tracked – not AsNoTracking)
            var request = await _approvalRepo.GetByIdAsync(requestId);

            if (request == null)
            {
                await _unitOfWork.RollBackTransactionAsync();
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);
            }

            if (request.Status != RequestStatusConst.Pending)
            {
                await _unitOfWork.RollBackTransactionAsync();
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);
            }

            request.Status = RequestStatusConst.Approved;
            request.AdminId = adminId;
            request.UpdatedAt = DateTime.Now;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var additionalInfo = !string.IsNullOrWhiteSpace(request.Additional)
                ? JsonSerializer.Deserialize<HotelAdditionalInfo>(request.Additional, options) ?? new HotelAdditionalInfo()
                : new HotelAdditionalInfo();

            var hotelAdditionalData = new HotelEntityAdditionalData
            {
                StarRating = additionalInfo.StarRating,
                PublicPhone = additionalInfo.PublicPhone,
                PublicEmail = additionalInfo.PublicEmail,
                Latitude = additionalInfo.Latitude,
                Longitude = additionalInfo.Longitude,

                TaxCode = request.TaxCode,
                BusinessLicenseUrl = request.BusinessLicenseUrl
            };

            string hotelAdditionalJson = JsonSerializer.Serialize(hotelAdditionalData);

            var newHotel = new Hotel
            {
                Name = request.Name,
                OwnerId = request.OwnerId,
                Address = request.Address,
                Description = additionalInfo.Description,
                PropertyTypeId = additionalInfo.PropType.Id,
                CountryId = additionalInfo.Country.Id,
                ProvinceId = additionalInfo.Province.Id,
                WardId = additionalInfo.Ward.Id,

                Additional = hotelAdditionalJson,

                CreatedAt = DateTime.Now,
                IsVerified = true,
                IsDeleted = false,
                Status = "Active",
                CoverImageUrl = string.Empty

            };

            await _hotelRepo.AddAsync(newHotel);
            await _approvalRepo.UpdateAsync(request);

            var saved = await _unitOfWork.SaveChangesAsync() > 0;

            if (saved)
            {
                await _unitOfWork.CommitTransactionAsync();
                return ResponseFactory.Success(true, MessageResponse.RequestManagement.HotelApproval.APPROVED_SUCCESS);
            }
            else
            {
                await _unitOfWork.RollBackTransactionAsync();
                return ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.HotelApproval.APPROVE_FAILED);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollBackTransactionAsync();
            _logger.LogError(ex, "[AdminHotelApprovalRequestService - ApproveRequestAsync] Error approving hotel registration request with ID {RequestId} by Admin with ID: {AdminId}: {ErrorMessage}", requestId, adminId, ex.Message);
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

            var request = await _approvalRepo.GetByIdAsync(requestId);

            if (request == null)
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_NOT_FOUND);

            if (request.Status != RequestStatusConst.Pending)
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.HotelApproval.STATUS_INVALID);

            request.Status = RequestStatusConst.Rejected;
            request.AdminId = adminId;
            request.UpdatedAt = DateTime.Now;

            await _approvalRepo.UpdateAsync(request);
            var saved = await _unitOfWork.SaveChangesAsync() > 0;

            return saved
                ? ResponseFactory.Success(true, MessageResponse.RequestManagement.HotelApproval.REJECTED_SUCCESS)
                : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.HotelApproval.REJECT_FAILED);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AdminHotelApprovalRequestService - RejectRequestAsync] Error rejecting hotel registration request with ID {RequestId} by Admin with ID: {AdminId}: {ErrorMessage}", requestId, adminId, ex.Message);
            return ResponseFactory.ServerError<bool>();
        }
    }
}