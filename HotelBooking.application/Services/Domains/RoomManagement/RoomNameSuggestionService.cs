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
            string quality = names.QualityName!; // e.g., "Deluxe"

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

            // Remove duplicates and return
            return ResponseFactory.Success(suggestions.Distinct().ToList(), MessageResponse.Common.GET_SUCCESSFULLY);
        }
    }
}