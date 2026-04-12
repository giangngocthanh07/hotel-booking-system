// Common Validator for ServiceDTO base class
using System.Linq.Expressions;
using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.Services;

public class ServiceValidator : AbstractValidator<ServiceDTO>
{
    public const decimal MIN_PRICE = 10000;

    public ServiceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Service.LONG_NAME);

        RuleFor(x => x.Price)
            .Must(p => p == 0 || p >= MIN_PRICE)
            .WithMessage($"{MessageResponse.AdminManagement.Service.INVALID_AMOUNT} or minimum {MIN_PRICE:N0} VND!");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);

        RuleFor(x => x.TypeId)
            .NotEmpty().WithMessage(MessageResponse.Validation.TYPE_ID_REQUIRED);
    }
}

public static class ServiceValidationHelper
{
    public static void ApplyStandardRules<T>(AbstractValidator<T> validator,
        Expression<Func<T, string>> unitSelector,
        Expression<Func<T, decimal>> priceSelector)
    {
        validator.RuleFor(unitSelector)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_UNIT_NAME)
            .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.Service.LONG_UNIT);

        validator.RuleFor(priceSelector)
            .Must(p => p >= ServiceValidator.MIN_PRICE)
            .WithMessage(MessageResponse.AdminManagement.Service.STANDARD_SERVICE_PRICE_GREATER_THAN_ZERO);
    }

    public static void ApplyAirportRules<T>(AbstractValidator<T> validator,
        Expression<Func<T, int?>> maxPassengersSelector,
        Expression<Func<T, int?>> maxLuggageSelector,
        Func<T, bool> hasRoundTripSelector,
        Func<T, bool> isRoundTripPaidSelector,
        Expression<Func<T, decimal?>> roundTripPriceSelector,
        Func<T, bool> hasNightFeeSelector,
        Expression<Func<T, decimal?>> additionalFeeSelector,
        Expression<Func<T, TimeOnly?>> startTimeSelector,
        Expression<Func<T, TimeOnly?>> endTimeSelector)
    {
        validator.RuleFor(maxPassengersSelector)
            .GreaterThan(0).When(x => maxPassengersSelector.Compile()(x).HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MIN_PASSENGERS)
            .LessThanOrEqualTo(45).When(x => maxPassengersSelector.Compile()(x).HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MAX_PASSENGERS);

        validator.RuleFor(maxLuggageSelector)
            .GreaterThanOrEqualTo(0).When(x => maxLuggageSelector.Compile()(x).HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MIN_LUGGAGE)
            .LessThanOrEqualTo(45).When(x => maxLuggageSelector.Compile()(x).HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MAX_LUGGAGE);

        validator.RuleFor(roundTripPriceSelector)
            .Must(p => p == 0 || p >= ServiceValidator.MIN_PRICE)
            .When(x => hasRoundTripSelector(x) && isRoundTripPaidSelector(x))
            .WithMessage($"{MessageResponse.AdminManagement.Service.INVALID_ROUND_TRIP_PRICE} or minimum {ServiceValidator.MIN_PRICE:N0} VND!");

        validator.When(x => hasNightFeeSelector(x), () =>
        {
            validator.RuleFor(additionalFeeSelector).NotNull().Must(f => f == 0 || f >= ServiceValidator.MIN_PRICE)
                .WithMessage($"{MessageResponse.AdminManagement.Service.DEFAULT_ADDITIONAL_FEE} {ServiceValidator.MIN_PRICE:N0} VND!");
            validator.RuleFor(startTimeSelector).NotNull()
                .WithMessage(MessageResponse.AdminManagement.Service.MISSING_ADDITIONAL_FEE_START_TIME);
            validator.RuleFor(endTimeSelector).NotNull()
                .WithMessage(MessageResponse.AdminManagement.Service.MISSING_ADDITIONAL_FEE_END_TIME);
        });

        validator.RuleFor(x => x).Custom((dto, context) =>
        {
            var hasNightFee = hasNightFeeSelector(dto);
            var startTime = startTimeSelector.Compile()(dto);
            var endTime = endTimeSelector.Compile()(dto);

            if (hasNightFee && startTime.HasValue && endTime.HasValue)
            {
                var duration = endTime.Value - startTime.Value;
                double totalHours = duration.TotalHours < 0 ? duration.TotalHours + 24 : duration.TotalHours;

                if (totalHours > 12)
                {
                    var propName = ((System.Reflection.MemberInfo)((MemberExpression)endTimeSelector.Body).Member).Name;
                    context.AddFailure(propName, MessageResponse.AdminManagement.Service.ADDITIONAL_FEE_TIME_EXCEEDS_LIMIT);
                }

                if (startTime == endTime)
                {
                    var propName = ((System.Reflection.MemberInfo)((MemberExpression)endTimeSelector.Body).Member).Name;
                    context.AddFailure(propName, MessageResponse.AdminManagement.Service.INVALID_ADDITIONAL_FEE_START_END_TIME);
                }
            }
        });
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
    public ServiceStandardCreateValidator()
    {
        ServiceValidationHelper.ApplyStandardRules(this, x => x.Unit, x => x.Price);
    }
}

// --- CREATE: Airport Service ---
public class ServiceAirportCreateValidator : AbstractValidator<ServiceAirportCreateDTO>
{
    public ServiceAirportCreateValidator()
    {
        ServiceValidationHelper.ApplyAirportRules(this,
            x => x.MaxPassengers,
            x => x.MaxLuggage,
            x => x.HasRoundTrip,
            x => x.IsRoundTripPaid,
            x => x.RoundTripPrice,
            x => x.HasNightFee,
            x => x.AdditionalFee,
            x => x.AdditionalFeeStartTime,
            x => x.AdditionalFeeEndTime);
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

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);

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
    public ServiceStandardUpdateValidator()
    {
        ServiceValidationHelper.ApplyStandardRules(this, x => x.Unit, x => x.Price);
    }
}

// --- UPDATE: Airport Service ---
public class ServiceAirportUpdateValidator : AbstractValidator<ServiceAirportUpdateDTO>
{
    public ServiceAirportUpdateValidator()
    {
        ServiceValidationHelper.ApplyAirportRules(this,
            x => x.MaxPassengers,
            x => x.MaxLuggage,
            x => x.HasRoundTrip,
            x => x.IsRoundTripPaid,
            x => x.RoundTripPrice,
            x => x.HasNightFee,
            x => x.AdditionalFee,
            x => x.AdditionalFeeStartTime,
            x => x.AdditionalFeeEndTime);
    }
}