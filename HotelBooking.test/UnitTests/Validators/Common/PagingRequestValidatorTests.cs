
using FluentAssertions;
using FluentValidation.TestHelper;
using HotelBooking.application.Validators.Common;

namespace HotelBooking.test.UnitTests.Validators.Common;

public class PagingRequestValidatorTests
{
    PagingRequestValidator _validator;

    public PagingRequestValidatorTests()
    {
        _validator = new PagingRequestValidator();
    }

    [Fact]
    public async Task Validate_ValidRequest_ShouldNotHaveAnyErrors()
    {
        // 1. Arrange
        var pagingRequest = new PagingRequest
        {
            PageIndex = 1,
            PageSize = 10

        };

        // 2. Act
        var result = await _validator.TestValidateAsync(pagingRequest);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_InvalidPageIndex_ShouldHaveErrors()
    {
        // 1. Arrange
        var pagingRequest = new PagingRequest
        {
            PageIndex = 0,
            PageSize = 10
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(pagingRequest);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.PageIndex)
            .WithErrorMessage(MessageResponse.Pagination.INVALID_PAGE_INDEX);

    }

    [Fact]
    public async Task Validate_InvalidPageSize_ShouldHaveErrors()
    {
        // 1. Arrange
        var pagingRequest = new PagingRequest
        {
            PageIndex = 1,
            PageSize = 0
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(pagingRequest);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(MessageResponse.Pagination.INVALID_PAGE_SIZE);

    }

    [Fact]
    public async Task Validate_NullPageIndex_ShouldHaveErrors()
    {
        // 1. Arrange
        var pagingRequest = new PagingRequest
        {
            PageIndex = null!,
            PageSize = 10
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(pagingRequest);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.PageIndex)
            .WithErrorMessage(MessageResponse.Pagination.MISSING_PAGE_INDEX);

    }

    [Fact]
    public async Task Validate_NullPageSize_ShouldHaveErrors()
    {
        // 1. Arrange
        var pagingRequest = new PagingRequest
        {
            PageIndex = 1,
            PageSize = null!
        };

        // 2. Act
        var result = await _validator.TestValidateAsync(pagingRequest);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(MessageResponse.Pagination.MISSING_PAGE_SIZE);
    }

    [Fact]
    public async Task Validate_PageSizeGreaterThan100_ShouldHaveErrors()
    {
        // 1. Arrange
        PagingRequest pagingRequest = new PagingRequest
        {
            PageIndex = 1,
            PageSize = 101
        };


        // 2. Act
        var result = await _validator.TestValidateAsync(pagingRequest);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(MessageResponse.Pagination.PAGE_SIZE_TOO_LARGE);
    }

}