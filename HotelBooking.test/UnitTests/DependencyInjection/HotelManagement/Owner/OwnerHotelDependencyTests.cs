using HotelBooking.application.Services.Domains.HotelManagement.Owner;
using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using HotelBooking.application.DTOs.Hotel.Owner.Images;
using HotelBooking.application.Validators.HotelManagement.Owner.Images;
using Microsoft.Extensions.Configuration;
using FluentValidation;

namespace HotelBooking.test.UnitTests.DependencyInjection.HotelManagement.Owner;

[Trait("Requirement", "FR-CAT-002")]
public class OwnerHotelDependencyTests
{
    [Fact]
    [Trait("Requirement", "FR-MED-003")]
    public void ImageUpload_ResolvesPhotoProviderAndSeparateImageValidator()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(Mock.Of<IHotelImageRepository>());
        services.AddSingleton(Mock.Of<IUnitOfWork>());
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["Cloudinary:CloudName"] = "test-cloud", ["Cloudinary:ApiKey"] = "test-key", ["Cloudinary:ApiSecret"] = "test-secret" }).Build());
        services.AddApplicationServices();
        services.AddAppValidators();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<OwnerHotelImageUploadService>(scope.ServiceProvider.GetRequiredService<IOwnerHotelImageUploadService>());
        Assert.IsType<OwnerHotelImageUploadValidator>(scope.ServiceProvider.GetRequiredService<IValidator<OwnerHotelImageFileDTO>>());
    }
    [Fact]
    public void UpdateService_ResolvesItsValidatorAndPersistenceDependencies()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(Mock.Of<IHotelRepository>());
        services.AddSingleton(Mock.Of<IHotelImageRepository>());
        services.AddSingleton(Mock.Of<IUnitOfWork>());
        services.AddApplicationServices();
        services.AddAppValidators();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<OwnerHotelUpdateService>(scope.ServiceProvider.GetRequiredService<IOwnerHotelUpdateService>());
        Assert.IsType<OwnerHotelImageReadService>(scope.ServiceProvider.GetRequiredService<IOwnerHotelImageReadService>());
    }
}
