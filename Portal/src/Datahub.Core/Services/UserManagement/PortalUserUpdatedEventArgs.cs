using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Services.UserManagement;

public class PortalUserUpdatedEventArgs : EventArgs
{
        public PortalUser UpdatedUser { get; set; }

        public PortalUserUpdatedEventArgs(PortalUser updatedUser)
        {
            UpdatedUser = updatedUser;
        }
}