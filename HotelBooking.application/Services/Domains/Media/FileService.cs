using FluentValidation;
using HotelBooking.application.Services.Helpers.Media;

namespace HotelBooking.application.Services.Domains.Media;

public interface IFileService
{
    Task<ApiResponse<UploadResultDTO>> UploadBusinessLicenseAsync(UploadFileDTO file, int userId, CancellationToken cancellationToken = default);
}

public class FileService : IFileService
{
    private readonly IBusinessLicenseUploader _uploader;
    private readonly IValidator<UploadFileDTO> _validator;
    private readonly ILogger<FileService> _logger;

    public FileService(IBusinessLicenseUploader uploader, IValidator<UploadFileDTO> validator, ILogger<FileService> logger)
    {
        _uploader = uploader;
        _validator = validator;
        _logger = logger;
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
        return await _uploader.UploadAsync(new UploadFileDTO
        {
            FileName = file.FileName, ContentType = file.ContentType, Size = content.Length, Content = content,
        }, userId, cancellationToken);
    }

    private static ApiResponse<UploadResultDTO> Invalid(string message) =>
        ResponseFactory.Failure<UploadResultDTO>(StatusCodeResponse.BadRequest, message);
}
