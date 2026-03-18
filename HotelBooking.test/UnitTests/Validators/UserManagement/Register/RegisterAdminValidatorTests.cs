using FluentAssertions;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.Validators.UserManagement.Register;

namespace HotelBooking.test.UnitTests.Validators.UserManagement.Register
{
    public class RegisterAdminValidatorTests
    {
        private readonly RegisterAdminValidator _validator;

        public RegisterAdminValidatorTests()
        {
            _validator = new RegisterAdminValidator();
        }

        [Fact]
        public void RegisterAdminValidator_ValidRequest_ShouldPassValidation()
        {
            // 1. Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeTrue();
        }


        [Fact]
        public void RegisterAdminValidator_EmptyUsername_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterAdminDTO
            {
                Username = "",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Username" && e.ErrorMessage == MessageResponse.UserManagement.Register.USERNAME_REQUIRED);
        }

        [Fact]
        public void RegisterAdminValidator_ShortUsername_ShouldHaveValidationError()
        {
            // 1.Arrange
            var model = new RegisterAdminDTO
            {
                Username = "ab",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Username" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_USERNAME);
        }

        [Fact]
        public void RegisterAdminValidator_LongUsername_ShouldHaveValidationError()
        {
            // 1.Arrange
            var request = new RegisterAdminDTO
            {
                Username = new string('a', 51),
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Username" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_USERNAME);
        }

        [Fact]
        public void RegisterAdminValidator_EmptyEmail_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "",
                PhoneNumber = "0912345678",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.ErrorMessage == MessageResponse.UserManagement.Register.EMAIL_REQUIRED);
        }

        [Fact]
        public void RegisterAdminValidator_LongEmail_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = new string('a', 51) + "@gmail.com",
                PhoneNumber = "0912345678",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_EMAIL);
        }

        [Fact]
        public void RegisterAdminValidator_InvalidEmailFormat_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "invalidemail",
                PhoneNumber = "0912345678",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_EMAIL_FORMAT);
        }

        [Fact]
        public void RegisterAdminValidator_EmptyFullName_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert 
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "FullName" && e.ErrorMessage == MessageResponse.UserManagement.Register.FULLNAME_REQUIRED);
        }

        [Fact]
        public void RegisterAdminValidator_LongFullName_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = new string('a', 51),
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "FullName" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_FULLNAME);
        }


        [Fact]
        public void RegisterAdminValidator_EmptyPhoneNumber_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == MessageResponse.UserManagement.Register.EMPTY_PHONE);
        }

        [Fact]
        public void RegisterAdminValidator_InvalidPhoneNumber_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "invalidphone",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_PHONE);
        }

        [Fact]
        public void RegisterAdminValidator_ShortPhoneNumber_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "091234567",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_PHONE);
        }

        [Fact]
        public void RegisterAdminValidator_LongPhoneNumber_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "09123456789",
                Password = "ValidPass@123",
                ConfirmPassword = "ValidPass@123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_PHONE);
        }

        [Fact]
        public void RegisterAdminValidator_EmptyPassword_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "",
                ConfirmPassword = "ValidPass@123"
            };
            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.EMPTY_PASSWORD);
        }

        [Fact]
        public void RegisterAdminValidator_ShortPassword_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "shoR1!",
                ConfirmPassword = "shoR1!"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.SHORT_PASSWORD);
        }

        [Fact]
        public void RegisterAdminValidator_NoCapitalLetter_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "nouppercase1!",
                ConfirmPassword = "nouppercase1!"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.UPPERCASE_LETTER_PASSWORD);
        }

        [Fact]
        public void RegisterAdminValidator_NoLowerCaseLetter_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "NOLOWERCASE1!",
                ConfirmPassword = "NOLOWERCASE1!"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.LOWERCASE_LETTER_PASSWORD);
        }

        [Fact]
        public void RegisterAdminValidator_NoSpecialCharacter_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "NoSpecialChar1",
                ConfirmPassword = "NoSpecialChar1"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.SPECIAL_CHARACTER_PASSWORD);
        }

        [Fact]
        public void RegisterAdminValidator_NoNumberCharacter_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "NoNumberChar!",
                ConfirmPassword = "NoNumberChar!"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.NUMBER_PASSWORD);
        }

        [Fact]
        public void RegisterAdminValidator_PasswordsDoNotMatch_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RegisterAdminDTO
            {
                Username = "validuser",
                FullName = "Valid User",
                Email = "valid@gmail.com",
                PhoneNumber = "0912345678",
                Password = "NoMismatch1!",
                ConfirmPassword = "Mismatch1!"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "ConfirmPassword" && e.ErrorMessage == MessageResponse.UserManagement.Register.PASSWORDS_DO_NOT_MATCH);
        }
    }
}