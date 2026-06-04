using System.Text.Json;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Domain.Entities;

namespace WorshipFlow.Infrastructure.Auditing;

public sealed class AuditService(IWorshipFlowDbContext db, ICurrentUser currentUser) : IAuditService
{
    public async Task RecordAsync(string action, string entityName, Guid? entityId, object? details = null, CancellationToken cancellationToken = default)
    {
        db.AuditLogs.Add(new AuditLog { TenantId = currentUser.TenantId, ActorUserId = currentUser.UserId, Action = action, EntityName = entityName, EntityId = entityId, DetailsJson = details is null ? null : JsonSerializer.Serialize(details), IpAddress = currentUser.IpAddress });
        await db.SaveChangesAsync(cancellationToken);
    }
}
