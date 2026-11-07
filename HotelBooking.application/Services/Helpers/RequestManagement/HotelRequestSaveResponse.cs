// FR-ONB-019: give every competing hotel-request action the same predictable conflict response.
public static class HotelRequestSaveResponse
{
    public static ApiResponse<bool> Conflict() =>
        ResponseFactory.Failure<bool>(StatusCodeResponse.Conflict,
            "This hotel registration request was changed by another action. Reload it before continuing.");
}
