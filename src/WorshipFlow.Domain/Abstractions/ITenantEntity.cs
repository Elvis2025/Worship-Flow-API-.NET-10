namespace WorshipFlow.Domain.Abstractions;

public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
