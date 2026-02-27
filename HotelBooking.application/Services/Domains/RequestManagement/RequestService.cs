using HotelBooking.infrastructure.Models;
using HotelBooking.application.Helpers;

namespace HotelBooking.application.Services.Domains.RequestManagement
{
    public interface IUpgradeRequestService
    {
        public Task<ApiResponse<UserForUpgradeDTO?>> GetUserForUpgradeAsync(int userId);
        public Task<ApiResponse<bool>> CreateRequestAsync(int userId, string address, string taxCode);
        public Task<ApiResponse<IEnumerable<UpgradeRequestDTO>>> GetAllRequestAsync(string? status = null);
        public Task<ApiResponse<UpgradeRequestDTO>> GetByRequestIdAsync(int requestId);
        public Task<ApiResponse<bool>> ApproveRequestAsync(int requestId, int adminId);
        public Task<ApiResponse<bool>> RejectRequestAsync(int requestId, int adminId);
    }

    public class UpgradeRequestService : IUpgradeRequestService
    {
        private readonly IUpgradeRequestRepository _upgradeRequestRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUnitOfWork _unitOfWork;

        public UpgradeRequestService(IUpgradeRequestRepository upgradeRequestRepo, IUserRepository userRepo, IUserRoleRepository userRoleRepo, IUnitOfWork unitOfWork)
        {
            _upgradeRequestRepo = upgradeRequestRepo;
            _userRepo = userRepo;
            _userRoleRepo = userRoleRepo;
            _unitOfWork = unitOfWork;
        }

        [Obsolete]
        public async Task<ApiResponse<UserForUpgradeDTO?>> GetUserForUpgradeAsync(int userId)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                    return ResponseFactory.Failure<UserForUpgradeDTO?>(StatusCodeResponse.NotFound, MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

                // Check for existing requests
                var existingRequests = await _upgradeRequestRepo.GetAllAsync();
                var userRequest = existingRequests?
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.RequestedAt)
                    .FirstOrDefault();

                var requestStatus = userRequest?.Status ?? "None";

                var userForUpgradeDTO = new UserForUpgradeDTO
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName ?? "",
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RequestStatus = requestStatus
                };

                return ResponseFactory.Success<UserForUpgradeDTO?>(userForUpgradeDTO, MessageResponse.RequestManagement.UpgradeRequest.USER_INFO_RETRIEVED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<UserForUpgradeDTO?>();
            }
        }

        [Obsolete]
        public async Task<ApiResponse<bool>> CreateRequestAsync(int userId, string address, string taxCode)
        {
            try
            {
                // Check if user exists
                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.NotFound, MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

                // Check role qua UserRoles
                var hasCustomerRole = await _userRoleRepo
                    .AnyAsync(ur => ur.UserId == userId && ur.RoleId == RoleTypeConstDTO.Customer);

                if (!hasCustomerRole)
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.Forbidden, MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);

                // Check if there's already a pending request for this user
                var existingRequests = await _upgradeRequestRepo.GetPendingByIdAsync(userId);
                if (existingRequests.Any())
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.Conflict, MessageResponse.RequestManagement.UpgradeRequest.PENDING_REQUEST_EXISTS);

                // Create new upgrade request
                var request = new UpgradeRequest
                {
                    UserId = userId,
                    Address = address,
                    TaxCode = taxCode,
                    Status = "Pending",
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

        [Obsolete]
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
                            PhoneNumber = user.PhoneNumber ?? "",
                            Address = request.Address ?? "",
                            TaxCode = request.TaxCode ?? "",
                            Status = request.Status ?? "Pending",
                            RequestedAt = request.RequestedAt
                        });
                    }
                }
                return ResponseFactory.Success(results.OrderByDescending(r => r.RequestedAt).AsEnumerable(), MessageResponse.RequestManagement.UpgradeRequest.REQUESTS_RETRIEVED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<IEnumerable<UpgradeRequestDTO>>();
            }
        }

        [Obsolete]
        public async Task<ApiResponse<UpgradeRequestDTO>> GetByRequestIdAsync(int requestId)
        {
            try
            {
                var request = await _upgradeRequestRepo.GetByIdAsync(requestId);
                if (request == null)
                    return ResponseFactory.Failure<UpgradeRequestDTO>(StatusCodeResponse.NotFound, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_NOT_FOUND);

                var user = await _userRepo.GetByIdAsync(request.UserId);
                if (user == null)
                    return ResponseFactory.Failure<UpgradeRequestDTO>(StatusCodeResponse.NotFound, MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

                var requestDTO = new UpgradeRequestDTO
                {
                    RequestId = request.Id,
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Address = request.Address ?? "",
                    TaxCode = request.TaxCode ?? "",
                    Status = request.Status ?? "Pending",
                    RequestedAt = request.RequestedAt
                };
                return ResponseFactory.Success(requestDTO, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_RETRIEVED);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<UpgradeRequestDTO>();
            }
        }

        [Obsolete]
        public async Task<ApiResponse<bool>> ApproveRequestAsync(int requestId, int adminId)
        {
            try
            {
                var request = await _upgradeRequestRepo.GetByIdAsync(requestId);
                if (request == null || request.Status != "Pending")
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);

                var user = await _userRepo.GetByIdAsync(request.UserId);
                if (user == null)
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.NotFound, MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

                var hasCustomerRole = await _userRoleRepo.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == RoleTypeConstDTO.Customer);
                if (!hasCustomerRole)
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);

                // Add Owner role to user
                var ownerRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = RoleTypeConstDTO.Owner
                };
                await _userRoleRepo.AddAsync(ownerRole);

                // Update request status
                request.Status = "Approved";
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

        [Obsolete]
        public async Task<ApiResponse<bool>> RejectRequestAsync(int requestId, int adminId)
        {
            try
            {
                var request = await _upgradeRequestRepo.GetByIdAsync(requestId);
                if (request == null || request.Status != "Pending")
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);

                // Update request status to Rejected
                request.Status = "Rejected";
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
    }
}