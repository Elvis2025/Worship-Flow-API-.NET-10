using WorshipFlow.Domain.Abstractions;

namespace WorshipFlow.Domain.Entities;

public sealed class UserAvailability : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsAvailable { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Notes { get; set; }
}
