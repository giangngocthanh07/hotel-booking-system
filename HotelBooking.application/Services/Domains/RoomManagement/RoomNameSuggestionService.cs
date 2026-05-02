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
        private readonly ILogger _logger;


        private const string WITH = "with";
        private const string FOR = "for";

        private const string FEATURE_BALCONY = "Balcony";
        private const string FEATURE_TERRACE = "Terrace";
        private const string FEATURE_BATHROOM = "Private Bathroom";


        public RoomNameSuggestionService(IRoomAttributeFacade attributeFacade, IValidator<RoomNameSuggestionRequest> validator, ILogger logger)
        {
            _attributeFacade = attributeFacade;
            _validator = validator;
            _logger = logger;
        }

        public async Task<ApiResponse<List<string>>> SuggestRoomNamesAsync(RoomNameSuggestionRequest request)
        {
            // ***** GUARD CLAUSE: NULL REQUEST *****
            if (request == null)
            {
                return ResponseFactory.Failure<List<string>>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.Common.REQUEST_CANNOT_BE_NULL
                );
            }

            // --- STEP 1: VALIDATION INPUT ---
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return ResponseFactory.Failure<List<string>>(
                    StatusCodeResponse.BadRequest,
                    validationResult.Errors[0].ErrorMessage
                );
            }

            // 1b) BUSINESS LOGIC VALIDATION
            var existingUnitType = await _attributeFacade.IsUnitTypeExistedAsync(request.UnitTypeId);
            if (!existingUnitType)
            {
                return ResponseFactory.Failure<List<string>>(StatusCodeResponse.NotFound, MessageResponse.RoomManagement.ROOM_TYPE_UNIT_TYPE_NOT_FOUND);
            }

            if (request.BedTypes != null)
            {
                foreach (var bedType in request.BedTypes)
                {
                    var existingBedType = await _attributeFacade.IsBedTypeExistedAsync(bedType.BedTypeId);
                    if (!existingBedType)
                    {
                        return ResponseFactory.Failure<List<string>>(StatusCodeResponse.NotFound, MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPE_NOT_FOUND);
                    }
                }
            }

            if (request.QualityId.HasValue)
            {
                var existingQuality = await _attributeFacade.IsRoomQualityExistedAsync(request.QualityId.Value);
                if (!existingQuality)
                {
                    return ResponseFactory.Failure<List<string>>(StatusCodeResponse.NotFound, MessageResponse.RoomManagement.ROOM_TYPE_QUALITY_NOT_FOUND);
                }
            }

            if (request.RoomViewId.HasValue)
            {
                var existingView = await _attributeFacade.IsRoomViewExistedAsync(request.RoomViewId.Value);
                if (!existingView)
                {
                    return ResponseFactory.Failure<List<string>>(StatusCodeResponse.NotFound, MessageResponse.RoomManagement.ROOM_TYPE_ROOM_VIEW_NOT_FOUND);
                }

            }

            // --- STEP 2: CALL FACADE ---
            // The facade will internally call the necessary services to get attribute names based on the request
            var names = await _attributeFacade.GetRoomAttributeNamesAsync(request);

            // --- STEP 3: GENERATE SUGGESTIONS BASED ON ATTRIBUTES ---
            string unit = names.UnitTypeName; // e.g., "Double Room"

            // ----- BedConfigurationHelper
            string bed = BedConfigurationHelper.FormatBedConfiguration(names.BedTypeNames); // e.g., "with 1 King Bed"

            // ***** GUARD CLAUSES FOR REQUIRED ATTRIBUTES *****
            // ----- Make sure UnitType is required
            if (string.IsNullOrWhiteSpace(unit))
            {
                return ResponseFactory.Failure<List<string>>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_UNIT_TYPE_REQUIRED
                );
            }

            // ----- Make sure Bedtype is required
            if (string.IsNullOrWhiteSpace(bed))
            {
                return ResponseFactory.Failure<List<string>>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_BED_TYPES_REQUIRED
                );
            }

            // ----- Nullable attributes should be treated as empty strings to avoid "null" appearing in suggestions ----- //
            string quality = names.QualityName ?? string.Empty; // e.g., "Deluxe"
            string view = names.RoomViewName ?? string.Empty; // e.g., "with Sea View"
                                                              // ------------------------------------------

            // ----- CapacityHelper
            string capacityPart = CapacityHelper.FormatCapacity(request.AdultCapacity, request.ChildrenCapacity); // e.g., "2 Adults, 1 Child"

            if (string.IsNullOrWhiteSpace(capacityPart))
            {
                return ResponseFactory.Failure<List<string>>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_CAPACITY_REQUIRED
                );
            }

            // ----- Collect the features that are enabled (true) -----
            var activeFeatures = new List<string>();
            if (request.HasBalcony) activeFeatures.Add(FEATURE_BALCONY);
            if (request.HasTerrace) activeFeatures.Add(FEATURE_TERRACE);
            if (request.IsPrivateBathroom) activeFeatures.Add(FEATURE_BATHROOM);


            var suggestions = new List<string>();

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
            suggestions.Add($"{bed} {unit}");


            // ==========================================
            // GROUP 2: BED + QUALITY (2 templates)
            // ==========================================

            // Template D: [Quality] + [Bed] + [UnitType]
            if (!string.IsNullOrWhiteSpace(quality))
            {
                suggestions.Add($"{quality} {bed} {unit}");
            }

            // Template E: [UnitType] + [View] + [Capacity]
            if (!string.IsNullOrWhiteSpace(view))
            {
                suggestions.Add($"{unit} {WITH} {view} {FOR} {capacityPart}");
            }

            // ==========================================
            // GROUP 3: VIEW FOCUS (4 templates)
            // ==========================================
            if (!string.IsNullOrWhiteSpace(view))
            {
                // Template F: [UnitType] + [View]
                suggestions.Add($"{unit} {WITH} {view}");

                // Template G: [Quality] + [UnitType] + [View]
                if (!string.IsNullOrWhiteSpace(quality))
                {
                    suggestions.Add($"{quality} {unit} {WITH} {view}");
                }

                // Template H: [Bed] + [UnitType] + [View]

                suggestions.Add($"{bed} {unit} {WITH} {view}");


                // Template I: [Quality] + [Bed] + [UnitType] + [View]
                if (!string.IsNullOrWhiteSpace(quality))
                {
                    suggestions.Add($"{quality} {bed} {unit} {WITH} {view}");
                }
            }

            // ==========================================
            // GROUP 4: FEATURE FOCUS (12 templates - x3 variants)
            // ==========================================


            // Loop through each active feature to generate templates J, K, L, M
            foreach (var feature in activeFeatures)
            {
                // Template J: [UnitType] + [Feature]
                suggestions.Add($"{unit} {WITH} {feature}");

                // Template K: [Quality] + [UnitType] + [Feature]
                if (!string.IsNullOrWhiteSpace(quality))
                {
                    suggestions.Add($"{quality} {unit} {WITH} {feature}");
                }

                // Template L: [Bed] + [UnitType] + [Feature]
                suggestions.Add($"{bed} {unit} {WITH} {feature}");

                // Template M: [Quality] + [Bed] + [UnitType] + [Feature]
                if (!string.IsNullOrWhiteSpace(quality))
                {
                    suggestions.Add($"{quality} {bed} {unit} {WITH} {feature}");
                }
            }

            // ==========================================
            // GROUP 5: CAPACITY FOCUS (4 templates)
            // ==========================================

            // Template N: [UnitType] + [Capacity]
            suggestions.Add($"{unit} {FOR} {capacityPart}");

            // Template O: [Quality] + [UnitType] + [Capacity]
            if (!string.IsNullOrWhiteSpace(quality))
            {
                suggestions.Add($"{quality} {unit} {FOR} {capacityPart}");
            }

            // Template P: [Bed] + [UnitType] + [Capacity]
            suggestions.Add($"{bed} {unit} {FOR} {capacityPart}");


            // Template Q: [Quality] + [Bed] + [UnitType] + [Capacity]
            if (!string.IsNullOrWhiteSpace(quality))
            {
                suggestions.Add($"{quality} {bed} {unit} {FOR} {capacityPart}");
            }

            // ==========================================
            // GROUP 6: FULL COMBO (10 templates - mix)
            // ==========================================

            if (!string.IsNullOrWhiteSpace(view))
            {
                // Template S: [Quality] + [Bed] + [UnitType] + [View]
                if (!string.IsNullOrWhiteSpace(quality))
                {
                    suggestions.Add($"{quality} {bed} {unit} {WITH} {view}");
                }

                // Combine View and Feature (Use "and" according to design standards)
                foreach (var feature in activeFeatures)
                {
                    // Template R: [UnitType] + [View] + [Feature]
                    suggestions.Add($"{unit} {WITH} {view} and {feature}");

                    // Template U: [Quality] + [Bed] + [UnitType] + [View] + [Feature]
                    if (!string.IsNullOrWhiteSpace(quality))
                    {
                        suggestions.Add($"{quality} {bed} {unit} {WITH} {view} and {feature}");
                    }
                }
            }

            // Template T: [Quality] + [Bed] + [UnitType] + [Feature]
            if (!string.IsNullOrWhiteSpace(quality))
            {
                foreach (var feature in activeFeatures)
                {
                    suggestions.Add($"{quality} {bed} {unit} {WITH} {feature}");
                }
            }

            // --- STEP 4: RETURN ---
            // Remove duplicates, whitespace, and return
            var finalSuggestions = suggestions.Select(s => s.Trim()).Distinct().ToList();

            return ResponseFactory.Success(finalSuggestions, MessageResponse.Common.GET_SUCCESSFULLY);
        }
    }
}