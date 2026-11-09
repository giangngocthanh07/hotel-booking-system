namespace HotelBooking.application.Services.Domains.Media;

// FR-ONB-013: only the authorized use case supplies registry metadata; no arbitrary URL is accepted.
public sealed record BusinessLicenseDownloadSource(
    string PublicId, string ResourceType, string DeliveryType, string ContentType, long SizeBytes);
