using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Services;

public interface IApartmentService
{
    Task<IReadOnlyList<ApartmentListItemDto>> ListCatalogAsync(ListApartmentsQueryDto query,
        CancellationToken cancellationToken = default);
}
