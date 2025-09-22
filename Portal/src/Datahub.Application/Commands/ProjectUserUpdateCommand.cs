using Datahub.Core.Model.Projects;

namespace Datahub.Application.Commands;

public class ProjectUserUpdateCommand
{
        public UserRoleLinks ProjectUser { get; set; }
        public int NewRoleId { get; set; }
        public bool IsDataSteward { get; set; }
}