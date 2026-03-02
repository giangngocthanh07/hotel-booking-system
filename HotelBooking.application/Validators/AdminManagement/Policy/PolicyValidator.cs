using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.Policy;

// 1. Validator cha cho CREATE (điều phối đa hình)
public class PolicyCreateValidator : AbstractValidator<PolicyCreateDTO>
{
    public PolicyCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Policy name is required.")
            .MaximumLength(150).WithMessage("Policy name is too long.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description is too long.");

        // Kiểm tra TypeId phải thuộc Enum đã định nghĩa
        RuleFor(x => x.TypeId)
            .IsInEnum().WithMessage("Invalid Policy Type ID.");

        RuleFor(x => x).SetInheritanceValidator(v =>
        {
            v.Add(new CheckInOutPolicyCreateValidator());
            v.Add(new CancellationPolicyCreateValidator());
            v.Add(new ChildrenPolicyCreateValidator());
            v.Add(new PetPolicyCreateValidator());
        });
    }
}

// 2. Validator cha cho UPDATE (điều phối đa hình)
public class PolicyUpdateValidator : AbstractValidator<PolicyUpdateDTO>
{
    public PolicyUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Policy name is required.")
            .MaximumLength(150).WithMessage("Policy name is too long.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description is too long.");

        // Đa hình: tự động chọn validator con theo kiểu thực tế
        RuleFor(x => x).SetInheritanceValidator(v =>
        {
            v.Add(new CheckInOutPolicyUpdateValidator());
            v.Add(new CancellationPolicyUpdateValidator());
            v.Add(new ChildrenPolicyUpdateValidator());
            v.Add(new PetPolicyUpdateValidator());
        });
    }
}

// 3. Các validator con cho từng loại policy (CREATE)
public class CheckInOutPolicyCreateValidator : AbstractValidator<CheckInOutPolicyCreateDTO>
{
    public CheckInOutPolicyCreateValidator()
    {
        // Kiểm tra nếu dùng DTO này thì TypeId BẮT BUỘC phải là 1002
        RuleFor(x => x.TypeId)
            .Equal((int)PolicyTypeEnum.CheckInOut)
            .WithMessage("TypeId mismatch: Expected 1002 for CheckInOut policy.");

        RuleFor(x => x.CheckInTime).NotNull().WithMessage("Check-in time is required.");
        RuleFor(x => x.CheckOutTime).NotNull().WithMessage("Check-out time is required.");

        RuleFor(x => x.EarlyCheckInFee)
            .GreaterThanOrEqualTo(0).When(x => x.EarlyCheckInFee.HasValue)
            .WithMessage("Early check-in fee must be >= 0.");

        RuleFor(x => x.LateCheckOutFee)
            .GreaterThanOrEqualTo(0).When(x => x.LateCheckOutFee.HasValue)
            .WithMessage("Late check-out fee must be >= 0.");
    }
}

public class CancellationPolicyCreateValidator : AbstractValidator<CancellationPolicyCreateDTO>
{
    public CancellationPolicyCreateValidator()
    {
        RuleFor(x => x.DaysBeforeCheckIn)
            .GreaterThanOrEqualTo(0).When(x => x.DaysBeforeCheckIn.HasValue)
            .WithMessage("Days before check-in must be >= 0.");

        RuleFor(x => x.RefundPercent)
            .InclusiveBetween(0, 100).When(x => x.RefundPercent.HasValue)
            .WithMessage("Refund percent must be between 0 and 100.");
    }
}

public class ChildrenPolicyCreateValidator : AbstractValidator<ChildrenPolicyCreateDTO>
{
    public ChildrenPolicyCreateValidator()
    {
        RuleFor(x => x.MinAge)
            .GreaterThanOrEqualTo(0).When(x => x.MinAge.HasValue);

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(x => x.MinAge ?? 0).When(x => x.MaxAge.HasValue && x.MinAge.HasValue)
            .WithMessage("Max age must be greater than or equal to min age.");

        RuleFor(x => x.ExtraBedFee)
            .GreaterThanOrEqualTo(0).When(x => x.ExtraBedFee.HasValue);
    }
}

public class PetPolicyCreateValidator : AbstractValidator<PetPolicyCreateDTO>
{
    public PetPolicyCreateValidator()
    {
        RuleFor(x => x.PetFee)
            .GreaterThanOrEqualTo(0).When(x => x.PetFee.HasValue);

        // Không cần validate IsPetAllowed vì bool luôn có giá trị
    }
}

// 4. Các validator con cho từng loại policy (UPDATE)
public class CheckInOutPolicyUpdateValidator : AbstractValidator<CheckInOutPolicyUpdateDTO>
{
    public CheckInOutPolicyUpdateValidator()
    {
        RuleFor(x => x.CheckInTime)
            .NotNull().WithMessage("Check-in time is required.");

        RuleFor(x => x.CheckOutTime)
            .NotNull().WithMessage("Check-out time is required.");

        RuleFor(x => x.EarlyCheckInFee)
            .GreaterThanOrEqualTo(0).When(x => x.EarlyCheckInFee.HasValue)
            .WithMessage("Early check-in fee must be >= 0.");

        RuleFor(x => x.LateCheckOutFee)
            .GreaterThanOrEqualTo(0).When(x => x.LateCheckOutFee.HasValue)
            .WithMessage("Late check-out fee must be >= 0.");
    }
}

public class CancellationPolicyUpdateValidator : AbstractValidator<CancellationPolicyUpdateDTO>
{
    public CancellationPolicyUpdateValidator()
    {
        RuleFor(x => x.DaysBeforeCheckIn)
            .GreaterThanOrEqualTo(0).When(x => x.DaysBeforeCheckIn.HasValue)
            .WithMessage("Days before check-in must be >= 0.");

        RuleFor(x => x.RefundPercent)
            .InclusiveBetween(0, 100).When(x => x.RefundPercent.HasValue)
            .WithMessage("Refund percent must be between 0 and 100.");
    }
}

public class ChildrenPolicyUpdateValidator : AbstractValidator<ChildrenPolicyUpdateDTO>
{
    public ChildrenPolicyUpdateValidator()
    {
        RuleFor(x => x.MinAge)
            .GreaterThanOrEqualTo(0).When(x => x.MinAge.HasValue);

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(x => x.MinAge ?? 0).When(x => x.MaxAge.HasValue && x.MinAge.HasValue)
            .WithMessage("Max age must be greater than or equal to min age.");

        RuleFor(x => x.ExtraBedFee)
            .GreaterThanOrEqualTo(0).When(x => x.ExtraBedFee.HasValue);
    }
}

public class PetPolicyUpdateValidator : AbstractValidator<PetPolicyUpdateDTO>
{
    public PetPolicyUpdateValidator()
    {
        RuleFor(x => x.PetFee)
            .GreaterThanOrEqualTo(0).When(x => x.PetFee.HasValue);
    }
}