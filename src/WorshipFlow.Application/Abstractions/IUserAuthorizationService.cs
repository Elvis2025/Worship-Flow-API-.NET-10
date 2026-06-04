namespace WorshipFlow.Application.Abstractions;

public interface IUserAuthorizationService
{
    Task<bool> HasPermissionAsync(Guid tenantId, Guid userId, string permission, CancellationToken cancellationToken = default);
}
