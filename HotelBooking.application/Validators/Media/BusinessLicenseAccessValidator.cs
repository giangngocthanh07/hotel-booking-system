using FluentValidation;
using HotelBooking.application.Services.Domains.Media;

namespace HotelBooking.application.Validators.Media;

public class BusinessLicenseAccessValidator : AbstractValidator<BusinessLicenseAccessRequest>
{
    public BusinessLicenseAccessValidator()
    {
        RuleFor(request => request.RequestId).GreaterThan(0);
        RuleFor(request => request.UserId).GreaterThan(0);
    }
}
