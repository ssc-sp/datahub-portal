using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Datahub.Core.UserTracking;
using System.Security.Claims;

namespace Datahub.Core.Services
{
    public class UserInformationService : IUserInformationService
    {
        private readonly ILogger<UserInformationService> _logger;
        private readonly IDbContextFactory<UserTrackingContext> _contextFactory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IConfiguration _configuration;
        private readonly NavigationManager _navigationManager;
        private readonly IKeyVaultService _keyVaultService;

        private ClaimsPrincipal _authenticatedUser;
        private GraphServiceClient _graphServiceClient;
        private string _clientSecret = null;

        public User CurrentUser { get; set; }

        private User AnonymousUser => UserInformationServiceConstants.GetAnonymousUser();

        public UserInformationService(
            ILogger<UserInformationService> logger,
            AuthenticationStateProvider authenticationStateProvider,
            NavigationManager navigationManager,
            IConfiguration configureOptions,
            UserTrackingContext eFCoreDatahubContext,
            GraphServiceClient graphServiceClient,
            IKeyVaultService keyVaultService,
            IDbContextFactory<UserTrackingContext> contextFactory)
        {
            _logger = logger;
            _authenticationStateProvider = authenticationStateProvider;
            _navigationManager = navigationManager;
            _configuration = configureOptions;
            _graphServiceClient = graphServiceClient;
            _keyVaultService = keyVaultService;
            _contextFactory = contextFactory;
        }

        public string UserLanguage { get; set; }

        public async Task<string> GetUserIdString()
        {
            await CheckUser();
            return getOID();
        }

        public async Task<string> GetUserEmailDomain()
        {
            await CheckUser();
            try
            {
                MailAddress email = new MailAddress(CurrentUser.Mail);
                return email.Host.ToLower();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot parse email from {CurrentUser?.Mail}");
                return "?";
            }
        }

        public async Task<string> GetUserEmailPrefix()
        {
            await CheckUser();
            try
            {
                MailAddress email = new MailAddress(CurrentUser.Mail);
                return email.User.ToLower();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot parse email from {CurrentUser?.Mail}");
                return "?";
            }

        }

        public async Task<string> GetUserRootFolder()
        {
            var domain = await GetUserEmailDomain();
            var prefix = await GetUserEmailPrefix();
            return $"{domain}/{prefix}";
        }

        private async Task<User> GetUserAsyncInternal()
        {
            if (CurrentUser != null)
                return CurrentUser;
            try
            {
                if (_authenticatedUser is null)
                    _authenticatedUser = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
                var claimsList = _authenticatedUser.Claims.ToList();
                var email = _authenticatedUser.Identity.Name;
                string userId = getOID();
                if (email is null)
                {
                    throw new InvalidOperationException("Cannot resolve user email");
                }

                await PrepareAuthenticatedClient();
                CurrentUser = await _graphServiceClient.Users[userId].Request().GetAsync();

                return CurrentUser;
            }
            catch (ServiceException e)
            {
                if (e.InnerException is MsalUiRequiredException || e.InnerException is MicrosoftIdentityWebChallengeUserException)
                    throw;
                _logger.LogError(e, "Error Loading User");
                throw new InvalidOperationException("Cannot retrieve user", e);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Loading User");
                throw new InvalidOperationException("Cannot retrieve user list", e);

            }
        }

        private string getOID()
        {
            return _authenticatedUser.Claims.First(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
        }

        public async Task<User> GetUserAsync()
        {
            await CheckUser();
            return CurrentUser;
        }

        private async Task PrepareAuthenticatedClient()
        {
            //if (graphServiceClient != null) return;
            try
            {
                _clientSecret ??= await _keyVaultService.GetClientSecret();
                IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(_configuration.GetSection("AzureAd").GetValue<string>("ClientId"))
                    .WithTenantId(_configuration.GetSection("AzureAd").GetValue<string>("TenantId"))
                    .WithClientSecret(_clientSecret)
                    .Build();
                ClientCredentialProvider authProvider = new(confidentialClientApplication);
                _graphServiceClient = new GraphServiceClient(authProvider);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error preparing authentication client: {e.Message}");
                Console.WriteLine($"Error preparing authentication client: {e.Message}");
                throw;
            }
        }

        public async Task<bool> RegisterUserTAC()
        {
            var userId = await GetUserIdString();
            _logger.LogInformation($"User: {CurrentUser.DisplayName} has accepted Terms and Conditions.");

            try
            {
                using var eFCoreDatahubContext = _contextFactory.CreateDbContext();
                var userSetting = eFCoreDatahubContext.UserSettings.FirstOrDefault(u => u.UserId == userId);
                if (userSetting == null)
                {
                    _logger.LogError($"User: {CurrentUser.DisplayName} with user id: {userId} is not in DB to register TAC.");
                    return false;
                }

                userSetting.UserName = CurrentUser.DisplayName;
                userSetting.AcceptedDate = DateTime.UtcNow;

                if (await eFCoreDatahubContext.SaveChangesAsync() <= 0)
                {
                    _logger.LogInformation($"User: {CurrentUser.DisplayName} has accepted Terms and Conditions. Changes NOT saved");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"User: {CurrentUser.DisplayName} registering TAC failed.");
            }

            return false;
        }

        public async Task<bool> RegisterUserLanguage(string language)
        {
            var userId = await GetUserIdString();
            _logger.LogInformation($"User: {CurrentUser.DisplayName} has selected language: {language}.");

            try
            {
                using var eFCoreDatahubContext = _contextFactory.CreateDbContext();
                var userSetting = eFCoreDatahubContext.UserSettings.FirstOrDefault(u => u.UserId == userId);
                if (userSetting == null)
                {
                    userSetting = new UserTracking.UserSettings() { UserId = userId };
                    eFCoreDatahubContext.UserSettings.Add(userSetting);
                }

                userSetting.UserName = CurrentUser.DisplayName;
                userSetting.Language = language;
                if (await eFCoreDatahubContext.SaveChangesAsync() <= 0)
                {
                    _logger.LogInformation($"User: {CurrentUser.DisplayName} has selected language: {language}. Changes NOT saved");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"User: {CurrentUser.DisplayName} registering TAC failed.");
            }

            return false;
        }

        public async Task<string> GetUserLanguage()
        {
            var userId = await GetUserIdString();
            using var eFCoreDatahubContext = _contextFactory.CreateDbContext();
            var userSetting = eFCoreDatahubContext.UserSettings.FirstOrDefault(u => u.UserId == userId);
            return userSetting != null ? userSetting.Language : string.Empty;
        }

        public bool SetLanguage(string language)
        {
            if (!Thread.CurrentThread.CurrentCulture.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(_navigationManager.Uri).GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
                var query = $"?culture={Uri.EscapeDataString(language)}&" +
                            $"redirectionUri={Uri.EscapeDataString(uri)}";
                _navigationManager.NavigateTo($"/Culture/SetCulture{query}", forceLoad: true);

                //                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
                return true;
            }

            return false;
        }

        public async Task<bool> IsFrench()
        {
            var lang = await GetUserLanguage();
            return !lang.ToLower().Contains("en");
        }

        public async Task<bool> HasUserAcceptedTAC()
        {
            var userId = await GetUserIdString();
            using var eFCoreDatahubContext = _contextFactory.CreateDbContext();
            var userSetting = eFCoreDatahubContext.UserSettings.FirstOrDefault(u => u.UserId == userId);

            return userSetting != null ? userSetting.AcceptedDate.HasValue : false;
        }

        private async Task CheckUser()
        {
            if (CurrentUser == null)
            {
                await GetUserAsyncInternal();
            }
        }

        public Task<User> GetAnonymousUserAsync()
        {
            return Task.FromResult(AnonymousUser);
        }

        public async Task<User> GetUserAsync(string userId)
        {
            try
            {
                await PrepareAuthenticatedClient();
                CurrentUser = await _graphServiceClient.Users[userId].Request().GetAsync();

                return CurrentUser;
            }
            catch (ServiceException e)
            {
                if (e.InnerException is MsalUiRequiredException || e.InnerException is MicrosoftIdentityWebChallengeUserException)
                    throw;
                _logger.LogError(e, "Error Loading User");
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Loading User");
                return null;
            }
        }
    }
}
