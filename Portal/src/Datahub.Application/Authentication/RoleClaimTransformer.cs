using System.Collections.Immutable;
using System.Security;
using System.Security.Claims;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Core.Configuration;
using Datahub.Core.Data;
using Datahub.Core.Model.Projects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;

namespace Datahub.Application.Authentication;

//https://stackoverflow.com/questions/58483620/net-core-3-0-claimstransformation
public class RoleClaimTransformer(IServiceAuthManager serviceAuthManager, DatahubPortalConfiguration portalConfiguration,
    IFeatureManagerSnapshot featureManager,
    ILogger<RoleClaimTransformer> logger)
    : IClaimsTransformation
{
    public const string GENERATED_CLAIM_ISSUER = "Datahub.RoleClaimTransformer";
    // Not included in ClaimTypes or ClaimConstants
    public const string IDENTITY_PROVIDER_CLAIM_TYPE = "http://schemas.microsoft.com/identity/claims/identityprovider";
    //public const string IDP_PROVIDER_CLAIM = "idp_provider";
    public const string IDP_QUALIFIER_CLAIM = "idp_qualifier";
    public const string EXTERNAL_LOCALE_CLAIM = "locale";

    public const string IDP_GCCF = "clegc-gckey.gc.ca";
    private bool? traceClaimsEnabled = null;

    private void AddAndTraceClaim(ClaimsIdentity claimsIdentity, Claim claim, string? message = null)
    {
        var generatedClaim = new Claim(
            claim.Type,
            claim.Value,
            claim.ValueType,
            GENERATED_CLAIM_ISSUER,
            GENERATED_CLAIM_ISSUER,
            claimsIdentity);
        claimsIdentity.AddClaim(generatedClaim);
        if (traceClaimsEnabled == true)
        {
            logger.LogInformation("Adding claim to user {UserId}: {ClaimType} = {ClaimValue}{Message}", claimsIdentity.Name, claim.Type, claim.Value, $" ({message})");
        }
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        return await TransformAsync(principal, forceRefresh: false, throwOnFailure: false);
    }

    /// <summary>
    /// Rebuilds portal-generated role claims from current database state.
    /// </summary>
    public async Task<ClaimsPrincipal> RefreshAsync(ClaimsPrincipal principal)
    {
        var refreshedPrincipal = CloneWithoutGeneratedClaims(principal);
        return await TransformAsync(refreshedPrincipal, forceRefresh: true, throwOnFailure: true);
    }

    private async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal, bool forceRefresh, bool throwOnFailure)
    {
        try
        {
            if (traceClaimsEnabled is null)
            {
                traceClaimsEnabled = await featureManager.IsEnabledAsync(Features.Trace_Claims);
            }
            if (principal?.Identity is not ClaimsIdentity claims)
                return principal!;
            foreach (var generatedClaim in claims.Claims.Where(claim => claim.Issuer == GENERATED_CLAIM_ISSUER).ToList())
            {
                claims.RemoveClaim(generatedClaim);
            }
            bool isEntra = VerifyTrustedEntraLogin(claims);
            ImmutableList<(Project_Role Role, Datahub_Project Project)> authorizedProjects;
            if (!isEntra)
            {
                var externalId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("No GCCF ID available");
                AddAndTraceClaim(claims, new Claim(ClaimTypes.Role, externalId.ToString()));
                authorizedProjects = forceRefresh
                    ? await serviceAuthManager.GetFreshExternalUserAuthorizations(externalId)
                    : await serviceAuthManager.GetExternalUserAuthorizations(externalId);
                foreach (var (role, project) in authorizedProjects)
                {
                    foreach (var roleSuffix in RoleConstants.GetRoleSuffixes(role))
                    {
                        AddAndTraceClaim(claims, new Claim(ClaimTypes.Role, $"{project.Project_Acronym_CD}{roleSuffix}"));
                    }
                }
            }
            else
            {

                var userEntraId = principal.Claims.FirstOrDefault(c => c.Type == ClaimConstants.ObjectId)?.Value ?? throw new InvalidOperationException("User Entra ID not found");
                AddAndTraceClaim(claims, new Claim(ClaimTypes.Role, userEntraId.ToString()));
                authorizedProjects = forceRefresh
                    ? await serviceAuthManager.GetFreshEntraUserAuthorizations(userEntraId)
                    : await serviceAuthManager.GetEntraUserAuthorizations(userEntraId);

                var userEmail = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (userEmail is null)
                {
                    logger.LogError($"email not available for user with uid {userEntraId}");
                }
                else if (await serviceAuthManager.IsUserCbrOwner(userEmail))
                {
                    AddAndTraceClaim(claims, new Claim(ClaimTypes.Role, RoleConstants.CBR_OWNER_ROLE));
                    var cbrWorkspaces = await serviceAuthManager.GetUserCbrWorkspaceAcronyms(userEmail);
                    foreach (var workspace in cbrWorkspaces)
                    {
                        AddAndTraceClaim(claims, new Claim(ClaimTypes.Role, $"{workspace}{RoleConstants.CBR_OWNER_SUFFIX}"));
                    }
                }

                // Ensure that the user can't be both approver and admin
                var alreadyAdded = claims.HasClaim(ClaimTypes.Role, RoleConstants.DATAHUB_ROLE_ADMIN_AS_GUEST) || claims.HasClaim(ClaimTypes.Role, RoleConstants.DATAHUB_APPROVER_ROLE);
                var isAdminMode = serviceAuthManager.IsAdminModeEnabled(userEntraId);
                foreach (var (role, project) in authorizedProjects)
                {
                    if (!alreadyAdded && project.Project_Acronym_CD == RoleConstants.DATAHUB_ADMIN_PROJECT && !isAdminMode)
                    {
                        AddAndTraceClaim(claims, new Claim(ClaimTypes.Role, RoleConstants.DATAHUB_ROLE_ADMIN_AS_GUEST));
                    }
                    else if (!alreadyAdded && project.Project_Acronym_CD == RoleConstants.DATAHUB_APPROVER_PROJECT)
                    {
                        AddAndTraceClaim(claims, new Claim(ClaimTypes.Role, RoleConstants.DATAHUB_APPROVER_ROLE));
                    }
                    else
                    {
                        foreach (var roleSuffix in RoleConstants.GetRoleSuffixes(role))
                        {
                            AddAndTraceClaim(claims, new Claim(ClaimTypes.Role, $"{project.Project_Acronym_CD}{roleSuffix}"));
                        }
                    }
                    if (project.WebAppEnabled == true)
                    {
                        AddAndTraceClaim(claims, new Claim(ClaimTypes.Role, $"{project.Project_Acronym_CD}{RoleConstants.WEBAPP_SUFFIX}"));
                    }
                }
            }


        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Cannot load project permissions");
            if (throwOnFailure)
            {
                throw;
            }
        }
        return principal!;
    }

    private static ClaimsPrincipal CloneWithoutGeneratedClaims(ClaimsPrincipal principal)
    {
        var identities = principal.Identities.Select(identity =>
        {
            var clone = new ClaimsIdentity(
                identity.Claims.Where(claim => claim.Issuer != GENERATED_CLAIM_ISSUER),
                identity.AuthenticationType,
                identity.NameClaimType,
                identity.RoleClaimType)
            {
                Label = identity.Label,
                BootstrapContext = identity.BootstrapContext
            };
            return clone;
        });

        return new ClaimsPrincipal(identities);
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
            //var idp = claims.Claims.FirstOrDefault(c => c.Type == IDP_QUALIFIER_CLAIM)?.Value;
            //if (idp?.EndsWith(IDP_GCCF) ?? false)
            //{
            AddAndTraceClaim(claims, new Claim(ClaimTypes.Role, RoleConstants.EXTERNAL_LOGIN));
            //}
            //else
            //    throw new SecurityException("Invalid IDP login");
            return false;
        }
        else
        {
            var trustedClaim = new Claim(ClaimTypes.Role, RoleConstants.TRUSTED_ENTRA_LOGIN);
            AddAndTraceClaim(claims, trustedClaim);
            return true;
        }
    }
}
