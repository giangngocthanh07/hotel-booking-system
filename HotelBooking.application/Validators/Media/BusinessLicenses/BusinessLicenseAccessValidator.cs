using HotelBooking.application.DTOs.Media.BusinessLicenses;
using FluentValidation;

namespace HotelBooking.application.Validators.Media.BusinessLicenses;

public class BusinessLicenseAccessValidator : AbstractValidator<BusinessLicenseAccessRequest>
{
    public BusinessLicenseAccessValidator()
    {
        RuleFor(request => request.RequestId).GreaterThan(0);
        RuleFor(request => request.UserId).GreaterThan(0);
    }
}
