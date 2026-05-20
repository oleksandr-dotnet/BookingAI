using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Services;

public interface IApartmentUpsertService
{
    Task<UpsertApartmentResult> UpsertAsync(string hostId, UpsertApartmentRequestDto request,
        CancellationToken cancellationToken = default);
}
