using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Domain.Entities;
using BookingSystemAI.Infrastructure.Data;
using BookingSystemAI.Infrastructure.Data.Entities;
using BookingSystemAI.Infrastructure.Sql;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAI.Infrastructure.Repositories;

public sealed class BookingRepository(ApplicationDbContext dbContext) : IBookingRepository
{
    public async Task<bool> HasOverlapAsync(Guid apartmentId, DateTimeOffset start, DateTimeOffset end,
        CancellationToken cancellationToken = default) =>
        await dbContext.Bookings.AnyAsync(
            b => b.ApartmentId == apartmentId && b.Start < end && b.End > start,
            cancellationToken);

    public async Task<bool> TryAddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var hasOverlap = await dbContext.Bookings.AnyAsync(
            b => b.ApartmentId == booking.ApartmentId && b.Start < booking.End && b.End > booking.Start,
            cancellationToken);

        if (hasOverlap)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        dbContext.Bookings.Add(new BookingRecord
        {
            Id = booking.Id,
            ApartmentId = booking.ApartmentId,
            UserId = booking.UserId,
            Start = booking.Start,
            End = booking.End,
            PricePerNight = booking.PricePerNight,
            GuestCount = booking.GuestCount,
            Amenities = EntityMapping.MapAmenityNames(booking.Amenities)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<Booking>> ListByUserIdAsync(string userId,
        CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.Start)
            .ToListAsync(cancellationToken);

        return records.Select(EntityMapping.MapToDomain).ToList();
    }
}
