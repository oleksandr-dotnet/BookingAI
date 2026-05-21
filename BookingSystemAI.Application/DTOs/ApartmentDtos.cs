using System.Text.Json;



namespace BookingSystemAI.Application.DTOs;



public record ApartmentListItemDto(

    Guid Id,

    string Name,

    string City,

    IReadOnlyList<string> ImageUrls,

    string ThumbnailUrl,

    string Description,

    decimal PricePerNight,

    int GuestCount,

    IReadOnlyList<string> Amenities,

    int Version,

    bool? IsAvailable = null,

    string? PropertyType = null,

    int? BedroomCount = null,

    int? BedCount = null,

    int? BathroomCount = null,

    IReadOnlyList<string>? Highlights = null);



public record CreateApartmentRequestDto(

    string Name,

    string City,

    string Description,

    decimal PricePerNight,

    int GuestCount,

    IReadOnlyList<string> Amenities,

    IReadOnlyList<string>? ImageUrls = null,

    JsonElement? Metadata = null);



public record ApartmentResponseDto(

    Guid Id,

    string Name,

    string City,

    IReadOnlyList<string> ImageUrls,

    string ThumbnailUrl,

    string Description,

    decimal PricePerNight,

    int GuestCount,

    IReadOnlyList<string> Amenities,

    int Version,

    JsonElement? Metadata = null);



public record UpdateApartmentRequestDto(

    string Name,

    string City,

    string Description,

    decimal PricePerNight,

    int GuestCount,

    IReadOnlyList<string> Amenities,

    int Version,

    IReadOnlyList<string>? ImageUrls = null,

    JsonElement? Metadata = null);



public record ImageUploadConfigDto(string CloudName, string UploadPreset);



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

