namespace WorshipFlow.Application.Features.Profile;
public sealed record UpdateProfileDto(string FirstName, string LastName, string? Phone, string MainInstrument, string? VocalRange, string? ComfortableKey);
