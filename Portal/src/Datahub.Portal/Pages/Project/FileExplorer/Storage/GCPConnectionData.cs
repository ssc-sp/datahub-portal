using Newtonsoft.Json;

namespace Datahub.Portal.Pages.Project.FileExplorer.Storage
{
    public class GCPConnectionData
    {
        private string _rawConnectionData = string.Empty;
        
        public string ConnectionData
        {
            get => _rawConnectionData;
            set
            {
                _rawConnectionData = value;
            }
        }

        private string _serviceAccountCredentialsString;
        private ServiceAccountCredentials _serviceAccountCredentials;

        private bool IsRefreshRequired => _serviceAccountCredentials == null 
            || _serviceAccountCredentialsString == null 
            || _serviceAccountCredentialsString != _rawConnectionData;

        public bool IsValid => Credentials != null;

        public string ProjectId => Credentials?.project_id;

        private ServiceAccountCredentials Credentials
        {
            get
            {
                if (IsRefreshRequired)
                {
                    try
                    {
                        _serviceAccountCredentials = JsonConvert.DeserializeObject<ServiceAccountCredentials>(_rawConnectionData);
                        _serviceAccountCredentialsString = _rawConnectionData;
                    }
                    catch (JsonReaderException)
                    {
                        _serviceAccountCredentials = null;
                    }
                }

                return _serviceAccountCredentials;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
        private class ServiceAccountCredentials
        {
            string type { get; set; }
            public string project_id { get; set; }
            string private_key_id { get; set; }
            string private_key { get; set; }
            string client_email { get; set; }
            string client_id { get; set; }
            string auth_uri { get; set; }
            string token_uri { get; set; }
            string auth_provider_x509_cert_url { get; set; }
            string client_x509_cert_url { get; set; }
            string universe_domain { get; set; }
        }
    }
}
