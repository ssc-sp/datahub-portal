using System.Collections.Immutable;
using System.Security;
using System.Security.Claims;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Core.Data;
using Datahub.Core.Model.Projects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

namespace Datahub.Application.RoleManagement;

//https://stackoverflow.com/questions/58483620/net-core-3-0-claimstransformation
public class RoleClaimTransformer(IServiceAuthManager serviceAuthManager, DatahubPortalConfiguration portalConfiguration, ILogger<RoleClaimTransformer> logger)
    : IClaimsTransformation
{
    // Not included in ClaimTypes or ClaimConstants
    public const string IDENTITY_PROVIDER_CLAIM_TYPE = "http://schemas.microsoft.com/identity/claims/identityprovider";
    public const string IDP_PROVIDER_CLAIM = "idp_provider";
    public const string IDP_GCCF = "clegc-gckey.gc.ca";

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        try
        {
            // ReSharper disable once StringLiteralTypo
            if (principal?.Identity is not ClaimsIdentity claims)
                return principal!;
            bool isEntra = VerifyTrustedEntraLogin(claims);
            ImmutableList<(Project_Role Role, Datahub_Project Project)> authorizedProjects;
            if (!isEntra)
            {
                var externalId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("No GCCF ID available");
                authorizedProjects = await serviceAuthManager.GetExternalUserAuthorizations(externalId);
                foreach (var (role, project) in authorizedProjects)
                {                   
                    claims.AddClaim(new Claim(ClaimTypes.Role, $"{project.Project_Acronym_CD}{RoleConstants.GetRoleSuffixes(role)}"));
                }
            }
            else
            {

                var userEntraId = principal.Claims.FirstOrDefault(c => c.Type == ClaimConstants.ObjectId)?.Value ?? throw new InvalidOperationException("User Entra ID not found");
                authorizedProjects = await serviceAuthManager.GetEntraUserAuthorizations(userEntraId);
                claims.AddClaim(new Claim(ClaimTypes.Role, "default"));
                claims.AddClaim(new Claim(ClaimTypes.Role, userEntraId));

                var userEmail = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (userEmail is null)
                {
                    logger.LogError($"email not available for user with uid {userEntraId}");
                }
                else if (await serviceAuthManager.IsUserCbrOwner(userEmail))
                {
                    claims.AddClaim(new Claim(ClaimTypes.Role, RoleConstants.CBR_OWNER_ROLE));
                    var cbrWorkspaces = await serviceAuthManager.GetUserCbrWorkspaceAcronyms(userEmail);
                    claims.AddClaims(cbrWorkspaces.Select(w => new Claim(ClaimTypes.Role, $"{w}{RoleConstants.CBR_OWNER_SUFFIX}")));
                }

                // Ensure that the user can't be both approver and admin
                var alreadyAdded = claims.HasClaim(ClaimTypes.Role, RoleConstants.DATAHUB_ROLE_ADMIN_AS_GUEST) || claims.HasClaim(ClaimTypes.Role, RoleConstants.DATAHUB_APPROVER_ROLE);
                var isAdminMode = serviceAuthManager.IsAdminModeEnabled(userEntraId);
                foreach (var (role, project) in authorizedProjects)
                {
                    if (!alreadyAdded && project.Project_Acronym_CD == RoleConstants.DATAHUB_ADMIN_PROJECT && !isAdminMode)
                    {
                        claims.AddClaim(new Claim(ClaimTypes.Role, RoleConstants.DATAHUB_ROLE_ADMIN_AS_GUEST));
                    }
                    else if (!alreadyAdded && project.Project_Acronym_CD == RoleConstants.DATAHUB_APPROVER_PROJECT)
                    {
                        claims.AddClaim(new Claim(ClaimTypes.Role, RoleConstants.DATAHUB_APPROVER_ROLE));
                    }
                    else
                    {
                        claims.AddClaim(new Claim(ClaimTypes.Role, $"{project.Project_Acronym_CD}{RoleConstants.GetRoleSuffixes(role)}"));
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
        return principal!;
    }

    private bool VerifyTrustedEntraLogin(ClaimsIdentity claims)
    {
        if (claims.HasClaim(ClaimTypes.Role, RoleConstants.TRUSTED_ENTRA_LOGIN) 
            || claims.HasClaim(ClaimTypes.Role, RoleConstants.EXTERNAL_LOGIN))
        {
            // User is already marked as trusted or external
            return claims.HasClaim(ClaimTypes.Role, RoleConstants.TRUSTED_ENTRA_LOGIN);
        }        

        var utid = claims.Claims.FirstOrDefault(c => c.Type == ClaimConstants.UniqueTenantIdentifier)?.Value;

        var tenantId = portalConfiguration.AzureAd.TenantId;

        var identityProviderClaim = claims.Claims.FirstOrDefault(c => c.Type == IDENTITY_PROVIDER_CLAIM_TYPE);

        var tenantIssuer = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        var idProvider = $"https://sts.windows.net/{utid}/";
        
        bool trusted = identityProviderClaim != null &&
            identityProviderClaim.Value == idProvider &&
            identityProviderClaim.Issuer == tenantIssuer;

        if (!trusted)
        {
            var idp = claims.Claims.FirstOrDefault(c => c.Type == IDP_PROVIDER_CLAIM)?.Value;
            if (idp?.EndsWith(IDP_GCCF) ?? false)
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, RoleConstants.EXTERNAL_LOGIN));
            }
            else
                throw new SecurityException("Invalid IDP login");
            return false;
        }
        else
        {
            var trustedClaim = new Claim(ClaimTypes.Role, RoleConstants.TRUSTED_ENTRA_LOGIN);
            claims.AddClaim(trustedClaim);
            return true;
        }
    }
}
