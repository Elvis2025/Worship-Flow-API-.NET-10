using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorshipFlow.Application.Features.Auth;

namespace WorshipFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct) => Ok(await mediator.Send(new LoginCommand(request, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent), ct));

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken ct) => Ok(await mediator.Send(new RefreshCommand(request, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent), ct));

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken ct) => Ok(await mediator.Send(new LogoutCommand(request), ct));

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct) => Ok(await mediator.Send(new MeQuery(), ct));
}
