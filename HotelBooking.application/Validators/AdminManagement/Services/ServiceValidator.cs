// Common Validator for ServiceDTO base class
using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.Services;

public class ServiceValidator : AbstractValidator<ServiceDTO>
{
    // Assume minimum price is 10,000 VND
    private const decimal minPrice = 10000;

    public ServiceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Service.LONG_NAME);

        RuleFor(x => x.Price)
            .Must(p => p == 0 || p >= minPrice)
            .WithMessage($"{MessageResponse.AdminManagement.Service.INVALID_AMOUNT} or minimum {minPrice:N0} VND!");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);

        RuleFor(x => x.TypeId)
            .NotEmpty().WithMessage(MessageResponse.Validation.TYPE_ID_REQUIRED);
    }
}

// =========================================================================
// 1. VALIDATOR FOR CREATE (Parent Class & Polymorphic Coordination)
// =========================================================================
public class ServiceCreateValidator : AbstractValidator<ServiceCreateDTO>
{
    public ServiceCreateValidator()
    {
        // A. Validate common fields (Base)
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Service.LONG_NAME);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);

        // B. Polymorphic Configuration
        // Automatically selects the corresponding child validator based on concrete type (Standard or Airport)
        RuleFor(x => x).SetInheritanceValidator(v =>
        {
            v.Add(new ServiceStandardCreateValidator()); // Register child validator
            v.Add(new ServiceAirportCreateValidator());  // Register child validator
        });
    }
}

// --- CREATE: Standard Service ---
public class ServiceStandardCreateValidator : AbstractValidator<ServiceStandardCreateDTO>
{
    private const decimal minPrice = 10000;
    public ServiceStandardCreateValidator()
    {
        // 1. Specific Validation (Unit)
        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_UNIT_NAME)
            .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.Service.LONG_UNIT);

        RuleFor(x => x.Price)
            .Must(p => p >= minPrice)
            .WithMessage(MessageResponse.AdminManagement.Service.STANDARD_SERVICE_PRICE_GREATER_THAN_ZERO);
    }
}

// --- CREATE: Airport Service ---
public class ServiceAirportCreateValidator : AbstractValidator<ServiceAirportCreateDTO>
{
    private const decimal minPrice = 10000;

    public ServiceAirportCreateValidator()
    {
        // Validate Capacity
        RuleFor(x => x.MaxPassengers)
            .GreaterThan(0).When(x => x.MaxPassengers.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MIN_PASSENGERS)
            .LessThanOrEqualTo(45).When(x => x.MaxPassengers.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MAX_PASSENGERS);

        RuleFor(x => x.MaxLuggage)
            .GreaterThanOrEqualTo(0).When(x => x.MaxLuggage.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MIN_LUGGAGE)
            .LessThanOrEqualTo(45).When(x => x.MaxLuggage.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MAX_LUGGAGE);

        // Validate Round-trip (If applicable)
        RuleFor(x => x.RoundTripPrice)
            .Must(p => p == 0 || p >= minPrice)
            .When(x => x.HasRoundTrip && x.IsRoundTripPaid)
            .WithMessage($"{MessageResponse.AdminManagement.Service.INVALID_ROUND_TRIP_PRICE} or minimum {minPrice:N0} VND!");

        // Validate Night Fee
        When(x => x.HasNightFee, () =>
        {
            RuleFor(x => x.AdditionalFee).NotNull().Must(f => f == 0 || f >= minPrice)
                .WithMessage($"{MessageResponse.AdminManagement.Service.DEFAULT_ADDITIONAL_FEE} {minPrice:N0} VND!");
            RuleFor(x => x.AdditionalFeeStartTime).NotNull()
                .WithMessage(MessageResponse.AdminManagement.Service.MISSING_ADDITIONAL_FEE_START_TIME);
            RuleFor(x => x.AdditionalFeeEndTime).NotNull()
                .WithMessage(MessageResponse.AdminManagement.Service.MISSING_ADDITIONAL_FEE_END_TIME);
        });

        // Custom Validation for Night Fee Time Range
        RuleFor(x => x).Custom((dto, context) =>
        {
            if (dto.HasNightFee && dto.AdditionalFeeStartTime.HasValue && dto.AdditionalFeeEndTime.HasValue)
            {
                var duration = dto.AdditionalFeeEndTime.Value - dto.AdditionalFeeStartTime.Value;
                double totalHours = duration.TotalHours < 0 ? duration.TotalHours + 24 : duration.TotalHours;

                if (totalHours > 12)
                {
                    context.AddFailure("AdditionalFeeEndTime", MessageResponse.AdminManagement.Service.ADDITIONAL_FEE_TIME_EXCEEDS_LIMIT);
                }

                if (dto.AdditionalFeeStartTime == dto.AdditionalFeeEndTime)
                {
                    context.AddFailure("AdditionalFeeEndTime", MessageResponse.AdminManagement.Service.INVALID_ADDITIONAL_FEE_START_END_TIME);
                }
            }
        });
    }
}

// =========================================================================
// 2. GROUP VALIDATOR FOR UPDATE
// =========================================================================

// 2.1. Parent Validator (Coordination)
public class ServiceUpdateValidator : AbstractValidator<ServiceUpdateDTO>
{
    public ServiceUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Service.LONG_NAME);

        RuleFor(x => x).SetInheritanceValidator(v =>
        {
            v.Add(new ServiceStandardUpdateValidator());
            v.Add(new ServiceAirportUpdateValidator());
        });
    }
}

// --- UPDATE: Standard Service ---
public class ServiceStandardUpdateValidator : AbstractValidator<ServiceStandardUpdateDTO>
{
    private const decimal minPrice = 10000;

    public ServiceStandardUpdateValidator()
    {
        // Same logic as Create
        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_UNIT_NAME)
            .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.Service.LONG_UNIT);

        RuleFor(x => x.Price)
            .Must(p => p >= minPrice)
            .WithMessage(MessageResponse.AdminManagement.Service.STANDARD_SERVICE_PRICE_GREATER_THAN_ZERO);
    }
}

// --- UPDATE: Airport Service ---
public class ServiceAirportUpdateValidator : AbstractValidator<ServiceAirportUpdateDTO>
{
    private const decimal minPrice = 10000;

    public ServiceAirportUpdateValidator()
    {
        RuleFor(x => x.MaxPassengers)
            .GreaterThan(0).When(x => x.MaxPassengers.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MIN_PASSENGERS)
            .LessThanOrEqualTo(45).When(x => x.MaxPassengers.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MAX_PASSENGERS);

        RuleFor(x => x.MaxLuggage)
            .GreaterThan(0).When(x => x.MaxLuggage.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MIN_LUGGAGE)
            .LessThanOrEqualTo(45).When(x => x.MaxLuggage.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MAX_LUGGAGE);

        RuleFor(x => x.RoundTripPrice)
            .Must(p => p == 0 || p >= minPrice)
            .When(x => x.HasRoundTrip && x.IsRoundTripPaid)
            .WithMessage($"{MessageResponse.AdminManagement.Service.INVALID_ROUND_TRIP_PRICE} or minimum {minPrice:N0} VND!");

        When(x => x.HasNightFee, () =>
        {
            RuleFor(x => x.AdditionalFee).NotNull().Must(f => f == 0 || f >= minPrice)
                .WithMessage($"{MessageResponse.AdminManagement.Service.DEFAULT_ADDITIONAL_FEE} {minPrice:N0} VND!");
            RuleFor(x => x.AdditionalFeeStartTime).NotNull()
                .WithMessage(MessageResponse.AdminManagement.Service.MISSING_ADDITIONAL_FEE_START_TIME);
            RuleFor(x => x.AdditionalFeeEndTime).NotNull()
                .WithMessage(MessageResponse.AdminManagement.Service.MISSING_ADDITIONAL_FEE_END_TIME);
        });

        RuleFor(x => x).Custom((dto, context) =>
        {
            if (dto.HasNightFee && dto.AdditionalFeeStartTime.HasValue && dto.AdditionalFeeEndTime.HasValue)
            {
                var duration = dto.AdditionalFeeEndTime.Value - dto.AdditionalFeeStartTime.Value;
                double totalHours = duration.TotalHours < 0 ? duration.TotalHours + 24 : duration.TotalHours;

                if (totalHours > 12)
                {
                    context.AddFailure("AdditionalFeeEndTime", MessageResponse.AdminManagement.Service.ADDITIONAL_FEE_TIME_EXCEEDS_LIMIT);
                }

                if (dto.AdditionalFeeStartTime == dto.AdditionalFeeEndTime)
                {
                    context.AddFailure("AdditionalFeeEndTime", MessageResponse.AdminManagement.Service.INVALID_ADDITIONAL_FEE_START_END_TIME);
                }
            }
        });
    }
}