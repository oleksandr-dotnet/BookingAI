using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Services;
using Moq;
using Shouldly;

namespace BookingSystemAI.Application.Tests.Services;

public class AdminBookingServiceTests
{
    private readonly Mock<IAdminBookingQuery> _adminBookingQuery = new();
    private readonly AdminBookingService _sut;

    public AdminBookingServiceTests()
    {
        _sut = new AdminBookingService(_adminBookingQuery.Object);
    }

    [Fact]
    public async Task ListBookingsAsync_ShouldUseDefaultPageSize_WhenPageSizeIsZero()
    {
        var query = new AdminBookingListQuery(null, 1, 0);
        _adminBookingQuery
            .Setup(q => q.ListAsync(It.IsAny<AdminBookingListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<AdminBookingListItemDto>(), 0));

        var result = await _sut.ListBookingsAsync(query);

        result.Succeeded.ShouldBeTrue();
        result.Response!.PageSize.ShouldBe(20);
    }
}
