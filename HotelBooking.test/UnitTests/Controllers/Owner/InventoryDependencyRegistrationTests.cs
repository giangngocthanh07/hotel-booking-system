using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Interfaces;
using HotelBooking.application.Services.Domains.RoomManagement;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HotelBooking.test.UnitTests.Controllers.Owner;

[Trait("Requirement", "FR-INV-013")]
// Catches missing DI registrations introduced by the new authorization dependencies.
public class InventoryDependencyRegistrationTests
{
    [Fact]
    public async Task InventoryServices_ResolveWithProductionRegistrations()
    {
        var services = CreateServices();
        await using var provider = services.BuildServiceProvider();
        // IUnitOfWork supports asynchronous disposal, matching the request-scope lifecycle.
        await using var scope = provider.CreateAsyncScope();

        Assert.IsType<RoomService>(scope.ServiceProvider.GetRequiredService<IRoomService>());
        Assert.IsType<RoomTypeService>(scope.ServiceProvider.GetRequiredService<IRoomTypeService>());
        Assert.IsType<InventoryAccessService>(scope.ServiceProvider.GetRequiredService<IInventoryAccessService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IValidator<BatchAddRoomsRequestDTO>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IValidator<int>>());
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        // Use production registrations so the test validates the actual constructor graph.
        services.AddApplicationServices();
        services.AddAppValidators();
        // Replace external collaborators to resolve services without a running database.
        services.AddScoped(_ => Mock.Of<IHotelRepository>());
        services.AddScoped(_ => Mock.Of<IRoomRepository>());
        services.AddScoped(_ => Mock.Of<IRoomTypeRepository>());
        services.AddScoped(_ => Mock.Of<IUnitOfWork>());
        services.AddScoped(_ => Mock.Of<IRoomAttributeFacade>());
        return services;
    }
}
