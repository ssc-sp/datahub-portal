using Datahub.Markdown;
using Xunit;


namespace Datahub.Tests.Docs
{
	public class MarkdownTests
    {
        [Fact]
        public void GivenMarkdown_RemoveFrontmatter()
        {
            var md = @"---
remarks: Automatically translated with DeepL
source: /Banners/Landing.md
---

_(draft documentation, please review)_

## Qu'est-ce que le DataHub ?

Le DataHub est une plateforme d'entreprise permettant de stocker, de travailler et de collaborer sur des initiatives de données dans tous les secteurs. Il s'agit d'un emplacement central permettant aux utilisateurs de stocker des données, d'effectuer des analyses collaboratives, de manipuler des données à l'aide d'outils d'analyse avancés et de mener des expériences de science des données.

[En savoir plus]()
";
            var expected = @"
_(draft documentation, please review)_

## Qu'est-ce que le DataHub ?

Le DataHub est une plateforme d'entreprise permettant de stocker, de travailler et de collaborer sur des initiatives de données dans tous les secteurs. Il s'agit d'un emplacement central permettant aux utilisateurs de stocker des données, d'effectuer des analyses collaboratives, de manipuler des données à l'aide d'outils d'analyse avancés et de mener des expériences de science des données.

[En savoir plus]()
";
            var cleaned = MarkdownHelper.RemoveFrontMatter(md);
            Assert.DoesNotContain("---", cleaned);
            Assert.Equal(expected, cleaned);

        }




        [Fact]
        public void GivenSidebarLine_ExtractIcon()
        {
            var line = "- Learn [](Icon:LibraryBooks)";
            Assert.Equal("LibraryBooks", MarkdownHelper.ExtractIconFromComments(line));
        }
    }
}
