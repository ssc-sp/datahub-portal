using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration
{
    public class PostgresConfiguration : IWorkspaceToolConfiguration
    {
        public string PSQL_SKU { get; set; }
        public string ResourceNameSuffix { get; set; }

        public IWorkspaceToolConfiguration Clone() => new PostgresConfiguration()
        {
            PSQL_SKU = PSQL_SKU,
            ResourceNameSuffix = ResourceNameSuffix
        };
    }
}
