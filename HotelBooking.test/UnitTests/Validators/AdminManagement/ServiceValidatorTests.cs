using FluentValidation.TestHelper;
using HotelBooking.application.Validators.AdminManagement.Services;

namespace HotelBooking.test.UnitTests.Validators.AdminManagement;

// =========================================================================
// STANDARD SERVICE VALIDATOR TESTS
// =========================================================================

public class ServiceStandardCreateValidatorTests
{
    private readonly ServiceStandardCreateValidator _validator;

    public ServiceStandardCreateValidatorTests()
    {
        _validator = new ServiceStandardCreateValidator();
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var dto = new ServiceStandardCreateDTO
        {
            Price = 50000,
            Unit = "kg"
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Create_EmptyUnit_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new ServiceStandardCreateDTO
        {
            Price = 50000,
            Unit = ""
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Unit)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.EMPTY_UNIT_NAME);
    }

    [Fact]
    public async Task Create_LongUnit_ReturnsBadRequest()
    {
        // 1. Arrange — MaximumLength(20)
        var dto = new ServiceStandardCreateDTO
        {
            Price = 50000,
            Unit = new string('A', 21)
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Unit)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.LONG_UNIT);
    }

    [Fact]
    public async Task Create_PriceBelowMinimum_ReturnsBadRequest()
    {
        // 1. Arrange — MIN_PRICE = 10000, any value below it and above 0 is invalid
        var dto = new ServiceStandardCreateDTO
        {
            Price = 9999,
            Unit = "kg"
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.STANDARD_SERVICE_PRICE_GREATER_THAN_ZERO);
    }

    [Fact]
    public async Task Create_Price_AtMinimum_ReturnsSuccess()
    {
        // 1. Arrange — exactly 10000 is valid
        var dto = new ServiceStandardCreateDTO
        {
            Price = ServiceValidator.MIN_PRICE,
            Unit = "kg"
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }
}

public class ServiceStandardUpdateValidatorTests
{
    private readonly ServiceStandardUpdateValidator _validator;

    public ServiceStandardUpdateValidatorTests()
    {
        _validator = new ServiceStandardUpdateValidator();
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsSuccess()
    {
        var dto = new ServiceStandardUpdateDTO
        {
            Name = "Updated Laundry",
            Price = 60000,
            Unit = "piece"
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Update_EmptyUnit_ReturnsBadRequest()
    {
        var dto = new ServiceStandardUpdateDTO
        {
            Name = "Laundry",
            Price = 50000,
            Unit = ""
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Unit)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.EMPTY_UNIT_NAME);
    }

    [Fact]
    public async Task Update_PriceBelowMinimum_ReturnsBadRequest()
    {
        var dto = new ServiceStandardUpdateDTO
        {
            Name = "Laundry",
            Price = 500,
            Unit = "kg"
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.STANDARD_SERVICE_PRICE_GREATER_THAN_ZERO);
    }
}

// =========================================================================
// AIRPORT TRANSFER SERVICE VALIDATOR TESTS
// =========================================================================

public class ServiceAirportCreateValidatorTests
{
    private readonly ServiceAirportCreateValidator _validator;

    public ServiceAirportCreateValidatorTests()
    {
        _validator = new ServiceAirportCreateValidator();
    }

    private ServiceAirportCreateDTO ValidAirportDto() => new ServiceAirportCreateDTO
    {
        Price = 0,
        MaxPassengers = 4,
        MaxLuggage = 4,
        IsOneWayPaid = true,
        HasRoundTrip = false,
        IsRoundTripPaid = false,
        HasNightFee = false
    };

    [Fact]
    public async Task Create_ValidRequest_NoRoundTrip_NoNightFee_ReturnsSuccess()
    {
        // 1. Arrange
        var dto = ValidAirportDto();

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Create_ValidRequest_WithRoundTrip_WithFee_ReturnsSuccess()
    {
        // 1. Arrange
        var dto = ValidAirportDto();
        dto.HasRoundTrip = true;
        dto.IsRoundTripPaid = true;
        dto.RoundTripPrice = 200000;

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RoundTripPrice);
    }

    [Fact]
    public async Task Create_MaxPassengers_Zero_ReturnsBadRequest()
    {
        // 1. Arrange — GreaterThan(0) when HasValue
        var dto = ValidAirportDto();
        dto.MaxPassengers = 0;

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxPassengers)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.MIN_PASSENGERS);
    }

    [Fact]
    public async Task Create_MaxPassengers_AboveLimit_ReturnsBadRequest()
    {
        // 1. Arrange — LessThanOrEqualTo(45)
        var dto = ValidAirportDto();
        dto.MaxPassengers = 46;

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxPassengers)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.MAX_PASSENGERS);
    }

    [Fact]
    public async Task Create_MaxLuggage_Negative_ReturnsBadRequest()
    {
        // 1. Arrange — GreaterThanOrEqualTo(0) when HasValue
        var dto = ValidAirportDto();
        dto.MaxLuggage = -1;

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxLuggage)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.MIN_LUGGAGE);
    }

    [Fact]
    public async Task Create_MaxLuggage_AboveLimit_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = ValidAirportDto();
        dto.MaxLuggage = 46; // Error

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxLuggage)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.MAX_LUGGAGE);
    }

    [Fact]
    public async Task Create_RoundTripPaid_PriceBelowMinimum_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = ValidAirportDto();
        dto.HasRoundTrip = true;
        dto.IsRoundTripPaid = true;
        dto.RoundTripPrice = 5000; // below MIN_PRICE=10000

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.RoundTripPrice);
    }

    [Fact]
    public async Task Create_HasNightFee_AdditionalFeeBelowMinimum_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = ValidAirportDto();
        dto.HasNightFee = true;
        dto.AdditionalFee = 5000; // Error
        dto.AdditionalFeeStartTime = new TimeOnly(22, 0);
        dto.AdditionalFeeEndTime = new TimeOnly(6, 0);

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.AdditionalFee)
            .WithErrorMessage($"{MessageResponse.AdminManagement.Service.DEFAULT_ADDITIONAL_FEE} {ServiceValidator.MIN_PRICE:N0} VND!");
    }

    [Fact]
    public async Task Create_HasNightFee_MissingStartTime_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = ValidAirportDto();
        dto.HasNightFee = true;
        dto.AdditionalFee = 50000;
        dto.AdditionalFeeStartTime = null; // missing
        dto.AdditionalFeeEndTime = new TimeOnly(6, 0);

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.AdditionalFeeStartTime)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.MISSING_ADDITIONAL_FEE_START_TIME);
    }

    [Fact]
    public async Task Create_HasNightFee_MissingEndTime_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = ValidAirportDto();
        dto.HasNightFee = true;
        dto.AdditionalFee = 50000;
        dto.AdditionalFeeStartTime = new TimeOnly(22, 0);
        dto.AdditionalFeeEndTime = null; // missing

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.AdditionalFeeEndTime)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.MISSING_ADDITIONAL_FEE_END_TIME);
    }

    [Fact]
    public async Task Create_HasNightFee_TimeWindow_ExceedsLimit_ReturnsBadRequest()
    {
        // 1. Arrange — window > 12 hours is invalid
        var dto = ValidAirportDto();
        dto.HasNightFee = true;
        dto.AdditionalFee = 50000;
        dto.AdditionalFeeStartTime = new TimeOnly(6, 0);   // start
        dto.AdditionalFeeEndTime = new TimeOnly(20, 0);    // end → 14h window > 12h

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.AdditionalFeeEndTime)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.ADDITIONAL_FEE_TIME_EXCEEDS_LIMIT);
    }

    [Fact]
    public async Task Create_HasNightFee_SameStartAndEndTime_ReturnsBadRequest()
    {
        // 1. Arrange — start == end is invalid (zero-duration window)
        var dto = ValidAirportDto();
        dto.HasNightFee = true;
        dto.AdditionalFee = 50000;
        dto.AdditionalFeeStartTime = new TimeOnly(22, 0);
        dto.AdditionalFeeEndTime = new TimeOnly(22, 0); // same as start

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.AdditionalFeeEndTime)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.INVALID_ADDITIONAL_FEE_START_END_TIME);
    }

    [Fact]
    public async Task Create_HasNightFee_False_SkipsTimeValidation_ReturnsSuccess()
    {
        // 1. Arrange — HasNightFee=false → all night-fee fields are ignored
        var dto = ValidAirportDto();
        dto.HasNightFee = false;
        // leave start/end null

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AdditionalFeeStartTime);
        result.ShouldNotHaveValidationErrorFor(x => x.AdditionalFeeEndTime);
    }
}

public class ServiceAirportUpdateValidatorTests
{
    private readonly ServiceAirportUpdateValidator _validator;

    public ServiceAirportUpdateValidatorTests()
    {
        _validator = new ServiceAirportUpdateValidator();
    }

    private ServiceAirportUpdateDTO ValidUpdateDto() => new ServiceAirportUpdateDTO
    {
        Name = "Updated Airport Transfer",
        Price = 0,
        MaxPassengers = 5,
        MaxLuggage = 3,
        IsOneWayPaid = true,
        HasRoundTrip = false,
        IsRoundTripPaid = false,
        HasNightFee = false
    };

    [Fact]
    public async Task Update_ValidRequest_ReturnsSuccess()
    {
        var dto = ValidUpdateDto();

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Update_MaxPassengers_Zero_ReturnsBadRequest()
    {
        var dto = ValidUpdateDto();
        dto.MaxPassengers = 0;

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.MaxPassengers)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.MIN_PASSENGERS);
    }

    [Fact]
    public async Task Update_HasNightFee_SameStartAndEndTime_ReturnsBadRequest()
    {
        var dto = ValidUpdateDto();
        dto.HasNightFee = true;
        dto.AdditionalFee = 50000;
        dto.AdditionalFeeStartTime = new TimeOnly(23, 0);
        dto.AdditionalFeeEndTime = new TimeOnly(23, 0);

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.AdditionalFeeEndTime)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.INVALID_ADDITIONAL_FEE_START_END_TIME);
    }
}

// =========================================================================
// SERVICE CREATE/UPDATE PARENT VALIDATOR TESTS (Common base rules)
// =========================================================================

public class ServiceCreateValidatorTests
{
    private readonly ServiceCreateValidator _validator;

    public ServiceCreateValidatorTests()
    {
        _validator = new ServiceCreateValidator();
    }

    [Fact]
    public async Task Create_PolymorphicRouting_UsesStandardValidator()
    {
        // 1. Arrange: Init Standard DTO with Empty Unit
        var dto = new ServiceStandardCreateDTO
        {
            Name = "Standard Service",
            Price = 50000,
            Unit = "" // Error
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert: Parent class has to catch child class's error
        result.ShouldHaveValidationErrorFor(x => ((ServiceStandardCreateDTO)x).Unit)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.EMPTY_UNIT_NAME);
    }

    [Fact]
    public async Task Create_PolymorphicRouting_UsesAirportValidator()
    {
        // 1. Arrange: Init Airport DTO with MaxPassengers = 0
        var dto = new ServiceAirportCreateDTO
        {
            Name = "Airport Transfer",
            MaxPassengers = 0 // Error
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => ((ServiceAirportCreateDTO)x).MaxPassengers)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.MIN_PASSENGERS);
    }

    [Fact]
    public async Task Create_EmptyName_ReturnsBadRequest()
    {
        // 1. Arrange
        // Use ServiceStandardCreateDTO with the parent validator to test Name field (on base class)
        var dto = new ServiceStandardCreateDTO
        {
            Price = 50000,
            Unit = "kg"
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.EMPTY_NAME);
    }

    [Fact]
    public async Task Create_LongName_ReturnsBadRequest()
    {
        // 1. Arrange — MaximumLength(50)
        var dto = new ServiceStandardCreateDTO
        {
            Price = 50000,
            Unit = "kg"
        };
        dto.Name = new string('A', 51);

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.LONG_NAME);
    }

    [Fact]
    public async Task Create_LongDescription_ReturnsBadRequest()
    {
        // 1. Arrange — MaximumLength(500)
        var dto = new ServiceStandardCreateDTO
        {
            Price = 50000,
            Unit = "kg",
            Description = new string('A', 501)
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}

public class ServiceUpdateValidatorTests
{
    private readonly ServiceUpdateValidator _validator;

    public ServiceUpdateValidatorTests()
    {
        _validator = new ServiceUpdateValidator();
    }

    [Fact]
    public async Task Update_EmptyName_ReturnsBadRequest()
    {
        var dto = new ServiceStandardUpdateDTO
        {
            Name = "",
            Price = 50000,
            Unit = "kg"
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.EMPTY_NAME);
    }

    [Fact]
    public async Task Update_LongName_ReturnsBadRequest()
    {
        var dto = new ServiceStandardUpdateDTO
        {
            Name = new string('A', 51),
            Price = 50000,
            Unit = "kg"
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.LONG_NAME);
    }

    [Fact]
    public async Task Update_LongDescription_ReturnsBadRequest()
    {
        var dto = new ServiceStandardUpdateDTO
        {
            Name = "Valid Name",
            Price = 50000,
            Unit = "kg",
            Description = new string('A', 501)
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }

    // --- TEST POLYMORPHIC ROUTING FOR UPDATE ---

    [Fact]
    public async Task Update_PolymorphicRouting_UsesStandardValidator()
    {
        // Low Price
        var dto = new ServiceStandardUpdateDTO
        {
            Name = "Standard Service Update",
            Price = 500, // Error
            Unit = "kg"
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => ((ServiceStandardUpdateDTO)x).Price)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.STANDARD_SERVICE_PRICE_GREATER_THAN_ZERO);
    }

    [Fact]
    public async Task Update_PolymorphicRouting_UsesAirportValidator()
    {
        // SameTimes Error
        var dto = new ServiceAirportUpdateDTO
        {
            Name = "Airport Update",
            HasNightFee = true,
            AdditionalFee = 50000,
            AdditionalFeeStartTime = new TimeOnly(23, 0),
            AdditionalFeeEndTime = new TimeOnly(23, 0) // Error
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => ((ServiceAirportUpdateDTO)x).AdditionalFeeEndTime)
            .WithErrorMessage(MessageResponse.AdminManagement.Service.INVALID_ADDITIONAL_FEE_START_END_TIME);
    }

}
