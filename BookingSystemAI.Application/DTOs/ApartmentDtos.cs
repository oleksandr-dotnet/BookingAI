using System.Text.Json;

namespace BookingSystemAI.Application.DTOs;

public record ApartmentListItemDto(
    Guid Id,
    string Name,
    string Description,
    decimal PricePerNight,
    int GuestCount,
    IReadOnlyList<string> Amenities,
    bool? IsAvailable = null);

public record CreateApartmentRequestDto(
    string Name,
    string Description,
    decimal PricePerNight,
    int GuestCount,
    IReadOnlyList<string> Amenities,
    JsonElement? Metadata = null);

public record ApartmentResponseDto(
    Guid Id,
    string Name,
    string Description,
    decimal PricePerNight,
    int GuestCount,
    IReadOnlyList<string> Amenities,
    JsonElement? Metadata = null);

public record UpsertApartmentRequestDto(
    Guid? Id,
    Guid? SourceCompanyId,
    string? ExternalId,
    string Name,
    string Description,
    decimal PricePerNight,
    int GuestCount,
    IReadOnlyList<string> Amenities,
    JsonElement? Metadata = null);

public record ListApartmentsQueryDto(DateTimeOffset? From, DateTimeOffset? To, bool AvailableOnly);
