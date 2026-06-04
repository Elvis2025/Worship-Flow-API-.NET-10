using WorshipFlow.Domain.Enums;

namespace WorshipFlow.Application.Features.Users;

public sealed record UserDto(Guid Id, Guid TenantId, string FullName, string FirstName, string LastName, string Email, string? Phone,
    string? ProfilePhotoUrl, string MainInstrument, string? VocalRange, string? ComfortableKey, UserStatus Status,
    DateTimeOffset JoinedAt, DateTimeOffset? LastLoginAt, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);

public sealed record UserListItemDto(Guid Id, string FullName, string Email, string MainInstrument, UserStatus Status, string? ProfilePhotoUrl, IReadOnlyList<string> Roles);
public sealed record CreateUserDto(string FirstName, string LastName, string Email, string? Phone, string Password, string MainInstrument, string? VocalRange, string? ComfortableKey, UserStatus Status);
public sealed record UpdateUserDto(string FirstName, string LastName, string Email, string? Phone, string MainInstrument, string? VocalRange, string? ComfortableKey);
public sealed record UpdateStatusDto(UserStatus Status);
public sealed record UserFilterDto(int Page = 1, int PageSize = 20, string? Name = null, string? Email = null, UserStatus? Status = null, string? Role = null, string? Instrument = null);
