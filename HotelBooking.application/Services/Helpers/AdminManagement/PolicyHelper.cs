using System.Text.Json;
using HotelBooking.infrastructure.Models;

/// <summary>
/// Helper class for Policy — handles Entity ↔ DTO mapping via JSON.
/// Pattern: Same as ServiceHelper — stores polymorphic data in the Additional column.
/// </summary>
public static class PolicyHelper
{
    // Shared JSON options — avoids repeated allocations
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    // ===========================================================================
    // 1. GET TYPE ID FROM UPDATE DTO
    // ===========================================================================
    public static int? GetTypeIdFromUpdateDto(PolicyUpdateDTO dto)
    {
        return dto switch
        {
            CheckInOutPolicyUpdateDTO => (int)PolicyTypeEnum.CheckInOut,
            CancellationPolicyUpdateDTO => (int)PolicyTypeEnum.Cancellation,
            ChildrenPolicyUpdateDTO => (int)PolicyTypeEnum.Children,
            PetPolicyUpdateDTO => (int)PolicyTypeEnum.Pets,
            _ => null
        };
    }

    // ===========================================================================
    // 2. MAP ENTITY → DTO (Output)
    // ===========================================================================
    public static PolicyDTO? MapToPolicyDTO(Policy entity)
    {
        var additionalJson = entity.Additional ?? "{}";

        switch ((PolicyTypeEnum)entity.TypeId)
        {
            case PolicyTypeEnum.CheckInOut:
                var checkInOutData = JsonSerializer.Deserialize<CheckInOutAdditionalData>(additionalJson, _jsonOptions);
                return new CheckInOutPolicyDTO
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    IsDeleted = entity.IsDeleted,
                    TypeId = entity.TypeId,
                    // Semantic properties
                    CheckInTime = ParseTimeOnly(checkInOutData?.CheckInTime),
                    CheckOutTime = ParseTimeOnly(checkInOutData?.CheckOutTime),
                    EarlyCheckInFee = checkInOutData?.EarlyCheckInFee,
                    LateCheckOutFee = checkInOutData?.LateCheckOutFee
                };

            case PolicyTypeEnum.Cancellation:
                var cancelData = JsonSerializer.Deserialize<CancellationAdditionalData>(additionalJson, _jsonOptions);
                return new CancellationPolicyDTO
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    IsDeleted = entity.IsDeleted,
                    TypeId = entity.TypeId,
                    // Semantic properties
                    DaysBeforeCheckIn = cancelData?.DaysBeforeCheckIn,
                    RefundPercent = cancelData?.RefundPercent,
                    IsRefundable = cancelData?.IsRefundable ?? false
                };

            case PolicyTypeEnum.Children:
                var childrenData = JsonSerializer.Deserialize<ChildrenAdditionalData>(additionalJson, _jsonOptions);
                return new ChildrenPolicyDTO
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    IsDeleted = entity.IsDeleted,
                    TypeId = entity.TypeId,
                    // Semantic properties
                    MinAge = childrenData?.MinAge,
                    MaxAge = childrenData?.MaxAge,
                    ExtraBedFee = childrenData?.ExtraBedFee
                };

            case PolicyTypeEnum.Pets:
                var petData = JsonSerializer.Deserialize<PetAdditionalData>(additionalJson, _jsonOptions);
                return new PetPolicyDTO
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    IsDeleted = entity.IsDeleted,
                    TypeId = entity.TypeId,
                    // Semantic properties
                    PetFee = petData?.PetFee,
                    IsPetAllowed = petData?.IsPetAllowed ?? false
                };

            default:
                return null;
        }
    }

    // ===========================================================================
    // 3. MAP CREATE DTO → JSON STRING (Persist to DB)
    // ===========================================================================
    public static string MapToAdditionalJson(PolicyCreateDTO dto)
    {
        switch (dto)
        {
            case CheckInOutPolicyCreateDTO checkInOut:
                return JsonSerializer.Serialize(new CheckInOutAdditionalData
                {
                    CheckInTime = checkInOut.CheckInTime?.ToString("HH:mm"),
                    CheckOutTime = checkInOut.CheckOutTime?.ToString("HH:mm"),
                    EarlyCheckInFee = checkInOut.EarlyCheckInFee,
                    LateCheckOutFee = checkInOut.LateCheckOutFee
                }, _jsonOptions);

            case CancellationPolicyCreateDTO cancel:
                return JsonSerializer.Serialize(new CancellationAdditionalData
                {
                    DaysBeforeCheckIn = cancel.DaysBeforeCheckIn,
                    RefundPercent = cancel.RefundPercent,
                    IsRefundable = cancel.IsRefundable
                }, _jsonOptions);

            case ChildrenPolicyCreateDTO children:
                return JsonSerializer.Serialize(new ChildrenAdditionalData
                {
                    MinAge = children.MinAge,
                    MaxAge = children.MaxAge,
                    ExtraBedFee = children.ExtraBedFee
                }, _jsonOptions);

            case PetPolicyCreateDTO pet:
                return JsonSerializer.Serialize(new PetAdditionalData
                {
                    PetFee = pet.PetFee,
                    IsPetAllowed = pet.IsPetAllowed
                }, _jsonOptions);

            default:
                return "{}";
        }
    }

    // ===========================================================================
    // 4. MAP UPDATE DTO → JSON STRING (Update DB)
    // ===========================================================================
    public static string MapToAdditionalJson(PolicyUpdateDTO dto)
    {
        switch (dto)
        {
            case CheckInOutPolicyUpdateDTO checkInOut:
                return JsonSerializer.Serialize(new CheckInOutAdditionalData
                {
                    CheckInTime = checkInOut.CheckInTime?.ToString("HH:mm"),
                    CheckOutTime = checkInOut.CheckOutTime?.ToString("HH:mm"),
                    EarlyCheckInFee = checkInOut.EarlyCheckInFee,
                    LateCheckOutFee = checkInOut.LateCheckOutFee
                }, _jsonOptions);

            case CancellationPolicyUpdateDTO cancel:
                return JsonSerializer.Serialize(new CancellationAdditionalData
                {
                    DaysBeforeCheckIn = cancel.DaysBeforeCheckIn,
                    RefundPercent = cancel.RefundPercent,
                    IsRefundable = cancel.IsRefundable
                }, _jsonOptions);

            case ChildrenPolicyUpdateDTO children:
                return JsonSerializer.Serialize(new ChildrenAdditionalData
                {
                    MinAge = children.MinAge,
                    MaxAge = children.MaxAge,
                    ExtraBedFee = children.ExtraBedFee
                }, _jsonOptions);

            case PetPolicyUpdateDTO pet:
                return JsonSerializer.Serialize(new PetAdditionalData
                {
                    PetFee = pet.PetFee,
                    IsPetAllowed = pet.IsPetAllowed
                }, _jsonOptions);

            default:
                return "{}";
        }
    }

    // ===========================================================================
    // PRIVATE HELPERS
    // ===========================================================================
    private static TimeOnly? ParseTimeOnly(string? timeStr)
    {
        if (string.IsNullOrEmpty(timeStr)) return null;
        return TimeOnly.TryParse(timeStr, out var result) ? result : null;
    }
}