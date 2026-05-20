using System.Text.Json;
using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Listing;

public static class ApartmentDtoMapper
{
    public static ApartmentListItemDto ToListItem(Apartment apartment, bool? isAvailable = null) =>
        new(
            apartment.Id,
            apartment.Name,
            apartment.Description,
            apartment.PricePerNight,
            apartment.GuestCount,
            AmenityMapper.ToNames(apartment.Amenities),
            apartment.Version,
            isAvailable);

    public static ApartmentResponseDto ToResponse(Apartment apartment) =>
        new(
            apartment.Id,
            apartment.Name,
            apartment.Description,
            apartment.PricePerNight,
            apartment.GuestCount,
            AmenityMapper.ToNames(apartment.Amenities),
            apartment.Version,
            ParseMetadata(apartment.MetadataJson));

    public static ApartmentResponseDto ToResponse(ApartmentUpsertRow row) =>
        new(
            row.Id,
            row.Name,
            row.Description,
            row.PricePerNight,
            row.GuestCount,
            row.Amenities,
            row.Version,
            ParseMetadata(row.MetadataJson));

    private static JsonElement? ParseMetadata(string metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson) || metadataJson == "{}")
            return null;

        using var doc = JsonDocument.Parse(metadataJson);
        return doc.RootElement.Clone();
    }
}
