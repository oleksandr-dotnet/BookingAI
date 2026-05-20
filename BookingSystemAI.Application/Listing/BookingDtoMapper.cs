using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Listing;

public static class BookingDtoMapper
{
    public static BookingResponseDto ToResponse(Booking booking) =>
        new(
            booking.Id,
            booking.ApartmentId,
            booking.Start,
            booking.End,
            booking.PricePerNight,
            booking.GuestCount,
            AmenityMapper.ToNames(booking.Amenities));
}
