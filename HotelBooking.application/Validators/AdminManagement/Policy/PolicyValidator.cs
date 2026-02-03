using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.Policy;

public class PolicyCreateValidator : AbstractValidator<PolicyCreateDTO>
{
    public PolicyCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(MessageResponse.Validation.EMPTY_NAME).MaximumLength(50).WithMessage(MessageResponse.Validation.LONG_NAME);

        // Create BẮT BUỘC có TypeId
        RuleFor(x => x.TypeId).GreaterThan(0).WithMessage(MessageResponse.AdminManagement.Policy.EMPTY_TYPE);

        // Validate chung (Percent <= 100)
        RuleFor(x => x.Percent).InclusiveBetween(0, 100).When(x => x.Percent.HasValue).WithMessage(MessageResponse.Validation.PERCENT_INVALID);

        // Validate Description (Nếu cần - Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}

public class PolicyUpdateValidator : AbstractValidator<PolicyUpdateDTO>
{
    public PolicyUpdateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(MessageResponse.Validation.EMPTY_NAME).MaximumLength(50).WithMessage(MessageResponse.Validation.LONG_NAME);

        // Update KHÔNG check TypeId

        // Validate chung
        RuleFor(x => x.Percent).InclusiveBetween(0, 100).When(x => x.Percent.HasValue).WithMessage(MessageResponse.Validation.PERCENT_INVALID);

        // Validate Description (Nếu cần - Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}
