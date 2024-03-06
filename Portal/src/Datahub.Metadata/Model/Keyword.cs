namespace Datahub.Metadata.Model;

public class Keyword
{
    public int KeywordId { get; set; }
    public string EnglishTXT { get; set; }
    public string FrenchTXT { get; set; }
    public string Source { get; set; }
    public int Frequency { get; set; }
}