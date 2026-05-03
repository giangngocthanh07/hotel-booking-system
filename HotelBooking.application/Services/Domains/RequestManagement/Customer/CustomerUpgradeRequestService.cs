using HotelBooking.infrastructure.Models;
using HotelBooking.application.Helpers;
using FluentValidation;
using HotelBooking.application.DTOs.Role;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.UpgradeRequest;
using HotelBooking.application.Services.Domains.RequestManagement.Base;

namespace HotelBooking.application.Services.Domains.RequestManagement.Customer;

public interface ICustomerUpgradeRequestService : IBaseUserRequestService<UpgradeRequestDTO, CreateUpgradeRequestDTO>
{
    Task<ApiResponse<UserForUpgradeDTO?>> GetUserForUpgradeAsync(int userId);
}

public class CustomerUpgradeRequestService : ICustomerUpgradeRequestService
{
    private readonly IUpgradeRequestRepository _upgradeRequestRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUserRoleRepository _userRoleRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerUpgradeRequestService> _logger;
    private readonly IValidator<CreateUpgradeRequestDTO> _createRequestValidator;

    public CustomerUpgradeRequestService(
        IUpgradeRequestRepository upgradeRequestRepo,
        IUserRepository userRepo,
        IUserRoleRepository userRoleRepo,
        IUnitOfWork unitOfWork,
        ILogger<CustomerUpgradeRequestService> logger,
        IValidator<CreateUpgradeRequestDTO> createRequestValidator)
    {
        _upgradeRequestRepo = upgradeRequestRepo;
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _createRequestValidator = createRequestValidator;
    }

    public async Task<ApiResponse<UserForUpgradeDTO?>> GetUserForUpgradeAsync(int userId)
    {
        try
        {
            if (userId <= 0)
                return ResponseFactory.Failure<UserForUpgradeDTO?>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.UpgradeRequest.USERID_INVALID);

            // Check if user exists
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return ResponseFactory.Failure<UserForUpgradeDTO?>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

            // Check for existing requests
            var userRequests = await _upgradeRequestRepo.GetByUserIdAsync(userId);
            var latestRequest = userRequests?.OrderByDescending(r => r.RequestedAt).FirstOrDefault();

            var requestStatus = latestRequest?.Status ?? RequestStatusConst.None;

            var userForUpgradeDTO = new UserForUpgradeDTO
            {
                UserId = user.Id,
                UserName = user.UserName,
                FullName = user.FullName ?? "",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RequestStatus = requestStatus,
                RequestId = latestRequest?.Id
            };

            return ResponseFactory.Success<UserForUpgradeDTO?>(
                userForUpgradeDTO,
                MessageResponse.RequestManagement.UpgradeRequest.USER_INFO_RETRIEVED);
        }
        catch (Exception ex)
        {
            _logger.LogError("CustomerUpgradeRequestService.GetUserForUpgradeAsync: {ErrorMessage}", ex.Message);
            return ResponseFactory.ServerError<UserForUpgradeDTO?>();
        }
    }

    public async Task<ApiResponse<UpgradeRequestDTO>> CreateRequestAsync(int userId, CreateUpgradeRequestDTO createDto)
    {
        try
        {
            // Validate input
            var validation = await _createRequestValidator.ValidateAsync(createDto);
            if (!validation.IsValid)
            {
                return ResponseFactory.Failure<UpgradeRequestDTO>(
                    StatusCodeResponse.BadRequest,
                    validation.Errors.First().ErrorMessage);
            }

            // Validate userId
            if (userId <= 0)
                return ResponseFactory.Failure<UpgradeRequestDTO>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.UpgradeRequest.USERID_INVALID);

            // Check if user exists
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return ResponseFactory.Failure<UpgradeRequestDTO>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

            // Check role via UserRoles table
            var hasCustomerRole = await _userRoleRepo
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == RoleTypeConstDTO.Customer);

            if (!hasCustomerRole)
                return ResponseFactory.Failure<UpgradeRequestDTO>(
                    StatusCodeResponse.Forbidden,
                    MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);

            // Check if user is already a Owner
            var hasOwnerRole = await _userRoleRepo
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == RoleTypeConstDTO.Owner);

            if (hasOwnerRole)
                return ResponseFactory.Failure<UpgradeRequestDTO>(
                    StatusCodeResponse.Forbidden,
                    MessageResponse.RequestManagement.UpgradeRequest.USER_ALREADY_OWNER);

            // Check if there's already a pending request for this user
            var existingRequests = await _upgradeRequestRepo.GetPendingByIdAsync(userId);
            if (existingRequests.Any())
                return ResponseFactory.Failure<UpgradeRequestDTO>(
                    StatusCodeResponse.Conflict,
                    MessageResponse.RequestManagement.UpgradeRequest.PENDING_REQUEST_EXISTS);

            // Create new upgrade request
            var request = new UpgradeRequest
            {
                UserId = userId,
                Address = createDto.Address,
                TaxCode = createDto.TaxCode,
                Status = RequestStatusConst.Pending,
                RequestedAt = DateTime.Now
            };

            await _upgradeRequestRepo.AddAsync(request);
            var rowAffected = await _unitOfWork.SaveChangesAsync();

            if (rowAffected <= 0)
            {
                return ResponseFactory.Failure<UpgradeRequestDTO>(
                    StatusCodeResponse.Error, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CREATE_FAILED);
            }

            UpgradeRequestDTO saved = new UpgradeRequestDTO
            {
                RequestId = request.Id,
                UserId = request.UserId,
                UserName = user.UserName,
                FullName = user.FullName ?? "",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = request.Address,
                TaxCode = request.TaxCode,
                Status = request.Status,
                RequestedAt = request.RequestedAt
            };

            return ResponseFactory.Success(saved, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CREATED_SUCCESS);
        }
        catch (Exception ex)
        {
            _logger.LogError("CustomerUpgradeRequestService.CreateRequestAsync: {ErrorMessage}", ex.Message);
            return ResponseFactory.ServerError<UpgradeRequestDTO>();
        }
    }

    public async Task<ApiResponse<bool>> CancelRequestAsync(int userId, int requestId)
    {
        try
        {
            // Validate userId
            if (userId <= 0 || requestId <= 0)
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.UpgradeRequest.USERID_OR_REQUESTID_INVALID);

            // Find user's pending request
            var pendingRequests = await _upgradeRequestRepo.GetPendingByIdAsync(userId);
            var request = pendingRequests.FirstOrDefault(r => r.Id == requestId);

            if (request == null)
                return ResponseFactory.Failure<bool>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.UpgradeRequest.REQUEST_NOT_FOUND);

            request.Status = RequestStatusConst.Cancelled;
            await _upgradeRequestRepo.UpdateAsync(request);

            var saved = await _unitOfWork.SaveChangesAsync() > 0;
            return saved
                ? ResponseFactory.Success(true, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CANCELLED_SUCCESS)
                : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_CANCEL_FAILED);
        }
        catch (Exception ex)
        {
            _logger.LogError("CustomerUpgradeRequestService.CancelRequestAsync: {ErrorMessage}", ex.Message);
            return ResponseFactory.ServerError<bool>();
        }
    }

    public async Task<ApiResponse<List<UpgradeRequestDTO>>> GetMyRequestsAsync(int userId)
    {
        try
        {
            // Validate userId
            if (userId <= 0)
                return ResponseFactory.Failure<List<UpgradeRequestDTO>>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RequestManagement.UpgradeRequest.USERID_INVALID);

            var existingUser = await _userRepo.GetByIdAsync(userId);
            if (existingUser == null)
                return ResponseFactory.Failure<List<UpgradeRequestDTO>>(
                    StatusCodeResponse.NotFound,
                    MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);

            // Check role via UserRoles table
            var hasCustomerRole = await _userRoleRepo
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == RoleTypeConstDTO.Customer);

            if (!hasCustomerRole)
                return ResponseFactory.Failure<List<UpgradeRequestDTO>>(
                    StatusCodeResponse.Forbidden,
                    MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_CUSTOMER);

            // Check if user is already a Owner
            var hasOwnerRole = await _userRoleRepo
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == RoleTypeConstDTO.Owner);

            if (hasOwnerRole)
                return ResponseFactory.Failure<List<UpgradeRequestDTO>>(
                    StatusCodeResponse.Forbidden,
                    MessageResponse.RequestManagement.UpgradeRequest.USER_ALREADY_OWNER);

            var requests = await _upgradeRequestRepo.GetByUserIdAsync(userId);

            if (!requests.Any())
                return ResponseFactory.Success(new List<UpgradeRequestDTO>(), MessageResponse.RequestManagement.UpgradeRequest.NO_REQUESTS_FOUND);


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
                Status = r.Status ?? RequestStatusConst.Pending,
                RequestedAt = r.RequestedAt
            }).OrderByDescending(r => r.RequestedAt).ToList();

            return ResponseFactory.Success(dtos, MessageResponse.RequestManagement.UpgradeRequest.REQUESTS_RETRIEVED);
        }
        catch (Exception ex)
        {
            _logger.LogError("CustomerUpgradeRequestService.GetMyRequestsAsync: {ErrorMessage}", ex.Message);
            return ResponseFactory.ServerError<List<UpgradeRequestDTO>>();
        }
    }

}