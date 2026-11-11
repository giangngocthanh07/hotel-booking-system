using HotelBooking.application.Services.Domains.HotelManagement.Owner;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HotelBooking.test.UnitTests.DependencyInjection.HotelManagement.Owner;

[Trait("Requirement", "FR-CAT-002")]
public class OwnerHotelDependencyTests
{
    [Fact]
    public void UpdateService_ResolvesItsValidatorAndPersistenceDependencies()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(Mock.Of<IHotelRepository>());
        services.AddSingleton(Mock.Of<IUnitOfWork>());
        services.AddApplicationServices();
        services.AddAppValidators();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<OwnerHotelUpdateService>(scope.ServiceProvider.GetRequiredService<IOwnerHotelUpdateService>());
    }
}
