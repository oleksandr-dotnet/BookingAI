using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Services;
using BookingSystemAI.Domain.Entities;
using Moq;
using Shouldly;

namespace BookingSystemAI.Application.Tests.Services;

public class BookingServiceTests
{
    private readonly Mock<IApartmentRepository> _apartmentRepository = new();
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _sut = new BookingService(_apartmentRepository.Object, _bookingRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnConflict_WhenOverlapExists()
    {
        var apartmentId = Guid.NewGuid();
        var request = new CreateBookingRequestDto(
            apartmentId,
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2));

        _apartmentRepository.Setup(r => r.ExistsAsync(apartmentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _bookingRepository.Setup(r => r.TryAddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.CreateAsync("client-1", request);

        result.Succeeded.ShouldBeFalse();
        result.FailureReason.ShouldBe(CreateBookingFailureReason.Conflict);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnValidationFailure_WhenEndIsBeforeStart()
    {
        var request = new CreateBookingRequestDto(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(2),
            DateTimeOffset.UtcNow.AddHours(1));

        var result = await _sut.CreateAsync("client-1", request);

        result.Succeeded.ShouldBeFalse();
        result.FailureReason.ShouldBe(CreateBookingFailureReason.Validation);
        result.ValidationErrors!["end"].ShouldContain("End must be after start.");
    }
}
