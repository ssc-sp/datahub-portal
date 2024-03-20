namespace Datahub.Core.Services.ExcelParser;

public class ExcelTextContentParser : IExcelContentParser
{
    public String ValidMimeType { get; } = "text/plain";

    public Task<IList<String[]>> GetRows(String input) =>
        Task.FromResult<IList<String[]>>(input.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x =>
            x.Split("\t").Select(y => y ?? String.Empty).ToArray())
        .ToList());
}