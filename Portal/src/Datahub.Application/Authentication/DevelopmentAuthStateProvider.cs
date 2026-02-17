using System.Security.Claims;
using Datahub.Core.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;

namespace Datahub.Application.Authentication
{
    public class DevelopmentAuthStateProvider : AuthenticationStateProvider
    {
        // Shared fixed GUID so dev identity and DB seeding can align
        public const string DevUserObjectId = "00000000-0000-0000-0000-00000000D3F1";

        private readonly IConfiguration _configuration;

        public DevelopmentAuthStateProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // GCCF is always treated as an external user
            var identity = CreateGccfUser();

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }

        private ClaimsIdentity CreateGccfUser()
        {
            var email = _configuration["GccfOidc:DevAuth:UserEmail"] ?? "dev.user@example.com";
            var name = _configuration["GccfOidc:DevAuth:UserName"] ?? email;

            var identity = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, DevUserObjectId),
            new Claim(ClaimConstants.ObjectId, DevUserObjectId),
            new Claim(ClaimTypes.Role, RoleConstants.EXTERNAL_LOGIN),
        }, "Development");

            identity.AddClaim(new Claim(
                RoleClaimTransformer.IDP_QUALIFIER_CLAIM,
                "https://te.clegc-gckey.gc.ca"
            ));

            return identity;
        }
    }
}
