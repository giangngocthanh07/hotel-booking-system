using HotelBooking.infrastructure.Models;

public interface IUpgradeRequestService
{
    public Task<UserForUpgradeDTO?> GetUserForUpgradeAsync(int userId);
    public Task<bool> CreateRequestAsync(int userId, string address, string taxCode);
    public Task<IEnumerable<UpgradeRequestDTO>> GetAllRequestAsync(string? status = null);
    public Task<UpgradeRequestDTO> GetByRequestIdAsync(int requestId);
    public Task<bool> ApproveRequestAsync(int requestId, int adminId);
    public Task<bool> RejectRequestAsync(int requestId, int adminId);
}

public class UpgradeRequestService : IUpgradeRequestService
{
    HotelBookingDBContext _context;
    private readonly IUpgradeRequestRepository _upgradeRequestRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUserRoleRepository _userRoleRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpgradeRequestService(HotelBookingDBContext context, IUpgradeRequestRepository upgradeRequestRepo, IUserRepository userRepo, IUserRoleRepository userRoleRepo, IUnitOfWork unitOfWork)
    {
        _context = context;
        _upgradeRequestRepo = upgradeRequestRepo;
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserForUpgradeDTO?> GetUserForUpgradeAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserForUpgradeDTO
        {
            UserId = user.Id,
            UserName = user.UserName,
            FullName = user.FullName ?? "",
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
    }

    public async Task<bool> CreateRequestAsync(int userId, string address, string taxCode)
    {
        // Check if user exists
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
        {
            return false; // User does not exist
        }

        // Check role qua UserRoles
        var hasCustomerRole = await _userRoleRepo
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == RoleTypeConstDTO.Customer);

        if (!hasCustomerRole)
            return false;

        // Check if there's already a pending request for this user
        var existingRequests = await _upgradeRequestRepo.GetPendingByIdAsync(userId);
        if (existingRequests.Any())
        {
            return false; // There's already a pending request
        }

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
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<UpgradeRequestDTO>> GetAllRequestAsync(string? status = null)

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
        return results.OrderByDescending(r => r.RequestedAt);

    }

    public async Task<UpgradeRequestDTO> GetByRequestIdAsync(int requestId)
    {
        var request = await _upgradeRequestRepo.GetByIdAsync(requestId);
        if (request == null) return null;

        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user == null) return null;

        return new UpgradeRequestDTO
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
    }

    public async Task<bool> ApproveRequestAsync(int requestId, int adminId)
    {
        var request = await _upgradeRequestRepo.GetByIdAsync(requestId);
        if (request == null || request.Status != "Pending")
        {
            return false; // Request not found or not pending
        }

        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return false; // User not found
        }

        var hasCustomerRole = await _userRoleRepo.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == RoleTypeConstDTO.Customer);
        if (!hasCustomerRole)
        {
            return false; // User is not a customer
        }

        // Add Owner role to user
        var ownerRole = new UserRole
        {
            UserId = user.Id,
            RoleId = RoleTypeConstDTO.Owner
        };
        _context.UserRoles.Add(ownerRole);

        // Update request status
        request.Status = "Approved";
        request.ApprovedAt = DateTime.Now;
        request.ApprovedBy = adminId;
        await _upgradeRequestRepo.UpdateAsync(request);

        // Update User data
        user.Address = request.Address;
        user.TaxCode = request.TaxCode;
        await _userRepo.UpdateAsync(user);

        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<bool> RejectRequestAsync(int requestId, int adminId)
    {
        var request = await _upgradeRequestRepo.GetByIdAsync(requestId);
        if (request == null || request.Status != "Pending")
        {
            return false; // Request not found or not pending
        }

        // Update request status to Rejected
        request.Status = "Rejected";
        request.ApprovedAt = DateTime.Now;
        request.ApprovedBy = adminId;
        await _upgradeRequestRepo.UpdateAsync(request);

        return await _unitOfWork.SaveChangesAsync() > 0;
    }
}