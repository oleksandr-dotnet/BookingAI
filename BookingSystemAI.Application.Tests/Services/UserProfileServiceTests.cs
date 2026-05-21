using BookingSystemAI.Application;
using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Services;
using Moq;
using Shouldly;

namespace BookingSystemAI.Application.Tests.Services;

public class UserProfileServiceTests
{
    private readonly Mock<IUserProfileStore> _profileStore = new();
    private readonly Mock<IImageUrlValidator> _imageUrlValidator = new();
    private readonly UserProfileService _sut;

    public UserProfileServiceTests()
    {
        _imageUrlValidator
            .Setup(v => v.ValidateUrls(It.IsAny<IReadOnlyList<string>?>()))
            .Returns((IReadOnlyList<string>? urls) => null);
        _sut = new UserProfileService(_profileStore.Object, _imageUrlValidator.Object);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_ShouldReturnValidationFailure_WhenClientMissingBirthDate()
    {
        var record = CreateRecord([ApplicationRoles.Client]);
        _profileStore.Setup(s => s.GetByUserIdAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync(record);

        var result = await _sut.UpdateMyProfileAsync(
            "u1",
            new UpdateUserProfileRequestDto("Jane", "Doe", null, null));

        result.ValidationErrors!["dateOfBirth"].ShouldNotBeEmpty();
        _profileStore.Verify(
            s => s.UpdateProfileAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateOnly?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_ShouldSucceed_WhenAdminWithoutBirthDate()
    {
        var record = CreateRecord([ApplicationRoles.Admin]);
        _profileStore.Setup(s => s.GetByUserIdAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _profileStore
            .Setup(s => s.UpdateProfileAsync("u1", "Ada", "Admin", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _profileStore.SetupSequence(s => s.GetByUserIdAsync("u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(record)
            .ReturnsAsync(new UserProfileRecord
            {
                UserId = record.UserId,
                Email = record.Email,
                UserName = record.UserName,
                Roles = record.Roles,
                FirstName = "Ada",
                LastName = "Admin"
            });

        var result = await _sut.UpdateMyProfileAsync(
            "u1",
            new UpdateUserProfileRequestDto("Ada", "Admin", null, null));

        result.ValidationErrors.ShouldBeNull();
        result.Response!.ProfileComplete.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateMyProfileAsync_ShouldSucceed_WhenHostProvidesFullProfile()
    {
        var birth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30));
        var record = CreateRecord([ApplicationRoles.Host]);
        _profileStore.Setup(s => s.GetByUserIdAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _profileStore
            .Setup(s => s.UpdateProfileAsync("u1", "Host", "User", birth, "https://res.cloudinary.com/x/a.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _profileStore.SetupSequence(s => s.GetByUserIdAsync("u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(record)
            .ReturnsAsync(new UserProfileRecord
            {
                UserId = record.UserId,
                Email = record.Email,
                UserName = record.UserName,
                Roles = record.Roles,
                FirstName = "Host",
                LastName = "User",
                DateOfBirth = birth,
                ProfileImageUrl = "https://res.cloudinary.com/x/a.jpg"
            });

        var result = await _sut.UpdateMyProfileAsync(
            "u1",
            new UpdateUserProfileRequestDto("Host", "User", birth, "https://res.cloudinary.com/x/a.jpg"));

        result.Response!.ProfileComplete.ShouldBeTrue();
        result.Response.DisplayName.ShouldBe("Host User");
    }

    private static UserProfileRecord CreateRecord(IReadOnlyList<string> roles) =>
        new()
        {
            UserId = "u1",
            Email = "user@example.com",
            UserName = "user@example.com",
            Roles = roles
        };
}
