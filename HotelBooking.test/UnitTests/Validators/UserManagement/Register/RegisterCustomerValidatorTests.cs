using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.Validators.UserManagement.Register;

public class RegisterCustomerValidatorTests
{
    private readonly RegisterCustomerValidator _validator;

    public RegisterCustomerValidatorTests()
    {
        _validator = new RegisterCustomerValidator();
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Empty()
    {
        var model = new RegisterCustomerDTO
        {

            FullName = "",
            Email = "johndoe@example.com",
            Password = "password",
        };
    }
}