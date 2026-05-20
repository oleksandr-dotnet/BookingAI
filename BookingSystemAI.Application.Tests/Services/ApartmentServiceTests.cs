using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Services;
using Moq;
using Shouldly;

namespace BookingSystemAI.Application.Tests.Services;

public class ApartmentServiceTests
{
    private readonly Mock<IApartmentRepository> _apartmentRepository = new();
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly ApartmentService _sut;

    public ApartmentServiceTests()
    {
        _sut = new ApartmentService(_apartmentRepository.Object, _bookingRepository.Object);
    }

    [Fact]
    public async Task ListCatalogAsync_ShouldThrowValidationException_WhenAvailableOnlyWithoutWindow()
    {
        var query = new ListApartmentsQueryDto(null, null, true);

        var ex = await Should.ThrowAsync<ApartmentQueryValidationException>(() => _sut.ListCatalogAsync(query));

        ex.Errors["availableOnly"].ShouldContain("from and to are required when availableOnly is true.");
    }
}
