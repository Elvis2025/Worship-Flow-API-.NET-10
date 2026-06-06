using MediatR;
using Microsoft.AspNetCore.Http;
using WorshipFlow.Application.Common;
using WorshipFlow.Application.Features.Users;

namespace WorshipFlow.Application.Features.Profile;

public sealed record GetProfileQuery() : IRequest<ApiResponse<UserDto>>;
public sealed record UpdateProfileCommand(UpdateProfileDto Dto) : IRequest<ApiResponse<UserDto>>;
public sealed record UploadProfilePhotoCommand(IFormFile File) : IRequest<ApiResponse<UserDto>>;

public sealed class ProfileHandler(IMediator mediator, Abstractions.ICurrentUser currentUser)
    : IRequestHandler<GetProfileQuery, ApiResponse<UserDto>>, IRequestHandler<UpdateProfileCommand, ApiResponse<UserDto>>, IRequestHandler<UploadProfilePhotoCommand, ApiResponse<UserDto>>
{
    public Task<ApiResponse<UserDto>> Handle(GetProfileQuery request, CancellationToken ct) => currentUser.UserId is Guid id ? mediator.Send(new GetUserByIdQuery(id), ct) : Task.FromResult(ApiResponse<UserDto>.Fail("Not authenticated."));

    public async Task<ApiResponse<UserDto>> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is not Guid id)
        {
            return ApiResponse<UserDto>.Fail("Not authenticated.");
        }

        var currentProfile = await mediator.Send(new GetUserByIdQuery(id), ct);
        if (!currentProfile.Success || currentProfile.Data is null)
        {
            return ApiResponse<UserDto>.Fail(currentProfile.Message, currentProfile.Errors.ToArray());
        }

        var dto = new UpdateUserDto(
            request.Dto.FirstName,
            request.Dto.LastName,
            currentProfile.Data.Email,
            request.Dto.Phone,
            request.Dto.MainInstrument,
            request.Dto.VocalRange,
            request.Dto.ComfortableKey);

        return await mediator.Send(new UpdateUserCommand(id, dto), ct);
    }

    public Task<ApiResponse<UserDto>> Handle(UploadProfilePhotoCommand request, CancellationToken ct) => currentUser.UserId is Guid id ? mediator.Send(new UploadUserPhotoCommand(id, request.File), ct) : Task.FromResult(ApiResponse<UserDto>.Fail("Not authenticated."));
}
