namespace Datahub.Portal.Model
{
    public class AzureDBServer
    {
        public string SKU { get; set; } = "N/A";
        public int vCores { get; set; } = 0;
        public string MemorySize { get; set; } = "N/A";
        public string MaxIOPS { get; set; } = "N/A";
        public string MaxBandwidth { get; set; } = "N/A";
        public string Type { get; set; } = "N/A";
        public int StorageSize { get; set; } = 0;
        public string DatabaseHost { get; set; } = "<database_host>";
        public string DatabaseName { get; set; } = "<database_name>";
        public string Username { get; set; } = "<username>";
        public string Password { get; set; }= "<password>";
        public string Port { get; set; } = "<port>";
    }
}
