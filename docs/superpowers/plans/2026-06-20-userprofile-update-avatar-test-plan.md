# User Profile Update Avatar Test Case Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a unit test to verify that `AvatarUrl` remains unchanged during User Profile updates.

**Architecture:** Add a unit test to the existing `UserServiceTests` class using Moq to mock dependencies (`IUserRepository`, `IUnitOfWork`, `IValidator<UpdateUserProfileDTO>`).

**Tech Stack:** .NET 9, xUnit, Moq

---

### Task 1: Add Unit Test in `UserServiceTests`

**Files:**
- Modify: `HotelBooking.test/UnitTests/Services/UserManagement/UserServiceTests.cs`
- Modify (Temporarily for TDD verification): `HotelBooking.application/Services/Domains/UserManagement/UserService.cs`

- [ ] **Step 1: Write the unit test**
  Add `UpdateProfileAsync_ValidRequest_DoesNotUpdateAvatarUrl` to [UserServiceTests.cs](file:///E:/Cybersoft/FinalProject/Hotel_Blazor/HotelBooking.test/UnitTests/Services/UserManagement/UserServiceTests.cs):

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

- [ ] **Step 2: Temporarily modify production code to make the test fail**
  Temporarily edit `UpdateProfileAsync` in [UserService.cs](file:///E:/Cybersoft/FinalProject/Hotel_Blazor/HotelBooking.application/Services/Domains/UserManagement/UserService.cs#L61) to simulate a regression where `AvatarUrl` gets updated or cleared:

```csharp
                user.FullName = request.FullName;
                user.PhoneNumber = request.PhoneNumber;
                user.DateOfBirth = request.DateOfBirth.HasValue ? DateOnly.FromDateTime(request.DateOfBirth.Value) : null;
                user.AvatarUrl = "temp_changed_avatar.png"; // Temporarily force failure
```

- [ ] **Step 3: Run the tests to verify failure**
  Run: `dotnet test --filter "FullyQualifiedName~UserServiceTests"`
  Expected: One failure on `UpdateProfileAsync_ValidRequest_DoesNotUpdateAvatarUrl` due to mismatch of avatar URL.

- [ ] **Step 4: Revert production code to verify passing**
  Revert the temporary change in [UserService.cs](file:///E:/Cybersoft/FinalProject/Hotel_Blazor/HotelBooking.application/Services/Domains/UserManagement/UserService.cs#L61) to:

```csharp
                user.FullName = request.FullName;
                user.PhoneNumber = request.PhoneNumber;
                user.DateOfBirth = request.DateOfBirth.HasValue ? DateOnly.FromDateTime(request.DateOfBirth.Value) : null;
```

- [ ] **Step 5: Run all UserServiceTests to verify success**
  Run: `dotnet test --filter "FullyQualifiedName~UserServiceTests"`
  Expected: PASS for all tests (3 tests total).

- [ ] **Step 6: Commit**
  Run:
  ```bash
  git add HotelBooking.test/UnitTests/Services/UserManagement/UserServiceTests.cs
  git commit -m "test(userprofile): add test to verify profile update does not change avatar"
  ```
