using System.Text.Json;
using System.Text.Json.Nodes;
using HotelBooking.application.DTOs.Hotel.Owner;

namespace HotelBooking.application.Services.Helpers.HotelManagement.Owner;

public static class OwnerHotelInformationMetadata
{
    public static bool TryMerge(string? original, OwnerHotelUpdateDTO input, out string updated)
    {
        updated = string.Empty;
        try
        {
            var root = string.IsNullOrWhiteSpace(original) ? new JsonObject() : JsonNode.Parse(original);
            if (root != null && root is not JsonObject) return false;
            var document = root as JsonObject ?? new JsonObject();
            Set(document, "PublicPhone", JsonValue.Create(input.PublicPhone.Trim()));
            Set(document, "PublicEmail", JsonValue.Create(input.PublicEmail.Trim()));
            Set(document, "Latitude", JsonValue.Create(input.Latitude));
            Set(document, "Longitude", JsonValue.Create(input.Longitude));
            updated = document.ToJsonString();
            return true;
        }
        catch (JsonException) { return false; }
        catch (ArgumentException) { return false; }
    }

    private static void Set(JsonObject document, string name, JsonNode? value)
    {
        // FR-CAT-002: preserve unrelated legal/future fields and avoid duplicate case variants of editable keys.
        var keys = document.Select(pair => pair.Key).Where(key => key.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
        foreach (var key in keys) document.Remove(key);
        document[name] = value;
    }
}
