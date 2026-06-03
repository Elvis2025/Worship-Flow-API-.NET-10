using MediatR;
using Microsoft.EntityFrameworkCore;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Application.Common;
using WorshipFlow.Domain.Entities;

namespace WorshipFlow.Application.Features.Availability;

public sealed record GetAvailabilityQuery(Guid UserId) : IRequest<ApiResponse<IReadOnlyList<AvailabilityDto>>>;
public sealed record UpdateAvailabilityCommand(Guid UserId, UpdateAvailabilityDto Dto) : IRequest<ApiResponse<IReadOnlyList<AvailabilityDto>>>;

public sealed class AvailabilityHandler(IWorshipFlowDbContext db, ICurrentUser currentUser, IAuditService audit)
    : IRequestHandler<GetAvailabilityQuery, ApiResponse<IReadOnlyList<AvailabilityDto>>>, IRequestHandler<UpdateAvailabilityCommand, ApiResponse<IReadOnlyList<AvailabilityDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<AvailabilityDto>>> Handle(GetAvailabilityQuery request, CancellationToken ct)
    {
        if (!await db.Users.AnyAsync(x => x.TenantId == currentUser.TenantId && x.Id == request.UserId && !x.IsDeleted, ct)) return ApiResponse<IReadOnlyList<AvailabilityDto>>.Fail("User not found.");
        var items = await db.UserAvailability.Where(x => x.TenantId == currentUser.TenantId && x.UserId == request.UserId && !x.IsDeleted).OrderBy(x => x.DayOfWeek).Select(x => new AvailabilityDto(x.DayOfWeek, x.IsAvailable, x.StartTime, x.EndTime, x.Notes)).ToListAsync(ct);
        return ApiResponse<IReadOnlyList<AvailabilityDto>>.Ok(items);
    }

    public async Task<ApiResponse<IReadOnlyList<AvailabilityDto>>> Handle(UpdateAvailabilityCommand request, CancellationToken ct)
    {
        if (!await db.Users.AnyAsync(x => x.TenantId == currentUser.TenantId && x.Id == request.UserId && !x.IsDeleted, ct)) return ApiResponse<IReadOnlyList<AvailabilityDto>>.Fail("User not found.");
        db.UserAvailability.RemoveRange(db.UserAvailability.Where(x => x.TenantId == currentUser.TenantId && x.UserId == request.UserId));
        foreach (var item in request.Dto.Items) db.UserAvailability.Add(new UserAvailability { TenantId = currentUser.TenantId, UserId = request.UserId, DayOfWeek = item.DayOfWeek, IsAvailable = item.IsAvailable, StartTime = item.StartTime, EndTime = item.EndTime, Notes = item.Notes });
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync("UserAvailabilityChanged", nameof(AppUser), request.UserId, request.Dto.Items, ct);
        return await Handle(new GetAvailabilityQuery(request.UserId), ct);
    }
}
