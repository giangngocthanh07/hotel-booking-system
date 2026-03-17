using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.Validators.UserManagement.Register;

using FluentAssertions;
using Xunit;

namespace HotelBooking.test.UnitTests.Validators.UserManagement.Register
{
    public class RegisterCustomerValidatorTests
    {
        private readonly RegisterCustomerValidator _validator;

        public RegisterCustomerValidatorTests()
        {
            _validator = new RegisterCustomerValidator();
        }

        [Fact]
        public void RegisterCustomerValidator_ValidRequest_ShouldPassValidation()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeTrue();
        }

        // Username Tests
        [Fact]
        public void RegisterCustomerValidator_EmptyUsername_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {

                Username = "",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Username" && e.ErrorMessage == MessageResponse.UserManagement.Register.USERNAME_REQUIRED);
        }

        [Fact]
        public void RegisterCustomerValidator_ShortUsername_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "user", // 4 characters, should be at least 8
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Username" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_USERNAME);
        }

        [Fact]
        public void RegisterCustomerValidator_LongUsername_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = new string('a', 51), // 51 characters
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Username" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_USERNAME);
        }

        // Email Tests

        [Fact]
        public void RegisterCustomerValidator_EmptyEmail_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.ErrorMessage == MessageResponse.UserManagement.Register.EMAIL_REQUIRED);
        }

        [Fact]
        public void RegisterCustomerValidator_LongEmail_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = new string('a', 51) + "@example.com", // Local part is 51 characters
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_EMAIL);
        }

        [Fact]
        public void RegisterCustomerValidator_InvalidEmailFormat_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "invalid-email",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_EMAIL_FORMAT);
        }

        // FullName Tests
        [Fact]
        public void RegisterCustomerValidator_EmptyFullName_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "",
                Email = "john@example.com",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "FullName" && e.ErrorMessage == MessageResponse.UserManagement.Register.FULLNAME_REQUIRED);
        }

        [Fact]
        public void RegisterCustomerValidator_LongFullName_ShouldHaveValidationError()
        {
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = new string('a', 51),
                Email = "john@example.com",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "FullName" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_FULLNAME);
        }

        // PhoneNumber Tests
        [Fact]
        public void RegisterCustomerValidator_EmptyPhoneNumber_ShouldHaveValidationError()
        {
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "johndoe@gmail.com",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = ""
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == MessageResponse.UserManagement.Register.EMPTY_PHONE);
        }

        [Fact]
        public void RegisterCustomerValidator_InvalidPhoneNumber_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "abcdefghijk" // Invalid phone number
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_PHONE_FORMAT);
        }

        [Fact]
        public void RegisterCustomerValidator_ShortPhoneNumber_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "123456789" // 9 digits, should be 10
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_PHONE);
        }

        [Fact]
        public void RegisterCustomerValidator_LongPhoneNumber_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password@123",
                ConfirmPassword = "Password@123",
                PhoneNumber = "12345678901" // 11 digits, should be 10
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == MessageResponse.UserManagement.Register.INVALID_PHONE);
        }

        // Password Tests
        [Fact]
        public void RegisterCustomerValidator_EmptyPassword_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "",
                ConfirmPassword = "",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.EMPTY_PASSWORD);
        }

        [Fact]
        public void RegisterCustomerValidator_ShortPassword_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Pass1!",
                ConfirmPassword = "Pass1!",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.SHORT_PASSWORD);
        }

        [Fact]
        public void RegisterCustomerValidator_NoCapitalLetter_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "password@123",
                ConfirmPassword = "password@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.UPPERCASE_LETTER_PASSWORD);
        }

        [Fact]
        public void RegisterCustomerValidator_NoLowerCaseLetter_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "PASSWORD@123",
                ConfirmPassword = "PASSWORD@123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.LOWERCASE_LETTER_PASSWORD);
        }

        [Fact]
        public void RegisterCustomerValidator_NoSpecialCharacter_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password123",
                ConfirmPassword = "Password123",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.SPECIAL_CHARACTER_PASSWORD);
        }

        [Fact]
        public void RegisterCustomerValidator_NoNumberCharacter_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password@",
                ConfirmPassword = "Password@",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password" && e.ErrorMessage == MessageResponse.UserManagement.Register.NUMBER_PASSWORD);
        }

        [Fact]
        public void RegisterCustomerValidator_PasswordsDoNotMatch_ShouldHaveValidationError()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password@123",
                ConfirmPassword = "Password@456",
                PhoneNumber = "0912345678"
            };

            // 2. Act
            var result = _validator.Validate(request);

            // 3. Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "ConfirmPassword" && e.ErrorMessage == MessageResponse.UserManagement.Register.PASSWORDS_DO_NOT_MATCH);
        }
    }
}
