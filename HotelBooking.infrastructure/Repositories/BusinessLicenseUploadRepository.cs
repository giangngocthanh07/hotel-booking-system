using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

// GUID upload receipts use a specific repository rather than the int-key generic CRUD contract.
public interface IBusinessLicenseUploadRepository
{
    Task AddAsync(BusinessLicenseUpload upload, CancellationToken cancellationToken = default);
    Task<BusinessLicenseUpload?> GetOwnedActiveAsync(Guid id, int ownerId, CancellationToken cancellationToken = default);
}

public class BusinessLicenseUploadRepository(HotelBookingDBContext context) : IBusinessLicenseUploadRepository
{
    public async Task<BusinessLicenseUpload?> GetOwnedActiveAsync(Guid id, int ownerId, CancellationToken cancellationToken = default) =>
        await context.BusinessLicenseUploads.AsNoTracking().FirstOrDefaultAsync(
            upload => upload.Id == id && upload.OwnerId == ownerId && !upload.IsDeleted, cancellationToken);

    public async Task AddAsync(BusinessLicenseUpload upload, CancellationToken cancellationToken = default)
    {
        // FR-ONB-013: track only; the application use case owns the single persistence boundary.
        await context.BusinessLicenseUploads.AddAsync(upload, cancellationToken);
    }
}
