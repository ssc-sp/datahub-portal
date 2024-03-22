namespace Datahub.Markdown.Model;

#nullable enable

public enum DocItemType
{
    Root, Document, Tutorial, Folder, External
}

public class DocItem
{
    public static DocItem MakeRoot(DocumentationGuideRootSection guide, string id)
    {
        return new DocItem(guide, id, 0, true, null, null, DocItemType.Root);
    }

    public static DocItem GetItem(DocumentationGuideRootSection guide, string id, int level, string? title, string? markdownPage)
    {

        var itemtype = DocItemType.Folder;
        if (markdownPage != null)
        {
            if (markdownPage.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                itemtype = DocItemType.External;
            else
                itemtype = DocItemType.Document;

        }
        return new DocItem(guide, id, level, false, title, markdownPage, itemtype);
    }

    public static DocItem GetFolderItem(DocumentationGuideRootSection guide, string id, int level, string? title, IEnumerable<DocItem> children)
    {
        var item = new DocItem(guide, id, level, false, title, null, DocItemType.Folder);
        item.Children.AddRange(children);
        return item;
    }

    private DocItem(DocumentationGuideRootSection guide, string id, int level, bool root, string? title, string? markdownPage, DocItemType docType)
    {
        Id = id;
        Level = level;
        IsRoot = root;
        Title = title;
        MarkdownPage = markdownPage;
        Children = new List<DocItem>();
        RootSection = guide;
        DocType = docType;
    }

    public int Level { get; }
    public bool IsRoot { get; }

    public DocItemType DocType { get; private set; }

    public string? Title { get; set; }
    public string? Id { get; set; }
    public List<DocItem> Children { get; }
    public string? CustomIcon { get; set; }
    public string? Preview { get; set; }
    public string? ContentTitle { get; set; }
    //public string? Content { get; set; }
    private string? _content;

    public string? Content
    {
        get { return _content; }
        set {
            _content = value;
            if (_content?.Contains("<video",StringComparison.InvariantCultureIgnoreCase) ?? false)
                DocType = DocItemType.Tutorial;
        }
    }

    public string? MarkdownPage { get; init; }

    public DocumentationGuideRootSection RootSection { get; set; }

    public string GetDescription() => $"Card '{Title}' - '{MarkdownPage}'";
    public string? GetMarkdownFileName() => MarkdownPage;// ?? "README.md"

    public DocItem? LocateID(string id)
    {
        if (string.Equals(id, Id, StringComparison.InvariantCultureIgnoreCase))
            return this;

        foreach (var item in Children)
        {
            var found = item.LocateID(id);
            if (found != null)
                return found;
        }

        return null;
    }

    public DocItem? LocatePath(string path)
    {
        if (string.Equals(path, MarkdownPage, StringComparison.InvariantCultureIgnoreCase))
            return this;

        foreach (var item in Children)
        {
            var found = item.LocatePath(path);
            if (found != null)
                return found;
        }

        return null;
    }

    public void AddFirstChild(DocItem item)
    {
        Children.Insert(0, item);
    }

    public DocItem Clone()
    {
        var cloned = MemberwiseClone() as DocItem;
        cloned!.Id = Guid.NewGuid().ToString();
        return cloned;
    }

    public override string ToString() => GetDescription();
}

#nullable disable