using Datahub.Core.Model.Projects;

namespace Datahub.Application.Commands;

public class ProjectUserUpdateCommand
{
    public DatahubProjectUser ProjectUser { get; set; }
    public int NewRoleId { get; set; }
}