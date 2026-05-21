using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json;
using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Listing;
using BookingSystemAI.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookingSystemAI.IntegrationTests.Apartments;

[Collection(IntegrationTestCollection.Name)]
public class ApartmentEndpointsIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task GetApartmentById_ShouldReturnOk_WithoutAuthentication()
    {
        var apartmentId = await CreateHostApartmentAsync();

        var response = await _client.GetAsync($"/apartments/{apartmentId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = await response.Content.ReadFromJsonAsync<ApartmentListItemDto>();
        item!.Id.ShouldBe(apartmentId);
        item.Name.ShouldBe("Detail Flat");
        item.City.ShouldBe("Kyiv");
        item.ThumbnailUrl.ShouldBe(ApartmentListingFields.DefaultImageUrl(apartmentId));
        item.PricePerNight.ShouldBe(100);
        item.GuestCount.ShouldBe(2);
        item.Amenities.ShouldBe(["Shower"]);
        item.Version.ShouldBe(1);
        item.IsAvailable.ShouldBeNull();
    }

    [Fact]
    public async Task GetApartmentById_ShouldReturnNotFound_WhenIdUnknown()
    {
        var response = await _client.GetAsync($"/apartments/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetApartmentById_ShouldReturnBadRequest_WhenOnlyFromProvided()
    {
        var apartmentId = await CreateHostApartmentAsync();
        var from = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(1).ToString("o"));

        var response = await _client.GetAsync($"/apartments/{apartmentId}?from={from}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetApartmentById_ShouldReturnBadRequest_WhenToNotAfterFrom()
    {
        var apartmentId = await CreateHostApartmentAsync();
        var from = DateTimeOffset.UtcNow.AddDays(5);
        var to = from.AddDays(-1);
        var fromParam = Uri.EscapeDataString(from.ToString("o"));
        var toParam = Uri.EscapeDataString(to.ToString("o"));

        var response = await _client.GetAsync($"/apartments/{apartmentId}?from={fromParam}&to={toParam}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetApartmentById_ShouldIncludeIsAvailable_WhenWindowProvided()
    {
        var apartmentId = await CreateHostApartmentAsync();
        var from = DateTimeOffset.UtcNow.AddDays(10);
        var to = from.AddDays(2);
        var fromParam = Uri.EscapeDataString(from.ToString("o"));
        var toParam = Uri.EscapeDataString(to.ToString("o"));

        var response = await _client.GetAsync($"/apartments/{apartmentId}?from={fromParam}&to={toParam}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = await response.Content.ReadFromJsonAsync<ApartmentListItemDto>();
        item!.IsAvailable.ShouldBe(true);
    }

    [Fact]
    public async Task GetApartmentById_ShouldProjectPresentationFields_WhenMetadataSet()
    {
        var hostToken = await RegisterAndLoginAsync(ApplicationRoles.Host);
        using var metadata = JsonDocument.Parse(
            """{"propertyType":"House","bedroomCount":3,"bedCount":4,"bathroomCount":2,"highlights":["Great view","Self check-in"]}""");
        var createResponse = await SendAuthorizedAsync(
            HttpMethod.Post,
            "/host/apartments",
            hostToken,
            new CreateApartmentRequestDto(
                "Presentation Home",
                "Lviv",
                "Rich listing test",
                150,
                4,
                ["Shower", "LargeBed"],
                null,
                metadata.RootElement));
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ApartmentResponseDto>();

        var listResponse = await _client.GetAsync("/apartments");
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<ApartmentListItemDto[]>();
        var listed = list!.Single(a => a.Id == created!.Id);
        listed.PropertyType.ShouldBe("House");
        listed.BedroomCount.ShouldBe(3);
        listed.BedCount.ShouldBe(4);
        listed.BathroomCount.ShouldBe(2);
        listed.Highlights.ShouldBe(["Great view", "Self check-in"]);

        var detailResponse = await _client.GetAsync($"/apartments/{created!.Id}");
        var detail = await detailResponse.Content.ReadFromJsonAsync<ApartmentListItemDto>();
        detail!.PropertyType.ShouldBe("House");
        detail.Highlights!.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetApartmentById_ShouldReturnIsAvailableFalse_WhenBookingOverlapsWindow()
    {
        var apartmentId = await CreateHostApartmentAsync();
        var start = DateTimeOffset.UtcNow.AddDays(20);
        var end = start.AddDays(2);

        var clientToken = await RegisterAndLoginAsync(ApplicationRoles.Client);
        var bookResponse = await SendAuthorizedAsync(
            HttpMethod.Post,
            "/bookings",
            clientToken,
            new CreateBookingRequestDto(apartmentId, 1, start, end));
        bookResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var fromParam = Uri.EscapeDataString(start.ToString("o"));
        var toParam = Uri.EscapeDataString(end.ToString("o"));
        var response = await _client.GetAsync($"/apartments/{apartmentId}?from={fromParam}&to={toParam}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = await response.Content.ReadFromJsonAsync<ApartmentListItemDto>();
        item!.IsAvailable.ShouldBe(false);
    }

    private async Task<Guid> CreateHostApartmentAsync()
    {
        var hostToken = await RegisterAndLoginAsync(ApplicationRoles.Host);
        var createResponse = await SendAuthorizedAsync(
            HttpMethod.Post,
            "/host/apartments",
            hostToken,
            new CreateApartmentRequestDto("Detail Flat", "Kyiv", "For detail endpoint test", 100, 2, ["Shower"]));
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var apartment = await createResponse.Content.ReadFromJsonAsync<ApartmentResponseDto>();
        return apartment!.Id;
    }

    private async Task<string> RegisterAndLoginAsync(string role)
    {
        var email = $"{role.ToLower()}-detail-{Guid.NewGuid():N}@example.com";
        var password = "Password1";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequestDto(email, password, role));
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginRequestDto(email, password));
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.AccessToken;
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        return await _client.SendAsync(request);
    }
}
