using System;
using System.Collections.Generic;

namespace Datahub.Core.Services.Docs;

#nullable enable

public class DocItem
{
    public static DocItem MakeRoot(string id)
    {
        return new DocItem(id, 0, true, null, null);
    }
    
    public static DocItem GetItem(string id, int level, string? title, string? markdownPage)
    {
        return new DocItem(id, level, false, title, markdownPage);
    }

    private DocItem(string id, int level, bool root, string? title, string? markdownPage)
    {
        Id = id;
        Level = level;
        IsRoot = root;
        Title = title;
        MarkdownPage = markdownPage;
        Childs = new List<DocItem>();
    }

    public int Level { get; } 
    public bool IsRoot { get; }
    public string? Title { get; set; }
    public string? Id { get; set; }
    public List<DocItem> Childs { get; }
    public string? Preview { get; set; }
    public string? ContentTitle { get; set; }
    public string? Content { get; set; }
    public string? MarkdownPage { get; set; }

    public string GetDescription() => $"Card '{Title}' - '{MarkdownPage}'";
    public string GetMarkdownFileName() => MarkdownPage ?? "README.md";

    public DocItem? LocateID(string id)
    {
        if (string.Equals(id, this.Id, StringComparison.InvariantCultureIgnoreCase)) 
            return this;

        foreach (var item in Childs)
        {
            var found = item.LocateID(id);
            if (found != null)
                return found;
        }

        return null;
    }
}

#nullable disable