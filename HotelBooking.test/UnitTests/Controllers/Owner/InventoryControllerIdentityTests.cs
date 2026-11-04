using System.Security.Claims;
using HotelBooking.api.Controllers.V1.Owner;
using HotelBooking.API.Controllers.V1.Owner;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Interfaces;
using HotelBooking.application.Helpers.Common;
using HotelBooking.application.Services.Domains.RoomManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelBooking.test.UnitTests.Controllers.Owner;

[Trait("Requirement", "FR-IAM-011")]
[Trait("Requirement", "FR-INV-013")]
// Direct controller tests cover claims parsing and service forwarding.
// Authentication and role middleware require separate HTTP integration tests.
public class InventoryControllerIdentityTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2147483648")] // Reject values outside the Int32 range as well as malformed IDs.
    public async Task CreateRoomType_InvalidIdentity_ReturnsUnauthorized(string? claim)
    {
        var service = new Mock<IRoomTypeService>(MockBehavior.Strict);
        var controller = new RoomTypeController(Mock.Of<IRoomNameSuggestionService>(), service.Object);
        SetIdentity(controller, claim);

        var result = await controller.CreateRoomType(new RoomTypeCreateDTO());

        Assert.IsType<UnauthorizedResult>(result);
        // Invalid claims must never reach the application service.
        service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2147483648")]
    public async Task BatchAddRooms_InvalidIdentity_ReturnsUnauthorized(string? claim)
    {
        var service = new Mock<IRoomService>(MockBehavior.Strict);
        var controller = new RoomController(service.Object);
        SetIdentity(controller, claim);

        var result = await controller.BatchAddRooms(new BatchAddRoomsRequestDTO());

        Assert.IsType<UnauthorizedResult>(result);
        service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2147483648")]
    public async Task GetRooms_InvalidIdentity_ReturnsUnauthorized(string? claim)
    {
        var service = new Mock<IRoomService>(MockBehavior.Strict);
        var controller = new RoomController(service.Object);
        SetIdentity(controller, claim);

        var result = await controller.GetRoomsByRoomType(10);

        Assert.IsType<UnauthorizedResult>(result);
        service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(StatusCodeResponse.Success, StatusCodes.Status200OK)]
    [InlineData(StatusCodeResponse.Forbidden, StatusCodes.Status403Forbidden)]
    public async Task CreateRoomType_ForwardsTrustedIdentityAndCancellation(string status, int httpStatus)
    {
        using var cancellation = new CancellationTokenSource();
        var service = new Mock<IRoomTypeService>(MockBehavior.Strict);
        var request = new RoomTypeCreateDTO { HotelId = 100 };
        var response = new ApiResponse<RoomTypeResponseDTO> { StatusCode = status };
        service.Setup(s => s.CreateRoomTypeAsync(request, 42, cancellation.Token)).ReturnsAsync(response);
        var controller = new RoomTypeController(Mock.Of<IRoomNameSuggestionService>(), service.Object);
        // The acting owner comes from claims; HotelId 100 identifies only the target resource.
        SetIdentity(controller, "42");

        var result = await controller.CreateRoomType(request, cancellation.Token);

        AssertResponse(result, httpStatus, response);
        service.Verify(s => s.CreateRoomTypeAsync(request, 42, cancellation.Token), Times.Once);
        service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(StatusCodeResponse.Success, StatusCodes.Status200OK)]
    [InlineData(StatusCodeResponse.Forbidden, StatusCodes.Status403Forbidden)]
    public async Task BatchAddRooms_ForwardsTrustedIdentityAndCancellation(string status, int httpStatus)
    {
        using var cancellation = new CancellationTokenSource();
        var service = new Mock<IRoomService>(MockBehavior.Strict);
        var request = new BatchAddRoomsRequestDTO { HotelId = 100, RoomTypeId = 10 };
        var response = new ApiResponse<IEnumerable<RoomResponseDTO>> { StatusCode = status };
        service.Setup(s => s.BatchAddRoomsAsync(request, 42, cancellation.Token)).ReturnsAsync(response);
        var controller = new RoomController(service.Object);
        SetIdentity(controller, "42");

        var result = await controller.BatchAddRooms(request, cancellation.Token);

        AssertResponse(result, httpStatus, response);
        service.Verify(s => s.BatchAddRoomsAsync(request, 42, cancellation.Token), Times.Once);
        service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(StatusCodeResponse.Success, StatusCodes.Status200OK)]
    [InlineData(StatusCodeResponse.Forbidden, StatusCodes.Status403Forbidden)]
    public async Task GetRooms_ForwardsTrustedIdentityAndCancellation(string status, int httpStatus)
    {
        using var cancellation = new CancellationTokenSource();
        var service = new Mock<IRoomService>(MockBehavior.Strict);
        var response = new ApiResponse<IEnumerable<RoomResponseDTO>> { StatusCode = status };
        service.Setup(s => s.GetRoomsByRoomTypeAsync(10, 42, cancellation.Token)).ReturnsAsync(response);
        var controller = new RoomController(service.Object);
        SetIdentity(controller, "42");

        var result = await controller.GetRoomsByRoomType(10, cancellation.Token);

        AssertResponse(result, httpStatus, response);
        service.Verify(s => s.GetRoomsByRoomTypeAsync(10, 42, cancellation.Token), Times.Once);
        service.VerifyNoOtherCalls();
    }

    private static void AssertResponse<T>(IActionResult result, int expectedStatus, ApiResponse<T> response)
    {
        // Preserve the application envelope while mapping its outcome to the HTTP status.
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatus, objectResult.StatusCode);
        Assert.Same(response, objectResult.Value);
    }

    private static void SetIdentity(ControllerBase controller, string? claim)
    {
        // Build a test principal without issuing a real JWT or running authentication middleware.
        var claims = claim == null ? Array.Empty<Claim>() : new[] { new Claim(ClaimTypes.NameIdentifier, claim) };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            }
        };
    }
}
