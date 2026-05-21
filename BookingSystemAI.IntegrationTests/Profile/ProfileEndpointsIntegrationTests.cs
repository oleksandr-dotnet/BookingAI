using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookingSystemAI.IntegrationTests.Profile;

[Collection(IntegrationTestCollection.Name)]
public class ProfileEndpointsIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task GetMyProfile_ShouldReturnOk_WhenAuthenticated()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Client);
        var response = await SendAuthorizedAsync(HttpMethod.Get, "/profile/me", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        body!.Email.ShouldNotBeNullOrWhiteSpace();
        body.Roles.ShouldContain(ApplicationRoles.Client);
    }

    [Fact]
    public async Task UpdateMyProfile_ShouldReturnOk_WhenClientProvidesRequiredFields()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Client);
        var birth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25));
        var response = await SendAuthorizedAsync(
            HttpMethod.Put,
            "/profile/me",
            token,
            new UpdateUserProfileRequestDto("Test", "Client", birth, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        body!.FirstName.ShouldBe("Test");
        body.LastName.ShouldBe("Client");
        body.ProfileComplete.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateMyProfile_ShouldReturnBadRequest_WhenHostMissingBirthDate()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Host);
        var response = await SendAuthorizedAsync(
            HttpMethod.Put,
            "/profile/me",
            token,
            new UpdateUserProfileRequestDto("Host", "Only", null, null));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserDisplay_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Client);
        var response = await SendAuthorizedAsync(
            HttpMethod.Get,
            "/users/00000000-0000-0000-0000-000000009999/display",
            token);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserDisplay_ShouldReturnDisplayCard_WhenUserExists()
    {
        var email = $"display-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequestDto(email, "Password1", ApplicationRoles.Client));
        var login = await _client.PostAsJsonAsync("/auth/login", new LoginRequestDto(email, "Password1"));
        var auth = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
        var token = auth!.AccessToken;

        var me = await SendAuthorizedAsync(HttpMethod.Get, "/profile/me", token);
        var profile = await me.Content.ReadFromJsonAsync<UserProfileDto>();

        var displayResponse = await SendAuthorizedAsync(
            HttpMethod.Get,
            $"/users/{profile!.UserId}/display",
            token);
        displayResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var display = await displayResponse.Content.ReadFromJsonAsync<UserDisplayDto>();
        display!.UserId.ShouldBe(profile.UserId);
        display.Initials.ShouldNotBeNullOrWhiteSpace();
    }

    private async Task<string> RegisterAndLoginAsync(string role)
    {
        var email = $"profile-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequestDto(email, "Password1", role));
        var login = await _client.PostAsJsonAsync("/auth/login", new LoginRequestDto(email, "Password1"));
        var body = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.AccessToken;
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpMethod method,
        string path,
        string token,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return await _client.SendAsync(request);
    }
}
