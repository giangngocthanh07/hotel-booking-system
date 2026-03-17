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
            if (request is null)
                return ResponseFactory.Failure<List<string>>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_REQUEST_NULL // thêm message này
                );

            

            // Implementation will be added in the next steps, focusing on generating room name suggestions based on the provided attributes
            throw new NotImplementedException();
        }
    }
}