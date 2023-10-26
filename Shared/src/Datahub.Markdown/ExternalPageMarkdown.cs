using Markdig.Extensions.Yaml;
using Markdig.Renderers.Roundtrip;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Markdown
{
    public class ExternalPageMarkdown
    {
        public static bool IsExternalMarkdown(string input)
        {
            var document = Markdig.Markdown.Parse(input, MarkdownHelper.pipeline);            
            var allDescendants = document.Descendants<ParagraphBlock>().ToList();
            // extract the front matter from markdown document
            if (allDescendants.Count > 1)
                return false;
            var t = allDescendants[0]?.Inline?.FirstChild?.ToString();
            return t?.Contains("https://")?? false;
        }
    }
}
