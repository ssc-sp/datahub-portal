using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Core.Components.AuthViews;
using Datahub.Core.Data;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Utilities;
using Datahub.Portal.Pages.Tools.LockedUsers;

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
            
            // MOCK: Add fake locked users for demo purposes
            await AddMockLockedUsers();
            
            _originalUserInfo = _projectUsers.Select(u => new WorkspaceUserInfo(u.PortalUserId, u.RoleId, u.IsDataSteward)).ToList();
            ProjectMemberRoleFilter(_currentRoleFilter);
        }
        
        // MOCK: Add fake locked users to demonstrate the locked user workflow
        private async Task AddMockLockedUsers()
        {
            // Create fake locked users if they don't already exist
            var fakeLockedUsers = new[]
            {
                new { Email = "external.contractor@example.com", Name = "External Contractor" },
                new { Email = "test.user@example.com", Name = "Test User" }
            };
            
            var index = 0;
            foreach (var fakeUser in fakeLockedUsers)
            {
                // Check if user already exists
                if (!_projectUsers.Any(u => u.PortalUser?.Email == fakeUser.Email))
                {
                    // Create a fake UserRole with PortalUser
                    // Use negative IDs to avoid conflicts with real database records
                    var mockPortalUserId = -1000 - index; // Negative IDs for mock data
                    
                    var mockPortalUser = new PortalUser
                    {
                        Email = fakeUser.Email,
                        DisplayName = fakeUser.Name,
                        Id = mockPortalUserId
                    };
                    
                    var mockRole = new Project_Role
                    {
                        Id = (int)Project_Role.RoleNames.Collaborator,
                        Name = "Collaborator",
                        Description = "Collaborator role for demo"
                    };
                    
                    var mockUser = new UserRoleLinks
                    {
                        PortalUser = mockPortalUser,
                        Role = mockRole,
                        RoleId = mockRole.Id,
                        IsDataSteward = false,
                        Approved_DT = DateTime.Now.AddDays(-30),
                        PortalUserId = mockPortalUser.Id
                    };
                    
                    _projectUsers.Add(mockUser);
                }
                index++;
            }
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

        private async Task OpenUploadEvidenceDialog(PortalUser user)
        {
            var parameters = new DialogParameters
            {
                { "User", user }
            };

            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium };
            var dialog = await _dialogService.ShowAsync<UploadVirusScanEvidenceDialog>(
                Localizer["Upload Virus Scan Evidence"], 
                parameters, 
                options);

            var result = await dialog.Result;
            if (result != null && !result.Canceled)
            {
                await InitializedProjectMembers();
            }
        }
        
        // MOCK: Simulate locked user based on email pattern for UI demo
        private bool IsMockLockedUser(PortalUser? user)
        {
            if (user?.Email == null) return false;
            // For demo purposes, treat users with 'external' or 'test' in email as locked
            return user.Email.Contains("external", StringComparison.OrdinalIgnoreCase) ||
                   user.Email.Contains("test", StringComparison.OrdinalIgnoreCase);
        }
    }
}
