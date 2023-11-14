using System.Security.Claims;
using System.Text.Encodings.Web;
using Datahub.Core.Model.Datahub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Datahub.Stories.Utils;

public class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    
    public MockAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IDbContextFactory<DatahubProjectDBContext> contextFactory
    )
        : base(options, logger, encoder, clock)
    {
        _contextFactory = contextFactory;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // get total number of users
        var totalUsers = await context.PortalUsers.CountAsync();
        
        // get a random user
        var randomUser = await context.PortalUsers
            .Skip(new Random().Next(0, totalUsers))
            .FirstOrDefaultAsync();
        
        var claims = new []
        {
            new Claim(ClaimTypes.Name, randomUser.DisplayName),
            new Claim(ClaimTypes.Email, randomUser.Email),
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", randomUser.GraphGuid.ToString()),
        };
        
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return AuthenticateResult.Success(ticket);
    }
}