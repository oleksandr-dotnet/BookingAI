using BookingSystemAI.Application;
using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Services;
using Moq;
using Shouldly;

namespace BookingSystemAI.Application.Tests.Services;

public class AdminUserServiceTests
{
    private readonly Mock<IAdminUserQuery> _adminUserQuery = new();
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<IAdminIdentityOperations> _adminIdentityOperations = new();
    private readonly AdminUserService _sut;

    public AdminUserServiceTests()
    {
        _sut = new AdminUserService(_adminUserQuery.Object, _bookingRepository.Object, _adminIdentityOperations.Object);
    }

    [Fact]
    public async Task ListUsersAsync_ShouldReturnValidationFailure_WhenRoleIsInvalid()
    {
        var query = new AdminUserListQuery("SuperUser", null, 1, 20, "email");

        var result = await _sut.ListUsersAsync(query);

        result.Succeeded.ShouldBeFalse();
        result.ValidationErrors!["role"].ShouldNotBeEmpty();
        _adminUserQuery.Verify(
            q => q.ListAsync(It.IsAny<AdminUserListQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ListUsersAsync_ShouldUseDefaultPageSize_WhenPageSizeIsZero()
    {
        var query = new AdminUserListQuery(null, null, 1, 0, "email");
        _adminUserQuery
            .Setup(q => q.ListAsync(It.IsAny<AdminUserListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<AdminUserRecord>(), 0));

        var result = await _sut.ListUsersAsync(query);

        result.Succeeded.ShouldBeTrue();
        result.Response!.PageSize.ShouldBe(20);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _adminUserQuery
            .Setup(q => q.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminUserRecord?)null);

        var result = await _sut.GetUserByIdAsync("missing");

        result.NotFound.ShouldBeTrue();
        result.Response.ShouldBeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnDetail_WhenUserExists()
    {
        var record = new AdminUserRecord
        {
            UserId = "user-1",
            Email = "user@example.com",
            UserName = "user@example.com",
            Roles = [ApplicationRoles.Client],
            EmailConfirmed = true,
            LockoutEnabled = false,
            LockoutEnd = null,
            SourceCompanyId = null,
            ExternalId = null,
            BookingCount = 2
        };

        _adminUserQuery
            .Setup(q => q.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        var result = await _sut.GetUserByIdAsync("user-1");

        result.NotFound.ShouldBeFalse();
        result.Response!.UserId.ShouldBe("user-1");
        result.Response.Roles.ShouldBe([ApplicationRoles.Client]);
    }

    [Fact]
    public async Task SetRolesAsync_ShouldReturnConflict_WhenRemovingLastAdmin()
    {
        var record = new AdminUserRecord
        {
            UserId = "admin-1",
            Email = "admin@example.com",
            UserName = "admin@example.com",
            Roles = [ApplicationRoles.Admin],
            EmailConfirmed = true,
            LockoutEnabled = false,
            LockoutEnd = null,
            BookingCount = 0
        };

        _adminUserQuery.Setup(q => q.GetByIdAsync("admin-1", It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _adminIdentityOperations
            .Setup(o => o.UserExistsAsync("admin-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _adminIdentityOperations
            .Setup(o => o.CountUsersInRoleAsync(ApplicationRoles.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.SetRolesAsync("admin-1", new SetUserRolesRequestDto([ApplicationRoles.Client]));

        result.Conflict.ShouldBeTrue();
        _adminIdentityOperations.Verify(
            o => o.SetRolesAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetRolesAsync_ShouldReturnValidationFailure_WhenRolesEmpty()
    {
        _adminIdentityOperations
            .Setup(o => o.UserExistsAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.SetRolesAsync("user-1", new SetUserRolesRequestDto([]));

        result.ValidationErrors.ShouldNotBeNull();
    }
}
