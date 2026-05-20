using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookingSystemAI.IntegrationTests.Booking;

[Collection(IntegrationTestCollection.Name)]
public class BookingEndpointsIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task HostCreateApartment_ShouldReturnCreated_WhenUserIsHost()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Host);
        var request = new CreateApartmentRequestDto("Studio A", "Cozy studio");

        var response = await SendAuthorizedAsync(HttpMethod.Post, "/host/apartments", token, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ClientCreateBooking_ShouldReturnForbidden_WhenUserIsHost()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Host);
        var request = new CreateBookingRequestDto(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2));

        var response = await SendAuthorizedAsync(HttpMethod.Post, "/bookings", token, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task HostCreateApartment_ShouldReturnForbidden_WhenUserIsClient()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Client);
        var request = new CreateApartmentRequestDto("Studio B", "Another studio");

        var response = await SendAuthorizedAsync(HttpMethod.Post, "/host/apartments", token, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListApartments_ShouldReturnOk_WithoutAuthentication()
    {
        var response = await _client.GetAsync("/apartments");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BookAndList_ShouldWork_WhenClientBooksAvailableApartment()
    {
        var hostToken = await RegisterAndLoginAsync(ApplicationRoles.Host);
        var createApartment = await SendAuthorizedAsync(
            HttpMethod.Post,
            "/host/apartments",
            hostToken,
            new CreateApartmentRequestDto("Bookable Flat", "For integration test"));
        createApartment.StatusCode.ShouldBe(HttpStatusCode.Created);
        var apartment = await createApartment.Content.ReadFromJsonAsync<ApartmentResponseDto>();

        var clientToken = await RegisterAndLoginAsync(ApplicationRoles.Client);
        var start = DateTimeOffset.UtcNow.AddDays(10);
        var end = start.AddDays(2);
        var bookingRequest = new CreateBookingRequestDto(apartment!.Id, start, end);

        var bookResponse = await SendAuthorizedAsync(HttpMethod.Post, "/bookings", clientToken, bookingRequest);
        bookResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var listResponse = await SendAuthorizedAsync(HttpMethod.Get, "/bookings", clientToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var bookings = await listResponse.Content.ReadFromJsonAsync<BookingResponseDto[]>();
        bookings!.Length.ShouldBe(1);
        bookings[0].ApartmentId.ShouldBe(apartment.Id);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnBadRequest_WhenRoleIsMissing()
    {
        var email = $"norole-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/auth/register", new { email, password = "Password1" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<string> RegisterAndLoginAsync(string role)
    {
        var email = $"{role.ToLower()}-{Guid.NewGuid():N}@example.com";
        var password = "Password1";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequestDto(email, password, role));
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginRequestDto(email, password));
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        var token = body!.AccessToken;
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.ShouldContain(c => c.Type == ClaimTypes.Role && c.Value == role);
        return token;
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
