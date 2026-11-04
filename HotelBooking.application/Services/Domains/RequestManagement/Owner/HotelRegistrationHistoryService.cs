using System.Text.Json;
using FluentValidation;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.RequestManagement.Owner;

public interface IHotelRegistrationHistoryService
{
    Task<ApiResponse<PagedResult<HotelRegistrationDetailDTO>>> GetPageAsync(
        int ownerId, PagingRequest paging, CancellationToken cancellationToken = default);
}

// Keep the paged read independent of registration commands and the legacy unpaged API.
public class HotelRegistrationHistoryService(IHotelApprovalRequestRepository requests,
    IUserRepository users, IValidator<PagingRequest> validator) : IHotelRegistrationHistoryService
{
    public async Task<ApiResponse<PagedResult<HotelRegistrationDetailDTO>>> GetPageAsync(
        int ownerId, PagingRequest paging, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ownerId <= 0) return Failure(StatusCodeResponse.Unauthorized, "Invalid Owner identity.");
        if (paging == null) return Failure(StatusCodeResponse.BadRequest, "Pagination is required.");
        var validation = await validator.ValidateAsync(paging, cancellationToken);
        if (!validation.IsValid) return Failure(StatusCodeResponse.BadRequest, validation.Errors[0].ErrorMessage);
        if ((long)(paging.PageIndex!.Value - 1) * paging.PageSize!.Value > int.MaxValue)
            return Failure(StatusCodeResponse.BadRequest, "Page index is too large.");
        var (items, total) = await requests.GetPageByOwnerAsync(ownerId, paging.PageIndex.Value, paging.PageSize.Value, cancellationToken);
        var names = await HotelRequestDecisionMapper.LoadNamesAsync(items, users);
        var dtos = items.Select(item => Map(item, names)).ToList();
        return ResponseFactory.Success(new PagedResult<HotelRegistrationDetailDTO>(dtos, total, paging.PageIndex, paging.PageSize), "Registration history retrieved.");
    }

    private static HotelRegistrationDetailDTO Map(HotelApprovalRequest request, Dictionary<int, string?> names)
    {
        var extra = string.IsNullOrWhiteSpace(request.Additional) ? new HotelAdditionalInfo()
            : JsonSerializer.Deserialize<HotelAdditionalInfo>(request.Additional, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        var dto = new HotelRegistrationDetailDTO
        {
            RequestId = request.Id,
            OwnerId = request.OwnerId,
            Name = request.Name,
            Address = request.Address,
            TaxCode = request.TaxCode,
            BusinessLicenseUrl = request.BusinessLicenseUrl,
            RequestedAt = request.CreatedAt,
            Status = request.Status ?? "None"
        };
        MapProperty(dto, extra);
        HotelRequestDecisionMapper.Apply(dto, request, names);
        return dto;
    }

    private static void MapProperty(HotelRegistrationDetailDTO dto, HotelAdditionalInfo extra)
    {
        dto.Description = extra.Description;
        dto.PropertyTypeId = extra.PropType?.Id ?? 0;
        dto.PropertyTypeName = extra.PropType?.Name ?? string.Empty;
        dto.CountryId = extra.Country?.Id ?? 4;
        dto.CountryName = extra.Country?.Name ?? string.Empty;
        dto.ProvinceId = extra.Province?.Id ?? 0;
        dto.ProvinceName = extra.Province?.Name ?? string.Empty;
        dto.WardId = extra.Ward?.Id ?? 0;
        dto.WardName = extra.Ward?.Name ?? string.Empty;
        dto.StarRating = extra.StarRating;
        dto.PublicPhone = extra.PublicPhone;
        dto.PublicEmail = extra.PublicEmail;
        dto.Latitude = extra.Latitude;
        dto.Longitude = extra.Longitude;
    }

    private static ApiResponse<PagedResult<HotelRegistrationDetailDTO>> Failure(string status, string message) =>
        ResponseFactory.Failure<PagedResult<HotelRegistrationDetailDTO>>(status, message);
}
