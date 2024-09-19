using Datahub.Core.Model.Projects;

namespace Datahub.Application.Commands;

public class ProjectUserUpdateCommand
{
        public Datahub_Project_User ProjectUser { get; set; }
        public int NewRoleId { get; set; }
        public bool IsDataSteward { get; set; }
}