using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WorshipFlow.Application.Abstractions;

namespace WorshipFlow.Infrastructure.Security;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid TenantId => Guid.TryParse(User?.FindFirstValue("tenant_id") ?? accessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault(), out var id) ? id : Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Guid? UserId => Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub"), out var id) ? id : null;
    public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email");
    public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    private ClaimsPrincipal? User => accessor.HttpContext?.User;
}
