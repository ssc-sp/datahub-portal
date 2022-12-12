using System;
using System.Collections.Generic;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Datahub.Metadata.Utils;
using Xunit;

namespace Datahub.Tests.MetadataTests;

public class CatalogUtilsTests
{
    [Fact]
    public void GroupResults_CountAndPickedResults_AsExpected()
    {
        var targetGroup = Guid.NewGuid();
        var results = new List<CatalogObjectResult>
        {
            new CatalogObjectResult() { CatalogObjectId = 1, Language = CatalogObjectLanguage.English, GroupId = targetGroup },
            new CatalogObjectResult() { CatalogObjectId = 2, Language = CatalogObjectLanguage.French, GroupId = targetGroup },
            new CatalogObjectResult() { CatalogObjectId = 3, Language = CatalogObjectLanguage.English },
        };

        var grouped = CatalogUtils.GroupResults(results, CatalogObjectLanguage.English);
            
        Assert.NotNull(grouped);
        Assert.Equal(2, grouped.Count);
        Assert.Equal(1, grouped[0].CatalogObjectId);
        Assert.Equal(3, grouped[1].CatalogObjectId);
    }

    [Fact]
    public void GroupResults_ResultsOrder_AsExpected()
    {
        var targetGroup = Guid.NewGuid();
        var results = new List<CatalogObjectResult>
        {
            new CatalogObjectResult() { CatalogObjectId = 1, Language = CatalogObjectLanguage.French, GroupId = targetGroup },
            new CatalogObjectResult() { CatalogObjectId = 2, Language = CatalogObjectLanguage.English, GroupId = targetGroup },
            new CatalogObjectResult() { CatalogObjectId = 3, Language = CatalogObjectLanguage.English },
        };

        var grouped = CatalogUtils.GroupResults(results, CatalogObjectLanguage.English);

        Assert.NotNull(grouped);
        Assert.Equal(2, grouped.Count);
        Assert.Equal(2, grouped[0].CatalogObjectId);
        Assert.Equal(3, grouped[1].CatalogObjectId);
    }

    [Fact]
    public void GroupResults_PlantRResults_AsExpected()
    {
        var targetGroup = Guid.NewGuid();
        var results = new List<CatalogObjectResult>
        {
            new CatalogObjectResult() { CatalogObjectId = 362, Language = CatalogObjectLanguage.English },
            new CatalogObjectResult() { CatalogObjectId = 364, Language = CatalogObjectLanguage.English },
            new CatalogObjectResult() { CatalogObjectId = 371, Language = CatalogObjectLanguage.English, GroupId = targetGroup },
            new CatalogObjectResult() { CatalogObjectId = 372, Language = CatalogObjectLanguage.French, GroupId = targetGroup }
        };

        var grouped = CatalogUtils.GroupResults(results, CatalogObjectLanguage.English);

        Assert.NotNull(grouped);
        Assert.Equal(3, grouped.Count);
        Assert.Equal(371, grouped[2].CatalogObjectId);
    }
}