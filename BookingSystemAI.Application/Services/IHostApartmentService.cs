using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Services;

public interface IHostApartmentService
{
    Task<CreateApartmentResult> CreateAsync(string hostId, CreateApartmentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApartmentResponseDto>> ListMineAsync(string hostId,
        CancellationToken cancellationToken = default);

    Task<UpdateApartmentResult> UpdateAsync(string hostId, Guid apartmentId, UpdateApartmentRequestDto request,
        CancellationToken cancellationToken = default);
}
