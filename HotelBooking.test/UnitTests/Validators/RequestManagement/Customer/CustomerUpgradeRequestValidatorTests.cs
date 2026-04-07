
using FluentAssertions;
using FluentValidation.TestHelper;
using HotelBooking.application.DTOs.Request.UpgradeRequest;
using HotelBooking.application.Validators.RequestManagement.Customer;

namespace HotelBooking.test.UnitTests.Validators.RequestManagement.Customer;

public class CustomerUpgradeRequestValidatorTests
{
    [Fact]
    public void CreateUpgradeRequestValidator_EmptyAddress_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateUpgradeRequestValidator();
        var request = new CreateUpgradeRequestDTO
        {
            Address = "", // Empty address
            TaxCode = "1234567890" // Valid tax code
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.ADDRESS_REQUIRED);
    }

    [Fact]
    public void CreateUpgradeRequestValidator_AddressTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateUpgradeRequestValidator();
        var request = new CreateUpgradeRequestDTO
        {
            Address = new string('A', 501), // Address exceeding 500 characters
            TaxCode = "1234567890" // Valid tax code
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.ADDRESS_TOO_LONG);
    }

    [Fact]
    public void CreateUpgradeRequestValidator_InvalidTaxCode_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateUpgradeRequestValidator();
        var request = new CreateUpgradeRequestDTO
        {
            Address = "123 Main St", // Valid address
            TaxCode = "ABC123" // Invalid tax code
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.TAX_CODE_INVALID);
    }

    [Fact]
    public void CreateUpgradeRequestValidator_EmptyTaxCode_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateUpgradeRequestValidator();
        var request = new CreateUpgradeRequestDTO
        {
            Address = "123 Main St", // Valid address
            TaxCode = "" // Empty tax code
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(MessageResponse.RequestManagement.UpgradeRequest.TAX_CODE_REQUIRED);
    }
}
