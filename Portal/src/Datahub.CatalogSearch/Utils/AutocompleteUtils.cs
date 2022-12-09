namespace Datahub.CatalogSearch.Utils;

static class AutocompleteUtils
{
    public static IEnumerable<string> GetSuggestions(IEnumerable<string> hits, string searchText)
    {
        HashSet<string> suggestions = new();
        var searchWords = SplitWords(searchText);
        var joinedSearchText = string.Join(" ", searchWords);
        foreach (var hit in hits)
        {
            var hitWords = SplitWords(hit);

            var index = FindMatchPosition(searchWords, hitWords, 0);
            if (index is null)
                continue;

            if (index.Value < hitWords.Length)
            {
                var suggestion = $"{joinedSearchText} {hitWords[index.Value]}";
                if (!suggestions.Contains(suggestion))
                {
                    suggestions.Add(suggestion);
                    yield return suggestion;
                }
            }
        }
    }

    static int? FindMatchPosition(string[] searchWords, string[] hitWords, int delta)
    {
        var p = 0;
        var fails = 0;
        for (var i = 0; i < hitWords.Length; i++)
        {
            var word = hitWords[i];
            if (word == searchWords[p])
            {
                if (++p == searchWords.Length)
                {
                    return i + 1;
                }
            }
            else
            {
                if (++fails >= delta)
                {
                    p = 0;
                }
            }
        }
        return default;
    }

    static string[] SplitWords(string text) => (text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
}