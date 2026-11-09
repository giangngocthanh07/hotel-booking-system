using FluentAssertions;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.RequestManagement.Owner;

public partial class HotelRegistrationServiceTests
{
    [Fact]
    [Trait("Requirement", "FR-ONB-013")]
    public async Task OwnUpload_DerivesUrlAndLinkFromRegistryInsteadOfClient()
    {
        MockValidationSuccess();
        var input = CreateValidRequest();
        input.BusinessLicenseUrl = "https://attacker.example/foreign-license.pdf";
        HotelApprovalRequest? stored = null;
        _mockApprovalRepo.Setup(repo => repo.AddAsync(It.IsAny<HotelApprovalRequest>()))
            .Callback<HotelApprovalRequest>(request => stored = request).Returns(Task.CompletedTask);
        var result = await _service.CreateRequestAsync(1, input);
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        Assert.NotNull(stored);
        Assert.Equal(input.BusinessLicenseUploadId, stored.BusinessLicenseUploadId);
        Assert.Equal("https://example.com/license.pdf", stored.BusinessLicenseUrl);
        Assert.Equal(stored.BusinessLicenseUrl, result.Content!.BusinessLicenseUrl);
    }

    [Fact]
    [Trait("Requirement", "FR-ONB-013")]
    public async Task UnavailableUpload_DoesNotCreateRequestOrStartTransaction()
    {
        MockValidationSuccess();
        _mockUploads.Setup(repo => repo.GetOwnedActiveAsync(It.IsAny<Guid>(), 1, default)).ReturnsAsync((BusinessLicenseUpload?)null);
        var result = await _service.CreateRequestAsync(1, CreateValidRequest());
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        _mockApprovalRepo.Verify(repo => repo.AddAsync(It.IsAny<HotelApprovalRequest>()), Times.Never);
        _mockUnitOfWork.Verify(unit => unit.BeginTransactionAsync(), Times.Never);
        VerifyRegistrationSaved(0);
    }

    [Fact]
    [Trait("Requirement", "FR-ONB-013")]
    public async Task OwnershipLookupCancellation_PropagatesWithoutRollbackOrSave()
    {
        using var cancellation = new CancellationTokenSource();
        _mockValidator.Setup(validator => validator.ValidateAsync(It.IsAny<HotelBooking.application.DTOs.Hotel.HotelRegistrationDTO>(), cancellation.Token))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockUploads.Setup(repo => repo.GetOwnedActiveAsync(It.IsAny<Guid>(), 1, cancellation.Token))
            .ThrowsAsync(new OperationCanceledException(cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.CreateRequestAsync(1, CreateValidRequest(), cancellation.Token));
        _mockUnitOfWork.Verify(unit => unit.RollBackTransactionAsync(), Times.Never);
        VerifyRegistrationSaved(0);
    }

    [Fact]
    [Trait("Requirement", "FR-ONB-013")]
    public async Task RegistrationSaveCancellation_RollsBackWithoutCommit()
    {
        using var cancellation = new CancellationTokenSource();
        _mockValidator.Setup(validator => validator.ValidateAsync(It.IsAny<HotelBooking.application.DTOs.Hotel.HotelRegistrationDTO>(), cancellation.Token))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockUnitOfWork.Setup(unit => unit.TrySaveChangesAsync(cancellation.Token))
            .ThrowsAsync(new OperationCanceledException(cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.CreateRequestAsync(1, CreateValidRequest(), cancellation.Token));
        _mockUnitOfWork.Verify(unit => unit.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(unit => unit.RollBackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(unit => unit.CommitTransactionAsync(), Times.Never);
        VerifyRegistrationSaved(1);
    }
}
