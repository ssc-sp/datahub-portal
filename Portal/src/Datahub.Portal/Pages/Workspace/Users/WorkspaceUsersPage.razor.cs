using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Core.Components.AuthViews;
using Datahub.Core.Data;
using Datahub.Core.Model.Projects;
using Datahub.Portal.Components.Dialogs;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Utilities;

namespace Datahub.Portal.Pages.Workspace.Users
{
    internal record WorkspaceUserInfo(int? PortalUserId, int? RoleId, bool IsDataSteward);

    public partial class WorkspaceUsersPage
    {
        protected override void OnInitialized()
        {
            _loading = true;
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
            _loading = false;
        }

        private async Task InitializedProjectMembers()
        {
            _projectUsers = await _projectUserManagementService.GetProjectUsersAsync(WorkspaceAcronym);
            _originalUserInfo = _projectUsers.Select(u => new WorkspaceUserInfo(u.PortalUserId, u.RoleId, u.IsDataSteward)).ToList();
            ProjectMemberRoleFilter(_currentRoleFilter);
        }

        private bool CombinedFilter(UserRoleLinks projectUser)
        {
            // use originalUser for role filtering to ensure users don't disappear from their corresponding role tab when changing role
            var originalUser = _originalUserInfo.FirstOrDefault(u => u.PortalUserId == projectUser.PortalUserId);

            var matchesSearch = string.IsNullOrWhiteSpace(_filterString) ||
                projectUser.PortalUser?.DisplayName?.Contains(_filterString, StringComparison.OrdinalIgnoreCase) == true ||
                projectUser.PortalUser?.Email?.Contains(_filterString, StringComparison.OrdinalIgnoreCase) == true;
            var matchesFilteredRole = _currentRoleFilter is null || originalUser?.RoleId == _currentRoleFilter;
            var isNotRemoved = originalUser?.RoleId != (int)Project_Role.RoleNames.Removed;

            return matchesSearch && matchesFilteredRole && isNotRemoved;
        }

        private void ProjectMemberRoleFilter(int? roleId = null)
        {
            _currentRoleFilter = roleId;
        }

        private static bool IsDataStewardHavingRole(bool isDataSteward, UserRoleLinks projectUser) => isDataSteward && IsAllowedRoleForDataSteward(projectUser);

        private static bool IsRevertUpdate(ProjectUserUpdateCommand command, WorkspaceUserInfo originalInfo) => command?.NewRoleId == originalInfo?.RoleId && command?.IsDataSteward == originalInfo?.IsDataSteward;

        private void ManageUserUpdateCommand(UserRoleLinks projectUser)
        {
            var existingUpdateCommand = _usersToUpdate.FirstOrDefault(x => x.ProjectUser.PortalUser == projectUser.PortalUser);
            var originalUserInfo = _originalUserInfo.FirstOrDefault(x => x.PortalUserId == projectUser.PortalUserId);

            if (existingUpdateCommand != null)
            {
                existingUpdateCommand.NewRoleId = projectUser.RoleId ?? 0;
                existingUpdateCommand.IsDataSteward = projectUser.IsDataSteward;

                if (IsRevertUpdate(existingUpdateCommand, originalUserInfo))
                {
                    _usersToUpdate.Remove(existingUpdateCommand);
                    ValidateWorkspaceRules();
                }
            }
            else
            {
                var updateCommand = new ProjectUserUpdateCommand()
                {
                    ProjectUser = projectUser,
                    NewRoleId = projectUser.RoleId ?? 0,
                    IsDataSteward = projectUser.IsDataSteward
                };

                if (!IsRevertUpdate(updateCommand, originalUserInfo))
                {
                    _usersToUpdate.Add(updateCommand);
                    ValidateWorkspaceRules();
                }
            }
        }


        private void ValidateWorkspaceRules()
        {
            _validationErrorMessage = null;
            var allWorkspaceLeads = _usersToUpdate.Select(_usersToUpdate => _usersToUpdate.ProjectUser).Where(x => x.RoleId == (int)Project_Role.RoleNames.WorkspaceLead).Count();
            var existingWorkspaceLeads = _projectUsers.Except(_usersToUpdate.Select(p => p.ProjectUser)).Where(x => x.RoleId == (int)Project_Role.RoleNames.WorkspaceLead).Count();
            var newLeads = _usersToAdd.Count(x => x.RoleId == (int)Project_Role.RoleNames.WorkspaceLead);
            if (allWorkspaceLeads + newLeads + existingWorkspaceLeads > 1)
            {
                _validationErrorMessage = Localizer["You cannot have more than one workspace lead."];
            }
        }

        private void UpdateProjectMemberRole(UserRoleLinks projectUser, int newRoleId)
        {
            projectUser.RoleId = newRoleId;
            projectUser.IsDataSteward = IsDataStewardHavingRole(projectUser.IsDataSteward, projectUser);

            ManageUserUpdateCommand(projectUser);
            ValidateWorkspaceRules();
            InvokeAsync(StateHasChanged);
        }

        private void UpdateProjectMemberRoleCommand(ProjectUserAddEntraUserCommand projectUser, int newRoleId)
        {
            projectUser.RoleId = newRoleId;
            if (projectUser.RoleId == (int)Project_Role.RoleNames.Removed)
            {
                _usersToAdd.Remove(projectUser);
            }
            ValidateWorkspaceRules();

            InvokeAsync(StateHasChanged);
        }

        private bool IsModified(UserRoleLinks projectUser)
        {
            return _usersToUpdate.Any(x => x.ProjectUser.PortalUser == projectUser.PortalUser);
        }
        private DatahubAuthView.AuthLevels GetAuthLevel(UserRoleLinks projectUser)
        {
            return projectUser.Role?.Id == 2 ? DatahubAuthView.AuthLevels.DatahubSupport : DatahubAuthView.AuthLevels.WorkspaceAdmin;
        }

        private static bool IsAllowedRoleForDataSteward(UserRoleLinks projectUser) => RoleConstants.AllowedDataStewardRoleIds.Contains(projectUser.RoleId ?? 0);

        private static bool IsDataStewardCheckboxDisabled(UserRoleLinks projectUser) => !(projectUser.IsDataSteward || IsAllowedRoleForDataSteward(projectUser));

        private void ChangeDataStewardFlag(UserRoleLinks projectUser, bool newValue)
        {
            projectUser.IsDataSteward = IsDataStewardHavingRole(newValue, projectUser);

            ManageUserUpdateCommand(projectUser);

            InvokeAsync(StateHasChanged);
        }

        private async Task OpenDialog()
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
            var dialogOptions = new DialogOptions { MaxWidth = MaxWidth.ExtraLarge };
            var dialogParameters = new DialogParameters
        {
            { "CurrentProjectUsers", _projectUsers.Where(x => x.Role.Id != (int)Project_Role.RoleNames.Removed).ToList() },
            { "ProjectAcronym", WorkspaceAcronym },
            { "Inviter", currentUser }
        };
            var dialog = await _dialogService.ShowAsync<AddNewEntraUsersToProjectDialog>(Localizer["Invite New Users"], dialogParameters, dialogOptions);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                if (result.Data is not List<ProjectUserAddEntraUserCommand> userAddUserCommands)
                {
                    _snackbar.Add(Localizer["Error inviting new users to workspace"], Severity.Error);
                }
                else
                {
                    _usersToAdd.AddRange(userAddUserCommands
                        .Where(c =>
                            !_usersToAdd.Any(x => x.Email.Equals(c.Email, StringComparison.InvariantCultureIgnoreCase)))
                        .ToList());
                    ValidateWorkspaceRules();
                    StateHasChanged();
                }
            }
        }

        private async Task OpenExternalUserFileRestrictionDialog()
        {
            var uploadLimits = await _projectUserManagementService.GetExternalUserUploadLimits(WorkspaceAcronym);

            var dialogTitle = "External User File Upload Restrictions";
            var dialogOptions = new DialogOptions { MaxWidth = MaxWidth.Medium };
            var dialogParameters = new DialogParameters
            {
                { nameof(WorkspaceExternalUserFileLimitsDialog.UploadLimit), uploadLimits },
                { nameof(WorkspaceExternalUserFileLimitsDialog.Title), dialogTitle },
            };
            var dialog = await _dialogService.ShowAsync<WorkspaceExternalUserFileLimitsDialog>(Localizer[dialogTitle], dialogParameters, dialogOptions);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                if (result.Data is not ExternalUserUploadLimit updatedLimits)
                {
                    _snackbar.Add(Localizer["Error updating external user file upload restrictions"], Severity.Error);
                }
                else
                {
                    try
                    {
                        await _projectUserManagementService.UpdateExternalUserUploadLimits(WorkspaceAcronym, updatedLimits);

                        _snackbar.Add(Localizer["External user file upload restrictions updated successfully"], Severity.Success);
                    }
                    catch (Exception)
                    {
                        _snackbar.Add(Localizer["Error updating external user file upload restrictions"], Severity.Error);
                    }
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
