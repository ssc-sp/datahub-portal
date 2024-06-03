namespace Datahub.Core.Services.Api
{
    public class FunctionHealthResult
    {
        public string FunctionName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
    public class InfrastructureHealthResult
    {
        public string ResourceType { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}