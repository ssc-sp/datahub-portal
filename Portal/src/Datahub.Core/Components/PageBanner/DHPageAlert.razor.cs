using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Datahub.Core.Components.PageBanner
{
    public partial class DHPageAlert
    {
        
        private const string Prefix = "tutorials";
        
        public const string Home = $"{Prefix}/home";
        public static readonly PageAlertObject WelcomeAlert = new("Landing", "Atterrissage");
        public static readonly PageAlertObject LayoutAlert = new("Layout", "Mise-en-page");
        public static readonly PageAlertObject TopbarAlert = new("Topbar", "Barre-supérieure");
        public static readonly PageAlertObject SidebarAlert = new("Sidebar", "Encadré");
        public static readonly PageAlertObject NewWorkspaceAlert = new("NewWorkspace", "Nouvel-espace-de-travail");
        
        public const string Workspace = $"{Prefix}/workspace";
        public static readonly PageAlertObject WorkspaceAlert = new("Workspace", "Espace-de-travail");
        public static readonly PageAlertObject WorkspaceLayoutAlert = new("WorkspaceLayout", "Structure-de-l'espace-de-travail");
        public static readonly PageAlertObject WorkspaceSidebarAlert = new("WorkspaceSidebar", "Encadré-de-l'espace-de-travail");
        public static readonly PageAlertObject WorkspaceMetadataAlert = new("WorkspaceMetadata", "Métadonnées-de-l'espace-de-travail");
        public static readonly PageAlertObject WorkspaceMembersAlert = new("WorkspaceMembers", "Membres-de-l'espace-de-travail");
        public static readonly PageAlertObject WorkspaceToolboxAlert = new("WorkspaceToolbox", "Boîte-à-outils-de-l'espace-de-travail");
        

        public static readonly Dictionary<string, List<PageAlertObject>> Alerts = new()
        {
            {Home, new List<PageAlertObject>() {WelcomeAlert,LayoutAlert, TopbarAlert, SidebarAlert,  NewWorkspaceAlert}},
            {Workspace, new List<PageAlertObject>() {WorkspaceAlert, WorkspaceLayoutAlert, WorkspaceSidebarAlert, WorkspaceMetadataAlert, WorkspaceMembersAlert, WorkspaceToolboxAlert}},
        };
    }
    
    public struct PageAlertObject
    {
        private const string BannerPath = "/Banners/";
        private const string BannerPathFR = "/fr/Banners/";
        
        public PageAlertObject(string wikiLinkEN, string wikiLinkFR)
        {
            WikiLinkEN = BannerPath+wikiLinkEN;
            WikiLinkFR = BannerPathFR+wikiLinkFR;
        }

        public string WikiLinkEN { get; set; }
        public string WikiLinkFR { get; set; }
    }
}