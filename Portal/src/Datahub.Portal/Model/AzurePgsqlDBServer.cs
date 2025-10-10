using Datahub.Shared.Entities;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;

namespace Datahub.Portal.Model
{
    public class AzurePgsqlDBServer
    {
        public PostgresTier PostgresTier { get; set; } = new PostgresTier();
        public int StorageSize { get; set; } = 0;
        public string DatabaseHost { get; set; } = "<database_host>";
        public string DatabaseName { get; set; } = "<database_name>";
        public string Username { get; set; } = "<username>";
        public string Password { get; set; }= "<password>";
        public string Port { get; set; } = "<port>";
    }
}
