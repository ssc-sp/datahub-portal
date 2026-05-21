using Azure.Core;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Datahub.Infrastructure.Services.Security
{

    public class TokenCredentialAdapter : ServiceClientCredentials
    {
        private readonly TokenCredential _tokenCredential;
        private readonly string[] _scopes;

        public TokenCredentialAdapter(TokenCredential tokenCredential, string[] scopes)
        {
            _tokenCredential = tokenCredential;
            _scopes = scopes;
        }

        public override async Task ProcessHttpRequestAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _tokenCredential.GetTokenAsync(
                new TokenRequestContext(_scopes),
                cancellationToken);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Token);

            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

}
