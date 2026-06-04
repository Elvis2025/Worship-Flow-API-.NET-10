using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using WorshipFlow.Application.Common;
using WorshipFlow.Application.Features.Auth;
using WorshipFlow.Application.Features.Permissions;
using WorshipFlow.Application.Features.Roles;
using WorshipFlow.Application.Features.Users;
using WorshipFlow.Domain.Constants;
using WorshipFlow.Domain.Enums;
using Xunit;

namespace WorshipFlow.Tests;

public sealed class AuthAndUsersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthAndUsersTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(_ => { }).CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", "11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public async Task Login_WithSeededAdministrator_ReturnsTokensAndUserDto()
    {
        var result = await LoginAsAdministratorAsync();

        result.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Data.User.Email.Should().Be("admin@worshipflow.local");
    }

    [Fact]
    public async Task Refresh_WithValidToken_RotatesRefreshToken()
    {
        var login = await LoginAsAdministratorAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(login.Data!.RefreshToken, "test-device", "Test Device"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
        body!.Success.Should().BeTrue();
        body.Data!.RefreshToken.Should().NotBe(login.Data.RefreshToken);
    }

    [Fact]
    public async Task CreateUser_WithAdministratorToken_CreatesTenantUser()
    {
        await AuthenticateAsAdministratorAsync();
        var email = $"ana-{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsJsonAsync("/api/users", new CreateUserDto("Ana", "Rivera", email, null, "Password123!", "Piano", "Alto", "C", UserStatus.Active));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.Email.Should().Be(email);
        body.Data.MainInstrument.Should().Be("Piano");
    }

    [Fact]
    public async Task EditUser_WithAdministratorToken_UpdatesUserDto()
    {
        await AuthenticateAsAdministratorAsync();
        var user = await CreateUserAsync();

        var response = await _client.PutAsJsonAsync($"/api/users/{user.Id}", new UpdateUserDto("Ana Maria", "Rivera", user.Email, "+15555550123", "Guitar", "Soprano", "D"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.FullName.Should().Be("Ana Maria Rivera");
        body.Data.MainInstrument.Should().Be("Guitar");
    }

    [Fact]
    public async Task ChangeRoles_WithAdministratorToken_ReplacesUserRoles()
    {
        await AuthenticateAsAdministratorAsync();
        var user = await CreateUserAsync();

        var response = await _client.PutAsJsonAsync($"/api/users/{user.Id}/roles", new UpdateUserRolesDto([SystemRoles.Singer, SystemRoles.Musician]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<string>>>();
        body!.Success.Should().BeTrue();
        body.Data.Should().BeEquivalentTo([SystemRoles.Musician, SystemRoles.Singer]);
    }

    [Fact]
    public async Task ChangePermissions_WithAdministratorToken_ReplacesDirectPermissions()
    {
        await AuthenticateAsAdministratorAsync();
        var user = await CreateUserAsync();

        var response = await _client.PutAsJsonAsync($"/api/users/{user.Id}/permissions", new UpdateUserPermissionsDto([SystemPermissions.ProfileEditOwn, SystemPermissions.UsersView]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<string>>>();
        body!.Success.Should().BeTrue();
        body.Data.Should().BeEquivalentTo([SystemPermissions.ProfileEditOwn, SystemPermissions.UsersView]);
    }

    private async Task AuthenticateAsAdministratorAsync()
    {
        var login = await LoginAsAdministratorAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Data!.AccessToken);
    }

    private async Task<ApiResponse<AuthResult>> LoginAsAdministratorAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@worshipflow.local", "ChangeMe123!", "test-device", "Test Device", null, null));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
        return body!;
    }

    private async Task<UserDto> CreateUserAsync()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/users", new CreateUserDto("Ana", "Rivera", email, null, "Password123!", "Piano", null, null, UserStatus.Active));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        body!.Success.Should().BeTrue();
        return body.Data!;
    }
}
