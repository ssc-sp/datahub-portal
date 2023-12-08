using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Datahub.Core.Components.PageBanner
{
    public partial class DHPageAlert
    {
        private const string Prefix = "tutorials";
        
        public const string Home = $"{Prefix}/home";
        public static readonly PageAlertObject HomeAlert = new("/Banners/Landing", "/fr/Banners/Atterrissage");
        
        public const string Projects = $"{Prefix}/projects";
        public static readonly PageAlertObject ProjectsAlert = new("/Banners/AWS-storage-Databricks", "/fr/Banners/Stockage-AWS-Databricks");
        
        public const string ProjectFeatures = $"{Prefix}/features";
        public const string Storage = $"{Prefix}/storage";

        public static readonly Dictionary<string, dynamic> Alerts = new()
        {
            {Home, new List<PageAlertObject>() {HomeAlert, ProjectsAlert}},
        };
    }
    
    public struct PageAlertObject
    {
        public PageAlertObject(string wikiLinkEN, string wikiLinkFR)
        {
            WikiLinkEN = wikiLinkEN;
            WikiLinkFR = wikiLinkFR;
        }

        public string WikiLinkEN { get; set; }
        public string WikiLinkFR { get; set; }
    }
}