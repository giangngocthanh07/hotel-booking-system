using HotelBooking.infrastructure.Models;
using HotelBooking.application.Helpers;
using FluentValidation;
using System.Linq.Expressions;
using HotelBooking.application.DTOs.Role;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.UpgradeRequest;
using HotelBooking.application.Services.Domains.RequestManagement.Base;

namespace HotelBooking.application.Services.Domains.RequestManagement.Admin
{
    public interface IAdminUpgradeRequestService : IBaseAdminRequestService<UpgradeRequestDTO>
    {
    }

    public class AdminUpgradeRequestService : IAdminUpgradeRequestService
    {
        private readonly IUpgradeRequestRepository _upgradeRequestRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<PagingRequest> _pagingValidator;
        private readonly IValidator<CreateUpgradeRequestDTO> _createRequestValidator;

        public AdminUpgradeRequestService(
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

                // 2. Build filter expression by status
                Expression<Func<HotelBooking.infrastructure.Models.UpgradeRequest, bool>>? filter = null;
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
                    Status = request.Status ?? RequestStatusConst.Pending,
                    RequestedAt = request.RequestedAt
                }).ToList();

                // 5. Return PagedResult
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
                    Status = request.Status ?? RequestStatusConst.Pending,
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
                if (request == null || request.Status != RequestStatusConst.Pending)
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
                request.Status = RequestStatusConst.Approved;
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
                if (request == null || request.Status != RequestStatusConst.Pending)
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.BadRequest,
                        MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);

                // Update request status to Rejected
                request.Status = RequestStatusConst.Rejected;
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
    }
}