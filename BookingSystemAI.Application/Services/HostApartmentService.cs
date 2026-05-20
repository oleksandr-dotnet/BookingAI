using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Services;

public class HostApartmentService(IApartmentRepository apartmentRepository) : IHostApartmentService
{
    public async Task<CreateApartmentResult> CreateAsync(string hostId, CreateApartmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateRequest(request);
        if (validationErrors is not null)
            return CreateApartmentResult.ValidationFailure(validationErrors);

        var apartment = new Apartment
        {
            Id = Guid.NewGuid(),
            HostId = hostId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty
        };

        await apartmentRepository.AddAsync(apartment, cancellationToken);
        return CreateApartmentResult.Success(new ApartmentResponseDto(apartment.Id, apartment.Name, apartment.Description));
    }

    public async Task<IReadOnlyList<ApartmentResponseDto>> ListMineAsync(string hostId,
        CancellationToken cancellationToken = default)
    {
        var apartments = await apartmentRepository.ListByHostIdAsync(hostId, cancellationToken);
        return apartments
            .Select(a => new ApartmentResponseDto(a.Id, a.Name, a.Description))
            .ToList();
    }

    private static IReadOnlyDictionary<string, string[]>? ValidateRequest(CreateApartmentRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new Dictionary<string, string[]>
            {
                ["name"] = ["Name is required."]
            };
        }

        return null;
    }
}
