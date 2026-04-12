using FluentValidation.TestHelper;
using HotelBooking.application.Validators.AdminManagement.Policies;

namespace HotelBooking.test.UnitTests.Validators.AdminManagement;

public class PolicyCreateValidatorTests
{
    private readonly PolicyCreateValidator _validator;

    public PolicyCreateValidatorTests()
    {
        _validator = new PolicyCreateValidator();
    }

    [Fact]
    public async Task Validate_PolymorphicRouting_UsesCorrectChildValidator()
    {
        // Send a PetPolicy (Negative PetFee) to test Parent class's routing
        // Go down PetPolicyCreateValidator to throw error
        var dto = new PetPolicyCreateDTO
        {
            Name = "Pet Policy",
            IsPetAllowed = true,
            PetFee = -50
        };

        var result = await _validator.TestValidateAsync(dto);

        // Parent class need to be received the errors from Child class
        result.ShouldHaveValidationErrorFor(x => ((PetPolicyCreateDTO)x).PetFee)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_PET_FEE);
    }
}

public class PolicyUpdateValidatorTests
{
    private readonly PolicyUpdateValidator _validator;

    public PolicyUpdateValidatorTests()
    {
        _validator = new PolicyUpdateValidator();
    }

    [Fact]
    public async Task ValidateUpdate_PolymorphicRouting_UsesChildrenPolicyValidator()
    {
        // 1. Arrange: Lần này test routing xuống ChildrenPolicy
        var dto = new ChildrenPolicyUpdateDTO
        {
            Name = "Updated Children Policy",
            MinAge = 10,
            MaxAge = 5,  // Lỗi: MaxAge đang nhỏ hơn MinAge
            ExtraBedFee = 0
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert: Kiểm tra xem nó có đúng bị vướng rules của ChildrenPolicy hay không
        result.ShouldHaveValidationErrorFor(x => ((ChildrenPolicyUpdateDTO)x).MaxAge)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE);
    }

    [Fact]
    public async Task ValidateUpdate_PolymorphicRouting_UsesCheckInOutPolicyValidator()
    {
        // 1. Arrange: Test routing to CheckInOutPolicy
        var dto = new CheckInOutPolicyUpdateDTO
        {
            Name = "Updated CheckIn",
            CheckInTime = null, // Error
            CheckOutTime = new TimeOnly(12, 0)
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => ((CheckInOutPolicyUpdateDTO)x).CheckInTime)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKIN_TIME);
    }
}

// =========================================================================
// COMMON POLICY VALIDATOR TESTS (Base class rules: Name, Description)
// =========================================================================

public class CommonPolicyCreateValidatorTests
{
    private readonly CommonPolicyCreateValidator _validator;

    public CommonPolicyCreateValidatorTests()
    {
        _validator = new CommonPolicyCreateValidator();
    }

    [Fact]
    public async Task Create_ValidBaseFields_ReturnsSuccess()
    {
        // Note: TypeId is not on base PolicyCreateDTO — tested in subtype validators
        var dto = new CheckInOutPolicyCreateDTO
        {
            Name = "Check In Policy",
            Description = "Standard check-in rules",
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = new TimeOnly(12, 0)
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Create_EmptyName_ReturnsBadRequest()
    {
        var dto = new CheckInOutPolicyCreateDTO { Name = "" };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME);
    }

    [Fact]
    public async Task Create_LongName_ReturnsBadRequest()
    {
        var dto = new CheckInOutPolicyCreateDTO { Name = new string('A', 51) };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.LONG_NAME);
    }

    [Fact]
    public async Task Create_LongDescription_ReturnsBadRequest()
    {
        var dto = new CheckInOutPolicyCreateDTO
        {
            Name = "Valid Name",
            Description = new string('A', 501)
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.LONG_DESCRIPTION);
    }

    [Fact]
    public async Task Create_NullDescription_ReturnsSuccess()
    {
        var dto = new CheckInOutPolicyCreateDTO
        {
            Name = "Valid Name",
            Description = null
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}

// =========================================================================
// CHECK-IN/OUT POLICY VALIDATOR TESTS
// =========================================================================

public class CheckInOutPolicyCreateValidatorTests
{
    private readonly CheckInOutPolicyCreateValidator _validator;

    public CheckInOutPolicyCreateValidatorTests()
    {
        _validator = new CheckInOutPolicyCreateValidator();
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var dto = new CheckInOutPolicyCreateDTO
        {
            Name = "Standard CheckInOut",
            Description = "Check-in at 2PM, check-out at 12PM",
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = new TimeOnly(12, 0),
            EarlyCheckInFee = 200000,
            LateCheckOutFee = 150000
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Create_NullCheckInTime_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new CheckInOutPolicyCreateDTO
        {
            Name = "Policy",
            CheckInTime = null,
            CheckOutTime = new TimeOnly(12, 0)
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.CheckInTime)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKIN_TIME);
    }

    [Fact]
    public async Task Create_NullCheckOutTime_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new CheckInOutPolicyCreateDTO
        {
            Name = "Policy",
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = null
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.CheckOutTime)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKOUT_TIME);
    }

    [Fact]
    public async Task Create_NegativeEarlyCheckInFee_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new CheckInOutPolicyCreateDTO
        {
            Name = "Policy",
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = new TimeOnly(12, 0),
            EarlyCheckInFee = -1
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.EarlyCheckInFee)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_EARLY_CHECKIN_FEE);
    }

    [Fact]
    public async Task Create_NegativeLateCheckOutFee_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new CheckInOutPolicyCreateDTO
        {
            Name = "Policy",
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = new TimeOnly(12, 0),
            LateCheckOutFee = -1
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.LateCheckOutFee)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_LATE_CHECKOUT_FEE);
    }

    [Fact]
    public async Task Create_ZeroFees_ReturnsSuccess()
    {
        // 1. Arrange — zero is a valid fee (free)
        var dto = new CheckInOutPolicyCreateDTO
        {
            Name = "Policy",
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = new TimeOnly(12, 0),
            EarlyCheckInFee = 0,
            LateCheckOutFee = 0
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EarlyCheckInFee);
        result.ShouldNotHaveValidationErrorFor(x => x.LateCheckOutFee);
    }
}

// =========================================================================
// COMMON UPDATE VALIDATOR TESTS (Check Base Class for Update)
// =========================================================================
public class CommonPolicyUpdateValidatorTests
{
    private readonly CommonPolicyUpdateValidator _validator;

    public CommonPolicyUpdateValidatorTests()
    {
        _validator = new CommonPolicyUpdateValidator();
    }

    [Fact]
    public async Task Update_ValidBaseFields_ReturnsSuccess()
    {
        // Using CheckInOutPolicyUpdateDTO because it is derived from PolicyUpdateDTO
        var dto = new CheckInOutPolicyUpdateDTO
        {
            Name = "Updated Policy",
            Description = "Updated Description",
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = new TimeOnly(12, 0)
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Update_EmptyName_ReturnsBadRequest()
    {
        var dto = new CheckInOutPolicyUpdateDTO { Name = "" };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME);
    }

    [Fact]
    public async Task Update_LongName_ReturnsBadRequest()
    {
        var dto = new CheckInOutPolicyUpdateDTO { Name = new string('A', 51) };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.LONG_NAME);
    }

    [Fact]
    public async Task Update_LongDescription_ReturnsBadRequest()
    {
        var dto = new CheckInOutPolicyUpdateDTO
        {
            Name = "Valid Name",
            Description = new string('A', 501)
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.LONG_DESCRIPTION);
    }
}

public class CheckInOutPolicyUpdateValidatorTests
{
    private readonly CheckInOutPolicyUpdateValidator _validator;

    public CheckInOutPolicyUpdateValidatorTests()
    {
        _validator = new CheckInOutPolicyUpdateValidator();
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsSuccess()
    {
        var dto = new CheckInOutPolicyUpdateDTO
        {
            Name = "Updated CheckInOut Policy",
            CheckInTime = new TimeOnly(15, 0),
            CheckOutTime = new TimeOnly(11, 0),
            EarlyCheckInFee = 0,
            LateCheckOutFee = 200000
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Update_EmptyName_ReturnsBadRequest()
    {
        var dto = new CheckInOutPolicyUpdateDTO
        {
            Name = "",
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = new TimeOnly(12, 0)
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME);
    }

    [Fact]
    public async Task Update_NullCheckInTime_ReturnsBadRequest()
    {
        var dto = new CheckInOutPolicyUpdateDTO
        {
            Name = "Policy",
            CheckInTime = null,
            CheckOutTime = new TimeOnly(12, 0)
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.CheckInTime)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKIN_TIME);
    }
}

// =========================================================================
// CANCELLATION POLICY VALIDATOR TESTS
// =========================================================================

public class CancellationPolicyCreateValidatorTests
{
    private readonly CancellationPolicyCreateValidator _validator;

    public CancellationPolicyCreateValidatorTests()
    {
        _validator = new CancellationPolicyCreateValidator();
    }

    [Fact]
    public async Task Create_ValidRequest_IsRefundable_True_ReturnsSuccess()
    {
        // 1. Arrange
        var dto = new CancellationPolicyCreateDTO
        {
            Name = "Flexible Cancellation",
            IsRefundable = true,
            DaysBeforeCheckIn = 3,
            RefundPercent = 80
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Create_ValidRequest_IsRefundable_False_SkipsRefundValidation_ReturnsSuccess()
    {
        // 1. Arrange — when not refundable, DaysBeforeCheckIn and RefundPercent are ignored
        var dto = new CancellationPolicyCreateDTO
        {
            Name = "Non-Refundable",
            IsRefundable = false,
            DaysBeforeCheckIn = null,
            RefundPercent = null
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Create_IsRefundable_True_NegativeDaysBeforeCheckIn_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new CancellationPolicyCreateDTO
        {
            Name = "Flexible",
            IsRefundable = true,
            DaysBeforeCheckIn = -1, // negative = invalid
            RefundPercent = 80
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.DaysBeforeCheckIn)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_DAYS_BEFORE_CHECKIN);
    }

    [Fact]
    public async Task Create_IsRefundable_True_NullDaysAndPercent_ReturnsBadRequest()
    {
        var dto = new CancellationPolicyCreateDTO
        {
            Name = "Flexible",
            IsRefundable = true,
            DaysBeforeCheckIn = null,
            RefundPercent = null
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.DaysBeforeCheckIn);
        result.ShouldHaveValidationErrorFor(x => x.RefundPercent);
    }

    [Fact]
    public async Task Create_IsRefundable_True_RefundPercentAbove100_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new CancellationPolicyCreateDTO
        {
            Name = "Flexible",
            IsRefundable = true,
            DaysBeforeCheckIn = 3,
            RefundPercent = 101
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.RefundPercent)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_REFUND_PERCENT);
    }

    [Fact]
    public async Task Create_IsRefundable_True_RefundPercentNegative_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new CancellationPolicyCreateDTO
        {
            Name = "Flexible",
            IsRefundable = true,
            DaysBeforeCheckIn = 3,
            RefundPercent = -1
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.RefundPercent)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_REFUND_PERCENT);
    }

    [Fact]
    public async Task Create_IsRefundable_True_RefundPercentAtBoundary_100_ReturnsSuccess()
    {
        // 1. Arrange — 100% is valid (full refund)
        var dto = new CancellationPolicyCreateDTO
        {
            Name = "Full Refund",
            IsRefundable = true,
            DaysBeforeCheckIn = 7,
            RefundPercent = 100
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RefundPercent);
    }
}

public class CancellationPolicyUpdateValidatorTests
{
    private readonly CancellationPolicyUpdateValidator _validator;

    public CancellationPolicyUpdateValidatorTests()
    {
        _validator = new CancellationPolicyUpdateValidator();
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsSuccess()
    {
        var dto = new CancellationPolicyUpdateDTO
        {
            Name = "Updated Cancellation",
            IsRefundable = true,
            DaysBeforeCheckIn = 5,
            RefundPercent = 50
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Update_EmptyName_ReturnsBadRequest()
    {
        var dto = new CancellationPolicyUpdateDTO { Name = "", IsRefundable = false };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME);
    }

    [Fact]
    public async Task Update_IsRefundable_False_SkipsRefundValidation_ReturnsSuccess()
    {
        var dto = new CancellationPolicyUpdateDTO
        {
            Name = "Non-Refundable Policy",
            IsRefundable = false
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

// =========================================================================
// CHILDREN POLICY VALIDATOR TESTS
// =========================================================================

public class ChildrenPolicyCreateValidatorTests
{
    private readonly ChildrenPolicyCreateValidator _validator;

    public ChildrenPolicyCreateValidatorTests()
    {
        _validator = new ChildrenPolicyCreateValidator();
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var dto = new ChildrenPolicyCreateDTO
        {
            Name = "Children Policy",
            MinAge = 0,
            MaxAge = 12,
            ExtraBedFee = 100000
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Create_NegativeMinAge_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new ChildrenPolicyCreateDTO
        {
            Name = "Children Policy",
            MinAge = -1,
            MaxAge = 12,
            ExtraBedFee = 0
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.MinAge)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_MIN_AGE);
    }

    [Fact]
    public async Task Create_MaxAge_LessThan_MinAge_ReturnsBadRequest()
    {
        // 1. Arrange — MaxAge < MinAge violates the "must be >= minAge" rule
        var dto = new ChildrenPolicyCreateDTO
        {
            Name = "Children Policy",
            MinAge = 10,
            MaxAge = 5,
            ExtraBedFee = 0
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxAge)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE);
    }

    [Fact]
    public async Task Create_MaxAge_EqualTo_MinAge_ReturnsSuccess()
    {
        // 1. Arrange — equal is valid (single-age policy)
        var dto = new ChildrenPolicyCreateDTO
        {
            Name = "Children Policy",
            MinAge = 5,
            MaxAge = 5,
            ExtraBedFee = 0
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxAge);
    }

    [Fact]
    public async Task Create_NegativeExtraBedFee_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new ChildrenPolicyCreateDTO
        {
            Name = "Children Policy",
            MinAge = 0,
            MaxAge = 12,
            ExtraBedFee = -1
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.ExtraBedFee)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_EXTRA_BED_FEE);
    }

    [Fact]
    public async Task Create_ZeroExtraBedFee_ReturnsSuccess()
    {
        // 1. Arrange — free is valid
        var dto = new ChildrenPolicyCreateDTO
        {
            Name = "Children Policy",
            MinAge = 0,
            MaxAge = 6,
            ExtraBedFee = 0
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExtraBedFee);
    }
}

public class ChildrenPolicyUpdateValidatorTests
{
    private readonly ChildrenPolicyUpdateValidator _validator;

    public ChildrenPolicyUpdateValidatorTests()
    {
        _validator = new ChildrenPolicyUpdateValidator();
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsSuccess()
    {
        var dto = new ChildrenPolicyUpdateDTO
        {
            Name = "Updated Children Policy",
            MinAge = 2,
            MaxAge = 14,
            ExtraBedFee = 50000
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Update_EmptyName_ReturnsBadRequest()
    {
        var dto = new ChildrenPolicyUpdateDTO { Name = "" };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME);
    }

    [Fact]
    public async Task Update_MaxAge_LessThan_MinAge_ReturnsBadRequest()
    {
        var dto = new ChildrenPolicyUpdateDTO
        {
            Name = "Children Policy",
            MinAge = 8,
            MaxAge = 3
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.MaxAge)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE);
    }
}

// =========================================================================
// PET POLICY VALIDATOR TESTS
// =========================================================================

public class PetPolicyCreateValidatorTests
{
    private readonly PetPolicyCreateValidator _validator;

    public PetPolicyCreateValidatorTests()
    {
        _validator = new PetPolicyCreateValidator();
    }

    [Fact]
    public async Task Create_ValidRequest_PetAllowed_WithFee_ReturnsSuccess()
    {
        // 1. Arrange
        var dto = new PetPolicyCreateDTO
        {
            Name = "Pet Policy",
            IsPetAllowed = true,
            PetFee = 100000
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Create_ValidRequest_PetNotAllowed_SkipsFeeValidation_ReturnsSuccess()
    {
        // 1. Arrange — when pet not allowed, PetFee is irrelevant
        var dto = new PetPolicyCreateDTO
        {
            Name = "No Pets Allowed",
            IsPetAllowed = false,
            PetFee = null
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Create_PetAllowed_NegativePetFee_ReturnsBadRequest()
    {
        // 1. Arrange
        var dto = new PetPolicyCreateDTO
        {
            Name = "Pet Policy",
            IsPetAllowed = true,
            PetFee = -1
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.PetFee)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_PET_FEE);
    }

    [Fact]
    public async Task Create_PetAllowed_ZeroPetFee_ReturnsSuccess()
    {
        // 1. Arrange — zero means free (pets allowed at no charge)
        var dto = new PetPolicyCreateDTO
        {
            Name = "Pet Policy",
            IsPetAllowed = true,
            PetFee = 0
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PetFee);
    }

    [Fact]
    public async Task Create_PetNotAllowed_NegativeFee_DoesNotValidateFee_ReturnsSuccess()
    {
        // 1. Arrange — IsPetAllowed=false → fee validation is skipped by When() condition
        var dto = new PetPolicyCreateDTO
        {
            Name = "No Pets",
            IsPetAllowed = false,
            PetFee = -999 // would fail if validated, but When() skips it
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(dto);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PetFee);
    }
}

public class PetPolicyUpdateValidatorTests
{
    private readonly PetPolicyUpdateValidator _validator;

    public PetPolicyUpdateValidatorTests()
    {
        _validator = new PetPolicyUpdateValidator();
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsSuccess()
    {
        var dto = new PetPolicyUpdateDTO
        {
            Name = "Updated Pet Policy",
            IsPetAllowed = true,
            PetFee = 200000
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Update_EmptyName_ReturnsBadRequest()
    {
        var dto = new PetPolicyUpdateDTO
        {
            Name = "",
            IsPetAllowed = false
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME);
    }

    [Fact]
    public async Task Update_PetAllowed_NegativePetFee_ReturnsBadRequest()
    {
        var dto = new PetPolicyUpdateDTO
        {
            Name = "Pet Policy",
            IsPetAllowed = true,
            PetFee = -50
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.PetFee)
            .WithErrorMessage(MessageResponse.AdminManagement.Policy.INVALID_PET_FEE);
    }
}
