using System.ComponentModel.DataAnnotations;

namespace HotelBooking.webapp.ViewModels.Validations;

/// <summary>
/// Validates that a nullable <see cref="DateTime"/> value is strictly in the past (before today).
/// Mirrors the FluentValidation rule: .LessThan(DateTime.Today)
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PastDateAttribute : ValidationAttribute
{
    public PastDateAttribute()
    {
        ErrorMessage = "Ngày sinh phải ở quá khứ.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success; // .When(x => x.DateOfBirth.HasValue) — skip if null

        if (value is DateTime date && date < DateTime.Today)
            return ValidationResult.Success;

        return new ValidationResult(ErrorMessage);
    }
}
