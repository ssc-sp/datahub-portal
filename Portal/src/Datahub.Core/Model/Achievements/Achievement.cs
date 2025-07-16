namespace Datahub.Core.Model.Achievements;

/// <summary>
/// Represents an achievement that can be earned by users within the DataHub portal.
/// </summary>
public class Achievement
{
    /// <summary>
    /// Separator used to split rules within the achievement definitions.
    /// </summary>
    private const char RuleSeparator = '\n';

    /// <summary>
    /// Initializes a new instance of the <see cref="Achievement"/> class.
    /// </summary>
    private Achievement()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Achievement"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">Unique identifier for this achievement.</param>
    /// <param name="name">Name of the achievement.</param>
    /// <param name="description">Description of the achievement.</param>
    /// <param name="points">Point value awarded by this achievement.</param>
    /// <param name="rules">Rules used to evaluate this achievement.</param>
    public Achievement(string id, string name, string description, int points, params string[] rules)
    {
        Id = id;
        Name = name;
        Description = description;
        Points = points;
        ConcatenatedRules = string.Join($"{RuleSeparator}", rules);
    }

    /// <summary>
    /// Gets or sets the unique identifier for this achievement.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the achievement.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the achievement.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the point value of this achievement.
    /// </summary>
    public int Points { get; set; } = 1;

    /// <summary>
    /// Gets or sets the concatenated rules used to evaluate this achievement.
    /// </summary>
    public string ConcatenatedRules { get; set; }

    #region Navigation props

    /// <summary>
    /// Gets or sets the collection of user achievements linked to this achievement.
    /// </summary>
    public virtual ICollection<UserAchievement> UserAchievements { get; set; }

    #endregion

    #region Utility functions

    /// <summary>
    /// Splits <see cref="ConcatenatedRules"/> into an array of individual rules.
    /// </summary>
    /// <returns>An array of rule strings.</returns>
    public string[] GetRules() => (ConcatenatedRules ?? string.Empty).Split(RuleSeparator);

    /// <summary>
    /// Determines whether this achievement is a trophy by checking the ID suffix.
    /// </summary>
    /// <returns><see langword="true"/> if the achievement is a trophy; otherwise <see langword="false"/>.</returns>
    public bool IsTrophy() => Id.EndsWith("-000");

    #endregion

    #region Seeding

    /// <summary>
    /// Retrieves all predefined achievements.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="Achievement"/> objects.</returns>
    public static IEnumerable<Achievement> GetAll() => achievements.Value;

    /// <summary>
    /// Lazy-loaded list of achievements to be created.
    /// </summary>
    private static Lazy<IEnumerable<Achievement>> achievements = new(CreateAchievements);

    /// <summary>
    /// Creates and returns a list of predefined achievements.
    /// </summary>
    /// <returns>A collection of initialized <see cref="Achievement"/> instances.</returns>
    public static IEnumerable<Achievement> CreateAchievements()
    {
        return new List<Achievement>
        {
            // Datahub Achievements
            new Achievement(
                "DHA-001", "Collaboration Connoisseur", "Logged in to DataHub", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserLogin}", currentMetric)"""),
            new Achievement(
                "DHA-002", "Collaboration Commander", "Invite a user to your workspace", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserSentInvite}", currentMetric)"""),
            new Achievement(
                "DHA-003",  "Workspace Warrior", "Join a workspace", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserJoinedProject}", currentMetric)"""),
            new Achievement(
                "DHA-004", "Workspace Wanderlust", "Leave a workspace", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserLeftProject}", currentMetric)"""),
            new Achievement(
                "DHA-005", "Consistent Contributor", "Login on multiple days", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserLoginMultipleDays}", currentMetric)"""),

            // Storage Explorer Achievements
            new Achievement(
                "STR-000", "Storage Savant", "Unlock all the 2.0 Storage Explorer achievements", 1,
                """Utils.OwnsAchievement("STR-001", achivements)""",
                """Utils.OwnsAchievement("STR-003", achivements)""",
                """Utils.OwnsAchievement("STR-004", achivements)""",
                """Utils.OwnsAchievement("STR-005", achivements)""",
                """Utils.OwnsAchievement("STR-006", achivements)"""),
            new Achievement(
                "STR-001", "Unstoppable Uploader", "Upload a file using the workspace Storage Explorer", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserUploadFile}", currentMetric)"""),
            new Achievement(
                "STR-002", "Storage Socialite", "Share a file using the workspace Storage Explorer", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserShareFile}", currentMetric)"""),
            new Achievement(
                "STR-003", "File Fetcher", "Download a file using the workspace Storage Explorer", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserDownloadFile}", currentMetric)"""),
            new Achievement(
                "STR-004", "Daredevil Deleter", "Delete a file from the workspace with the Storage Explorer", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserDeleteFile}", currentMetric)"""),
            new Achievement(
                "STR-005", "Folder Fashionista", "Create a folder in the workspace's Storage Explorer", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserCreateFolder}", currentMetric)"""),
            new Achievement(
                "STR-006", "Folder Farewell", "Delete a folder in the workspace's Storage Explorer", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserDeletedFolder}", currentMetric)"""),

            // Exploration Achievements
            new Achievement(
                "EXP-000", "Explorer Extraordinaire", "Unlock all the 2.0 Exploration achievements", 1,
                """Utils.OwnsAchievement("EXP-001", achivements)""",
                """Utils.OwnsAchievement("EXP-002", achivements)""",
                """Utils.OwnsAchievement("EXP-003", achivements)""",
                """Utils.OwnsAchievement("EXP-004", achivements)""",
                """Utils.OwnsAchievement("EXP-005", achivements)""",
                """Utils.OwnsAchievement("EXP-006", achivements)""",
                """Utils.OwnsAchievement("EXP-007", achivements)""",
                """Utils.OwnsAchievement("EXP-008", achivements)""",
                """Utils.OwnsAchievement("EXP-009", achivements)"""),
            new Achievement(
                "EXP-001", "Storage Safari", "Navigate to the Storage Explorer page of a workspace", 1,
                $"""Utils.MatchUrl("\\/w\\/([0-9a-zA-Z]+)?\\/filelist$", currentMetric)"""),
            new Achievement(
                "EXP-002", "Databricks Discovery", "Navigate to the Databricks page of a workspace", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserOpenDatabricks}", currentMetric)"""),
            new Achievement(
                "EXP-003", "Resource Ranger", "View the resources section of DataHub", 1,
                $"""Utils.MatchUrl("\\/resources$", currentMetric)"""),
            new Achievement(
                "EXP-004", "Workspace Wanderer", "View a workspace you are not a member of", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserViewProjectNotMemberOf}", currentMetric)"""),
            new Achievement(
                "EXP-005", "Workspace Wayfarer", "Visit one of your own workspaces", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserViewProject}", currentMetric)"""),
            new Achievement(
                "EXP-006", "Link Legend", "Use a recent link to get to the same page again", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserRecentLink}", currentMetric)"""),
            new Achievement(
                "EXP-007", "Prolific Polyglot", "Switch languages in the portal", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserToggleCulture}", currentMetric)"""),
            new Achievement(
                "EXP-008", "Profile Peruser", "View your own profile page", 1,
                $"""Utils.MatchUrl("\\/profile$", currentMetric)"""),
            new Achievement(
                "EXP-009", "Profile Prowler", "View another person's profile", 1,
                $"""Utils.MatchMetric("{TelemetryEvents.UserViewOtherProfile}", currentMetric)""")
        };
    }

    #endregion
}
