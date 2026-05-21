using System.Text.Json;
using BookingSystemAI.Application.Listing;
using Shouldly;

namespace BookingSystemAI.Application.Tests.Listing;

public class ListingPresentationTests
{
    [Fact]
    public void Validate_ShouldReturnError_WhenPropertyTypeIsInvalid()
    {
        using var doc = JsonDocument.Parse("""{"propertyType":"Castle"}""");
        var errors = ListingPresentation.Validate(doc.RootElement);

        errors.ShouldNotBeNull();
        errors!["metadata"][0].ShouldContain("propertyType");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenHighlightsExceedLimit()
    {
        using var doc = JsonDocument.Parse(
            """{"highlights":["a","b","c","d","e","f"]}""");
        var errors = ListingPresentation.Validate(doc.RootElement);

        errors.ShouldNotBeNull();
        errors!["metadata"][0].ShouldContain("5");
    }

    [Fact]
    public void Project_ShouldMapPresentationFields_WhenMetadataValid()
    {
        var projection = ListingPresentation.Project(
            """{"propertyType":"Apartment","bedroomCount":2,"bedCount":3,"bathroomCount":1,"highlights":["Self check-in"]}""");

        projection.PropertyType.ShouldBe("Apartment");
        projection.BedroomCount.ShouldBe(2);
        projection.BedCount.ShouldBe(3);
        projection.BathroomCount.ShouldBe(1);
        projection.Highlights.ShouldBe(["Self check-in"]);
    }

    [Fact]
    public void Project_ShouldReturnEmptyPresentation_WhenMetadataMissing()
    {
        var projection = ListingPresentation.Project("{}");

        projection.PropertyType.ShouldBeNull();
        projection.Highlights.ShouldBeNull();
    }

    [Fact]
    public void ValidateMetadata_ShouldAcceptValidPresentationKeys()
    {
        using var doc = JsonDocument.Parse(
            """{"propertyType":"Studio","bedroomCount":1,"highlights":["Quiet street"]}""");
        var errors = ListingValidation.ValidateMetadata(doc.RootElement);

        errors.ShouldBeNull();
    }
}
