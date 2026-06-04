using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorshipFlow.Domain.Entities;

namespace WorshipFlow.Application.Abstractions;

public interface ICurrentUser
{
    Guid TenantId { get; }
    Guid? UserId { get; }
    string? Email { get; }
    string? IpAddress { get; }
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public sealed record TokenPair(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt, DateTimeOffset RefreshTokenExpiresAt);

public interface ITokenService
{
    TokenPair CreateTokenPair(AppUser user, string? deviceId, string? deviceName, string? ipAddress, string? userAgent);
    string HashToken(string token);
}

public interface IFileStorage
{
    Task<StoredFile> SaveAsync(IFormFile file, Guid tenantId, string category, CancellationToken cancellationToken);
}

public sealed record StoredFile(string OriginalFileName, string StoredFileName, string ContentType, long SizeBytes, string Url);

public interface IAuditService
{
    Task RecordAsync(string action, string entityName, Guid? entityId, object? details = null, CancellationToken cancellationToken = default);
}

public interface IWorshipFlowDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<AppUser> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<UserPermission> UserPermissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<DeviceToken> DeviceTokens { get; }
    DbSet<UserAvailability> UserAvailability { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<FileResource> FileResources { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
