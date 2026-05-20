using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Infrastructure.Identity;
using BookingSystemAI.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookingSystemAI.IntegrationTests.Analytics;

[Collection(IntegrationTestCollection.Name)]
public class ApartmentAnalyticsIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task UpsertAndAnalytics_ShouldRoundTripMetadataAndSnapshotBooking()
    {
        var hostToken = await RegisterAndLoginAsync(ApplicationRoles.Host);
        var upsertBody = new
        {
            name = "SQL Loft",
            description = "Upserted via Dapper",
            pricePerNight = 150m,
            guestCount = 2,
            amenities = new[] { "LargeBed", "Bath" },
            metadata = new { channel = "partner-x", listingCode = "LX-9" }
        };

        var upsertResponse = await SendAuthorizedAsync(HttpMethod.Put, "/host/apartments/upsert", hostToken, upsertBody);
        upsertResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var upserted = await upsertResponse.Content.ReadFromJsonAsync<ApartmentResponseDto>();
        upserted!.PricePerNight.ShouldBe(150);
        upserted.GuestCount.ShouldBe(2);
        upserted.Amenities.ShouldBe(["LargeBed", "Bath"]);

        var listResponse = await SendAuthorizedAsync(HttpMethod.Get, "/host/apartments", hostToken);
        var apartments = await listResponse.Content.ReadFromJsonAsync<ApartmentResponseDto[]>();
        var mine = apartments!.Single(a => a.Id == upserted.Id);
        mine.Metadata.ShouldNotBeNull();
        mine.Metadata!.Value.GetProperty("listingCode").GetString().ShouldBe("LX-9");

        var clientToken = await RegisterAndLoginAsync(ApplicationRoles.Client);
        var start = DateTimeOffset.UtcNow.AddDays(20);
        var end = start.AddDays(2);
        var bookResponse = await SendAuthorizedAsync(
            HttpMethod.Post,
            "/bookings",
            clientToken,
            new CreateBookingRequestDto(upserted.Id, start, end));
        bookResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var booking = await bookResponse.Content.ReadFromJsonAsync<BookingResponseDto>();
        booking!.PricePerNight.ShouldBe(150);
        booking.GuestCount.ShouldBe(2);
        booking.Amenities.ShouldBe(["LargeBed", "Bath"]);

        var adminToken = await LoginAdminAsync();
        var summaryResponse = await SendAuthorizedAsync(HttpMethod.Get, "/analytics/bookings/summary", adminToken);
        summaryResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var summary = await summaryResponse.Content.ReadFromJsonAsync<BookingSummaryAnalyticsDto>();
        summary!.TotalBookings.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task AnalyticsSummary_ShouldReturnForbidden_WhenUserIsClient()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Client);
        var response = await SendAuthorizedAsync(HttpMethod.Get, "/analytics/bookings/summary", token);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AnalyticsSummary_ShouldReturnForbidden_WhenUserIsHost()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Host);
        var response = await SendAuthorizedAsync(HttpMethod.Get, "/analytics/bookings/summary", token);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AnalyticsSummary_ShouldReturnOk_WhenUserIsAdmin()
    {
        var token = await LoginAdminAsync();
        var response = await SendAuthorizedAsync(HttpMethod.Get, "/analytics/bookings/summary", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private async Task<string> LoginAdminAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequestDto(IdentityAdminSeeder.DefaultEmail, IdentityAdminSeeder.DefaultPassword));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.AccessToken;
    }

    private async Task<string> RegisterAndLoginAsync(string role)
    {
        var email = $"{role.ToLower()}-analytics-{Guid.NewGuid():N}@example.com";
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
