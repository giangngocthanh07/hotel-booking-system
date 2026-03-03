using System.Text.Json;
using HotelBooking.infrastructure.Models;

public static class ServiceHelper
{
    // Static readonly shared instance — avoids repeated allocations. Implements STATIC SIMPLE FACTORY PATTERN
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public static int? GetTypeIdFromUpdateDto(ServiceUpdateDTO dto)
    {
        return dto switch
        {
            ServiceStandardUpdateDTO => (int)ServiceTypeEnum.Standard,
            ServiceAirportUpdateDTO => (int)ServiceTypeEnum.AirportTransfer,
            _ => null
        };
    }

    public static ServiceDTO? MapToServiceDTO(Service service)
    {
        var additionalJson = service.Additional ?? "{}";

        switch ((ServiceTypeEnum)service.TypeId)
        {
            case ServiceTypeEnum.Standard: // ID = 1
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(additionalJson, _jsonOptions);
                return new ServiceStandardDTO
                {
                    Id = service.Id,
                    Name = service.Name,
                    Description = service.Description,
                    Price = service.Price,
                    IsDeleted = service.IsDeleted,
                    TypeId = service.TypeId,
                    // Map Standard-specific field
                    Unit = dict?.GetValueOrDefault("Unit", "") ?? ""
                };

            case ServiceTypeEnum.AirportTransfer: // ID = 2
                var airportTransferData = JsonSerializer.Deserialize<ServiceAirportAdditionalData>(additionalJson, _jsonOptions);
                return new ServiceAirportTransferDTO
                {
                    Id = service.Id,
                    Name = service.Name,
                    Description = service.Description,
                    Price = service.Price,
                    IsDeleted = service.IsDeleted,
                    TypeId = service.TypeId,
                    // Map boolean logic flags
                    IsOneWayPaid = airportTransferData?.IsOneWayPaid ?? (service.Price > 0),
                    HasRoundTrip = airportTransferData?.HasRoundTrip ?? false,
                    IsRoundTripPaid = airportTransferData?.IsRoundTripPaid ?? false,
                    // Map actual data values
                    MaxPassengers = airportTransferData?.MaxPassengers,
                    MaxLuggage = airportTransferData?.MaxLuggage,
                    RoundTripPrice = airportTransferData?.RoundTripPrice,
                    // Logic: if AdditionalFee has a value then HasNightFee must be true
                    HasNightFee = airportTransferData?.HasNightFee ?? (airportTransferData?.AdditionalFee > 0),
                    AdditionalFee = airportTransferData?.AdditionalFee,
                    // --- Convert string (JSON) to TimeOnly (DTO) ---
                    AdditionalFeeStartTime = !string.IsNullOrEmpty(airportTransferData?.AdditionalFeeStartTime)
            ? TimeOnly.Parse(airportTransferData.AdditionalFeeStartTime) : null,

                    AdditionalFeeEndTime = !string.IsNullOrEmpty(airportTransferData?.AdditionalFeeEndTime)
            ? TimeOnly.Parse(airportTransferData.AdditionalFeeEndTime) : null
                };

            default:
                return null;
        }
    }

    // Serializes DTO additional data into a JSON string for storage — STATIC SIMPLE FACTORY PATTERN
    // =========================================================================
    // 2. MAP CREATE DTO -> JSON STRING (Persist to DB)
    // =========================================================================
    public static string MapToAdditionalJson(ServiceCreateDTO dto)
    {
        switch (dto)
        {
            case ServiceStandardCreateDTO std:
                return JsonSerializer.Serialize(new { Unit = std.Unit }, _jsonOptions);

            case ServiceAirportCreateDTO air:
                var data = new ServiceAirportAdditionalData();
                // Map and sanitize data before persisting
                ApplyAirportLogic(data, air);
                return JsonSerializer.Serialize(data, _jsonOptions);

            default:
                return "{}";
        }
    }

    // =========================================================================
    // 3. MAP UPDATE DTO -> JSON STRING (Update DB)
    // =========================================================================
    public static string MapToAdditionalJson(ServiceUpdateDTO dto)
    {
        switch (dto)
        {
            case ServiceStandardUpdateDTO std:
                return JsonSerializer.Serialize(new { Unit = std.Unit }, _jsonOptions);

            case ServiceAirportUpdateDTO air:
                var data = new ServiceAirportAdditionalData();
                // Map and sanitize data before persisting
                ApplyAirportLogic(data, air);
                return JsonSerializer.Serialize(data, _jsonOptions);

            default:
                return "{}";
        }
    }

    // =========================================================================
    // PRIVATE HELPER: Shared airport business logic (avoids code duplication)
    // =========================================================================

    // Overload for Create
    private static void ApplyAirportLogic(ServiceAirportAdditionalData data, ServiceAirportCreateDTO src)
    {
        data.MaxPassengers = src.MaxPassengers;
        data.MaxLuggage = src.MaxLuggage;
        data.IsOneWayPaid = src.IsOneWayPaid;

        // Round-trip logic: if disabled, clear price and payment flag
        data.HasRoundTrip = src.HasRoundTrip;
        data.IsRoundTripPaid = src.HasRoundTrip && src.IsRoundTripPaid;
        data.RoundTripPrice = (src.HasRoundTrip && src.IsRoundTripPaid) ? src.RoundTripPrice : null;

        // 2. Night surcharge logic: enforced here as a safeguard
        data.HasNightFee = src.HasNightFee;
        if (src.HasNightFee && src.AdditionalFeeStartTime.HasValue && src.AdditionalFeeEndTime.HasValue)
        {
            // Calculate the duration of the surcharge window
            TimeSpan duration = src.AdditionalFeeEndTime.Value - src.AdditionalFeeStartTime.Value;
            double totalHours = duration.TotalHours < 0 ? duration.TotalHours + 24 : duration.TotalHours;

            // @--BEST_PRACTICE_LIMIT_12H--@
            if (totalHours > 12)
            {
                // Can throw or log a warning here. Currently allows saving — validation should catch this upstream.
                // data.HasNightFee = false; // More aggressive guard option
            }

            data.AdditionalFee = src.AdditionalFee;
            data.AdditionalFeeStartTime = src.AdditionalFeeStartTime?.ToString("HH:mm");
            data.AdditionalFeeEndTime = src.AdditionalFeeEndTime?.ToString("HH:mm");
        }
        else
        {
            // Xóa sạch như đã làm ở bước trước
            data.AdditionalFee = null;
            data.AdditionalFeeStartTime = null;
            data.AdditionalFeeEndTime = null;
            data.HasNightFee = false;
        }
    }

    // Overload for Update
    private static void ApplyAirportLogic(ServiceAirportAdditionalData data, ServiceAirportUpdateDTO src)
    {
        data.MaxPassengers = src.MaxPassengers;
        data.MaxLuggage = src.MaxLuggage;
        data.IsOneWayPaid = src.IsOneWayPaid;

        data.HasRoundTrip = src.HasRoundTrip;
        data.IsRoundTripPaid = src.HasRoundTrip && src.IsRoundTripPaid;
        data.RoundTripPrice = (src.HasRoundTrip && src.IsRoundTripPaid) ? src.RoundTripPrice : null;

        // 2. Night surcharge logic: enforced here as a safeguard
        data.HasNightFee = src.HasNightFee;
        if (src.HasNightFee && src.AdditionalFeeStartTime.HasValue && src.AdditionalFeeEndTime.HasValue)
        {
            // Calculate the duration of the surcharge window
            TimeSpan duration = src.AdditionalFeeEndTime.Value - src.AdditionalFeeStartTime.Value;
            double totalHours = duration.TotalHours < 0 ? duration.TotalHours + 24 : duration.TotalHours;

            // @--BEST_PRACTICE_LIMIT_12H--@
            if (totalHours > 12)
            {
                // Can throw or log a warning here. Currently allows saving — validation should catch this upstream.
                // data.HasNightFee = false; // More aggressive guard option
            }

            data.AdditionalFee = src.AdditionalFee;
            data.AdditionalFeeStartTime = src.AdditionalFeeStartTime?.ToString("HH:mm");
            data.AdditionalFeeEndTime = src.AdditionalFeeEndTime?.ToString("HH:mm");
        }
        else
        {
            // Clear night surcharge fields
            data.AdditionalFee = null;
            data.AdditionalFeeStartTime = null;
            data.AdditionalFeeEndTime = null;
            data.HasNightFee = false;
        }
    }
}