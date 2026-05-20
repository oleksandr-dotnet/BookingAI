using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Services;

public class BookingService(IApartmentRepository apartmentRepository, IBookingRepository bookingRepository)
    : IBookingService
{
    public async Task<CreateBookingResult> CreateAsync(string userId, CreateBookingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateRequest(request);
        if (validationErrors is not null)
            return CreateBookingResult.ValidationFailure(validationErrors);

        if (!await apartmentRepository.ExistsAsync(request.ApartmentId, cancellationToken))
            return CreateBookingResult.NotFound();

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            ApartmentId = request.ApartmentId,
            UserId = userId,
            Start = request.Start,
            End = request.End
        };

        var added = await bookingRepository.TryAddAsync(booking, cancellationToken);
        if (!added)
            return CreateBookingResult.Conflict();

        return CreateBookingResult.Success(new BookingResponseDto(
            booking.Id,
            booking.ApartmentId,
            booking.Start,
            booking.End));
    }

    public async Task<IReadOnlyList<BookingResponseDto>> ListMineAsync(string userId,
        CancellationToken cancellationToken = default)
    {
        var bookings = await bookingRepository.ListByUserIdAsync(userId, cancellationToken);
        return bookings
            .Select(b => new BookingResponseDto(b.Id, b.ApartmentId, b.Start, b.End))
            .ToList();
    }

    private static IReadOnlyDictionary<string, string[]>? ValidateRequest(CreateBookingRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.ApartmentId == Guid.Empty)
            errors["apartmentId"] = ["ApartmentId is required."];

        if (request.End <= request.Start)
            errors["end"] = ["End must be after start."];

        if (request.Start < DateTimeOffset.UtcNow)
            errors["start"] = ["Start cannot be in the past."];

        return errors.Count == 0 ? null : errors;
    }
}
