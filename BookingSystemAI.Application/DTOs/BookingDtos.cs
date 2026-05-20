namespace BookingSystemAI.Application.DTOs;

public record CreateBookingRequestDto(Guid ApartmentId, DateTimeOffset Start, DateTimeOffset End);

public record BookingResponseDto(
    Guid Id,
    Guid ApartmentId,
    DateTimeOffset Start,
    DateTimeOffset End,
    decimal PricePerNight,
    int GuestCount,
    IReadOnlyList<string> Amenities);
