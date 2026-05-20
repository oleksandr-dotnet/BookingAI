using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookingSystemAI.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public class AuthEndpointsIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task RegisterAsync_ShouldReturnCreated_WhenRequestIsValid()
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequestDto(email, "Password1", ApplicationRoles.Client);

        var response = await _client.PostAsJsonAsync("/auth/register", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
        body!.UserId.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequestDto(email, "Password1", ApplicationRoles.Client);
        await _client.PostAsJsonAsync("/auth/register", request);

        var response = await _client.PostAsJsonAsync("/auth/register", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        var password = "Password1";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequestDto(email, password, ApplicationRoles.Client));

        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequestDto(email, password));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.AccessToken.ShouldNotBeNullOrWhiteSpace();
        body.ExpiresIn.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        var email = $"wrong-pass-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequestDto(email, "Password1", ApplicationRoles.Client));

        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequestDto(email, "WrongPassword1"));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWeatherForecast_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var response = await _client.GetAsync("/weatherforecast");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWeatherForecast_ShouldReturnOk_WhenBearerTokenIsValid()
    {
        var email = $"weather-{Guid.NewGuid():N}@example.com";
        var password = "Password1";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequestDto(email, password, ApplicationRoles.Client));
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginRequestDto(email, password));
        var token = (await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>())!.AccessToken;

        var request = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var forecast = await response.Content.ReadFromJsonAsync<WeatherForecastDto[]>();
        forecast!.Length.ShouldBe(5);
    }

    private sealed record WeatherForecastDto(DateOnly Date, int TemperatureC, string? Summary, int TemperatureF);
}
