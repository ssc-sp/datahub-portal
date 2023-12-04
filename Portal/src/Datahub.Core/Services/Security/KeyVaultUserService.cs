using Microsoft.Azure.KeyVault;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Security
{
    public class KeyVaultUserService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IUserInformationService _userInfoService;
        private KeyVaultClient _keyVaultClient;

        public KeyVaultUserService(ITokenAcquisition tokenAcquisition, IUserInformationService userInfoService)
        {
            _tokenAcquisition = tokenAcquisition;
            _userInfoService = userInfoService;
        }

        private async Task<string> GetAccessToken(string auth, string res, string scope)
        {
            var scopes = new string[] { "https://vault.azure.net/user_impersonation" };
            var user = await _userInfoService.GetAuthenticatedUser();
            var result = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes, user: user);

            return await Task.FromResult(result);
        }

        public void GetKeyVaultClient(string kvName)
        {
            _keyVaultClient = new KeyVautdltClient(new KeyVaultClient.AuthenticationCallback(GetAccessToken));
            _keyVaultClient.GetSecretAsync(kvName, "datahubportal-client-secret").GetAwaiter().GetResult();
        }
    }
}
