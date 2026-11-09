using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement.Admin;
using HotelBooking.application.Services.Domains.RequestManagement.Owner;
using HotelBooking.application.Validators.Common;
using HotelBooking.application.Validators.RequestManagement.Admin;
using HotelBooking.application.Validators.RequestManagement.Owner;
using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace HotelBooking.Tests.Integration.RequestManagement;

// FR-ONB-019: pause after real repository reads, before SQL writes, to force stale Pending snapshots.
public sealed class HotelRequestRaceSession : TestHotelBookingDBContext
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task Ready => _ready.Task;
    public void Release() => _release.TrySetResult();

    public HotelRequestRaceSession(DbContextOptions<HotelBookingDBContext> options) : base(options) { }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _ready.TrySetResult();
        await _release.Task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<ApiResponse<bool>> ExecuteAsync(string action, HotelRequestSeed seed, int adminId)
    {
        var token = new TestToken();
        var requests = new HotelApprovalRequestRepository(this, token);
        var hotels = new HotelRepository(this, token);
        var users = new UserRepository(this, token);
        var unit = new UnitOfWork(this);
        if (action == "Cancelled") return await CreateOwner(requests, hotels, users, unit, token)
            .CancelRequestAsync(seed.OwnerId, seed.RequestId);
        var admin = new AdminHotelApprovalRequestService(requests, hotels, unit, new PagingRequestValidator(),
            NullLogger<AdminHotelApprovalRequestService>.Instance, new RejectHotelRegistrationValidator(), users);
        return action == "Approved"
            ? await admin.ApproveRequestAsync(seed.RequestId, adminId)
            : await admin.RejectRequestAsync(seed.RequestId, adminId,
                new RejectHotelRegistrationDTO { Reason = "The submitted business license is incomplete." });
    }

    private HotelRegistrationService CreateOwner(IHotelApprovalRequestRepository requests, IHotelRepository hotels,
        IUserRepository users, IUnitOfWork unit, ICancellationTokenProvider token) => new(
        requests, hotels, new PropertyTypeRepository(this, token), new ProvinceRepository(this, token),
        new WardRepository(this, token), new HotelRegistrationValidator(), unit,
        NullLogger<HotelRegistrationService>.Instance, users, new BusinessLicenseUploadRepository(this));

    private sealed class TestToken : ICancellationTokenProvider
    {
        public CancellationToken Token => CancellationToken.None;
    }
}
