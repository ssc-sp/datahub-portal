using Datahub.Core.Model.Projects;

namespace Datahub.Stories.Utils;

/// <summary>
/// This class is used to provide default values for the stories.
/// </summary>
public static class Placeholder
{
    /// <summary>
    /// The default project used in the stories.
    /// </summary>
    public static Datahub_Project DatahubProject = new()
    {
        Project_Acronym_CD = "STORY",
        Project_Name = "Blazing Story Workspace",
        Project_Name_Fr = "Espace de travail de Blazing Story",
        Project_Summary_Desc = "This is a placeholder workspace",
        Project_Summary_Desc_Fr = "Ceci est un espace de travail temporaire",
        
    };
}