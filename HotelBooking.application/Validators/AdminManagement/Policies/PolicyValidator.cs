using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.Policies;

// =========================================================================
// 1. PARENT VALIDATORS (Polymorphic coordination)
// =========================================================================

public class PolicyCreateValidator : AbstractValidator<PolicyCreateDTO>
{
    public PolicyCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Policy.LONG_NAME);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.AdminManagement.Policy.LONG_DESCRIPTION);

        RuleFor(x => x.TypeId)
            .IsInEnum().WithMessage(MessageResponse.AdminManagement.Policy.INVALID_TYPE);

        // Automatically selects the corresponding child validator based on concrete type
        RuleFor(x => x).SetInheritanceValidator(v =>
        {
            v.Add(new CheckInOutPolicyCreateValidator());
            v.Add(new CancellationPolicyCreateValidator());
            v.Add(new ChildrenPolicyCreateValidator());
            v.Add(new PetPolicyCreateValidator());
        });
    }
}

public class PolicyUpdateValidator : AbstractValidator<PolicyUpdateDTO>
{
    public PolicyUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Policy.LONG_NAME);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.AdminManagement.Policy.LONG_DESCRIPTION);

        // Automatically selects the corresponding child validator based on concrete type
        RuleFor(x => x).SetInheritanceValidator(v =>
        {
            v.Add(new CheckInOutPolicyUpdateValidator());
            v.Add(new CancellationPolicyUpdateValidator());
            v.Add(new ChildrenPolicyUpdateValidator());
            v.Add(new PetPolicyUpdateValidator());
        });
    }
}

// =========================================================================
// 2. CHILD VALIDATORS — CREATE
// =========================================================================

/// <summary>
/// Check-In/Out Policy (TypeId: 1002)
/// Bug Fix #1: Removed redundant TypeId == CheckInOut check (SetInheritanceValidator already ensures this).
/// Bug Fix #2: Moved .WithMessage() BEFORE .When() so the message is always bound to the rule.
/// </summary>
public class CheckInOutPolicyCreateValidator : AbstractValidator<CheckInOutPolicyCreateDTO>
{
    public CheckInOutPolicyCreateValidator()
    {
        // Check-in and check-out times are required for this policy type
        RuleFor(x => x.CheckInTime)
            .NotNull().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKIN_TIME);

        RuleFor(x => x.CheckOutTime)
            .NotNull().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKOUT_TIME);

        // Bug Fix #2: .WithMessage() must come directly after the validator method, then .When()
        RuleFor(x => x.EarlyCheckInFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_EARLY_CHECKIN_FEE)
            .When(x => x.EarlyCheckInFee.HasValue);

        RuleFor(x => x.LateCheckOutFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_LATE_CHECKOUT_FEE)
            .When(x => x.LateCheckOutFee.HasValue);
    }
}

/// <summary>
/// Cancellation Policy (TypeId: 1003)
/// Bug Fix #3: DaysBeforeCheckIn and RefundPercent are only relevant when IsRefundable = true.
///             Without the When() guard, validation errors fire even for non-refundable policies.
/// </summary>
public class CancellationPolicyCreateValidator : AbstractValidator<CancellationPolicyCreateDTO>
{
    public CancellationPolicyCreateValidator()
    {
        // Bug Fix #3: Only validate refund-specific fields when the policy IS refundable
        When(x => x.IsRefundable, () =>
        {
            RuleFor(x => x.DaysBeforeCheckIn)
                .GreaterThanOrEqualTo(0)
                .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_DAYS_BEFORE_CHECKIN)
                .When(x => x.DaysBeforeCheckIn.HasValue);

            RuleFor(x => x.RefundPercent)
                .InclusiveBetween(0, 100)
                .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_REFUND_PERCENT)
                .When(x => x.RefundPercent.HasValue);
        });
    }
}

/// <summary>
/// Children Policy (TypeId: 1004)
/// Bug Fix #4: Added base >= 0 check on MaxAge independently of MinAge presence.
///             Cross-field check (MaxAge >= MinAge) only fires when both values are provided.
/// </summary>
public class ChildrenPolicyCreateValidator : AbstractValidator<ChildrenPolicyCreateDTO>
{
    public ChildrenPolicyCreateValidator()
    {
        RuleFor(x => x.MinAge)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MIN_AGE)
            .When(x => x.MinAge.HasValue);

        // Bug Fix #4: Validate MaxAge >= 0 always (when provided), then cross-field only when both exist
        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE)
            .When(x => x.MaxAge.HasValue);

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(x => x.MinAge!.Value)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE)
            .When(x => x.MaxAge.HasValue && x.MinAge.HasValue);

        RuleFor(x => x.ExtraBedFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_EXTRA_BED_FEE)
            .When(x => x.ExtraBedFee.HasValue);
    }
}

/// <summary>
/// Pet Policy (TypeId: 2002)
/// Bug Fix #5: PetFee validation is only meaningful when IsPetAllowed = true.
///             Without the guard, the validator fires even when pets are not allowed.
/// </summary>
public class PetPolicyCreateValidator : AbstractValidator<PetPolicyCreateDTO>
{
    public PetPolicyCreateValidator()
    {
        // Bug Fix #5: Only validate PetFee when pets are actually allowed
        When(x => x.IsPetAllowed, () =>
        {
            RuleFor(x => x.PetFee)
                .GreaterThanOrEqualTo(0)
                .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_PET_FEE)
                .When(x => x.PetFee.HasValue);
        });
    }
}

// =========================================================================
// 3. CHILD VALIDATORS — UPDATE (mirrors CREATE logic exactly)
// =========================================================================

/// <summary>
/// Check-In/Out Policy - UPDATE
/// Bug Fix #2: Same .WithMessage() ordering fix as CREATE validator.
/// </summary>
public class CheckInOutPolicyUpdateValidator : AbstractValidator<CheckInOutPolicyUpdateDTO>
{
    public CheckInOutPolicyUpdateValidator()
    {
        RuleFor(x => x.CheckInTime)
            .NotNull().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKIN_TIME);

        RuleFor(x => x.CheckOutTime)
            .NotNull().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKOUT_TIME);

        // Bug Fix #2: .WithMessage() before .When()
        RuleFor(x => x.EarlyCheckInFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_EARLY_CHECKIN_FEE)
            .When(x => x.EarlyCheckInFee.HasValue);

        RuleFor(x => x.LateCheckOutFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_LATE_CHECKOUT_FEE)
            .When(x => x.LateCheckOutFee.HasValue);
    }
}

/// <summary>
/// Cancellation Policy - UPDATE
/// Bug Fix #3: Same IsRefundable guard as CREATE validator.
/// </summary>
public class CancellationPolicyUpdateValidator : AbstractValidator<CancellationPolicyUpdateDTO>
{
    public CancellationPolicyUpdateValidator()
    {
        // Bug Fix #3: Only validate refund-specific fields when the policy IS refundable
        When(x => x.IsRefundable, () =>
        {
            RuleFor(x => x.DaysBeforeCheckIn)
                .GreaterThanOrEqualTo(0)
                .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_DAYS_BEFORE_CHECKIN)
                .When(x => x.DaysBeforeCheckIn.HasValue);

            RuleFor(x => x.RefundPercent)
                .InclusiveBetween(0, 100)
                .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_REFUND_PERCENT)
                .When(x => x.RefundPercent.HasValue);
        });
    }
}

/// <summary>
/// Children Policy - UPDATE
/// Bug Fix #4: Same base >= 0 + conditional cross-field check as CREATE validator.
/// </summary>
public class ChildrenPolicyUpdateValidator : AbstractValidator<ChildrenPolicyUpdateDTO>
{
    public ChildrenPolicyUpdateValidator()
    {
        RuleFor(x => x.MinAge)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MIN_AGE)
            .When(x => x.MinAge.HasValue);

        // Bug Fix #4: Validate MaxAge >= 0 always (when provided), then cross-field only when both exist
        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE)
            .When(x => x.MaxAge.HasValue);

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(x => x.MinAge!.Value)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE)
            .When(x => x.MaxAge.HasValue && x.MinAge.HasValue);

        RuleFor(x => x.ExtraBedFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_EXTRA_BED_FEE)
            .When(x => x.ExtraBedFee.HasValue);
    }
}

/// <summary>
/// Pet Policy - UPDATE
/// Bug Fix #5: Same IsPetAllowed guard as CREATE validator.
/// </summary>
public class PetPolicyUpdateValidator : AbstractValidator<PetPolicyUpdateDTO>
{
    public PetPolicyUpdateValidator()
    {
        // Bug Fix #5: Only validate PetFee when pets are actually allowed
        When(x => x.IsPetAllowed, () =>
        {
            RuleFor(x => x.PetFee)
                .GreaterThanOrEqualTo(0)
                .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_PET_FEE)
                .When(x => x.PetFee.HasValue);
        });
    }
}