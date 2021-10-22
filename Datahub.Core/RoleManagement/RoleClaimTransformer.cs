using Microsoft.AspNetCore.Authentication;
using Datahub.Portal.Services;
using Datahub.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Datahub.Core.RoleManagement
{

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
                var userName = principal.Identity.Name;
                var userId = principal.Claims.ToList()[9].Value;

                var allProjects = serviceAuthManager.GetAllProjects();

                var authorizedProjects = await serviceAuthManager.GetUserAuthorizations(userId);

                foreach (var project in allProjects)
                {
                    if (await serviceAuthManager.IsProjectAdmin(userId, project))
                    {
                        ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(ClaimTypes.Role, $"{project}-admin"));
                    }
                }
                foreach (var project in authorizedProjects)
                {
                    ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(ClaimTypes.Role, project.Project_Acronym_CD));
                }

            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Cannot load project permissions");
            }
            return principal;

        }

    }
}
