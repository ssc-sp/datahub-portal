using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers.Roundtrip;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Docs
{
    public static class MarkdownHelper
    {
        private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter().EnableTrackTrivia().Build();

        public static string RemoveFrontMatter(string input)
        {
            var document = Markdown.Parse(input, pipeline);
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
    }
}
