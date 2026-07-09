using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.UserManagement;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Datahub.Functions.Services
{
    internal class FunctionUserInformationService : IUserInformationService
    {
        public event EventHandler<PortalUserUpdatedEventArgs> PortalUserUpdated;

        public Task<bool> CheckUserInTenant(string email)
        {
            throw new NotImplementedException();
        }

        public Task<PortalUser?> CreatePortalEntraUserAsync(string userGraphId)
        {
            throw new NotImplementedException();
        }

        public Task<PortalUser?> CreatePortalExternalUserAsync(string? userOid, string first, string last, string org, string email, DateTimeOffset expiry)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetAnonymousGraphUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetCurrentGraphUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PortalUser?> GetCurrentPortalUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PortalUser> GetCurrentPortalUserWithAchievementsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetCurrentUserEntraId()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDisplayName()
        {
            throw new NotImplementedException();
        }

        public Task<PortalUser> GetEntraUserAsync(string userGraphId)
        {
            throw new NotImplementedException();
        }

        public Task<PortalUser> GetEntraUserWithAchievementsAsync(string userGraphId)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetExternalUserNameIdentifier()
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetExternalUserNamePreferredLanguage()
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetGraphUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ExtendedPortalUser?> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserEmail()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserEmailDomain()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserEmailPrefix()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserRootFolder()
        {
            throw new NotImplementedException();
        }

        public Task HandleDeletedEntraUserRegistration(string email, string graphId, int portalUserId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAdminModeEnabled()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAuthorized()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsDailyLogin()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsEntraUser()
        {
            return Task.FromResult(false);
        }


        public Task<bool> IsExternalUser()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserDatahubAdmin()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserProjectAdmin(string projectAcronym)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserProjectMember(string projectAcronym)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserProjectWorkspaceLead(string projectAcronym)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserWithoutWorkspaces()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsViewingAsVisitor()
        {
            throw new NotImplementedException();
        }

        public Task RegisterAuthenticatedEntraUser()
        {
            throw new NotImplementedException();
        }

        public Task SetAdminModeView(bool isAdminMode)
        {
            throw new NotImplementedException();
        }

        public Task SetViewingAsVisitor(bool isVisitor)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePortalUserAsync(PortalUser updatedUser)
        {
            throw new NotImplementedException();
        }
    }
}
