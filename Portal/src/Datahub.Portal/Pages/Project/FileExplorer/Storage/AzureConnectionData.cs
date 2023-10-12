namespace Datahub.Portal.Pages.Project.FileExplorer.Storage
{
    public class AzureConnectionData
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string ConnectionData => $"{AccountName}|{AccountKey}";
    }
}
