using MediatR;
using Microsoft.EntityFrameworkCore;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Application.Common;
using WorshipFlow.Application.Features.Users;
using WorshipFlow.Domain.Entities;
using WorshipFlow.Domain.Enums;

namespace WorshipFlow.Application.Features.Auth;

public sealed record LoginCommand(LoginRequest Request, string? IpAddress, string? UserAgent) : IRequest<ApiResponse<AuthResult>>;
public sealed record RefreshCommand(RefreshRequest Request, string? IpAddress, string? UserAgent) : IRequest<ApiResponse<AuthResult>>;
public sealed record LogoutCommand(LogoutRequest Request) : IRequest<ApiResponse<bool>>;
public sealed record MeQuery() : IRequest<ApiResponse<MeDto>>;

public sealed class AuthHandler(IWorshipFlowDbContext db, ICurrentUser currentUser, IPasswordHasher hasher, ITokenService tokens, IAuditService audit)
    : IRequestHandler<LoginCommand, ApiResponse<AuthResult>>, IRequestHandler<RefreshCommand, ApiResponse<AuthResult>>, IRequestHandler<LogoutCommand, ApiResponse<bool>>, IRequestHandler<MeQuery, ApiResponse<MeDto>>
{
    public async Task<ApiResponse<AuthResult>> Handle(LoginCommand command, CancellationToken ct)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();
        var user = await IncludeUser(db.Users).FirstOrDefaultAsync(x => x.TenantId == currentUser.TenantId && x.Email == email && !x.IsDeleted, ct);
        if (user is null || !hasher.Verify(command.Request.Password, user.PasswordHash) || user.Status is not UserStatus.Active)
            return ApiResponse<AuthResult>.Fail("Invalid credentials.");

        var pair = tokens.CreateTokenPair(user, command.Request.DeviceId, command.Request.DeviceName, command.IpAddress, command.UserAgent);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.RefreshTokens.Add(new RefreshToken { TenantId = user.TenantId, UserId = user.Id, TokenHash = tokens.HashToken(pair.RefreshToken), DeviceId = command.Request.DeviceId, DeviceName = command.Request.DeviceName, IpAddress = command.IpAddress, UserAgent = command.UserAgent, ExpiresAt = pair.RefreshTokenExpiresAt });
        if (!string.IsNullOrWhiteSpace(command.Request.FcmToken))
            user.DeviceTokens.Add(new DeviceToken { TenantId = user.TenantId, UserId = user.Id, Token = command.Request.FcmToken, Platform = command.Request.Platform ?? "unknown", DeviceId = command.Request.DeviceId });
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync("Login", nameof(AppUser), user.Id, new { user.Email, command.Request.DeviceId }, ct);
        return ApiResponse<AuthResult>.Ok(new AuthResult(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt, pair.RefreshTokenExpiresAt, user.ToDto()));
    }

    public async Task<ApiResponse<AuthResult>> Handle(RefreshCommand command, CancellationToken ct)
    {
        var hash = tokens.HashToken(command.Request.RefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TenantId == currentUser.TenantId && x.TokenHash == hash, ct);
        if (stored is null || !stored.IsActive) return ApiResponse<AuthResult>.Fail("Refresh token is invalid or expired.");
        var user = await IncludeUser(db.Users).FirstOrDefaultAsync(x => x.TenantId == currentUser.TenantId && x.Id == stored.UserId && !x.IsDeleted, ct);
        if (user is null || user.Status is not UserStatus.Active) return ApiResponse<AuthResult>.Fail("User is not active.");
        var pair = tokens.CreateTokenPair(user, command.Request.DeviceId ?? stored.DeviceId, command.Request.DeviceName ?? stored.DeviceName, command.IpAddress, command.UserAgent);
        stored.RevokedAt = DateTimeOffset.UtcNow;
        stored.RevokedReason = "Rotated";
        stored.ReplacedByTokenHash = tokens.HashToken(pair.RefreshToken);
        db.RefreshTokens.Add(new RefreshToken { TenantId = user.TenantId, UserId = user.Id, TokenHash = stored.ReplacedByTokenHash, DeviceId = command.Request.DeviceId ?? stored.DeviceId, DeviceName = command.Request.DeviceName ?? stored.DeviceName, IpAddress = command.IpAddress, UserAgent = command.UserAgent, ExpiresAt = pair.RefreshTokenExpiresAt });
        await db.SaveChangesAsync(ct);
        return ApiResponse<AuthResult>.Ok(new AuthResult(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt, pair.RefreshTokenExpiresAt, user.ToDto()));
    }

    public async Task<ApiResponse<bool>> Handle(LogoutCommand command, CancellationToken ct)
    {
        var query = db.RefreshTokens.Where(x => x.TenantId == currentUser.TenantId && x.RevokedAt == null);
        if (command.Request.Global && currentUser.UserId.HasValue) query = query.Where(x => x.UserId == currentUser.UserId.Value);
        else if (!string.IsNullOrWhiteSpace(command.Request.DeviceId) && currentUser.UserId.HasValue) query = query.Where(x => x.UserId == currentUser.UserId.Value && x.DeviceId == command.Request.DeviceId);
        else query = query.Where(x => x.TokenHash == tokens.HashToken(command.Request.RefreshToken));
        var now = DateTimeOffset.UtcNow;
        await query.ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now).SetProperty(x => x.RevokedReason, "Logout"), ct);
        await audit.RecordAsync("Logout", nameof(AppUser), currentUser.UserId, new { command.Request.DeviceId, command.Request.Global }, ct);
        return ApiResponse<bool>.Ok(true, "Logged out.");
    }

    public async Task<ApiResponse<MeDto>> Handle(MeQuery query, CancellationToken ct)
    {
        if (currentUser.UserId is null) return ApiResponse<MeDto>.Fail("Not authenticated.");
        var user = await IncludeUser(db.Users).FirstOrDefaultAsync(x => x.TenantId == currentUser.TenantId && x.Id == currentUser.UserId && !x.IsDeleted, ct);
        return user is null ? ApiResponse<MeDto>.Fail("User not found.") : ApiResponse<MeDto>.Ok(new MeDto(user.ToDto()));
    }

    private static IQueryable<AppUser> IncludeUser(IQueryable<AppUser> users) => users.Include(x => x.Roles).ThenInclude(x => x.Role)!.ThenInclude(x => x.Permissions).ThenInclude(x => x.Permission).Include(x => x.Permissions).ThenInclude(x => x.Permission);
}
