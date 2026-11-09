using System.Text.Json;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement.Owner;
using HotelBooking.application.Validators.RequestManagement.Owner;
using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace HotelBooking.Tests.Integration.RequestManagement;

[Trait("Requirement", "FR-ONB-009")]
[Trait("Requirement", "FR-ONB-013")]
[Trait("Category", "SqlIntegration")]
public class HotelRegistrationOwnershipSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Fact]
    public async Task OwnUpload_PersistsLinkAndTrustedUrl_AndCanBeReused()
    {
        var (seed, input) = await SeedInputAsync("owned");
        await using var context = fixture.CreateContext();
        var service = Service(context);
        Assert.Equal(StatusCodeResponse.Success, (await service.CreateRequestAsync(seed.OwnerId, input)).StatusCode);
        input.Name = "Second " + seed.Name;
        Assert.Equal(StatusCodeResponse.Success, (await service.CreateRequestAsync(seed.OwnerId, input)).StatusCode);
        await using var verify = fixture.CreateContext();
        var stored = await verify.HotelApprovalRequests.Where(row => row.BusinessLicenseUploadId == input.BusinessLicenseUploadId).ToListAsync();
        Assert.Equal(2, stored.Count);
        Assert.All(stored, row =>
        {
            Assert.Equal(seed.OwnerId, row.OwnerId);
            Assert.Equal("https://example.com/owned-license.pdf", row.BusinessLicenseUrl);
            Assert.Equal("Pending", row.Status);
        });
        Assert.False(await verify.Hotels.AnyAsync(hotel => hotel.OwnerId == seed.OwnerId));
    }

    [Theory]
    [InlineData("foreign")]
    [InlineData("deleted")]
    [InlineData("unknown")]
    [InlineData("missing")]
    [InlineData("empty")]
    public async Task UnavailableReceipt_DoesNotCreateRequestOrHotel(string scenario)
    {
        var (seed, input) = await SeedInputAsync(scenario);
        await using var context = fixture.CreateContext();
        var response = await Service(context).CreateRequestAsync(seed.OwnerId, input);
        Assert.Equal(StatusCodeResponse.BadRequest, response.StatusCode);
        if (scenario is "foreign" or "deleted" or "unknown")
            Assert.Equal("Select a business license uploaded by your account.", response.Message);
        await using var verify = fixture.CreateContext();
        Assert.False(await verify.HotelApprovalRequests.AnyAsync(row => row.Name == input.Name));
        Assert.False(await verify.Hotels.AnyAsync(hotel => hotel.OwnerId == seed.OwnerId));
    }

    private async Task<(HotelRequestSeed, HotelRegistrationDTO)> SeedInputAsync(string scenario)
    {
        var seed = await fixture.SeedAsync();
        await using var context = fixture.CreateContext();
        var source = await context.HotelApprovalRequests.SingleAsync(row => row.Id == seed.RequestId);
        var info = JsonSerializer.Deserialize<HotelAdditionalInfo>(source.Additional!)!;
        var upload = new BusinessLicenseUpload
        {
            Id = Guid.NewGuid(), OwnerId = scenario == "foreign" ? seed.OtherAdminId : seed.OwnerId,
            PublicId = $"test/{Guid.NewGuid():N}", ResourceType = "image", DeliveryType = "upload",
            StoredUrl = "https://example.com/owned-license.pdf", OriginalFileName = "license.pdf",
            ContentType = "application/pdf", SizeBytes = 8, CreatedAt = DateTime.UtcNow, IsDeleted = scenario == "deleted",
        };
        context.BusinessLicenseUploads.Add(upload);
        await context.SaveChangesAsync();
        Guid? receipt = scenario switch { "unknown" => Guid.NewGuid(), "missing" => null, "empty" => Guid.Empty, _ => upload.Id };
        return (seed, Input(seed.Name, info, receipt));
    }

    private static HotelRegistrationDTO Input(string name, HotelAdditionalInfo info, Guid? receipt) => new()
    {
        Name = "New " + name, Address = "123 Integration Street", PublicPhone = "0901234567", PublicEmail = "owner@example.com",
        PropertyTypeId = info.PropType.Id, CountryId = info.Country.Id, ProvinceId = info.Province.Id, WardId = info.Ward.Id,
        TaxCode = "1234567890", BusinessLicenseUploadId = receipt, BusinessLicenseUrl = "https://attacker.example/file.pdf",
    };

    private static HotelRegistrationService Service(HotelBookingDBContext context)
    {
        var token = new HttpCancellationTokenProvider(new HttpContextAccessor());
        return new HotelRegistrationService(new HotelApprovalRequestRepository(context, token), new HotelRepository(context, token),
            new PropertyTypeRepository(context, token), new ProvinceRepository(context, token), new WardRepository(context, token),
            new HotelRegistrationValidator(), new UnitOfWork(context), NullLogger<HotelRegistrationService>.Instance,
            new UserRepository(context, token), new BusinessLicenseUploadRepository(context));
    }
}
