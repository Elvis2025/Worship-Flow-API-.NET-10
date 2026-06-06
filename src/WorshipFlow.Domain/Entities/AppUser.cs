using WorshipFlow.Domain.Abstractions;
using WorshipFlow.Domain.Enums;

namespace WorshipFlow.Domain.Entities;

public sealed class AppUser : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public string MainInstrument { get; set; } = string.Empty;
    public string? VocalRange { get; set; }
    public string? ComfortableKey { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Invited;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    public ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserAvailability> Availability { get; set; } = new List<UserAvailability>();
    public ICollection<DeviceToken> DeviceTokens { get; set; } = new List<DeviceToken>();
}
