using HotelBooking.application.Services.Helpers.Media.BusinessLicenses;
using HotelBooking.application.DTOs.Media.BusinessLicenses;
using FluentValidation;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.Media.BusinessLicenses;

public interface IFileService
{
    Task<ApiResponse<UploadResultDTO>> UploadBusinessLicenseAsync(UploadFileDTO file, int userId, CancellationToken cancellationToken = default);
}

public class FileService : IFileService
{
    private readonly IBusinessLicenseUploader _uploader;
    private readonly IValidator<UploadFileDTO> _validator;
    private readonly ILogger<FileService> _logger;
    private readonly IBusinessLicenseUploadRepository _uploads;
    private readonly IUnitOfWork _unitOfWork;

    public FileService(IBusinessLicenseUploader uploader, IValidator<UploadFileDTO> validator, ILogger<FileService> logger,
        IBusinessLicenseUploadRepository uploads, IUnitOfWork unitOfWork)
    {
        _uploader = uploader;
        _validator = validator;
        _logger = logger;
        _uploads = uploads;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<UploadResultDTO>> UploadBusinessLicenseAsync(UploadFileDTO file, int userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (file == null) return Invalid("A business license file is required.");
        var validation = await _validator.ValidateAsync(file, cancellationToken);
        if (!validation.IsValid) return Invalid(validation.Errors[0].ErrorMessage);
        if (userId <= 0) return ResponseFactory.Failure<UploadResultDTO>(StatusCodeResponse.Unauthorized, "Invalid Owner identity.");
        try { return await ValidateContentAndUploadAsync(file, userId, cancellationToken); }
        catch (OperationCanceledException) { throw; }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Business license upload failed for Owner {OwnerId}.", userId);
            return ResponseFactory.ServerError<UploadResultDTO>();
        }
    }

    private async Task<ApiResponse<UploadResultDTO>> ValidateContentAndUploadAsync(UploadFileDTO file, int userId, CancellationToken cancellationToken)
    {
        // FR-ONB-011 / FR-MED-003: bound actual bytes and verify the format before provider access.
        await using var content = await BusinessLicenseFileRules.ReadContentAsync(file.Content, cancellationToken);
        if (content == null) return Invalid("Business license must be no larger than 10 MiB.");
        if (content.Length == 0 || content.Length != file.Size) return Invalid("Business license content size does not match the declared size.");
        if (!BusinessLicenseFileRules.MatchesSignature(file.FileName, content.GetBuffer().AsSpan(0, (int)content.Length)))
            return Invalid("Business license content must match its file extension and MIME type.");
        var uploaded = await _uploader.UploadAsync(new UploadFileDTO
        {
            FileName = file.FileName, ContentType = file.ContentType, Size = content.Length, Content = content,
        }, userId, cancellationToken);
        if (uploaded.StatusCode != StatusCodeResponse.Success)
            return ResponseFactory.Failure<UploadResultDTO>(uploaded.StatusCode, uploaded.Message);
        if (uploaded.Content == null || !uploaded.Content.IsValid()) return ResponseFactory.ServerError<UploadResultDTO>();
        return await PersistUploadAsync(file, userId, uploaded.Content, cancellationToken);
    }

    private async Task<ApiResponse<UploadResultDTO>> PersistUploadAsync(UploadFileDTO file, int userId,
        BusinessLicenseProviderUpload provider, CancellationToken cancellationToken)
    {
        // FR-ONB-009/013: release a receipt only after the authenticated Owner's upload is durable.
        var upload = new BusinessLicenseUpload
        {
            Id = Guid.NewGuid(), OwnerId = userId, PublicId = provider.PublicId, ResourceType = provider.ResourceType,
            DeliveryType = provider.DeliveryType, StoredUrl = provider.StoredUrl, OriginalFileName = file.FileName,
            ContentType = file.ContentType.ToLowerInvariant(), SizeBytes = file.Size, CreatedAt = DateTime.UtcNow, IsDeleted = false,
        };
        await _uploads.AddAsync(upload, cancellationToken);
        var saved = await _unitOfWork.TrySaveChangesAsync(cancellationToken);
        if (saved.HasConcurrencyConflict || saved.AffectedRows < 1)
        {
            _logger.LogWarning("Upload registry save failed for Owner {OwnerId}, asset {PublicId}.", userId, provider.PublicId);
            return ResponseFactory.ServerError<UploadResultDTO>();
        }
        return ResponseFactory.Success(new UploadResultDTO
        {
            // FR-ONB-013: return the receipt only; the provider URL stays in the registry.
            UploadId = upload.Id, Uploaded = true, FileName = upload.OriginalFileName,
        }, MessageResponse.Common.UPDATE_SUCCESSFULLY);
    }

    private static ApiResponse<UploadResultDTO> Invalid(string message) =>
        ResponseFactory.Failure<UploadResultDTO>(StatusCodeResponse.BadRequest, message);
}
