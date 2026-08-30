
using System.Data;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;

namespace HotelBooking.application.Validators.RequestManagement.Owner;

public class HotelRegistrationValidator : AbstractValidator<HotelRegistrationDTO>
{
    public HotelRegistrationValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.Validation.EMPTY_NAME)
            .MinimumLength(6).WithMessage(MessageResponse.Validation.SHORT_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.Validation.LONG_NAME);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage(MessageResponse.Validation.EMPTY_ADDRESS)
            .MinimumLength(10).WithMessage(MessageResponse.Validation.SHORT_ADDRESS)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_ADDRESS);

        RuleFor(x => x.PropertyTypeId)
            .GreaterThan(0).WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_PROPERTY_TYPEID);

        RuleFor(x => x.StarRating)
            .InclusiveBetween(1, 5).WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_STARRATING)
            .When(x => x.StarRating.HasValue);

        RuleFor(x => x.PublicPhone)
            .NotEmpty().WithMessage(MessageResponse.Validation.EMPTY_PHONE_NUMBER)
            .Matches(@"^\d{10}$").WithMessage(MessageResponse.Validation.INVALID_PHONE_NUMBER);

        RuleFor(x => x.PublicEmail)
            .NotEmpty().WithMessage(MessageResponse.Validation.EMPTY_EMAIL)
            .EmailAddress().WithMessage(MessageResponse.Validation.INVALID_EMAIL_FORMAT);

        RuleFor(x => x.ProvinceId)
            .GreaterThan(0).WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_PROVINCE_ID);

        RuleFor(x => x.WardId)
            .GreaterThan(0).WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_WARD_ID);

        RuleFor(x => x.Latitude)
            .NotNull().When(x => x.Longitude.HasValue).WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_LATITUDE)
            .InclusiveBetween(-90.0, 90.0)
            .WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_LATITUDE)
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .NotNull().When(x => x.Latitude.HasValue).WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_LONGITUDE)
            .InclusiveBetween(-180.0, 180.0)
            .WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_LONGITUDE)
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.TaxCode)
            .NotEmpty().WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_EMPTY_TAX_CODE)
            .Matches(@"^\d{10}(\d{3})?$").WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_TAX_CODE);

        RuleFor(x => x.BusinessLicenseUrl)
            .NotEmpty().WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_EMPTY_BUSINESS_LICENSE_URL)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var outUri)
                 && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
            .WithMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_BUSINESS_LICENSE_URL);
    }


}
