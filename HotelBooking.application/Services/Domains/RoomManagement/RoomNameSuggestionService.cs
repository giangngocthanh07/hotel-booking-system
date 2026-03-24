using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Helpers;

namespace HotelBooking.application.Services.Domains.RoomManagement
{
    public interface IRoomNameSuggestionService
    {
        Task<ApiResponse<List<string>>> SuggestRoomNamesAsync(RoomNameSuggestionRequest request);
    }

    public class RoomNameSuggestionService : IRoomNameSuggestionService
    {
        private readonly IRoomAttributeFacade _attributeFacade;
        private readonly IValidator<RoomNameSuggestionRequest> _validator;


        public RoomNameSuggestionService(IRoomAttributeFacade attributeFacade, IValidator<RoomNameSuggestionRequest> validator)
        {
            _attributeFacade = attributeFacade;
            _validator = validator;
        }

        public async Task<ApiResponse<List<string>>> SuggestRoomNamesAsync(RoomNameSuggestionRequest request)
        {
            // --- STEP 1: VALIDATION INPUT ---
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return ResponseFactory.Failure<List<string>>(
                    StatusCodeResponse.BadRequest,
                    validationResult.Errors[0].ErrorMessage
                );
            }

            // --- STEP 2: CALL FACADE ---
            // The facade will internally call the necessary services to get attribute names based on the request
            var names = await _attributeFacade.GetRoomAttributeNamesAsync(request);
            var suggestions = new List<string>();

            // --- STEP 3: GENERATE SUGGESTIONS BASED ON ATTRIBUTES ---
            string unit = names.UnitTypeName; // e.g., "Double Room"
            string quality = names.QualityName ?? string.Empty; // e.g., "Deluxe"

            // BedConfigurationHelper
            string bed = BedConfigurationHelper.FormatBedConfiguration(names.BedTypeNames); // e.g., "with 1 King Bed"

            // Make sure UnitType is required
            if (string.IsNullOrEmpty(unit))
            {
                return ResponseFactory.Failure<List<string>>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_UNIT_TYPE_REQUIRED
                );
            }

            // ==========================================
            // GROUP 1: QUALITY FOCUS (3 templates)
            // ==========================================

            // Template A: [UnitType]
            suggestions.Add(unit);

            // Template B: [Quality] + [UnitType]
            if (!string.IsNullOrWhiteSpace(quality))
            {
                suggestions.Add($"{quality} {unit}");
            }

            // Template C: [Bed] + [UnitType]
            if (!string.IsNullOrWhiteSpace(bed))
            {
                suggestions.Add($"{bed} {unit}");
            }

            // ==========================================
            // GROUP 2: BED + QUALITY (2 templates)
            // ==========================================

            // Template D: [Quality] + [Bed] + [UnitType]
            if (!string.IsNullOrWhiteSpace(quality) && !string.IsNullOrWhiteSpace(bed))
            {
                suggestions.Add($"{quality} {bed} {unit}");
            }

            // Template E: [UnitType] + [View] + [Capacity]
            string view = names.RoomViewName ?? string.Empty; // e.g., "with Sea View"
            string capacityPart = CapacityHelper.FormatCapacity(request.AdultCapacity, request.ChildrenCapacity); // e.g., "2 Adults, 1 Child"
            if (!string.IsNullOrWhiteSpace(view) && !string.IsNullOrWhiteSpace(capacityPart))
            {
                suggestions.Add($"{unit} with {view} for {capacityPart}");
            }

            // ==========================================
            // GROUP 3: VIEW FOCUS (4 templates)
            // ==========================================
            if (!string.IsNullOrWhiteSpace(view))
            {
                // Template F: [UnitType] + [View]
                suggestions.Add($"{unit} with {view}");

                // Template G: [Quality] + [UnitType] + [View]
                if (!string.IsNullOrWhiteSpace(quality))
                {
                    suggestions.Add($"{quality} {unit} with {view}");
                }

                // Template H: [Bed] + [UnitType] + [View]
                if (!string.IsNullOrWhiteSpace(bed))
                {
                    suggestions.Add($"{bed} {unit} with {view}");
                }

                // Template I: [Quality] + [Bed] + [UnitType] + [View]
                if (!string.IsNullOrWhiteSpace(quality) && !string.IsNullOrWhiteSpace(bed))
                {
                    suggestions.Add($"{quality} {bed} {unit} with {view}");
                }
            }

            // ==========================================
            // GROUP 4: FEATURE FOCUS (12 templates - x3 variants)
            // ==========================================

            // Collect the features that are enabled (true)
            var activeFeatures = new List<string>();
            if (request.HasBalcony) activeFeatures.Add("Balcony");
            if (request.HasTerrace) activeFeatures.Add("Terrace");
            if (request.IsPrivateBathroom) activeFeatures.Add("Private Bathroom");

            // Loop through each active feature to generate templates J, K, L, M
            foreach (var feature in activeFeatures)
            {
                // Template J: [UnitType] + [Feature]
                suggestions.Add($"{unit} with {feature}");

                // Template K: [Quality] + [UnitType] + [Feature]
                if (!string.IsNullOrWhiteSpace(quality))
                {
                    suggestions.Add($"{quality} {unit} with {feature}");
                }

                // Template L: [Bed] + [UnitType] + [Feature]
                if (!string.IsNullOrWhiteSpace(bed))
                {
                    suggestions.Add($"{bed} {unit} with {feature}");
                }

                // Template M: [Quality] + [Bed] + [UnitType] + [Feature]
                if (!string.IsNullOrWhiteSpace(quality) && !string.IsNullOrWhiteSpace(bed))
                {
                    suggestions.Add($"{quality} {bed} {unit} with {feature}");
                }
            }

            // ==========================================
            // GROUP 5: CAPACITY FOCUS (4 templates)
            // ==========================================

            if (!string.IsNullOrWhiteSpace(capacityPart))
            {
                // Hardcode the "for [Capacity]" suffix since it will be common across all templates in this group
                string capSuffix = $"for {capacityPart}";

                // Template N: [UnitType] + [Capacity]
                suggestions.Add($"{unit} {capSuffix}");

                // Template O: [Quality] + [UnitType] + [Capacity]
                if (!string.IsNullOrWhiteSpace(quality))
                {
                    suggestions.Add($"{quality} {unit} {capSuffix}");
                }

                // Template P: [Bed] + [UnitType] + [Capacity]
                if (!string.IsNullOrWhiteSpace(bed))
                {
                    suggestions.Add($"{bed} {unit} {capSuffix}");
                }

                // Template Q: [Quality] + [Bed] + [UnitType] + [Capacity]
                if (!string.IsNullOrWhiteSpace(quality) && !string.IsNullOrWhiteSpace(bed))
                {
                    suggestions.Add($"{quality} {bed} {unit} {capSuffix}");
                }
            }

            // ==========================================
            // GROUP 6: FULL COMBO (10 templates - mix)
            // ==========================================

            if (!string.IsNullOrWhiteSpace(view))
            {
                // Template S: [Quality] + [Bed] + [UnitType] + [View]
                if (!string.IsNullOrWhiteSpace(quality) && !string.IsNullOrWhiteSpace(bed))
                {
                    suggestions.Add($"{quality} {bed} {unit} with {view}");
                }

                // Combine View and Feature (Use "and" according to design standards)
                foreach (var feature in activeFeatures)
                {
                    // Template R: [UnitType] + [View] + [Feature]
                    suggestions.Add($"{unit} with {view} and {feature}");

                    // Template U: [Quality] + [Bed] + [UnitType] + [View] + [Feature]
                    if (!string.IsNullOrWhiteSpace(quality) && !string.IsNullOrWhiteSpace(bed))
                    {
                        suggestions.Add($"{quality} {bed} {unit} with {view} and {feature}");
                    }
                }
            }

            // Template T: [Quality] + [Bed] + [UnitType] + [Feature]
            if (!string.IsNullOrWhiteSpace(quality) && !string.IsNullOrWhiteSpace(bed))
            {
                foreach (var feature in activeFeatures)
                {
                    suggestions.Add($"{quality} {bed} {unit} with {feature}");
                }
            }

            // --- STEP 4: RETURN ---
            // Remove duplicates and return
            return ResponseFactory.Success(suggestions.Distinct().ToList(), MessageResponse.Common.GET_SUCCESSFULLY);
        }
    }
}