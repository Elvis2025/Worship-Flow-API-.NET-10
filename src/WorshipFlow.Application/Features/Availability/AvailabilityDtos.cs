namespace WorshipFlow.Application.Features.Availability;
public sealed record AvailabilityDto(DayOfWeek DayOfWeek, bool IsAvailable, TimeOnly? StartTime, TimeOnly? EndTime, string? Notes);
public sealed record UpdateAvailabilityDto(IReadOnlyList<AvailabilityDto> Items);
