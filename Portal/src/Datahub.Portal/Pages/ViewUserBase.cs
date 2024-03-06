// ReSharper disable InconsistentNaming

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
    protected IUserInformationService UserInformationService { get; set; } = null!;
    [Inject]
    protected ILogger<T> Logger { get; set; } = null!;

    protected async Task<PortalUser> GetViewedPortalUserWithAchievementsAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(UserIdBase64))
            {
                return await UserInformationService.GetPortalUserWithAchievementsAsync(UserIdBase64.Base64Decode());
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Failed to get user information for user with id {UserIdBase64}, falling back to current user",
                UserIdBase64);
        }

        return await UserInformationService.GetCurrentPortalUserWithAchievementsAsync();
    }
}