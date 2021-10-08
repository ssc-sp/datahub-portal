using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Shared.Data
{
    public class APITarget
    {
        public string SearchServiceName { get; set; }
        public string StorageAccountName { get; set; }
        public string KeyVaultName { get; set; }
        public string FileSystemName { get; set; }      
        public string CognitiveSearchURL { get; set; }
        public string LogoutURL { get; set; }
        public string LoginURL { get; set; }
        public string FileIndexName { get; set; }
        public string FileIndexerName { get; set; }
    }
}
