using System.Security.Claims;
using System.Text.Encodings.Web;
using Datahub.Core.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace Datahub.Application.Authentication;

public class DevAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;
    public new const string Scheme = "DevAuth";
    public const string ActiveCookieName = "DevAuth.Active";

    public DevAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
   UrlEncoder encoder,
    IConfiguration configuration)
  : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var email = _configuration["GccfOidc:DevAuth:UserEmail"];
        if (string.IsNullOrWhiteSpace(email))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (!Request.Cookies.TryGetValue(ActiveCookieName, out var active) ||
            !string.Equals(active, bool.TrueString, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var name = _configuration["GccfOidc:DevAuth:UserName"] ?? email;

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email), new Claim(ClaimTypes.NameIdentifier, DevelopmentAuthStateProvider.DevUserObjectId),
            new Claim(ClaimConstants.ObjectId, DevelopmentAuthStateProvider.DevUserObjectId),
            new Claim(ClaimTypes.Role, RoleConstants.EXTERNAL_LOGIN),
            new Claim(RoleClaimTransformer.IDP_QUALIFIER_CLAIM, "https://te.clegc-gckey.gc.ca")
        }, Scheme);

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
