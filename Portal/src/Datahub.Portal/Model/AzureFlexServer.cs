namespace Datahub.Portal.Model
{
    public class AzureFlexServer
    {
        public string SKU { get; set; }
        public int vCores { get; set; }
        public string MemorySize { get; set; }
        public string MaxIOPS { get; set; }
        public string MaxBandwidth { get; set; }
        public string Type { get; set; }
        public int StorageSize { get; set; }
    }
}
