# Design Specification - User Profile Update Unit Test (No Avatar Modification)

This design specification details the implementation of a new unit test case to verify that the `AvatarUrl` field is not modified when a user updates their profile.

## Requirements
- Ensure that updating the user profile (via `UpdateProfileAsync`) does not change or overwrite the existing `AvatarUrl` in the database.
- Verify that the updated profile returned in the response still has the original `AvatarUrl`.

## Proposed Solution
Add a new unit test case in [UserServiceTests.cs](file:///E:/Cybersoft/FinalProject/Hotel_Blazor/HotelBooking.test/UnitTests/Services/UserManagement/UserServiceTests.cs):

```csharp
[Fact]
public async Task UpdateProfileAsync_ValidRequest_DoesNotUpdateAvatarUrl()
{
    // Arrange
    int userId = 1;
    string originalAvatarUrl = "https://example.com/avatar.png";
    var request = new UpdateUserProfileDTO
    {
        FullName = "New Name",
        PhoneNumber = "0123456789",
        DateOfBirth = new DateTime(1990, 1, 1)
    };

    var user = new User 
    { 
        Id = userId, 
        UserName = "testuser", 
        Email = "test@example.com",
        AvatarUrl = originalAvatarUrl
    };
    
    _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
    
    // Mock GetByIdAsync logic inside the service which calls GetUserWithRoles
    _mockUserRepo.Setup(r => r.GetUserWithRoles(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
        .ReturnsAsync(new User 
        { 
            Id = userId, 
            UserName = "testuser", 
            AvatarUrl = user.AvatarUrl, // Keep original
            UserRoles = new List<UserRole>() 
        });

    // Act
    var result = await _service.UpdateProfileAsync(userId, request);

    // Assert
    Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
    Assert.Equal(originalAvatarUrl, user.AvatarUrl); // Verify user object kept original AvatarUrl
    Assert.Equal(originalAvatarUrl, result.Content?.AvatarUrl); // Verify returned DTO has original AvatarUrl
    _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
}
```

## Testing and Verification
- Run the specific unit test: `dotnet test --filter "FullyQualifiedName~UserServiceTests"`
- Verify that the test compiles and passes.
