namespace WorshipFlow.Domain.Constants;

public static class SystemRoles
{
    public const string Administrator = "Administrator";
    public const string MusicalDirector = "MusicalDirector";
    public const string Singer = "Singer";
    public const string Musician = "Musician";
    public const string SoundTechnician = "SoundTechnician";
    public const string ProjectionTechnician = "ProjectionTechnician";
    public const string LogisticsCoordinator = "LogisticsCoordinator";

    public static readonly string[] All =
    [
        Administrator,
        MusicalDirector,
        Singer,
        Musician,
        SoundTechnician,
        ProjectionTechnician,
        LogisticsCoordinator
    ];
}
