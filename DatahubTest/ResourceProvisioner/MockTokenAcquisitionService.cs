using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace Datahub.Tests.ResourceProvisioner;

public class MockTokenAcquisitionService : ITokenAcquisition
{
    private readonly IConfiguration _configuration;
    
    public MockTokenAcquisitionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public Task<string> GetAccessTokenForUserAsync(IEnumerable<string> scopes, string authenticationScheme, string tenantId = null,
        string userFlow = null, ClaimsPrincipal user = null, TokenAcquisitionOptions tokenAcquisitionOptions = null)
    {
        return Task.FromResult(_configuration["ResourceProvisionerApi:AccessToken"]);
    }

    public Task<AuthenticationResult> GetAuthenticationResultForUserAsync(IEnumerable<string> scopes, string authenticationScheme, string tenantId = null,
        string userFlow = null, ClaimsPrincipal user = null, TokenAcquisitionOptions tokenAcquisitionOptions = null)
    {
        return Task.FromResult<AuthenticationResult>(null);
    }

    public Task<string> GetAccessTokenForAppAsync(string scope, string authenticationScheme, string tenant = null,
        TokenAcquisitionOptions tokenAcquisitionOptions = null)
    {
        return Task.FromResult<string>(null);
    }

    public Task<AuthenticationResult> GetAuthenticationResultForAppAsync(string scope, string authenticationScheme, string tenant = null,
        TokenAcquisitionOptions tokenAcquisitionOptions = null)
    {
        return Task.FromResult<AuthenticationResult>(null);
    }

    public void ReplyForbiddenWithWwwAuthenticateHeader(IEnumerable<string> scopes, MsalUiRequiredException msalServiceException,
        string authenticationScheme, HttpResponse httpResponse = null)
    {
        //do nothing
    }

    public string GetEffectiveAuthenticationScheme(string authenticationScheme)
    {
        return string.Empty;
    }

    public Task ReplyForbiddenWithWwwAuthenticateHeaderAsync(IEnumerable<string> scopes, MsalUiRequiredException msalServiceException,
        HttpResponse httpResponse = null)
    {
        return Task.CompletedTask;
    }
}