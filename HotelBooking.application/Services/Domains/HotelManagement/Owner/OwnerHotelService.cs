using System.Text.Json;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel.Owner;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Validators.HotelManagement.Owner;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.HotelManagement.Owner;

public interface IOwnerHotelService
{
    Task<ApiResponse<PagedResult<OwnerHotelSummaryDTO>>> GetPageAsync(int ownerId, PagingRequest paging, CancellationToken cancellationToken = default);
    Task<ApiResponse<OwnerHotelDetailsDTO>> GetByIdAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default);
}

public class OwnerHotelService(IHotelRepository hotels, IValidator<PagingRequest> pagingValidator) : IOwnerHotelService
{
    private readonly OwnerHotelIdValidator _idValidator = new();
    private static readonly JsonSerializerOptions MetadataOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ApiResponse<PagedResult<OwnerHotelSummaryDTO>>> GetPageAsync(int ownerId, PagingRequest paging,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ownerId <= 0) return InvalidOwner<PagedResult<OwnerHotelSummaryDTO>>();
        if (paging == null) return ResponseFactory.Failure<PagedResult<OwnerHotelSummaryDTO>>(StatusCodeResponse.BadRequest, "Pagination is required.");
        var validation = await pagingValidator.ValidateAsync(paging, cancellationToken);
        if (!validation.IsValid) return ResponseFactory.Failure<PagedResult<OwnerHotelSummaryDTO>>(StatusCodeResponse.BadRequest, validation.Errors[0].ErrorMessage);
        if ((long)(paging.PageIndex!.Value - 1) * paging.PageSize!.Value > int.MaxValue)
            return ResponseFactory.Failure<PagedResult<OwnerHotelSummaryDTO>>(StatusCodeResponse.BadRequest, "Page index is too large.");
        var (items, count) = await hotels.GetPageByOwnerAsync(ownerId, paging.PageIndex.Value, paging.PageSize.Value, cancellationToken);
        var result = new PagedResult<OwnerHotelSummaryDTO>(items.Select(MapSummary).ToList(), count, paging.PageIndex, paging.PageSize);
        return ResponseFactory.Success(result, "Owner Hotels retrieved.");
    }

    public async Task<ApiResponse<OwnerHotelDetailsDTO>> GetByIdAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ownerId <= 0) return InvalidOwner<OwnerHotelDetailsDTO>();
        var validation = await _idValidator.ValidateAsync(hotelId, cancellationToken);
        if (!validation.IsValid) return ResponseFactory.Failure<OwnerHotelDetailsDTO>(StatusCodeResponse.BadRequest, validation.Errors[0].ErrorMessage);
        // FR-CAT-001/003/008: public visibility rules must not hide the Owner's preparation workflow.
        var hotel = await hotels.GetByOwnerAsync(ownerId, hotelId, cancellationToken);
        if (hotel == null) return ResponseFactory.Failure<OwnerHotelDetailsDTO>(StatusCodeResponse.NotFound, "Hotel not found.");
        return ResponseFactory.Success(MapDetails(hotel), "Owner Hotel retrieved.");
    }

    private static OwnerHotelSummaryDTO MapSummary(Hotel hotel)
    {
        var dto = new OwnerHotelSummaryDTO();
        CopySummary(hotel, dto);
        return dto;
    }

    private static void CopySummary(Hotel hotel, OwnerHotelSummaryDTO dto)
    {
        dto.Id = hotel.Id;
        dto.Name = hotel.Name;
        dto.Address = hotel.Address;
        dto.CoverImageUrl = hotel.CoverImageUrl;
        dto.Status = hotel.Status;
        dto.IsVerified = hotel.IsVerified;
        dto.CreatedAt = hotel.CreatedAt;
    }

    private static OwnerHotelDetailsDTO MapDetails(Hotel hotel)
    {
        var dto = new OwnerHotelDetailsDTO
        {
            Description = hotel.Description,
            PropertyTypeId = hotel.PropertyTypeId, PropertyTypeName = hotel.PropertyType?.Name ?? string.Empty,
            CountryId = hotel.CountryId, CountryName = hotel.Country?.Name ?? string.Empty,
            ProvinceId = hotel.ProvinceId, ProvinceName = hotel.Province?.Name ?? string.Empty,
            WardId = hotel.WardId, WardName = hotel.Ward?.Name ?? string.Empty,
        };
        CopySummary(hotel, dto);
        CopyMetadata(hotel.Additional, dto);
        return dto;
    }

    private static void CopyMetadata(string? json, OwnerHotelDetailsDTO dto)
    {
        // FR-CAT-001/003: never expose raw JSON or business-license storage data.
        var metadata = ReadMetadata(json);
        dto.PublicPhone = metadata.PublicPhone;
        dto.PublicEmail = metadata.PublicEmail;
        dto.StarRating = metadata.StarRating;
        dto.Latitude = metadata.Latitude;
        dto.Longitude = metadata.Longitude;
        dto.TaxCode = metadata.TaxCode;
    }

    private static HotelEntityAdditionalData ReadMetadata(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        // Legacy malformed optional metadata must not hide an otherwise accessible owned Hotel.
        try { return JsonSerializer.Deserialize<HotelEntityAdditionalData>(json, MetadataOptions) ?? new(); }
        catch (JsonException) { return new(); }
    }

    private static ApiResponse<T> InvalidOwner<T>() => ResponseFactory.Failure<T>(StatusCodeResponse.Unauthorized, "Invalid Owner identity.");
}
