using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Listing;

namespace BookingSystemAI.Application.Services;

public class ApartmentService(IApartmentRepository apartmentRepository, IBookingRepository bookingRepository)
    : IApartmentService
{
    public async Task<IReadOnlyList<ApartmentListItemDto>> ListCatalogAsync(ListApartmentsQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateQuery(query);
        if (validationErrors is not null)
            throw new ApartmentQueryValidationException(validationErrors);

        var apartments = await apartmentRepository.ListAllAsync(cancellationToken);

        if (query.From is null && query.To is null)
        {
            return apartments.Select(a => ApartmentDtoMapper.ToListItem(a)).ToList();
        }

        var from = query.From!.Value;
        var to = query.To!.Value;
        var items = new List<ApartmentListItemDto>();

        foreach (var apartment in apartments)
        {
            var hasOverlap = await bookingRepository.HasOverlapAsync(apartment.Id, from, to, cancellationToken);
            var isAvailable = !hasOverlap;
            if (query.AvailableOnly && !isAvailable)
                continue;

            items.Add(ApartmentDtoMapper.ToListItem(apartment, isAvailable));
        }

        return items;
    }

    private static IReadOnlyDictionary<string, string[]>? ValidateQuery(ListApartmentsQueryDto query)
    {
        var hasFrom = query.From.HasValue;
        var hasTo = query.To.HasValue;

        if (query.AvailableOnly && (!hasFrom || !hasTo))
        {
            return new Dictionary<string, string[]>
            {
                ["availableOnly"] = ["from and to are required when availableOnly is true."]
            };
        }

        if (hasFrom != hasTo)
        {
            return new Dictionary<string, string[]>
            {
                ["from"] = ["from and to must both be provided."]
            };
        }

        if (hasFrom && query.From!.Value >= query.To!.Value)
        {
            return new Dictionary<string, string[]>
            {
                ["to"] = ["to must be after from."]
            };
        }

        return null;
    }
}

public sealed class ApartmentQueryValidationException(IReadOnlyDictionary<string, string[]> errors) : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
