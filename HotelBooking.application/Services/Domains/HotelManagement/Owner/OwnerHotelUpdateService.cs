using FluentValidation;
using HotelBooking.application.DTOs.Hotel.Owner;
using HotelBooking.application.Services.Helpers.HotelManagement.Owner;
using HotelBooking.application.Validators.HotelManagement.Owner;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.HotelManagement.Owner;

public interface IOwnerHotelUpdateService
{
    Task<ApiResponse<bool>> UpdateAsync(int ownerId, int hotelId, OwnerHotelUpdateDTO input, CancellationToken cancellationToken = default);
}

public class OwnerHotelUpdateService(IHotelRepository hotels, IUnitOfWork unit, IValidator<OwnerHotelUpdateDTO> validator)
    : IOwnerHotelUpdateService
{
    private readonly OwnerHotelIdValidator _idValidator = new();

    public async Task<ApiResponse<bool>> UpdateAsync(int ownerId, int hotelId, OwnerHotelUpdateDTO input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ownerId <= 0) return ResponseFactory.Failure<bool>(StatusCodeResponse.Unauthorized, "Invalid Owner identity.");
        if (input == null) return Invalid("Hotel information is required.");
        var id = await _idValidator.ValidateAsync(hotelId, cancellationToken);
        if (!id.IsValid) return Invalid(id.Errors[0].ErrorMessage);
        var validation = await validator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid) return Invalid(validation.Errors[0].ErrorMessage);
        return await UpdateOwnedAsync(ownerId, hotelId, input, cancellationToken);
    }

    private async Task<ApiResponse<bool>> UpdateOwnedAsync(int ownerId, int hotelId, OwnerHotelUpdateDTO input,
        CancellationToken cancellationToken)
    {
        var hotel = await hotels.GetOwnedForUpdateAsync(ownerId, hotelId, cancellationToken);
        if (hotel == null) return ResponseFactory.Failure<bool>(StatusCodeResponse.NotFound, "Hotel not found.");
        if (!await hotels.IsLocationValidAsync(input.CountryId, input.ProvinceId, input.WardId, cancellationToken))
            return Invalid("Country, Province and Ward must identify a valid location.");
        if (!OwnerHotelInformationMetadata.TryMerge(hotel.Additional, input, out var metadata))
            return ResponseFactory.Failure<bool>(StatusCodeResponse.Conflict, "Hotel metadata cannot be updated safely. Please contact support.");
        Apply(hotel, input, metadata);
        var saved = await unit.TrySaveChangesAsync(cancellationToken);
        if (saved.HasConcurrencyConflict) return ResponseFactory.Failure<bool>(StatusCodeResponse.Conflict, "Hotel changed. Refresh and try again.");
        return ResponseFactory.Success(true, "Hotel information updated.");
    }

    private static void Apply(Hotel hotel, OwnerHotelUpdateDTO input, string metadata)
    {
        // FR-CAT-002/003: edit tracked allowlisted fields, never mark the entire Hotel modified.
        hotel.Name = input.Name.Trim();
        hotel.Description = input.Description?.Trim();
        hotel.Address = input.Address.Trim();
        hotel.CountryId = input.CountryId;
        hotel.ProvinceId = input.ProvinceId;
        hotel.WardId = input.WardId;
        hotel.Additional = metadata;
    }

    private static ApiResponse<bool> Invalid(string message) => ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, message);
}
