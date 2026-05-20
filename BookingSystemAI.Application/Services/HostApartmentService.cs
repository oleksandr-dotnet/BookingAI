using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Listing;
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

        var amenityNames = ListingValidation.DeduplicateAmenityNames(request.Amenities);
        var apartment = new Apartment
        {
            Id = Guid.NewGuid(),
            HostId = hostId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            PricePerNight = request.PricePerNight,
            GuestCount = request.GuestCount,
            Amenities = AmenityMapper.FromNames(amenityNames),
            MetadataJson = ListingValidation.SerializeMetadata(request.Metadata)
        };

        await apartmentRepository.AddAsync(apartment, cancellationToken);
        return CreateApartmentResult.Success(ApartmentDtoMapper.ToResponse(apartment));
    }

    public async Task<IReadOnlyList<ApartmentResponseDto>> ListMineAsync(string hostId,
        CancellationToken cancellationToken = default)
    {
        var apartments = await apartmentRepository.ListByHostIdAsync(hostId, cancellationToken);
        return apartments.Select(ApartmentDtoMapper.ToResponse).ToList();
    }

    public async Task<UpdateApartmentResult> UpdateAsync(string hostId, Guid apartmentId,
        UpdateApartmentRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateUpdateRequest(request);
        if (validationErrors is not null)
            return UpdateApartmentResult.ValidationFailure(validationErrors);

        var existing = await apartmentRepository.GetByIdAsync(apartmentId, cancellationToken);
        if (existing is null || existing.HostId != hostId)
            return UpdateApartmentResult.NotFound();

        var amenityNames = ListingValidation.DeduplicateAmenityNames(request.Amenities);
        var updated = new Apartment
        {
            Id = apartmentId,
            HostId = hostId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            PricePerNight = request.PricePerNight,
            GuestCount = request.GuestCount,
            Amenities = AmenityMapper.FromNames(amenityNames),
            MetadataJson = ListingValidation.SerializeMetadata(request.Metadata),
            SourceCompanyId = existing.SourceCompanyId,
            ExternalId = existing.ExternalId,
            Version = existing.Version
        };

        var outcome = await apartmentRepository.TryUpdateAsync(updated, request.Version, cancellationToken);
        if (outcome == ApartmentUpdateOutcome.Conflict)
            return UpdateApartmentResult.Conflict();

        if (outcome != ApartmentUpdateOutcome.Success)
            return UpdateApartmentResult.NotFound();

        var reloaded = await apartmentRepository.GetByIdAsync(apartmentId, cancellationToken);
        return UpdateApartmentResult.Success(ApartmentDtoMapper.ToResponse(reloaded!));
    }

    private static IReadOnlyDictionary<string, string[]>? ValidateRequest(CreateApartmentRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors["name"] = ["Name is required."];

        ValidationErrors.Merge(errors, ListingValidation.ValidateEconomics(request.PricePerNight, request.GuestCount));
        ValidationErrors.Merge(errors, ListingValidation.ValidateAmenities(request.Amenities));
        ValidationErrors.Merge(errors, ListingValidation.ValidateMetadata(request.Metadata));

        return errors.Count == 0 ? null : errors;
    }

    private static IReadOnlyDictionary<string, string[]>? ValidateUpdateRequest(UpdateApartmentRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Version < 1)
            errors["version"] = ["Version must be at least 1."];

        if (string.IsNullOrWhiteSpace(request.Name))
            errors["name"] = ["Name is required."];

        ValidationErrors.Merge(errors, ListingValidation.ValidateEconomics(request.PricePerNight, request.GuestCount));
        ValidationErrors.Merge(errors, ListingValidation.ValidateAmenities(request.Amenities));
        ValidationErrors.Merge(errors, ListingValidation.ValidateMetadata(request.Metadata));

        return errors.Count == 0 ? null : errors;
    }
}
