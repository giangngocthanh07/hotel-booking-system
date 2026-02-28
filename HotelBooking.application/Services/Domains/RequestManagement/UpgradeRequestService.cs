using HotelBooking.infrastructure.Models;
using HotelBooking.application.Helpers;
using FluentValidation;
using System.Linq.Expressions;
using HotelBooking.application.DTOs.Request;

namespace HotelBooking.application.Services.Domains.RequestManagement
{
    public interface IUpgradeRequestService
    {
        // === Customer ===
        Task<ApiResponse<UserForUpgradeDTO?>> GetUserForUpgradeAsync(int userId);
        Task<ApiResponse<bool>> CreateRequestAsync(int userId, string address, string taxCode);
        Task<ApiResponse<bool>> CancelRequestAsync(int userId);
        Task<ApiResponse<List<UpgradeRequestDTO>>> GetMyRequestsAsync(int userId);

        // === Admin ===
        Task<ApiResponse<PagedResult<UpgradeRequestDTO>>> GetPagedRequestsAsync(PagingRequest pagingRequest, string? status = null);
        Task<ApiResponse<UpgradeRequestDTO>> GetByRequestIdAsync(int requestId);
        Task<ApiResponse<bool>> ApproveRequestAsync(int requestId, int adminId);
        Task<ApiResponse<bool>> RejectRequestAsync(int requestId, int adminId);
        Task<ApiResponse<List<string>>> GetAllStatusesAsync();

        // === Obsolete ===
        [Obsolete("Use GetPagedRequestsAsync instead")]
        Task<ApiResponse<IEnumerable<UpgradeRequestDTO>>> GetAllRequestAsync(string? status = null);
    }
    
    public class UpgradeRequestService : IUpgradeRequestService
    {
        private readonly IUpgradeRequestRepository _upgradeRequestRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<PagingRequest> _pagingValidator;
        private readonly IValidator<CreateUpgradeRequestDTO> _createRequestValidator;

        public UpgradeRequestService(
            IUpgradeRequestRepository upgradeRequestRepo,
            IUserRepository userRepo,
            IUserRoleRepository userRoleRepo,
            IUnitOfWork unitOfWork,
            IValidator<PagingRequest> pagingValidator,
            IValidator<CreateUpgradeRequestDTO> createRequestValidator)
        {
            _upgradeRequestRepo = upgradeRequestRepo;
            _userRepo = userRepo;
            _userRoleRepo = userRoleRepo;
            _unitOfWork = unitOfWork;
            _pagingValidator = pagingValidator;
            _createRequestValidator = createRequestValidator;
        }

        #region Customer Methods

        public async Task<ApiResponse<UserForUpgradeDTO?>> GetUserForUpgradeAsync(int userId)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                    return ResponseFactory.Failure<UserForUpgradeDTO?>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

                // Check for existing requests
                var existingRequests = await _upgradeRequestRepo.GetAllAsync();
                var userRequest = existingRequests?
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.RequestedAt)
                    .FirstOrDefault();

                var requestStatus = userRequest?.Status ?? UpgradeRequestStatusConst.None;

                var userForUpgradeDTO = new UserForUpgradeDTO
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName ?? "",
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RequestStatus = requestStatus
                };

                return ResponseFactory.Success<UserForUpgradeDTO?>(
                    userForUpgradeDTO,
                    MessageResponse.RequestManagement.UpgradeRequest.USER_INFO_RETRIEVED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<UserForUpgradeDTO?>();
            }
        }

        public async Task<ApiResponse<bool>> CreateRequestAsync(int userId, string address, string taxCode)
        {
            try
            {
                // Validate input
                var requestDto = new CreateUpgradeRequestDTO { Address = address, TaxCode = taxCode };
                var validation = await _createRequestValidator.ValidateAsync(requestDto);
                if (!validation.IsValid)
                {
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.BadRequest,
                        validation.Errors.First().ErrorMessage);
                }

                // Check if user exists
                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

                // Check role qua UserRoles
                var hasCustomerRole = await _userRoleRepo
                    .AnyAsync(ur => ur.UserId == userId && ur.RoleId == RoleTypeConstDTO.Customer);

                if (!hasCustomerRole)
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.Forbidden,
                        MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);

                // Check if there's already a pending request for this user
                var existingRequests = await _upgradeRequestRepo.GetPendingByIdAsync(userId);
                if (existingRequests.Any())
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.Conflict,
                        MessageResponse.RequestManagement.UpgradeRequest.PENDING_REQUEST_EXISTS);

                // Create new upgrade request
                var request = new UpgradeRequest
                {
                    UserId = userId,
                    Address = address,
                    TaxCode = taxCode,
                    Status = UpgradeRequestStatusConst.Pending,
                    RequestedAt = DateTime.Now
                };

                await _upgradeRequestRepo.AddAsync(request);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;

                return saved
                    ? ResponseFactory.Success(true, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CREATED_SUCCESS)
                    : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CREATE_FAILED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<bool>();
            }
        }

        public async Task<ApiResponse<bool>> CancelRequestAsync(int userId)
        {
            try
            {
                // Tìm request pending của user
                var pendingRequests = await _upgradeRequestRepo.GetPendingByIdAsync(userId);
                var request = pendingRequests.FirstOrDefault();

                if (request == null)
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.UpgradeRequest.REQUEST_NOT_FOUND);

                // Cập nhật status thành Cancelled
                request.Status = UpgradeRequestStatusConst.Cancelled;
                await _upgradeRequestRepo.UpdateAsync(request);

                var saved = await _unitOfWork.SaveChangesAsync() > 0;
                return saved
                    ? ResponseFactory.Success(true, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CANCELLED_SUCCESS)
                    : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CANCEL_FAILED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<bool>();
            }
        }

        public async Task<ApiResponse<List<UpgradeRequestDTO>>> GetMyRequestsAsync(int userId)
        {
            try
            {
                var requests = await _upgradeRequestRepo.GetByUserIdAsync(userId);

                var dtos = requests.Select(r => new UpgradeRequestDTO
                {
                    RequestId = r.Id,
                    UserId = r.UserId,
                    UserName = r.User?.UserName ?? "",
                    FullName = r.User?.FullName ?? "",
                    Email = r.User?.Email ?? "",
                    PhoneNumber = r.User?.PhoneNumber ?? "",
                    Address = r.Address ?? "",
                    TaxCode = r.TaxCode ?? "",
                    Status = r.Status ?? UpgradeRequestStatusConst.Pending,
                    RequestedAt = r.RequestedAt
                }).OrderByDescending(r => r.RequestedAt).ToList();

                return ResponseFactory.Success(dtos, MessageResponse.RequestManagement.UpgradeRequest.REQUESTS_RETRIEVED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<List<UpgradeRequestDTO>>();
            }
        }

        #endregion

        #region Admin Methods

        public async Task<ApiResponse<PagedResult<UpgradeRequestDTO>>> GetPagedRequestsAsync(
            PagingRequest pagingRequest,
            string? status = null)
        {
            try
            {
                // 1. Validate pagination params
                var validation = _pagingValidator.Validate(pagingRequest);
                if (!validation.IsValid)
                {
                    return ResponseFactory.Failure<PagedResult<UpgradeRequestDTO>>(
                        StatusCodeResponse.BadRequest,
                        validation.Errors.First().ErrorMessage);
                }

                // 2. Build filter expression theo status
                Expression<Func<HotelBooking.infrastructure.Models.UpgradeRequest, bool>>? filter = null;
                if (!string.IsNullOrEmpty(status))
                {
                    filter = r => r.Status == status;
                }

                // 3. Gọi Repository với pagination
                var (items, totalCount) = await _upgradeRequestRepo.GetPagedWithUserAsync(
                    filter,
                    pagingRequest.PageIndex ?? 1,
                    pagingRequest.PageSize ?? 10);

                // 4. Map sang DTO
                var dtoItems = items.Select(request => new UpgradeRequestDTO
                {
                    RequestId = request.Id,
                    UserId = request.User?.Id ?? 0,
                    UserName = request.User?.UserName ?? "",
                    FullName = request.User?.FullName ?? "",
                    Email = request.User?.Email ?? "",
                    PhoneNumber = request.User?.PhoneNumber ?? "",
                    Address = request.Address ?? "",
                    TaxCode = request.TaxCode ?? "",
                    Status = request.Status ?? UpgradeRequestStatusConst.Pending,
                    RequestedAt = request.RequestedAt
                }).ToList();

                // 5. Trả về PagedResult
                var pagedResult = new PagedResult<UpgradeRequestDTO>(
                    dtoItems,
                    totalCount,
                    pagingRequest.PageIndex,
                    pagingRequest.PageSize);

                return ResponseFactory.Success(pagedResult, MessageResponse.RequestManagement.UpgradeRequest.REQUESTS_RETRIEVED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<PagedResult<UpgradeRequestDTO>>();
            }
        }

        public async Task<ApiResponse<UpgradeRequestDTO>> GetByRequestIdAsync(int requestId)
        {
            try
            {
                var request = await _upgradeRequestRepo.GetByIdAsync(requestId);
                if (request == null)
                    return ResponseFactory.Failure<UpgradeRequestDTO>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.UpgradeRequest.REQUEST_NOT_FOUND);

                var user = await _userRepo.GetByIdAsync(request.UserId);
                if (user == null)
                    return ResponseFactory.Failure<UpgradeRequestDTO>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

                var requestDTO = new UpgradeRequestDTO
                {
                    RequestId = request.Id,
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName ?? "",
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Address = request.Address ?? "",
                    TaxCode = request.TaxCode ?? "",
                    Status = request.Status ?? UpgradeRequestStatusConst.Pending,
                    RequestedAt = request.RequestedAt
                };

                return ResponseFactory.Success(requestDTO, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_RETRIEVED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<UpgradeRequestDTO>();
            }
        }

        public async Task<ApiResponse<bool>> ApproveRequestAsync(int requestId, int adminId)
        {
            try
            {
                var request = await _upgradeRequestRepo.GetByIdAsync(requestId);
                if (request == null || request.Status != UpgradeRequestStatusConst.Pending)
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);

                var user = await _userRepo.GetByIdAsync(request.UserId);
                if (user == null)
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

                var hasCustomerRole = await _userRoleRepo.AnyAsync(
                    ur => ur.UserId == user.Id && ur.RoleId == RoleTypeConstDTO.Customer);
                if (!hasCustomerRole)
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);

                // Add Owner role to user
                var ownerRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = RoleTypeConstDTO.Owner
                };
                await _userRoleRepo.AddAsync(ownerRole);

                // Update request status
                request.Status = UpgradeRequestStatusConst.Approved;
                request.ApprovedAt = DateTime.Now;
                request.ApprovedBy = adminId;
                await _upgradeRequestRepo.UpdateAsync(request);

                // Update User data
                user.Address = request.Address;
                user.TaxCode = request.TaxCode;
                await _userRepo.UpdateAsync(user);

                var saved = await _unitOfWork.SaveChangesAsync() > 0;
                return saved
                    ? ResponseFactory.Success(true, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_APPROVED_SUCCESS)
                    : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_APPROVE_FAILED);
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
                var request = await _upgradeRequestRepo.GetByIdAsync(requestId);
                if (request == null || request.Status != UpgradeRequestStatusConst.Pending)
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);

                // Update request status to Rejected
                request.Status = UpgradeRequestStatusConst.Rejected;
                request.ApprovedAt = DateTime.Now;
                request.ApprovedBy = adminId;
                await _upgradeRequestRepo.UpdateAsync(request);

                var saved = await _unitOfWork.SaveChangesAsync() > 0;
                return saved
                    ? ResponseFactory.Success(true, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_REJECTED_SUCCESS)
                    : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_REJECT_FAILED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<bool>();
            }
        }

        public async Task<ApiResponse<List<string>>> GetAllStatusesAsync()
        {
            try
            {
                var statuses = await _upgradeRequestRepo.GetDistinctStatusesAsync();
                return ResponseFactory.Success(statuses, MessageResponse.Common.GET_SUCCESSFULLY);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<List<string>>();
            }
        }

        #endregion

        #region Obsolete

        [Obsolete("Use GetPagedRequestsAsync instead for better performance")]
        public async Task<ApiResponse<IEnumerable<UpgradeRequestDTO>>> GetAllRequestAsync(string? status = null)
        {
            try
            {
                var requests = await _upgradeRequestRepo.GetAllAsync();
                var filteredRequests = string.IsNullOrEmpty(status)
                    ? requests
                    : requests.Where(r => r.Status == status);

                var results = new List<UpgradeRequestDTO>();

                foreach (var request in filteredRequests)
                {
                    var user = await _userRepo.GetByIdAsync(request.UserId);
                    if (user != null)
                    {
                        results.Add(new UpgradeRequestDTO
                        {
                            RequestId = request.Id,
                            UserId = user.Id,
                            UserName = user.UserName,
                            FullName = user.FullName ?? "",
                            Email = user.Email ?? "",
                            PhoneNumber = user.PhoneNumber ?? "",
                            Address = request.Address ?? "",
                            TaxCode = request.TaxCode ?? "",
                            Status = request.Status ?? UpgradeRequestStatusConst.Pending,
                            RequestedAt = request.RequestedAt
                        });
                    }
                }
                return ResponseFactory.Success(
                    results.OrderByDescending(r => r.RequestedAt).AsEnumerable(),
                    MessageResponse.RequestManagement.UpgradeRequest.REQUESTS_RETRIEVED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<IEnumerable<UpgradeRequestDTO>>();
            }
        }

        #endregion
    }
}