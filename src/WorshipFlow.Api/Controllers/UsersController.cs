using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorshipFlow.Application.Features.Availability;
using WorshipFlow.Application.Features.Permissions;
using WorshipFlow.Application.Features.Roles;
using WorshipFlow.Application.Features.Users;
using WorshipFlow.Domain.Enums;

namespace WorshipFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? name = null, [FromQuery] string? email = null, [FromQuery] UserStatus? status = null, [FromQuery] string? role = null, [FromQuery] string? instrument = null, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetUsersQuery(new UserFilterDto(page, pageSize, name, email, status, role, instrument)), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct) => Ok(await mediator.Send(new GetUserByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto, CancellationToken ct) => Ok(await mediator.Send(new CreateUserCommand(dto), ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserDto dto, CancellationToken ct) => Ok(await mediator.Send(new UpdateUserCommand(id, dto), ct));

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateStatusDto dto, CancellationToken ct) => Ok(await mediator.Send(new UpdateUserStatusCommand(id, dto), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => Ok(await mediator.Send(new DeleteUserCommand(id), ct));

    [HttpPost("{id:guid}/photo")]
    public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file, CancellationToken ct) => Ok(await mediator.Send(new UploadUserPhotoCommand(id, file), ct));

    [HttpGet("{id:guid}/availability")]
    public async Task<IActionResult> GetAvailability(Guid id, CancellationToken ct) => Ok(await mediator.Send(new GetAvailabilityQuery(id), ct));

    [HttpPut("{id:guid}/availability")]
    public async Task<IActionResult> UpdateAvailability(Guid id, UpdateAvailabilityDto dto, CancellationToken ct) => Ok(await mediator.Send(new UpdateAvailabilityCommand(id, dto), ct));

    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> UpdateRoles(Guid id, UpdateUserRolesDto dto, CancellationToken ct) => Ok(await mediator.Send(new UpdateUserRolesCommand(id, dto), ct));

    [HttpPut("{id:guid}/permissions")]
    public async Task<IActionResult> UpdatePermissions(Guid id, UpdateUserPermissionsDto dto, CancellationToken ct) => Ok(await mediator.Send(new UpdateUserPermissionsCommand(id, dto), ct));
}
