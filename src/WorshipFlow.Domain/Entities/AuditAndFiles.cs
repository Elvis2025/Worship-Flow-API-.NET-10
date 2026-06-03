using WorshipFlow.Domain.Abstractions;

namespace WorshipFlow.Domain.Entities;

public sealed class AuditLog : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? DetailsJson { get; set; }
    public string? IpAddress { get; set; }
}

public sealed class FileResource : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Category { get; set; } = "ProfilePhoto";
}
