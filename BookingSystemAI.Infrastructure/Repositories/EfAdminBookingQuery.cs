using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAI.Infrastructure.Repositories;

public sealed class EfAdminBookingQuery(ApplicationDbContext dbContext) : IAdminBookingQuery
{
    public async Task<(IReadOnlyList<AdminBookingListItemDto> Items, int TotalCount)> ListAsync(
        AdminBookingListQuery query,
        CancellationToken cancellationToken = default)
    {
        var bookingsQuery = dbContext.Bookings.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.UserId))
            bookingsQuery = bookingsQuery.Where(b => b.UserId == query.UserId);

        var joined = bookingsQuery
            .Join(dbContext.Apartments.AsNoTracking(), b => b.ApartmentId, a => a.Id, (b, a) => new { Booking = b, Apartment = a })
            .Join(dbContext.Users.AsNoTracking(), x => x.Booking.UserId, u => u.Id, (x, u) => new
            {
                x.Booking,
                x.Apartment,
                UserEmail = u.Email ?? string.Empty
            })
            .OrderByDescending(x => x.Booking.Start);

        var totalCount = await joined.CountAsync(cancellationToken);
        var skip = (query.Page - 1) * query.PageSize;
        var rows = await joined.Skip(skip).Take(query.PageSize).ToListAsync(cancellationToken);

        var items = rows.Select(x => new AdminBookingListItemDto(
            x.Booking.Id,
            x.Booking.UserId,
            x.UserEmail,
            x.Booking.ApartmentId,
            x.Apartment.Name,
            x.Apartment.City,
            x.Booking.Start,
            x.Booking.End,
            x.Booking.PricePerNight)).ToList();

        return (items, totalCount);
    }
}
