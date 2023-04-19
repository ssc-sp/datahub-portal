namespace Datahub.Core.Data.Project
{
    public enum ProjectMemberRole
    {
        Remove = 0, // default is used in the UI to remove a user from a project
        Collaborator = 1,
        Admin = 2,
        WorkspaceLead = 3,
    }
    
}