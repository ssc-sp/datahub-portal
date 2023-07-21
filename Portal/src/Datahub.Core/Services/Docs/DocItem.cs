using System;
using System.Collections.Generic;

namespace Datahub.Core.Services.Docs;

#nullable enable

public class DocItem
{
    public static DocItem MakeRoot(DocumentationGuide guide, string id)
    {
        return new DocItem(guide, id, 0, true, null, null);
    }
    
    public static DocItem GetItem(DocumentationGuide guide, string id, int level, string? title, string? markdownPage)
    {
        return new DocItem(guide, id, level, false, title, markdownPage);
    }

    public static DocItem GetFolderItem(DocumentationGuide guide, string id, int level, string? title, IEnumerable<DocItem> children)
    {
        var item = new DocItem(guide, id, level, false, title, null, true);
        item.Children.AddRange(children);
        return item;
    }

    private DocItem(DocumentationGuide guide, string id, int level, bool root, string? title, string? markdownPage, bool isFolder = false)
    {
        Id = id;
        Level = level;
        IsRoot = root;
        Title = title;
        MarkdownPage = markdownPage;
        Children = new List<DocItem>();
        DocumentationGuide = guide;
        IsFolder = isFolder;
    }

    public int Level { get; } 
    public bool IsRoot { get; }

    public bool IsFolder { get; }
    public string? Title { get; set; }
    public string? Id { get; set; }
    public List<DocItem> Children { get; }
    public string? Preview { get; set; }
    public string? ContentTitle { get; set; }
    public string? Content { get; set; }
    public string? MarkdownPage { get; set; }

    public DocumentationGuide DocumentationGuide { get; set; } 

    public string GetDescription() => $"Card '{Title}' - '{MarkdownPage}'";
    public string? GetMarkdownFileName() => MarkdownPage;// ?? "README.md"

    public DocItem? LocateID(string id)
    {
        if (string.Equals(id, this.Id, StringComparison.InvariantCultureIgnoreCase)) 
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
        if (string.Equals(path, this.MarkdownPage, StringComparison.InvariantCultureIgnoreCase))
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
        var cloned = this.MemberwiseClone() as DocItem;
        cloned!.Id = Guid.NewGuid().ToString();
        return cloned;
    }

    public override string ToString() => GetDescription(); 
}

#nullable disable