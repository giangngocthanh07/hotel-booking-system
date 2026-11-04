using HotelBooking.infrastructure.Models;
using FluentValidation;
using System.Linq.Expressions;
using HotelBooking.application.DTOs.Role;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.UpgradeRequest;
using HotelBooking.application.Services.Domains.RequestManagement.Base;

namespace HotelBooking.application.Services.Domains.RequestManagement.Admin
{
    public interface IAdminUpgradeRequestService : IBaseAdminRequestService<AdminUpgradeRequestDTO>
    {
        Task<ApiResponse<bool>> RejectRequestAsync(int requestId, int adminId, RejectUpgradeRequestDTO input, CancellationToken cancellationToken = default);
    }

    public class AdminUpgradeRequestService : IAdminUpgradeRequestService
    {
        private readonly IUpgradeRequestRepository _upgradeRequestRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<PagingRequest> _pagingValidator;
        private readonly ILogger<AdminUpgradeRequestService> _logger;
        private readonly IValidator<RejectUpgradeRequestDTO> _rejectionValidator;


        public AdminUpgradeRequestService(
            IUpgradeRequestRepository upgradeRequestRepo,
            IUserRoleRepository userRoleRepo,
            IUnitOfWork unitOfWork,
            IValidator<PagingRequest> pagingValidator,
            ILogger<AdminUpgradeRequestService> logger,
            IValidator<RejectUpgradeRequestDTO> rejectionValidator)
        {
            _upgradeRequestRepo = upgradeRequestRepo;
            _userRoleRepo = userRoleRepo;
            _unitOfWork = unitOfWork;
            _pagingValidator = pagingValidator;
            _logger = logger;
            _rejectionValidator = rejectionValidator;
        }

        public async Task<ApiResponse<PagedResult<AdminUpgradeRequestDTO>>> GetPagedRequestsAsync(
                PagingRequest pagingRequest,
                string? status = null)
        {
            try
            {
                // 1. Validate pagination params
                var validation = await _pagingValidator.ValidateAsync(pagingRequest, default);

                if (!validation.IsValid)
                {
                    return ResponseFactory.Failure<PagedResult<AdminUpgradeRequestDTO>>(
                        StatusCodeResponse.BadRequest,
                        validation.Errors[0].ErrorMessage);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    var validStatuses = new HashSet<string>
                    {
                        RequestStatusConst.Pending,
                        RequestStatusConst.Approved,
                        RequestStatusConst.Rejected,
                        RequestStatusConst.Cancelled,
                        RequestStatusConst.None
                    };

                    if (!validStatuses.Contains(status))
                    {
                        return ResponseFactory.Failure<PagedResult<AdminUpgradeRequestDTO>>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.AdminUpgradeRequestService.INVALID_STATUS);
                    }
                }

                // 2. Build filter expression by status
                Expression<Func<UpgradeRequest, bool>>? filter = null;
                if (!string.IsNullOrEmpty(status))
                {
                    filter = r => r.Status == status;
                }

                // 3. Call Repository with pagination
                var (items, totalCount) = await _upgradeRequestRepo.GetPagedWithUserAsync(
                    filter,
                    pagingRequest.PageIndex ?? 1,
                    pagingRequest.PageSize ?? 10);

                // 4. Map to DTO
                var dtoItems = items.Select(request => new AdminUpgradeRequestDTO
                {
                    RequestId = request.Id,
                    UserId = request.User?.Id ?? 0,
                    UserName = request.User?.UserName ?? "",
                    FullName = request.User?.FullName ?? "",
                    Email = request.User?.Email ?? "",
                    PhoneNumber = request.User?.PhoneNumber ?? "",
                    Address = request.Address ?? "",
                    TaxCode = request.TaxCode ?? "",
                    Status = request.Status ?? RequestStatusConst.Pending,
                    RequestedAt = request.RequestedAt,
                    RejectionReason = request.RejectionReason
                }).ToList();

                // 5. Return PagedResult
                var pagedResult = new PagedResult<AdminUpgradeRequestDTO>(
                    dtoItems,
                    totalCount,
                    pagingRequest.PageIndex,
                    pagingRequest.PageSize);

                return ResponseFactory.Success(pagedResult, MessageResponse.RequestManagement.UpgradeRequest.REQUESTS_RETRIEVED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdminUpgradeRequestService - GetPagedRequestsAsync] Error getting upgrade requests: {ErrorMessage}", ex.Message);
                return ResponseFactory.ServerError<PagedResult<AdminUpgradeRequestDTO>>();
            }
        }

        public async Task<ApiResponse<AdminUpgradeRequestDTO>> GetByRequestIdAsync(int requestId)
        {
            try
            {
                if (requestId <= 0)
                {
                    return ResponseFactory.Failure<AdminUpgradeRequestDTO>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.UpgradeRequest.REQUEST_ID_INVALID);
                }

                var request = await _upgradeRequestRepo.GetByIdWithUserAsync(requestId);

                if (request == null)
                    return ResponseFactory.Failure<AdminUpgradeRequestDTO>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.UpgradeRequest.REQUEST_NOT_FOUND);


                if (request.User == null)
                    return ResponseFactory.Failure<AdminUpgradeRequestDTO>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

                var requestDTO = new AdminUpgradeRequestDTO
                {
                    RequestId = request.Id,
                    UserId = request.UserId,
                    UserName = request.User.UserName,
                    FullName = request.User.FullName ?? "",
                    Email = request.User.Email ?? "",
                    PhoneNumber = request.User.PhoneNumber ?? "",
                    Address = request.Address ?? "",
                    TaxCode = request.TaxCode ?? "",
                    Status = request.Status ?? RequestStatusConst.Pending,
                    RequestedAt = request.RequestedAt,
                    RejectionReason = request.RejectionReason
                };

                return ResponseFactory.Success(requestDTO, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_RETRIEVED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdminUpgradeRequestService - GetByRequestIdAsync] Error getting upgrade request with ID {RequestId}: {ErrorMessage}", requestId, ex.Message);
                return ResponseFactory.ServerError<AdminUpgradeRequestDTO>();
            }
        }

        public async Task<ApiResponse<bool>> ApproveRequestAsync(int requestId, int adminId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (requestId <= 0)
                {
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_ID_INVALID);
                }

                var request = await _upgradeRequestRepo.GetByIdWithUserAsync(requestId);

                if (request == null || request.Status != RequestStatusConst.Pending)
                {
                    await _unitOfWork.RollBackTransactionAsync();
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);
                }

                if (request.User == null)
                {
                    await _unitOfWork.RollBackTransactionAsync();
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);
                }

                var hasCustomerRole = await _userRoleRepo.AnyAsync(
                    ur => ur.UserId == request.UserId && ur.RoleId == RoleTypeConstDTO.Customer);

                if (!hasCustomerRole)
                {
                    await _unitOfWork.RollBackTransactionAsync();
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.Forbidden,
                        MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);
                }

                // User already Owner --> Fail
                var hasOwnerRole = await _userRoleRepo
                .AnyAsync(ur => ur.UserId == request.UserId && ur.RoleId == RoleTypeConstDTO.Owner);

                if (hasOwnerRole)
                {
                    await _unitOfWork.RollBackTransactionAsync();
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.Forbidden,
                        MessageResponse.RequestManagement.UpgradeRequest.USER_ALREADY_OWNER);
                }

                // Add Owner role to user
                var ownerRole = new UserRole
                {
                    UserId = request.UserId,
                    RoleId = RoleTypeConstDTO.Owner
                };
                await _userRoleRepo.AddAsync(ownerRole);

                // Update request status and User data
                request.Status = RequestStatusConst.Approved;
                request.ApprovedAt = DateTime.Now;
                request.ApprovedBy = adminId;
                request.User.Address = request.Address;
                request.User.TaxCode = request.TaxCode;
                await _upgradeRequestRepo.UpdateAsync(request);

                var saved = await _unitOfWork.SaveChangesAsync() > 0;
                
                if (saved)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return ResponseFactory.Success(true, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_APPROVED_SUCCESS);
                }
                else
                {
                    await _unitOfWork.RollBackTransactionAsync();
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_APPROVE_FAILED);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackTransactionAsync();
                _logger.LogError(ex, "[AdminUpgradeRequestService - ApproveRequestAsync] Error approving upgrade request with ID {RequestId} by Admin with ID: {AdminId}: {ErrorMessage}", requestId, adminId, ex.Message);
                return ResponseFactory.ServerError<bool>();
            }
        }

        public async Task<ApiResponse<bool>> RejectRequestAsync(int requestId, int adminId,
            RejectUpgradeRequestDTO input, CancellationToken cancellationToken = default)
        {
            // FR-ONB-008: validate before reading or changing the request, including direct service calls.
            cancellationToken.ThrowIfCancellationRequested();
            if (input == null) return InvalidReason();
            var validation = await _rejectionValidator.ValidateAsync(input, cancellationToken);
            if (!validation.IsValid) return InvalidReason();
            try
            {
                if (requestId <= 0)
                {
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_ID_INVALID);
                }
                var request = await _upgradeRequestRepo.GetByIdAsync(requestId);
                if (request == null)
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.UpgradeRequest.REQUEST_NOT_FOUND);

                if (request.Status != RequestStatusConst.Pending)
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);

                return await SaveRejectionAsync(request, adminId, input.Reason.Trim(), cancellationToken);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdminUpgradeRequestService - RejectRequestAsync] Error rejecting upgrade request: {ErrorMessage}", ex.Message);
                return ResponseFactory.ServerError<bool>();
            }
        }

        private static ApiResponse<bool> InvalidReason() => ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest,
            MessageResponse.RequestManagement.UpgradeRequest.REJECTION_REASON_INVALID);

        private async Task<ApiResponse<bool>> SaveRejectionAsync(UpgradeRequest request, int adminId,
            string reason, CancellationToken cancellationToken)
        {
            // Store the explanation and decision together; Customer DTOs deliberately do not expose it.
            cancellationToken.ThrowIfCancellationRequested();
            request.Status = RequestStatusConst.Rejected;
            request.ApprovedAt = DateTime.UtcNow;
            request.ApprovedBy = adminId;
            request.RejectionReason = reason;
            await _upgradeRequestRepo.UpdateAsync(request);
            cancellationToken.ThrowIfCancellationRequested();
            var saved = await _unitOfWork.SaveChangesAsync() > 0;
            return saved
                ? ResponseFactory.Success(true, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_REJECTED_SUCCESS)
                : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_REJECT_FAILED);
        }

        public async Task<ApiResponse<List<string>>> GetAllStatusesAsync()
        {
            try
            {
                var statuses = await _upgradeRequestRepo.GetDistinctStatusesAsync();
                return ResponseFactory.Success(statuses, MessageResponse.Common.GET_SUCCESSFULLY);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdminUpgradeRequestService - GetAllStatusesAsync] Error getting all statuses: {ErrorMessage}", ex.Message);
                return ResponseFactory.ServerError<List<string>>();
            }
        }
    }
}
