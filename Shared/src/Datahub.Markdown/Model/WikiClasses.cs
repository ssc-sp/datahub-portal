namespace Datahub.Markdown.Model;

public abstract class AbstractMarkdownPage
{
    public string Title { get; private set; }
    public AbstractMarkdownPage Parent { get; private set; }
    protected AbstractMarkdownPage(string title, AbstractMarkdownPage parent)
    {
        Title = title;
        Parent = parent;
    }
}

public class MarkdownCard : AbstractMarkdownPage
{
    public string Preview { get; private set; }
    public string Url { get; private set; }
    public MarkdownCategory ParentCategory => Parent as MarkdownCategory;

    public MarkdownCard(string title, string preview, string url, MarkdownCategory category): base(title, category)
    {
        Preview = preview;
        Url = url;
        category.AddCard(this);
    }
}

public class MarkdownCategory : AbstractMarkdownPage
{
    private readonly List<MarkdownCard> _cards;

    public IEnumerable<MarkdownCard> Cards => _cards.AsReadOnly();
    private MarkdownLanguageRoot ParentLanguage => Parent as MarkdownLanguageRoot;

    public MarkdownCategory(string title, MarkdownLanguageRoot languageRoot) : base(title, languageRoot)
    {
        _cards = new();
        languageRoot.AddCategory(this);
    }

    public void AddCard(MarkdownCard card)
    {
        _cards.Add(card);
        ParentLanguage?.Index(card);
    }
}

public class MarkdownLanguageRoot : AbstractMarkdownPage
{
    private Dictionary<string, AbstractMarkdownPage> _resourceIndex;
    private readonly List<MarkdownCategory> _categories;

    public IDictionary<string, AbstractMarkdownPage> ResourceIndex => _resourceIndex.AsReadOnly();
    public IEnumerable<MarkdownCategory> Categories => _categories.AsReadOnly();
    public IEnumerable<MarkdownCard> AllCards => Categories.SelectMany(c => c.Cards);

    public AbstractMarkdownPage this[string key] => ResourceIndex[key];

    public MarkdownLanguageRoot(string Language) : base(Language, null)
    {
        _resourceIndex = new Dictionary<string, AbstractMarkdownPage>();
        _categories = new List<MarkdownCategory>();
    }

    public void RefreshIndex()
    {
        var catIndex = _categories.ToDictionary(c => c.Title, c => c as AbstractMarkdownPage);
        var itemIndex = _categories.SelectMany(c => c.Cards).ToDictionary(c => c.Title, c => c as AbstractMarkdownPage);
        _resourceIndex = catIndex.Union(itemIndex).ToDictionary(c => c.Key, c => c.Value);
    }

    public void Index(AbstractMarkdownPage resource)
    {
        if (resource == null) return;

        _resourceIndex[resource.Title] = resource;
    }

    public void AddCategory(MarkdownCategory category)
    {
        _categories.Add(category);
        Index(category);
    }
}