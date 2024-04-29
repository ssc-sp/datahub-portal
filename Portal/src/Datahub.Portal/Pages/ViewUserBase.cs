// ReSharper disable InconsistentNaming

using Datahub.Application.Services.UserManagement;
using Datahub.Core.Extensions;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Services;
using Microsoft.AspNetCore.Components;

namespace Datahub.Portal.Pages;

public class ViewUserBase<T> : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "u")]
    public string UserIdBase64 { get; set; }
    
    [Inject]
    protected IUserInformationService _userInformationService { get; set; } = null!;
    [Inject]
    protected ILogger<T> _logger { get; set; } = null!;
    
    protected async Task<PortalUser> GetViewedPortalUserWithAchievementsAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(UserIdBase64))
            {
                return await _userInformationService.GetPortalUserWithAchievementsAsync(UserIdBase64.Base64Decode());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get user information for user with id {UserIdBase64}, falling back to current user",
                UserIdBase64);
        }

        return await _userInformationService.GetCurrentPortalUserWithAchievementsAsync();
    }
}