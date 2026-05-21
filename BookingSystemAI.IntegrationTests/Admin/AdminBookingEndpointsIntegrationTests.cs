using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookingSystemAI.IntegrationTests.Admin;

[Collection(IntegrationTestCollection.Name)]
public class AdminBookingEndpointsIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task ListBookings_ShouldReturnOk_WhenUserIsAdmin()
    {
        var token = await LoginAdminAsync();
        var response = await SendAuthorizedAsync(HttpMethod.Get, "/admin/bookings", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AdminBookingListResponseDto>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task ListBookings_ShouldReturnForbidden_WhenUserIsClient()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Client);
        var response = await SendAuthorizedAsync(HttpMethod.Get, "/admin/bookings", token);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListUserBookings_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var token = await LoginAdminAsync();
        var response = await SendAuthorizedAsync(
            HttpMethod.Get,
            "/admin/users/00000000-0000-0000-0000-000000009999/bookings",
            token);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LockUser_ShouldReturnNoContent_WhenUserIsAdmin()
    {
        var hostEmail = $"lock-host-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequestDto(hostEmail, "Password1", ApplicationRoles.Host));
        var adminToken = await LoginAdminAsync();
        var listResponse = await SendAuthorizedAsync(
            HttpMethod.Get,
            $"/admin/users?search={Uri.EscapeDataString(hostEmail)}",
            adminToken);
        var list = await listResponse.Content.ReadFromJsonAsync<AdminUserListResponseDto>();
        var userId = list!.Items.Single(i => i.Email == hostEmail).UserId;

        var lockResponse = await SendAuthorizedAsync(HttpMethod.Post, $"/admin/users/{userId}/lock", adminToken);
        lockResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var detailResponse = await SendAuthorizedAsync(HttpMethod.Get, $"/admin/users/{userId}", adminToken);
        var detail = await detailResponse.Content.ReadFromJsonAsync<AdminUserDetailDto>();
        detail!.LockoutEnabled.ShouldBeTrue();
        detail.LockoutEnd.ShouldNotBeNull();
    }

    private async Task<string> LoginAdminAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequestDto(TestAdminCredentials.Email, TestAdminCredentials.Password));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.AccessToken;
    }

    private async Task<string> RegisterAndLoginAsync(string role)
    {
        var email = $"{role.ToLower()}-admin-bookings-{Guid.NewGuid():N}@example.com";
        var password = "Password1";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequestDto(email, password, role));
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginRequestDto(email, password));
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.AccessToken;
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request);
    }
}
