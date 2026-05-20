using System.Text.Json;
using BookingSystemAI.Application.Listing;
using Shouldly;

namespace BookingSystemAI.Application.Tests.Listing;

public class ListingValidationTests
{
    [Fact]
    public void ValidateAmenities_ShouldReturnError_WhenUnknownAmenityProvided()
    {
        var errors = ListingValidation.ValidateAmenities(["LargeBed", "Pool"]);

        errors.ShouldNotBeNull();
        errors!["amenities"][0].ShouldContain("Unknown amenity 'Pool'.");
    }

    [Fact]
    public void ValidateEconomics_ShouldReturnError_WhenGuestCountIsZero()
    {
        var errors = ListingValidation.ValidateEconomics(100, 0);

        errors.ShouldNotBeNull();
        errors!["guestCount"][0].ShouldContain("at least 1");
    }

    [Fact]
    public void DeduplicateAmenityNames_ShouldRemoveDuplicates()
    {
        var result = ListingValidation.DeduplicateAmenityNames(["Shower", "Shower", "Bath"]);

        result.Count.ShouldBe(2);
        result.ShouldContain("Shower");
        result.ShouldContain("Bath");
    }

    [Fact]
    public void ValidateMetadata_ShouldReturnError_WhenMetadataExceedsLimit()
    {
        var large = new string('x', ListingValidation.MaxMetadataBytes + 1);
        using var doc = JsonDocument.Parse($"{{\"data\":\"{large}\"}}");
        var errors = ListingValidation.ValidateMetadata(doc.RootElement);

        errors.ShouldNotBeNull();
        errors!["metadata"][0].ShouldContain("16 KB");
    }
}
