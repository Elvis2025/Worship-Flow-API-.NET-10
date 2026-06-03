using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using WorshipFlow.Application.Common;
using WorshipFlow.Application.Features.Auth;
using WorshipFlow.Application.Features.Permissions;
using WorshipFlow.Application.Features.Roles;
using WorshipFlow.Application.Features.Users;
using WorshipFlow.Domain.Enums;
using Xunit;

namespace WorshipFlow.Tests;

public sealed class AuthAndUsersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthAndUsersTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => { }).CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", "11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsApiResponse()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@example.com", "wrong", "test-device", "Test Device", null, null));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
        body!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsFailure()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest("not-a-token", "test-device", "Test Device"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
        body!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateUser_WithoutToken_IsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/users", new CreateUserDto("Ana", "Rivera", "ana@example.com", null, "Password123!", "Piano", null, null, UserStatus.Active));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EditUser_WithoutToken_IsUnauthorized()
    {
        var response = await _client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", new UpdateUserDto("Ana", "Rivera", "ana@example.com", null, "Piano", null, null));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeRoles_WithoutToken_IsUnauthorized()
    {
        var response = await _client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/roles", new UpdateUserRolesDto(["Singer"]));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePermissions_WithoutToken_IsUnauthorized()
    {
        var response = await _client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/permissions", new UpdateUserPermissionsDto(["Users.View"]));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
