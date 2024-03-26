namespace Datahub.Core.Configuration;

public record DataProjectsConfiguration
{
    //        "DataProjects": {
    //  "PowerBI": false,
    //  "PublicSharing": false,
    //  "Databricks": true,
    //  "SQLServer": false,
    //  "PostgreSQL": true
    //}
    public bool PowerBI { get; set; } = true;
    public bool PublicSharing { get; set; } = true;
    public bool Databricks { get; set; } = true;
    public bool SQLServer { get; set; } = true;
    public bool PostgreSQL { get; set; } = true;
    public bool WebForms { get; set; } = true;
    public bool Costing { get; set; } = true;
    public bool Storage { get; set; } = true;
    public bool ProjectDatabases { get; set; } = true;
    public bool DataEntry { get; set; } = true;
    public bool IoT { get; set; } = true;
    public bool StorageCost { get; set; } = true;
    public bool ComputeCost { get; set; } = true;
    public bool VirtualMachines { get; set; } = false;

    public bool EnableGitModuleRepository { get; set; } = true;
}