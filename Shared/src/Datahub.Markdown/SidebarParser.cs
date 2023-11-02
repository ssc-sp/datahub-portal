using Datahub.Markdown.Model;
using Markdig.Syntax.Inlines;
using Markdig.Syntax;

namespace Datahub.Markdown
{
    public class SidebarParser
    {
        public static DocItem? ParseSidebar(DocumentationGuideRootSection guide, string? inputMarkdown, Func<string, string?> mapId)
        {
            if (string.IsNullOrEmpty(inputMarkdown))
            {
                return default;
            }

            var doc = Markdig.Markdown.Parse(inputMarkdown);

            var root = DocItem.MakeRoot(guide, MarkdownTools.GetIDFromString("root"));

            ProcessBlock(doc, 0, null, root, mapId);
            return root;
        }

        private static string? GetCommentInline(MarkdownObject? markdown)
        {
            switch (markdown)
            {
                case null:
                    return null;
                case LinkInline linkInline:
                    return MarkdownHelper.ExtractIconFromURL(linkInline.Url);
            }
            return null;
        }        

        private static DocItem? ProcessBlock(MarkdownObject markdownObject, int level, DocItem? currentItem, DocItem parent, Func<string, string?> mapId)
        {
            switch (markdownObject)
            {
                case LiteralInline literalInline:
                    var title = literalInline.ToString().Trim();
                    var id = MarkdownTools.GetIDFromString(title ?? "");
                    if (currentItem is null)
                    {
                        var docItem1 = DocItem.GetItem(parent.RootSection, id, level, title, null);
                        docItem1.CustomIcon = GetCommentInline(literalInline.NextSibling); //MarkdownHelper.ExtractIconFromComments(title);
                        parent?.Children.Add(docItem1);
                        return docItem1;
                    }
                    else
                    {
                        currentItem.Title = title;
                        currentItem.CustomIcon = GetCommentInline(literalInline.NextSibling); //MarkdownHelper.ExtractIconFromComments(title);
                    }

                    return null;

                case LinkInline linkInline:
                    //[Microservice_Architecture](/Architecture/Microservice_Architecture.md)
                    // special case for Icon style comments
                    if (linkInline.Url?.StartsWith("Icon")??false) return null;
                    var itemId = mapId.Invoke(linkInline.Url ?? "");
                    var docItem = DocItem.GetItem(parent.RootSection, itemId, level, linkInline.Title, linkInline.Url);
                    parent.Children.Add(docItem);

                    foreach (var child in linkInline)
                    {
                        ProcessBlock(child, level, docItem, parent, mapId);
                    }
                    return docItem;

                case LeafBlock paragraphBlock:
                    if (paragraphBlock.Inline != null)
                        return ProcessBlock(paragraphBlock.Inline, level, currentItem, parent, mapId);
                    break;

                case ContainerInline inline:
                    DocItem? newDoc = null;
                    foreach (var child in inline)
                    {
                        var res = ProcessBlock(child, level, currentItem, parent, mapId);
                        if (res != null)
                            newDoc = res;
                    }
                    return newDoc;

                case ContainerBlock containerBlock:
                    DocItem? currentParent = parent;
                    var currentLevel = level;
                    if (containerBlock is ListItemBlock)
                        currentLevel++;
                    foreach (var child in containerBlock)
                    {
                        var res2 = ProcessBlock(child, currentLevel, currentItem,
                            (containerBlock is ListItemBlock) ? currentParent : parent, mapId);
                        if (res2 != null)
                            currentParent = res2;
                    }
                    return currentParent;
            }
            return null;
        }

    }

}
