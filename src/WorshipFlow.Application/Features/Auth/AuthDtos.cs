using WorshipFlow.Application.Features.Users;

namespace WorshipFlow.Application.Features.Auth;

public sealed record LoginRequest(string Email, string Password, string? DeviceId, string? DeviceName, string? FcmToken, string? Platform);
public sealed record RefreshRequest(string RefreshToken, string? DeviceId, string? DeviceName);
public sealed record LogoutRequest(string RefreshToken, string? DeviceId, bool Global = false);
public sealed record AuthResult(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt, DateTimeOffset RefreshTokenExpiresAt, UserDto User);
public sealed record MeDto(UserDto User);
