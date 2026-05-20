using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Listing;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Services;

public sealed class ApartmentUpsertService(IApartmentRepository apartmentRepository, IApartmentSqlGateway sqlGateway)
    : IApartmentUpsertService
{
    public async Task<UpsertApartmentResult> UpsertAsync(string hostId, UpsertApartmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateRequest(request);
        if (validationErrors is not null)
            return UpsertApartmentResult.ValidationFailure(validationErrors);

        var amenityNames = ListingValidation.DeduplicateAmenityNames(request.Amenities);

        var resolvedId = await ResolveIdAsync(hostId, request, cancellationToken);
        if (resolvedId is null)
            return UpsertApartmentResult.NotFound();

        var existing = await apartmentRepository.GetByIdAsync(resolvedId.Value, cancellationToken);
        var metadataJson = request.Metadata.HasValue
            ? ListingValidation.SerializeMetadata(request.Metadata)
            : existing?.MetadataJson ?? "{}";

        var command = new ApartmentUpsertCommand(
            resolvedId.Value,
            hostId,
            request.Name.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.PricePerNight,
            request.GuestCount,
            amenityNames.ToArray(),
            metadataJson,
            request.SourceCompanyId,
            request.ExternalId,
            request.Metadata.HasValue);

        var row = await sqlGateway.UpsertApartmentAsync(command, cancellationToken);
        if (row is null)
            return UpsertApartmentResult.NotFound();

        return UpsertApartmentResult.Success(ApartmentDtoMapper.ToResponse(row));
    }

    private async Task<Guid?> ResolveIdAsync(string hostId, UpsertApartmentRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.Id.HasValue)
        {
            var byId = await apartmentRepository.GetByIdAsync(request.Id.Value, cancellationToken);
            return byId is null || !string.Equals(byId.HostId, hostId, StringComparison.Ordinal) ? null : request.Id;
        }

        if (request.SourceCompanyId.HasValue && !string.IsNullOrWhiteSpace(request.ExternalId))
        {
            var byExternal = await apartmentRepository.GetByExternalAsync(
                request.SourceCompanyId.Value,
                request.ExternalId,
                cancellationToken);

            if (byExternal is not null)
                return string.Equals(byExternal.HostId, hostId, StringComparison.Ordinal) ? byExternal.Id : null;

            return Guid.NewGuid();
        }

        return Guid.NewGuid();
    }

    private static IReadOnlyDictionary<string, string[]>? ValidateRequest(UpsertApartmentRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors["name"] = ["Name is required."];

        ValidationErrors.Merge(errors, ListingValidation.ValidateEconomics(request.PricePerNight, request.GuestCount));
        ValidationErrors.Merge(errors, ListingValidation.ValidateAmenities(request.Amenities));
        ValidationErrors.Merge(errors, ListingValidation.ValidateMetadata(request.Metadata));

        return errors.Count == 0 ? null : errors;
    }
}
