using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Datahub.Application.Authentication;

public class FakeAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity(
            new[]
        {
            new Claim(ClaimTypes.Name, "Offline User"),
        }, "Fake authentication type");

        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(user));
    }
}
