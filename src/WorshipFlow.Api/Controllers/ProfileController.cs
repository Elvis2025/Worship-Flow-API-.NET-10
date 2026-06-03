using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorshipFlow.Application.Features.Profile;

namespace WorshipFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await mediator.Send(new GetProfileQuery(), ct));

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProfileDto dto, CancellationToken ct) => Ok(await mediator.Send(new UpdateProfileCommand(dto), ct));

    [HttpPost("photo")]
    public async Task<IActionResult> UploadPhoto(IFormFile file, CancellationToken ct) => Ok(await mediator.Send(new UploadProfilePhotoCommand(file), ct));
}
