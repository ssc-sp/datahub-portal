using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Utilities;

namespace Datahub.Portal.Pages.Workspace.Users
{
    public partial class WorkspaceUsersPage
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();

            // red border on top, left, and bottom
            _modifiedCellStyle = new StyleBuilder()
                .AddStyle("background-color", $"{Colors.Amber.Default}4D")
                .Build();
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            await InitializedProjectMembers();
        }

        private async Task InitializedProjectMembers()
        {
            _projectUsers = await _projectUserManagementService.GetProjectUsersAsync(WorkspaceAcronym);
            ProjectMemberRoleFilter();
        }

        private bool SearchFilter(Datahub_Project_User projectUser)
        {
            if (string.IsNullOrWhiteSpace(_filterString))
                return true;
            if (projectUser.PortalUser?.DisplayName?.Contains(_filterString, StringComparison.OrdinalIgnoreCase) == true)
                return true;
            return projectUser.PortalUser?.Email?.Contains(_filterString, StringComparison.OrdinalIgnoreCase) == true;
        }

        private void ProjectMemberRoleFilter(int? roleId = null)
        {
            _filteredProjectUsers = _projectUsers
                .Where(x => roleId is null || x.Role.Id == roleId)
                .Where(x => x.Role.Id != (int)Project_Role.RoleNames.Remove)
                .Where(SearchFilter)
                .ToList();
        }

        private void UpdateProjectMemberRole(Datahub_Project_User projectUser, int newRoleId)
        {
            var existingUser = _usersToUpdate.FirstOrDefault(x => x.ProjectUser.PortalUser.GraphGuid == projectUser.PortalUser.GraphGuid);
            if (existingUser != null)
            {
                existingUser.NewRoleId = newRoleId;
                var selectedUser = _currentlySelected.FirstOrDefault(x => x.PortalUserId == projectUser.PortalUserId);
                if (selectedUser != null)
                {
                    selectedUser.RoleId = newRoleId;
                }
                if (NothingChanged())
                {
                    _usersToUpdate.Remove(existingUser);
                    _currentlySelected.RemoveAll(x => x.PortalUserId == projectUser.PortalUserId);
                }
            }
            else
            {
                _usersToUpdate.Add(new ProjectUserUpdateCommand()
                {
                    ProjectUser = projectUser,
                    NewRoleId = newRoleId
                });
                _currentlySelected.Add(new Datahub_Project_User()
                {
                    PortalUserId = projectUser.PortalUserId,
                    IsDataSteward = projectUser.IsDataSteward,
                    RoleId = newRoleId
                });
            }

            StateHasChanged();
        }

        private void UpdateProjectMemberRole(ProjectUserAddUserCommand projectUser, int newRoleId)
        {
            projectUser.RoleId = newRoleId;
            if (projectUser.RoleId == (int)Project_Role.RoleNames.Remove)
            {
                _usersToAdd.Remove(projectUser);
            }

            StateHasChanged();
        }

        private bool IsModified(Datahub_Project_User projectUser)
        {
            return _usersToUpdate.Any(x => x.ProjectUser.PortalUser.GraphGuid == projectUser.PortalUser.GraphGuid);
        }

        private void ChangeDataStewardFlag(Datahub_Project_User projectUser, bool newValue)
        {
            projectUser.IsDataSteward = newValue;
            var existingUser = _usersToUpdate.FirstOrDefault(x => x.ProjectUser.PortalUser.GraphGuid == projectUser.PortalUser.GraphGuid);
            if (existingUser != null)
            {
                existingUser.ProjectUser.IsDataSteward = projectUser.IsDataSteward;
                var selectedUser = _currentlySelected.FirstOrDefault(x => x.PortalUserId == projectUser.PortalUserId);
                if (selectedUser != null)
                {
                    selectedUser.IsDataSteward = projectUser.IsDataSteward;
                }
                if (NothingChanged())
                {
                    _usersToUpdate.Remove(existingUser);
                    _currentlySelected.RemoveAll(x => x.PortalUserId == projectUser.PortalUserId);
                }
            }
            else
            {
                _usersToUpdate.Add(new ProjectUserUpdateCommand()
                {
                    ProjectUser = projectUser,
                    IsDataSteward = newValue,
                    NewRoleId = (int)projectUser.RoleId
                });
                _currentlySelected.Add(new Datahub_Project_User()
                {
                    PortalUserId = projectUser.PortalUserId,
                    IsDataSteward = projectUser.IsDataSteward,
                    RoleId = projectUser.RoleId
                });
            }

            StateHasChanged();
        }

        private bool NothingChanged()
        {
            foreach (var user in _currentlySelected)
            {
                var original = _projectUsers.FirstOrDefault(x => x.PortalUserId == user.PortalUserId);
                if (original == null || original.IsDataSteward != user.IsDataSteward || original.RoleId != user.RoleId)
                {
                    return false;
                }
            }
            return true;
        }

        private async Task OpenDialog()
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
            var dialogOptions = new DialogOptions { MaxWidth = MaxWidth.ExtraLarge };
            var dialogParameters = new DialogParameters
        {
            { "CurrentProjectUsers", _projectUsers.Where(x => x.Role.Id != (int)Project_Role.RoleNames.Remove).ToList() },
            { "ProjectAcronym", WorkspaceAcronym },
            { "Inviter", currentUser }
        };
            var dialog = await _dialogService.ShowAsync<AddNewUsersToProjectDialog>(Localizer["Invite New Users"], dialogParameters, dialogOptions);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                if (result.Data is not List<ProjectUserAddUserCommand> userAddUserCommands)
                {
                    _snackbar.Add(Localizer["Error inviting new users to workspace"], Severity.Error);
                }
                else
                {
                    _usersToAdd.AddRange(userAddUserCommands
                        .Where(c =>
                            !_usersToAdd.Any(x => x.Email.Equals(c.Email, StringComparison.InvariantCultureIgnoreCase)))
                        .ToList());
                    StateHasChanged();
                }
            }
        }

        private async Task SaveChanges()
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
            if (_usersToUpdate.Any() || _usersToAdd.Any())
            {
                _updateInProgress = true;
                StateHasChanged();
                await _projectUserManagementService.ProcessProjectUserCommandsAsync(_usersToUpdate, _usersToAdd, currentUser.Id.ToString());
                _usersToUpdate.Clear();
                _usersToAdd.Clear();
            }
            else
            {
                _snackbar.Add(Localizer["No changes to save"], Severity.Info);
            }

            _updateInProgress = false;
            await InitializedProjectMembers();
            StateHasChanged();
        }
    }
}
