using FluentValidation;
using HotelBooking.application.Helpers; // Giả sử MessageResponse nằm ở đây

namespace HotelBooking.application.Validators.AdminManagement.Policies;

// 1. Parent Validator for CREATE (Polymorphic coordination)
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

        RuleFor(x => x).SetInheritanceValidator(v =>
        {
            v.Add(new CheckInOutPolicyCreateValidator());
            v.Add(new CancellationPolicyCreateValidator());
            v.Add(new ChildrenPolicyCreateValidator());
            v.Add(new PetPolicyCreateValidator());
        });
    }
}

// 2. Parent Validator for UPDATE (Polymorphic coordination)
public class PolicyUpdateValidator : AbstractValidator<PolicyUpdateDTO>
{
    public PolicyUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Policy.LONG_NAME);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.AdminManagement.Policy.LONG_DESCRIPTION);

        RuleFor(x => x).SetInheritanceValidator(v =>
        {
            v.Add(new CheckInOutPolicyUpdateValidator());
            v.Add(new CancellationPolicyUpdateValidator());
            v.Add(new ChildrenPolicyUpdateValidator());
            v.Add(new PetPolicyUpdateValidator());
        });
    }
}

// 3. Child Validators for each policy type (CREATE)
public class CheckInOutPolicyCreateValidator : AbstractValidator<CheckInOutPolicyCreateDTO>
{
    public CheckInOutPolicyCreateValidator()
    {
        RuleFor(x => x.TypeId)
            .Equal((int)PolicyTypeEnum.CheckInOut)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_ID_BY_TYPE);

        RuleFor(x => x.CheckInTime).NotNull().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKIN_TIME);
        RuleFor(x => x.CheckOutTime).NotNull().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKOUT_TIME);

        RuleFor(x => x.EarlyCheckInFee)
            .GreaterThanOrEqualTo(0).When(x => x.EarlyCheckInFee.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_EARLY_CHECKIN_FEE);

        RuleFor(x => x.LateCheckOutFee)
            .GreaterThanOrEqualTo(0).When(x => x.LateCheckOutFee.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_LATE_CHECKOUT_FEE);
    }
}

public class CancellationPolicyCreateValidator : AbstractValidator<CancellationPolicyCreateDTO>
{
    public CancellationPolicyCreateValidator()
    {
        RuleFor(x => x.DaysBeforeCheckIn)
            .GreaterThanOrEqualTo(0).When(x => x.DaysBeforeCheckIn.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_DAYS_BEFORE_CHECKIN);

        RuleFor(x => x.RefundPercent)
            .InclusiveBetween(0, 100).When(x => x.RefundPercent.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_REFUND_PERCENT);
    }
}

public class ChildrenPolicyCreateValidator : AbstractValidator<ChildrenPolicyCreateDTO>
{
    public ChildrenPolicyCreateValidator()
    {
        RuleFor(x => x.MinAge)
            .GreaterThanOrEqualTo(0).When(x => x.MinAge.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MIN_AGE);

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(x => x.MinAge ?? 0).When(x => x.MaxAge.HasValue && x.MinAge.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE);

        RuleFor(x => x.ExtraBedFee)
            .GreaterThanOrEqualTo(0).When(x => x.ExtraBedFee.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_EXTRA_BED_FEE);
    }
}

public class PetPolicyCreateValidator : AbstractValidator<PetPolicyCreateDTO>
{
    public PetPolicyCreateValidator()
    {
        RuleFor(x => x.PetFee)
            .GreaterThanOrEqualTo(0).When(x => x.PetFee.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_PET_FEE);
    }
}

// 4. Child Validators for each policy type (UPDATE)
public class CheckInOutPolicyUpdateValidator : AbstractValidator<CheckInOutPolicyUpdateDTO>
{
    public CheckInOutPolicyUpdateValidator()
    {
        RuleFor(x => x.CheckInTime).NotNull().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKIN_TIME);
        RuleFor(x => x.CheckOutTime).NotNull().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKOUT_TIME);

        RuleFor(x => x.EarlyCheckInFee)
            .GreaterThanOrEqualTo(0).When(x => x.EarlyCheckInFee.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_EARLY_CHECKIN_FEE);

        RuleFor(x => x.LateCheckOutFee)
            .GreaterThanOrEqualTo(0).When(x => x.LateCheckOutFee.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_LATE_CHECKOUT_FEE);
    }
}

public class CancellationPolicyUpdateValidator : AbstractValidator<CancellationPolicyUpdateDTO>
{
    public CancellationPolicyUpdateValidator()
    {
        RuleFor(x => x.DaysBeforeCheckIn)
            .GreaterThanOrEqualTo(0).When(x => x.DaysBeforeCheckIn.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_DAYS_BEFORE_CHECKIN);

        RuleFor(x => x.RefundPercent)
            .InclusiveBetween(0, 100).When(x => x.RefundPercent.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_REFUND_PERCENT);
    }
}

public class ChildrenPolicyUpdateValidator : AbstractValidator<ChildrenPolicyUpdateDTO>
{
    public ChildrenPolicyUpdateValidator()
    {
        RuleFor(x => x.MinAge)
            .GreaterThanOrEqualTo(0).When(x => x.MinAge.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MIN_AGE);

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(x => x.MinAge ?? 0).When(x => x.MaxAge.HasValue && x.MinAge.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE);

        RuleFor(x => x.ExtraBedFee)
            .GreaterThanOrEqualTo(0).When(x => x.ExtraBedFee.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_EXTRA_BED_FEE);
    }
}

public class PetPolicyUpdateValidator : AbstractValidator<PetPolicyUpdateDTO>
{
    public PetPolicyUpdateValidator()
    {
        RuleFor(x => x.PetFee)
            .GreaterThanOrEqualTo(0).When(x => x.PetFee.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_PET_FEE);
    }
}