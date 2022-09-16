using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;

namespace Datahub.Achievements.Test;

public static class Utils
{
    public static Mock<AuthenticationStateProvider> CreateMockAuth(string userId)
    {
        var mockAuth = new Mock<AuthenticationStateProvider>();

        var identity = new GenericIdentity(userId, "");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        identity.AddClaim(new Claim(ClaimTypes.Name, userId));

        var principal = new GenericPrincipal(identity, roles: Array.Empty<string>());
        var user = new ClaimsPrincipal(principal);
        var authState = new AuthenticationState(user);

        mockAuth.Setup(s => s.GetAuthenticationStateAsync())
            .ReturnsAsync(() => authState);
        return mockAuth;
    }
}