namespace Datahub.Portal.Pages.Help;

public sealed record SupportRequestFormData(
    IReadOnlyCollection<string> Topics,
    IReadOnlyCollection<string> Workspaces,
    string Description,
    string PreferredLanguage);
