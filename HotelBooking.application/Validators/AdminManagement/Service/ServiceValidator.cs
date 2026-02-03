// Validator chung cho lớp cha ServiceCreateOrUpdateDTO
using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.Service;

// =========================================================================
// 1. VALIDATOR CHO CREATE (Lớp cha & Điều phối đa hình)
// =========================================================================
public class ServiceCreateValidator : AbstractValidator<ServiceCreateDTO>
{
    public ServiceCreateValidator()
    {
        // A. Validate các trường chung (Base)
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Service.LONG_NAME);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage(MessageResponse.AdminManagement.Service.INVALID_AMOUNT);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);

        // B. Cấu hình Đa hình (Quan trọng nhất)
        // Dựa vào kiểu dữ liệu thực tế (Standard hay Airport), nó sẽ chạy validator con tương ứng
        RuleFor(x => x).SetInheritanceValidator(v =>
        {
            v.Add(new ServiceStandardCreateValidator()); // Đăng ký validator con
            v.Add(new ServiceAirportCreateValidator());  // Đăng ký validator con
        });
    }
}

// --- CREATE ---
public class ServiceStandardCreateValidator : AbstractValidator<ServiceStandardCreateDTO>
{
    public ServiceStandardCreateValidator()
    {

        // 1. Validate riêng (Unit)
        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_UNIT_NAME)
            .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.Service.LONG_UNIT);

    }
}

public class ServiceAirportCreateValidator : AbstractValidator<ServiceAirportCreateDTO>
{
    public ServiceAirportCreateValidator()
    {
        // Validate số lượng
        RuleFor(x => x.MaxPassengers)
            .GreaterThan(0).When(x => x.MaxPassengers.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MIN_PASSENGERS);

        RuleFor(x => x.MaxLuggage)
            .GreaterThanOrEqualTo(0).When(x => x.MaxLuggage.HasValue)
            .WithMessage(MessageResponse.AdminManagement.Service.MIN_LUGGAGE);

        // Validate Khứ hồi
        RuleFor(x => x.RoundTripPrice)
            .GreaterThan(0)
            .When(x => x.HasRoundTrip && x.IsRoundTripPaid)
            .WithMessage(MessageResponse.AdminManagement.Service.INVALID_ROUND_TRIP_PRICE);

        // Validate Phụ phí đêm
        When(x => x.HasNightFee, () =>
        {
            RuleFor(x => x.AdditionalFee).NotNull().GreaterThan(0)
                .WithMessage(MessageResponse.AdminManagement.Service.INVALID_ADDITIONAL_FEE);
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
                    context.AddFailure("AdditionalFeeEndTime", "Khung giờ phụ phí đêm không được quá 12 tiếng.");
                }

                if (dto.AdditionalFeeStartTime == dto.AdditionalFeeEndTime)
                {
                    context.AddFailure("AdditionalFeeEndTime", "Giờ bắt đầu và kết thúc không được trùng nhau.");
                }
            }
        });
    }
}

// =========================================================================
// 2. GROUP VALIDATOR CHO UPDATE
// =========================================================================

// 2.1. Validator Cha (Điều phối)
public class ServiceUpdateValidator : AbstractValidator<ServiceUpdateDTO>
{
    public ServiceUpdateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);

        RuleFor(x => x).SetInheritanceValidator(v =>
        {
            v.Add(new ServiceStandardUpdateValidator());
            v.Add(new ServiceAirportUpdateValidator());
        });
    }
}

// --- UPDATE ---
public class ServiceStandardUpdateValidator : AbstractValidator<ServiceStandardUpdateDTO>
{
    public ServiceStandardUpdateValidator()
    {
        // Logic y hệt Create
        RuleFor(x => x.Name).NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_NAME)
        .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.Service.LONG_NAME);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage(MessageResponse.AdminManagement.Service.INVALID_AMOUNT);
        RuleFor(x => x.Unit).NotEmpty().WithMessage(MessageResponse.AdminManagement.Service.EMPTY_UNIT_NAME)
        .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.Service.LONG_UNIT);
    }
}

public class ServiceAirportUpdateValidator : AbstractValidator<ServiceAirportUpdateDTO>
{
    public ServiceAirportUpdateValidator()
    {
        RuleFor(x => x.MaxPassengers)
            .GreaterThan(0).When(x => x.MaxPassengers.HasValue).WithMessage(MessageResponse.AdminManagement.Service.MIN_PASSENGERS);

        RuleFor(x => x.MaxLuggage)
            .GreaterThanOrEqualTo(0).When(x => x.MaxLuggage.HasValue).WithMessage(MessageResponse.AdminManagement.Service.MIN_LUGGAGE);

        RuleFor(x => x.RoundTripPrice)
            .GreaterThan(0)
            .When(x => x.HasRoundTrip && x.IsRoundTripPaid)
            .WithMessage(MessageResponse.AdminManagement.Service.INVALID_ROUND_TRIP_PRICE);

        When(x => x.HasNightFee, () =>
        {
            RuleFor(x => x.AdditionalFee).NotNull().GreaterThan(0).WithMessage(MessageResponse.AdminManagement.Service.INVALID_ADDITIONAL_FEE);
            RuleFor(x => x.AdditionalFeeStartTime).NotNull().WithMessage(MessageResponse.AdminManagement.Service.MISSING_ADDITIONAL_FEE_START_TIME);
            RuleFor(x => x.AdditionalFeeEndTime).NotNull().WithMessage(MessageResponse.AdminManagement.Service.MISSING_ADDITIONAL_FEE_END_TIME);
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
