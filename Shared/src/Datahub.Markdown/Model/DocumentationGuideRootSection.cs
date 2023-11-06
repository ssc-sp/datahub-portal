using System.ComponentModel;

namespace Datahub.Markdown.Model;

public enum DocumentationGuideRootSection
{
    [Description("UserGuide")]
    UserGuide,
    [Description("AdminGuide")]
    AdminGuide,
    [Description("DeveloperGuide")]
    DevGuide,
    [Description("")]
    RootFolder,
    [Description("Hidden")]
    Hidden,
}

#nullable disable