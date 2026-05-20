using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Services;
using BookingSystemAI.Domain;
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

        SetupApartment(apartmentId);
        _bookingRepository.Setup(r => r.TryAddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.CreateAsync("client-1", request);

        result.Succeeded.ShouldBeFalse();
        result.FailureReason.ShouldBe(CreateBookingFailureReason.Conflict);
    }

    [Fact]
    public async Task CreateAsync_ShouldSnapshotApartmentListingFields_WhenBookingSucceeds()
    {
        var apartmentId = Guid.NewGuid();
        var request = new CreateBookingRequestDto(
            apartmentId,
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2));

        SetupApartment(apartmentId, pricePerNight: 120, guestCount: 3, amenities: [Amenity.LargeBed, Amenity.Shower]);
        Booking? captured = null;
        _bookingRepository
            .Setup(r => r.TryAddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>((b, _) => captured = b)
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync("client-1", request);

        result.Succeeded.ShouldBeTrue();
        captured.ShouldNotBeNull();
        captured!.PricePerNight.ShouldBe(120);
        captured.GuestCount.ShouldBe(3);
        captured.Amenities.ShouldBe([Amenity.LargeBed, Amenity.Shower]);
        result.Response!.PricePerNight.ShouldBe(120);
        result.Response.GuestCount.ShouldBe(3);
        result.Response.Amenities.ShouldBe(["LargeBed", "Shower"]);
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

    private void SetupApartment(
        Guid apartmentId,
        decimal pricePerNight = 0,
        int guestCount = 1,
        IReadOnlyList<Amenity>? amenities = null) =>
        _apartmentRepository.Setup(r => r.GetByIdAsync(apartmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Apartment
            {
                Id = apartmentId,
                HostId = "host-1",
                Name = "Test",
                Description = "Desc",
                PricePerNight = pricePerNight,
                GuestCount = guestCount,
                Amenities = amenities ?? []
            });
}
