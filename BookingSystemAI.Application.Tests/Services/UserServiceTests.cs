using BookingSystemAI.Application;
using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Services;
using BookingSystemAI.Domain.Entities;
using Moq;
using Shouldly;

namespace BookingSystemAI.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IIdentityUserManager> _identityUserManager = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_identityUserManager.Object, _jwtTokenService.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnUserId_WhenRegistrationSucceeds()
    {
        var request = new RegisterRequestDto("user@example.com", "Password1", ApplicationRoles.Client);
        _identityUserManager
            .Setup(m => m.CreateAsync(request.Email, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityCreateUserResult.Success("user-123"));
        _identityUserManager
            .Setup(m => m.AssignRoleAsync("user-123", ApplicationRoles.Client, It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityAssignRoleResult.Success());

        var result = await _sut.RegisterAsync(request);

        result.Succeeded.ShouldBeTrue();
        result.Response!.UserId.ShouldBe("user-123");
        result.ValidationErrors.ShouldBeNull();
        _identityUserManager.Verify(
            m => m.AssignRoleAsync("user-123", ApplicationRoles.Client, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnValidationErrors_WhenRoleIsInvalid()
    {
        var request = new RegisterRequestDto("user@example.com", "Password1", "Admin");

        var result = await _sut.RegisterAsync(request);

        result.Succeeded.ShouldBeFalse();
        result.ValidationErrors!["role"].ShouldContain("Role must be Host or Client.");
        _identityUserManager.Verify(
            m => m.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnValidationErrors_WhenEmailIsMissing()
    {
        var request = new RegisterRequestDto("", "Password1", ApplicationRoles.Client);

        var result = await _sut.RegisterAsync(request);

        result.Succeeded.ShouldBeFalse();
        result.ValidationErrors!["email"].ShouldContain("Email is required.");
        _identityUserManager.Verify(
            m => m.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnValidationErrors_WhenPasswordIsMissing()
    {
        var request = new RegisterRequestDto("user@example.com", "", ApplicationRoles.Host);

        var result = await _sut.RegisterAsync(request);

        result.Succeeded.ShouldBeFalse();
        result.ValidationErrors!["password"].ShouldContain("Password is required.");
        _identityUserManager.Verify(
            m => m.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnIdentityErrors_WhenUserCreationFails()
    {
        var request = new RegisterRequestDto("user@example.com", "weak", ApplicationRoles.Client);
        var identityErrors = new Dictionary<string, string[]>
        {
            ["password"] = ["Passwords must have at least one digit."]
        };
        _identityUserManager
            .Setup(m => m.CreateAsync(request.Email, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityCreateUserResult.Failure(identityErrors));

        var result = await _sut.RegisterAsync(request);

        result.Succeeded.ShouldBeFalse();
        result.ValidationErrors!["password"].ShouldContain("Passwords must have at least one digit.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var request = new LoginRequestDto("user@example.com", "Password1");
        var user = new User { Id = "user-123", Email = request.Email };
        var token = new AuthResponseDto("jwt-token", 3600);
        _identityUserManager
            .Setup(m => m.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityUserManager
            .Setup(m => m.CheckPasswordAsync(user, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _identityUserManager
            .Setup(m => m.GetRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { ApplicationRoles.Client });
        _jwtTokenService.Setup(j => j.CreateToken(It.Is<User>(u => u.Roles.Contains(ApplicationRoles.Client))))
            .Returns(token);

        var result = await _sut.LoginAsync(request);

        result.Succeeded.ShouldBeTrue();
        result.Response.ShouldBe(token);
        _jwtTokenService.Verify(j => j.CreateToken(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnValidationErrors_WhenEmailIsMissing()
    {
        var request = new LoginRequestDto("  ", "Password1");

        var result = await _sut.LoginAsync(request);

        result.Succeeded.ShouldBeFalse();
        result.ValidationErrors!["email"].ShouldContain("Email is required.");
        _identityUserManager.Verify(
            m => m.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _jwtTokenService.Verify(j => j.CreateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUnauthorized_WhenUserIsNotFound()
    {
        var request = new LoginRequestDto("missing@example.com", "Password1");
        _identityUserManager
            .Setup(m => m.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(request);

        result.Succeeded.ShouldBeFalse();
        result.Response.ShouldBeNull();
        result.ValidationErrors.ShouldBeNull();
        _jwtTokenService.Verify(j => j.CreateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
    {
        var request = new LoginRequestDto("user@example.com", "WrongPassword1");
        var user = new User { Id = "user-123", Email = request.Email };
        _identityUserManager
            .Setup(m => m.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityUserManager
            .Setup(m => m.CheckPasswordAsync(user, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.LoginAsync(request);

        result.Succeeded.ShouldBeFalse();
        result.Response.ShouldBeNull();
        _jwtTokenService.Verify(j => j.CreateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldPassCancellationToken_WhenProvided()
    {
        var request = new LoginRequestDto("user@example.com", "Password1");
        var user = new User { Id = "user-123", Email = request.Email };
        using var cts = new CancellationTokenSource();
        _identityUserManager
            .Setup(m => m.FindByEmailAsync(request.Email, cts.Token))
            .ReturnsAsync(user);
        _identityUserManager
            .Setup(m => m.CheckPasswordAsync(user, request.Password, cts.Token))
            .ReturnsAsync(true);
        _identityUserManager
            .Setup(m => m.GetRolesAsync(user.Id, cts.Token))
            .ReturnsAsync(new List<string> { ApplicationRoles.Host });
        _jwtTokenService.Setup(j => j.CreateToken(It.IsAny<User>())).Returns(new AuthResponseDto("token", 60));

        await _sut.LoginAsync(request, cts.Token);

        _identityUserManager.Verify(m => m.FindByEmailAsync(request.Email, cts.Token), Times.Once);
        _identityUserManager.Verify(m => m.CheckPasswordAsync(user, request.Password, cts.Token), Times.Once);
        _identityUserManager.Verify(m => m.GetRolesAsync(user.Id, cts.Token), Times.Once);
    }
}
