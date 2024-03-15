using System.Security.Claims;
using Datahub.Core.Data;
using Datahub.Core.Services.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Datahub.Core.RoleManagement;

//https://stackoverflow.com/questions/58483620/net-core-3-0-claimstransformation
public class RoleClaimTransformer : IClaimsTransformation
{
    private readonly ServiceAuthManager serviceAuthManager;
    private readonly ILogger<RoleClaimTransformer> logger;

    public RoleClaimTransformer(ServiceAuthManager serviceAuthManager, ILogger<RoleClaimTransformer> logger)
    {
        this.serviceAuthManager = serviceAuthManager;
        this.logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        try
        {
            var userId = principal?.Claims.First(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            if (userId is null)
            {
                logger.LogCritical("user uid not available in claims");
            }
            else
            {
                var claims = ((ClaimsIdentity)principal.Identity);
                if (claims is null)
                    return principal;

                var authorizedProjects = await serviceAuthManager.GetUserAuthorizations(userId);
                claims.AddClaim(new Claim(ClaimTypes.Role, "default"));
                claims.AddClaim(new Claim(ClaimTypes.Role, userId));

                foreach (var (role, project) in authorizedProjects)
                {
                    if (project.Project_Acronym_CD == RoleConstants.DATAHUB_ADMIN_PROJECT && serviceAuthManager.GetViewingAsGuest(userId))
                    {
                        claims.AddClaim(new Claim(ClaimTypes.Role, RoleConstants.DATAHUB_ROLE_ADMIN_AS_GUEST));
                    }
                    else
                    {
                        claims.AddClaim(new Claim(ClaimTypes.Role, $"{project.Project_Acronym_CD}{RoleConstants.GetRoleConstants(role)}"));
                    }
                    if (project.WebAppEnabled == true)
                    {
                        claims.AddClaim(new Claim(ClaimTypes.Role, $"{project.Project_Acronym_CD}{RoleConstants.WEBAPP_SUFFIX}"));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Cannot load project permissions");
        }
        return principal;
    }
}