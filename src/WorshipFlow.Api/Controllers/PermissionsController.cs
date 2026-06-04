using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorshipFlow.Api.Authorization;
using WorshipFlow.Domain.Constants;
using WorshipFlow.Application.Features.Permissions;

namespace WorshipFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/permissions")]
public sealed class PermissionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.UsersManagePermissions)]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await mediator.Send(new GetPermissionsQuery(), ct));
}
