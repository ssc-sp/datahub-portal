// ReSharper disable InconsistentNaming

using Datahub.Application.Services.UserManagement;
using Datahub.Core.Extensions;
using Datahub.Core.Model.Users;
using Datahub.Core.Services;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http;

namespace Datahub.Portal.Pages;

public class ViewUserBase<T> : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "u")]
    public string? UserIdBase64 { get; set; }

    [Inject]
    protected IUserInformationService _userInformationService { get; set; } = null!;
    [Inject]
    protected ILogger<T> _logger { get; set; } = null!;

    private PortalUser? _portalUserWithAchievements;
    private PortalUser? _portalUser; // make nullable to allow caching of current user as well

    protected async Task<PortalUser> GetViewedPortalUserWithAchievementsAsync()
    {
        // If the user id parameter is missing, surface a404
        if (!string.IsNullOrWhiteSpace(UserIdBase64))
        {


            try
            {
                var (entra, external) = UserIdBase64.DecodeUserProfileLink();
                var id = entra ?? external ?? throw new HttpRequestException("User id is required", null, HttpStatusCode.NotFound);

                if (_portalUserWithAchievements != null && _portalUserWithAchievements.UserUID() == id)
                {
                    return _portalUserWithAchievements;
                }

                _portalUserWithAchievements = await _userInformationService.GetEntraUserWithAchievementsAsync(id);
                return _portalUserWithAchievements ?? throw new HttpRequestException("User id is required", null, HttpStatusCode.NotFound);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get user information for user with id {UserIdBase64}, falling back to current user",
                    UserIdBase64);
            }
        }

        // Cache current user with achievements when no specific id is provided
        if (_portalUserWithAchievements != null && string.IsNullOrWhiteSpace(UserIdBase64))
        {
            return _portalUserWithAchievements;
        }

        _portalUserWithAchievements = await _userInformationService.GetCurrentPortalUserWithAchievementsAsync();
        return _portalUserWithAchievements;
    }

    protected async Task<PortalUser> GetViewedPortalUserAsync()
    {
        // If the user id parameter is missing, surface a404
        if (!string.IsNullOrWhiteSpace(UserIdBase64))
        {

            try
            {
                var (entra, external) = UserIdBase64.DecodeUserProfileLink();
                var id = entra ?? external ?? throw new HttpRequestException("User id is required", null, HttpStatusCode.NotFound);


                if (_portalUser != null && _portalUser.UserUID() == id)
                {
                    return _portalUser;
                }

                _portalUser = await _userInformationService.GetEntraUserAsync(id);
                return _portalUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get user information for user with id {UserIdBase64}, falling back to current user",
                    UserIdBase64.Replace("\r", "").Replace("\n", ""));
            }
        }

        // Cache current user when no specific id is provided
        if (_portalUser != null && string.IsNullOrWhiteSpace(UserIdBase64))
        {
            return _portalUser;
        }

        _portalUser = await _userInformationService.GetCurrentPortalUserAsync() ?? throw new HttpRequestException("User id is required", null, HttpStatusCode.NotFound);
        return _portalUser;
    }
}