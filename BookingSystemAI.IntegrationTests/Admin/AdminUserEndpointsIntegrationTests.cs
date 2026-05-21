using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookingSystemAI.IntegrationTests.Admin;

[Collection(IntegrationTestCollection.Name)]
public class AdminUserEndpointsIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task ListUsers_ShouldReturnOk_WhenUserIsAdmin()
    {
        var token = await LoginAdminAsync();
        var response = await SendAuthorizedAsync(HttpMethod.Get, "/admin/users", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AdminUserListResponseDto>();
        body!.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
        body.Items.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ListUsers_ShouldReturnForbidden_WhenUserIsHost()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Host);
        var response = await SendAuthorizedAsync(HttpMethod.Get, "/admin/users", token);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListUsers_ShouldReturnForbidden_WhenUserIsClient()
    {
        var token = await RegisterAndLoginAsync(ApplicationRoles.Client);
        var response = await SendAuthorizedAsync(HttpMethod.Get, "/admin/users", token);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnOk_WhenUserIsAdmin()
    {
        var hostEmail = $"host-admin-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequestDto(hostEmail, "Password1", ApplicationRoles.Host));
        var hostLogin = await _client.PostAsJsonAsync("/auth/login", new LoginRequestDto(hostEmail, "Password1"));
        var hostBody = await hostLogin.Content.ReadFromJsonAsync<AuthResponseDto>();

        var adminToken = await LoginAdminAsync();
        var listResponse = await SendAuthorizedAsync(
            HttpMethod.Get,
            $"/admin/users?role=Host&search={Uri.EscapeDataString(hostEmail)}",
            adminToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<AdminUserListResponseDto>();
        var hostUser = list!.Items.Single(i => i.Email == hostEmail);

        var detailResponse = await SendAuthorizedAsync(
            HttpMethod.Get,
            $"/admin/users/{hostUser.UserId}",
            adminToken);
        detailResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var detail = await detailResponse.Content.ReadFromJsonAsync<AdminUserDetailDto>();
        detail!.Email.ShouldBe(hostEmail);
        detail.Roles.ShouldContain(ApplicationRoles.Host);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        var token = await LoginAdminAsync();
        var response = await SendAuthorizedAsync(
            HttpMethod.Get,
            "/admin/users/00000000-0000-0000-0000-000000009999",
            token);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
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
        var email = $"{role.ToLower()}-admin-users-{Guid.NewGuid():N}@example.com";
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
