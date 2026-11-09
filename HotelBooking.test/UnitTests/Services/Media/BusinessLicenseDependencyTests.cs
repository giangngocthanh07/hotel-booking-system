using FluentValidation;
using HotelBooking.application.Services.Domains.Media;
using HotelBooking.application.Validators.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.test.UnitTests.Services.Media;

[Trait("Requirement", "FR-ONB-011")]
public class BusinessLicenseDependencyTests
{
    [Fact]
    public void UploadService_ResolvesValidatorAndProviderWithoutNetworkAccess()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Cloudinary:CloudName"] = "test-cloud", ["Cloudinary:ApiKey"] = "test-key", ["Cloudinary:ApiSecret"] = "test-secret",
        }).Build();
        var services = new ServiceCollection().AddLogging().AddSingleton<IConfiguration>(configuration);
        services.AddApplicationServices();
        services.AddAppValidators();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<BusinessLicenseUploadValidator>(scope.ServiceProvider.GetRequiredService<IValidator<UploadFileDTO>>());
        Assert.IsType<BusinessLicenseUploader>(scope.ServiceProvider.GetRequiredService<IBusinessLicenseUploader>());
        Assert.IsType<FileService>(scope.ServiceProvider.GetRequiredService<IFileService>());
    }
}
