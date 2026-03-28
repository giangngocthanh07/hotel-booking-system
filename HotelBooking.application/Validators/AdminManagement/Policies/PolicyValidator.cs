using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.Policies;

// =========================================================================
// 1. COMMON VALIDATORS (Chỉ chứa rule cơ bản, KHÔNG chứa đa hình để tránh Loop)
// =========================================================================

public class CommonPolicyCreateValidator : AbstractValidator<PolicyCreateDTO>
{
    public CommonPolicyCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Policy.LONG_NAME);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.AdminManagement.Policy.LONG_DESCRIPTION);

    }
}

public class CommonPolicyUpdateValidator : AbstractValidator<PolicyUpdateDTO>
{
    public CommonPolicyUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Policy.LONG_NAME);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.AdminManagement.Policy.LONG_DESCRIPTION);
    }
}

// =========================================================================
// 2. PARENT VALIDATORS (Polymorphic coordination - Entry point cho Controller)
// =========================================================================

public class PolicyCreateValidator : AbstractValidator<PolicyCreateDTO>
{
    public PolicyCreateValidator()
    {
        // Kế thừa các rule cơ bản
        Include(new CommonPolicyCreateValidator());

        // Phân luồng validation theo kiểu dữ liệu con thực tế
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
        Include(new CommonPolicyUpdateValidator());

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
// 3. HELPER (Tối ưu hiệu năng, loại bỏ hoàn toàn .Compile())
// =========================================================================

public static class PolicyValidationHelper
{
    public static void ApplyCheckInOutRules<T>(AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, TimeOnly?>> checkInTimeSelector,
        System.Linq.Expressions.Expression<Func<T, TimeOnly?>> checkOutTimeSelector,
        System.Linq.Expressions.Expression<Func<T, decimal?>> earlyCheckInFeeSelector,
        System.Linq.Expressions.Expression<Func<T, decimal?>> lateCheckOutFeeSelector)
    {
        validator.RuleFor(checkInTimeSelector)
            .NotNull().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKIN_TIME);

        validator.RuleFor(checkOutTimeSelector)
            .NotNull().WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_CHECKOUT_TIME);

        validator.RuleFor(earlyCheckInFeeSelector)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_EARLY_CHECKIN_FEE);

        validator.RuleFor(lateCheckOutFeeSelector)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_LATE_CHECKOUT_FEE);
    }

    public static void ApplyCancellationRules<T>(AbstractValidator<T> validator,
        Func<T, bool> isRefundableSelector,
        System.Linq.Expressions.Expression<Func<T, int?>> daysBeforeCheckInSelector,
        System.Linq.Expressions.Expression<Func<T, double?>> refundPercentSelector)
    {
        validator.When(x => isRefundableSelector(x), () =>
        {
            validator.RuleFor(daysBeforeCheckInSelector)
                .GreaterThanOrEqualTo(0)
                .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_DAYS_BEFORE_CHECKIN);

            validator.RuleFor(refundPercentSelector)
                .InclusiveBetween(0, 100)
                .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_REFUND_PERCENT);
        });
    }

    public static void ApplyChildrenRules<T>(AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, int?>> minAgeSelector,
        System.Linq.Expressions.Expression<Func<T, int?>> maxAgeSelector,
        Func<T, int?> minAgeFunc, // Dùng Func trực tiếp thay vì Expression.Compile()
        System.Linq.Expressions.Expression<Func<T, decimal?>> extraBedFeeSelector)
    {
        validator.RuleFor(minAgeSelector)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MIN_AGE);

        validator.RuleFor(maxAgeSelector)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE);

        validator.RuleFor(maxAgeSelector)
            .Must((model, maxAge) =>
            {
                var minAge = minAgeFunc(model);
                if (!maxAge.HasValue || !minAge.HasValue) return true;
                return maxAge.Value >= minAge.Value;
            })
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_MAX_AGE);

        validator.RuleFor(extraBedFeeSelector)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_EXTRA_BED_FEE);
    }

    public static void ApplyPetRules<T>(AbstractValidator<T> validator,
        Func<T, bool> isPetAllowedSelector,
        System.Linq.Expressions.Expression<Func<T, decimal?>> petFeeSelector)
    {
        validator.When(x => isPetAllowedSelector(x), () =>
        {
            validator.RuleFor(petFeeSelector)
                .GreaterThanOrEqualTo(0)
                .WithMessage(MessageResponse.AdminManagement.Policy.INVALID_PET_FEE);
        });
    }
}

// =========================================================================
// 4. CHILD VALIDATORS — CREATE
// =========================================================================

public class CheckInOutPolicyCreateValidator : AbstractValidator<CheckInOutPolicyCreateDTO>
{
    public CheckInOutPolicyCreateValidator()
    {
        Include(new CommonPolicyCreateValidator()); // Gắn Common Rule, ngắt vòng lặp

        PolicyValidationHelper.ApplyCheckInOutRules(this,
            x => x.CheckInTime,
            x => x.CheckOutTime,
            x => x.EarlyCheckInFee,
            x => x.LateCheckOutFee);
    }
}

public class CancellationPolicyCreateValidator : AbstractValidator<CancellationPolicyCreateDTO>
{
    public CancellationPolicyCreateValidator()
    {
        Include(new CommonPolicyCreateValidator());

        PolicyValidationHelper.ApplyCancellationRules(this,
            x => x.IsRefundable,
            x => x.DaysBeforeCheckIn,
            x => x.RefundPercent);
    }
}

public class ChildrenPolicyCreateValidator : AbstractValidator<ChildrenPolicyCreateDTO>
{
    public ChildrenPolicyCreateValidator()
    {
        Include(new CommonPolicyCreateValidator());

        PolicyValidationHelper.ApplyChildrenRules(this,
            x => x.MinAge,
            x => x.MaxAge,
            x => x.MinAge,
            x => x.ExtraBedFee);
    }
}

public class PetPolicyCreateValidator : AbstractValidator<PetPolicyCreateDTO>
{
    public PetPolicyCreateValidator()
    {
        Include(new CommonPolicyCreateValidator());

        PolicyValidationHelper.ApplyPetRules(this,
            x => x.IsPetAllowed,
            x => x.PetFee);
    }
}

// =========================================================================
// 5. CHILD VALIDATORS — UPDATE
// =========================================================================

public class CheckInOutPolicyUpdateValidator : AbstractValidator<CheckInOutPolicyUpdateDTO>
{
    public CheckInOutPolicyUpdateValidator()
    {
        Include(new CommonPolicyUpdateValidator()); // Gắn Common Rule, ngắt vòng lặp

        PolicyValidationHelper.ApplyCheckInOutRules(this,
            x => x.CheckInTime,
            x => x.CheckOutTime,
            x => x.EarlyCheckInFee,
            x => x.LateCheckOutFee);
    }
}

public class CancellationPolicyUpdateValidator : AbstractValidator<CancellationPolicyUpdateDTO>
{
    public CancellationPolicyUpdateValidator()
    {
        Include(new CommonPolicyUpdateValidator());

        PolicyValidationHelper.ApplyCancellationRules(this,
            x => x.IsRefundable,
            x => x.DaysBeforeCheckIn,
            x => x.RefundPercent);
    }
}

public class ChildrenPolicyUpdateValidator : AbstractValidator<ChildrenPolicyUpdateDTO>
{
    public ChildrenPolicyUpdateValidator()
    {
        Include(new CommonPolicyUpdateValidator());

        PolicyValidationHelper.ApplyChildrenRules(this,
            x => x.MinAge,
            x => x.MaxAge,
            x => x.MinAge,
            x => x.ExtraBedFee);
    }
}

public class PetPolicyUpdateValidator : AbstractValidator<PetPolicyUpdateDTO>
{
    public PetPolicyUpdateValidator()
    {
        Include(new CommonPolicyUpdateValidator());

        PolicyValidationHelper.ApplyPetRules(this,
            x => x.IsPetAllowed,
            x => x.PetFee);
    }
}