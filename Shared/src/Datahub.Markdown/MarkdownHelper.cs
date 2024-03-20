using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers.Roundtrip;
using Markdig.Syntax;
using System.Text.RegularExpressions;

namespace Datahub.Markdown
{
	public static class MarkdownHelper
    {
        internal static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter().EnableTrackTrivia().Build();

        public static string RemoveFrontMatter(string input)
        {
            var document = Markdig.Markdown.Parse(input, pipeline);
            var sw = new StringWriter();
            var renderer = new RoundtripRenderer(sw);
            // extract the front matter from markdown document
            var yamlBlocks = document.Descendants<YamlFrontMatterBlock>();
            
            foreach (var block in yamlBlocks)
            {
                block.Lines = new Markdig.Helpers.StringLineGroup();
            }
            renderer.Write(document);
            return sw.ToString();
        }


        private static readonly Regex ICON_REGEX = new Regex(@"\(Icon:([^)]+)\)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static string? ExtractIconFromComments(string? line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            var hasIconMatch = ICON_REGEX.Match(line);
            if (hasIconMatch.Success)
            {
                return hasIconMatch.Groups[1].Value;
            }
            return null;

        }

        private static readonly Regex ICON_REGEX2 = new Regex(@"Icon:([^)]+)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static string? ExtractIconFromURL(string? line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            var hasIconMatch = ICON_REGEX2.Match(line);
            if (hasIconMatch.Success)
            {
                return hasIconMatch.Groups[1].Value;
            }
            return null;

        }
    }
}
