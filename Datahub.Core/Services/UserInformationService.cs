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
using Datahub.Core.Data;

namespace Datahub.Core.Services
{
    public class UserInformationService : IUserInformationService
    {
        private ILogger<UserInformationService> _logger;
        private GraphServiceClient graphServiceClient;
        private readonly IDbContextFactory<UserTrackingContext> contextFactory;
        private AuthenticationStateProvider _authenticationStateProvider;
        private NavigationManager _navigationManager;

        private IConfiguration _configuration;
        private readonly ServiceAuthManager serviceAuthManager;

        //private GraphServiceClient _graphServiceClient;
        public string imageHtml;
        private ClaimsPrincipal authenticatedUser;

        public User CurrentUser { get; set; }

        private User AnonymousUser => UserInformationServiceConstants.GetAnonymousUser();

        public UserInformationService(
            ILogger<UserInformationService> logger,
            AuthenticationStateProvider authenticationStateProvider,
            NavigationManager navigationManager,
            IConfiguration configureOptions, ServiceAuthManager serviceAuthManager,
            GraphServiceClient graphServiceClient,
            IDbContextFactory<UserTrackingContext> contextFactory
        )
        {
            _logger = logger;
            _authenticationStateProvider = authenticationStateProvider;
            _navigationManager = navigationManager;
            _configuration = configureOptions;
            this.serviceAuthManager = serviceAuthManager;
            this.graphServiceClient = graphServiceClient;
            this.contextFactory = contextFactory;
        }

        public async Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false)
        {
            if ( _authenticationStateProvider == null || forceReload)
			    authenticatedUser = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
			return authenticatedUser;
        }

        public string UserLanguage { get; set; }

        public async Task<string> GetUserIdString()
        {
            await CheckUser();
            return GetOid();
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
                _logger.LogError(ex, "Cannot parse email from {CurrentUserMail}", CurrentUser?.Mail);
                return "?";
            }
        }

        public async Task<string> GetUserEmailPrefix()
        {
            await CheckUser();
            try
            {
                var email = new MailAddress(CurrentUser.Mail);
                return email.User.ToLower();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot parse email from {CurrentUserMail}", CurrentUser?.Mail);
                return "?";
            }
        }

        public async Task<string> GetUserRootFolder()
        {
            var domain = await GetUserEmailDomain();
            var prefix = await GetUserEmailPrefix();
            return $"{domain}/{prefix}";
        }

        public async Task<bool> IsUserWithoutInitiatives()
        {
            if (isViewingAsVisitor)
                return true;
            var claims = (await GetAuthenticatedUser()).Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

            return (claims.Count() == 0 || (claims.Count() == 1 && claims[0].Value == "default"));             
        }

        private async Task GetUserAsyncInternal()
        {
            if (CurrentUser != null) return;
            try
            {
                var email = (await GetAuthenticatedUser())?.Identity?.Name;
                var userId = GetOid();
                if (email is null)
                {
                    throw new InvalidOperationException("Cannot resolve user email");
                }

                PrepareAuthenticatedClient();
                CurrentUser = await graphServiceClient.Users[userId].Request().GetAsync();
            }
            catch (ServiceException e)
            {
                if (e.InnerException is MsalUiRequiredException ||
                    e.InnerException is MicrosoftIdentityWebChallengeUserException)
                    throw;
                //_logger.LogError(e, "Error Loading User");
                throw new InvalidOperationException("Cannot retrieve user", e);
            }
            catch (Exception e)
            {
                //_logger.LogError(e, "Error Loading User"); redundant
                throw new InvalidOperationException("Cannot retrieve user list", e);
            }
        }

        private string GetOid()
        {
            return (authenticatedUser?.Claims?
                .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?? throw new InvalidOperationException("Cannot access user claims")).Value;
        }

        public async Task<User> GetCurrentGraphUserAsync()
        {
            await CheckUser();
            return CurrentUser;
        }
        

        private void PrepareAuthenticatedClient()
        {
            //if (graphServiceClient != null) return;
            try
            {
                IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(_configuration.GetSection("AzureAd").GetValue<string>("ClientId"))
                    .WithTenantId(_configuration.GetSection("AzureAd").GetValue<string>("TenantId"))
                    .WithClientSecret(_configuration.GetSection("AzureAd").GetValue<string>("ClientSecret"))
                    .Build();
                ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
                graphServiceClient = new GraphServiceClient(authProvider);
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
                using var eFCoreDatahubContext = contextFactory.CreateDbContext();
                var userSetting = eFCoreDatahubContext.UserSettings.FirstOrDefault(u => u.UserId == userId);
                if (userSetting == null)
                {
                    _logger.LogError(
                        $"User: {CurrentUser.DisplayName} with user id: {userId} is not in DB to register TAC.");
                    return false;
                }

                userSetting.UserName = CurrentUser.DisplayName;
                userSetting.AcceptedDate = DateTime.UtcNow;

                if (await eFCoreDatahubContext.SaveChangesAsync() <= 0)
                {
                    _logger.LogInformation(
                        $"User: {CurrentUser.DisplayName} has accepted Terms and Conditions. Changes NOT saved");
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
            _logger.LogInformation(
                "User: {DisplayName} has selected language: {Language}", 
                CurrentUser.DisplayName, language);

            try
            {
                await using var eFCoreDatahubContext = await contextFactory.CreateDbContextAsync();
                var userSetting = eFCoreDatahubContext.UserSettings.FirstOrDefault(u => u.UserId == userId);
                if (userSetting == null)
                {
                    userSetting = new UserTracking.UserSettings {UserId = userId};
                    eFCoreDatahubContext.UserSettings.Add(userSetting);
                }

                userSetting.UserName = CurrentUser.DisplayName;
                userSetting.Language = language;
                if (await eFCoreDatahubContext.SaveChangesAsync() > 0) 
                    return true;
                
                _logger.LogInformation(
                    "User: {DisplayName} has selected language: {Language}. Changes NOT saved", 
                    CurrentUser.DisplayName, language);
                return false;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User: {DisplayName} registering language failed", CurrentUser.DisplayName);
            }

            return false;
        }

        public async Task<string> GetUserLanguage()
        {
            var userId = await GetUserIdString();
            await using var eFCoreDatahubContext = await contextFactory.CreateDbContextAsync();
            var userSetting = eFCoreDatahubContext.UserSettings.FirstOrDefault(u => u.UserId == userId);
            return userSetting != null ? userSetting.Language : string.Empty;
        }

        public bool SetLanguage(string language)
        {
            if (!Thread.CurrentThread.CurrentCulture.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(_navigationManager.Uri).GetComponents(UriComponents.PathAndQuery,
                    UriFormat.Unescaped);
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
            await using var eFCoreDatahubContext = await contextFactory.CreateDbContextAsync();
            var userSetting = eFCoreDatahubContext.UserSettings.FirstOrDefault(u => u.UserId == userId);

            return userSetting is {AcceptedDate: { }};
        }

        private async Task CheckUser()
        {
            if (CurrentUser == null)
            {
                await GetUserAsyncInternal();
            }
        }

        public Task<User> GetAnonymousGraphUserAsync()
        {
            return Task.FromResult(AnonymousUser);
        }

        public async Task<User> GetGraphUserAsync(string userId)
        {
            try
            {
                PrepareAuthenticatedClient();
                CurrentUser = await graphServiceClient.Users[userId].Request().GetAsync();

                return CurrentUser;
            }
            catch (ServiceException e)
            {
                if (e.InnerException is MsalUiRequiredException ||
                    e.InnerException is MicrosoftIdentityWebChallengeUserException)
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

        public async Task<bool> IsViewingAsGuest()
        {
            return serviceAuthManager.GetViewingAsGuest((await GetCurrentGraphUserAsync()).Id);
        }

        public async Task SetViewingAsGuest(bool isGuest)
        {
            serviceAuthManager.SetViewingAsGuest((await GetCurrentGraphUserAsync()).Id,isGuest);
        }

        private bool isViewingAsVisitor = false;

        public Task<bool> IsViewingAsVisitor()
        {
            return Task.FromResult(isViewingAsVisitor);
        }

        public Task SetViewingAsVisitor(bool isVisitor)
        {
            isViewingAsVisitor = isVisitor;
            return Task.CompletedTask;
        }

        public async Task<bool> IsUserProjectAdmin(string projectAcronym)
        {            
            return (await GetAuthenticatedUser()).IsInRole($"{projectAcronym}-admin");
        }

        public async Task<bool> IsUserDatahubAdmin()
        {
            return (await GetAuthenticatedUser()).IsInRole(RoleConstants.DATAHUB_ROLE_ADMIN);
        }

        //IsDataHubAdmin = ;
    }
}