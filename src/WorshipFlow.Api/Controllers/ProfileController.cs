using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorshipFlow.Api.Authorization;
using WorshipFlow.Application.Features.Profile;
using WorshipFlow.Domain.Constants;

namespace WorshipFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await mediator.Send(new GetProfileQuery(), ct));

    [HttpPut]
    [Authorize(Policy = PermissionPolicies.ProfileEditOwn)]
    public async Task<IActionResult> Update(UpdateProfileDto dto, CancellationToken ct) => Ok(await mediator.Send(new UpdateProfileCommand(dto), ct));

    [HttpPost("photo")]
    [Authorize(Policy = PermissionPolicies.ProfileEditOwn)]
    public async Task<IActionResult> UploadPhoto(IFormFile file, CancellationToken ct) => Ok(await mediator.Send(new UploadProfilePhotoCommand(file), ct));
}
