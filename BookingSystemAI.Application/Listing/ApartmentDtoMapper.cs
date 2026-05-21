using System.Text.Json;

using BookingSystemAI.Application.Abstractions;

using BookingSystemAI.Application.DTOs;

using BookingSystemAI.Domain.Entities;



namespace BookingSystemAI.Application.Listing;



public static class ApartmentDtoMapper

{

    public static ApartmentListItemDto ToListItem(Apartment apartment, bool? isAvailable = null)

    {

        var imageUrls = ApartmentListingFields.NormalizeImageUrls(apartment.ImageUrls);

        var presentation = ListingPresentation.Project(apartment.MetadataJson);

        return new(

            apartment.Id,

            apartment.Name,

            apartment.City,

            imageUrls,

            ApartmentListingFields.ResolveThumbnail(apartment.Id, imageUrls),

            apartment.Description,

            apartment.PricePerNight,

            apartment.GuestCount,

            AmenityMapper.ToNames(apartment.Amenities),

            apartment.Version,

            isAvailable,

            presentation.PropertyType,

            presentation.BedroomCount,

            presentation.BedCount,

            presentation.BathroomCount,

            presentation.Highlights);

    }



    public static ApartmentResponseDto ToResponse(Apartment apartment)

    {

        var imageUrls = ApartmentListingFields.NormalizeImageUrls(apartment.ImageUrls);

        return new(

            apartment.Id,

            apartment.Name,

            apartment.City,

            imageUrls,

            ApartmentListingFields.ResolveThumbnail(apartment.Id, imageUrls),

            apartment.Description,

            apartment.PricePerNight,

            apartment.GuestCount,

            AmenityMapper.ToNames(apartment.Amenities),

            apartment.Version,

            ParseMetadata(apartment.MetadataJson));

    }



    public static ApartmentResponseDto ToResponse(ApartmentUpsertRow row)

    {

        var imageUrls = ApartmentListingFields.NormalizeImageUrls(row.ImageUrls);

        return new(

            row.Id,

            row.Name,

            row.City,

            imageUrls,

            ApartmentListingFields.ResolveThumbnail(row.Id, imageUrls),

            row.Description,

            row.PricePerNight,

            row.GuestCount,

            row.Amenities,

            row.Version,

            ParseMetadata(row.MetadataJson));

    }



    private static JsonElement? ParseMetadata(string metadataJson)

    {

        if (string.IsNullOrWhiteSpace(metadataJson) || metadataJson == "{}")

            return null;



        using var doc = JsonDocument.Parse(metadataJson);

        return doc.RootElement.Clone();

    }

}

