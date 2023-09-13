namespace Datahub.Core.Services.CatalogSearch;

public record DatahubSearchRequest(string Text, bool French, int MaxResults = 100);
