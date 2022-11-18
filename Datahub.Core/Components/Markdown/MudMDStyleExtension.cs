using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Components.Markdown
{
    public class MudMDStyleExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.DocumentProcessed -= Pipeline_DocumentProcessed;
            pipeline.DocumentProcessed += Pipeline_DocumentProcessed;
        }

        private static void Pipeline_DocumentProcessed(MarkdownDocument document)
        {
            
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            
        }
    }
}
