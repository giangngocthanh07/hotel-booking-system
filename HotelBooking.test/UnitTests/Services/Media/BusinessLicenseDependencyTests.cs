using FluentValidation;
using HotelBooking.application.Services.Domains.Media;
using HotelBooking.application.Validators.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HotelBooking.infrastructure.Models;
using HotelBooking.Tests.Integration;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.test.UnitTests.Services.Media;

[Trait("Requirement", "FR-ONB-011")]
public class BusinessLicenseDependencyTests
{
    [Fact]
    public async Task UploadService_ResolvesValidatorAndProviderWithoutNetworkAccess()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Cloudinary:CloudName"] = "test-cloud", ["Cloudinary:ApiKey"] = "test-key", ["Cloudinary:ApiSecret"] = "test-secret",
        }).Build();
        var services = new ServiceCollection().AddLogging().AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<HotelBookingDBContext>(new TestHotelBookingDBContext(new DbContextOptionsBuilder<HotelBookingDBContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options));
        services.AddAppRepositories();
        services.AddApplicationServices();
        services.AddAppValidators();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        Assert.IsType<BusinessLicenseUploadValidator>(scope.ServiceProvider.GetRequiredService<IValidator<UploadFileDTO>>());
        Assert.IsType<BusinessLicenseUploader>(scope.ServiceProvider.GetRequiredService<IBusinessLicenseUploader>());
        Assert.IsType<BusinessLicenseDownloader>(scope.ServiceProvider.GetRequiredService<IBusinessLicenseDownloader>());
        Assert.IsType<BusinessLicenseAccessService>(scope.ServiceProvider.GetRequiredService<IBusinessLicenseAccessService>());
        Assert.IsType<FileService>(scope.ServiceProvider.GetRequiredService<IFileService>());
        Assert.IsType<BusinessLicenseUploadRepository>(scope.ServiceProvider.GetRequiredService<IBusinessLicenseUploadRepository>());
    }
}
