namespace Datahub.Core.Services.Api
{
    public class FunctionHealthResult
    {
        public required string FunctionName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
    public class InfrastructureHealthResult
    {
        public required string ResourceType { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
