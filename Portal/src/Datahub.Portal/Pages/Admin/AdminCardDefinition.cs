namespace Datahub.Portal.Pages.Admin;

public abstract record AdminCardAction(string Caption);
public record AdminCardUrlAction(string Caption, string Url) : AdminCardAction(Caption);
public record AdminCardCodeAction(string Caption, Action Action) : AdminCardAction(Caption);

public class AdminCardDefinition
{
    public string Title { get; }
    public string Description { get; }
    public AdminCardAction Action { get; }
    public bool Localized { get; }

    private bool _isProcessing = false;
    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            _isProcessing = value;
            AdminCard?.UpdateState();
        }
    }

    public AdminCard AdminCard { private get; set; }

    public AdminCardDefinition(string title, string description, AdminCardAction action, bool localized = false)
    {
        Title = title;
        Description = description;
        Action = action;
        Localized = localized;
    }
}