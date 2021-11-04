using System;
using System.IO;
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
        //private GraphServiceClient _graphServiceClient;
        public string imageHtml;
        
        public User CurrentUser { get; set; }

        private User AnonymousUser => UserInformationServiceConstants.GetAnonymousUser();

        public UserInformationService(
            ILogger<UserInformationService> logger,
            AuthenticationStateProvider authenticationStateProvider,
            NavigationManager navigationManager,
            IConfiguration configureOptions,
            UserTrackingContext eFCoreDatahubContext,
            GraphServiceClient graphServiceClient,
            IDbContextFactory<UserTrackingContext> contextFactory
            )
        {
            _logger = logger;
            _authenticationStateProvider = authenticationStateProvider;
            _navigationManager = navigationManager;
            _configuration = configureOptions;
            
            this.graphServiceClient = graphServiceClient;
            this.contextFactory = contextFactory;
        }

        public string UserLanguage { get; set; }

        public async Task<string> GetUserIdString()
        {
            await CheckUser();
            return CurrentUser.Id;
        }

        public async Task<string> GetUserEmailDomain()
        {
            await CheckUser();
            MailAddress email = new MailAddress(CurrentUser.Mail);
            return email.Host.ToLower();
        }

        public async Task<string> GetUserEmailPrefix()
        {
            await CheckUser();
            MailAddress email = new MailAddress(CurrentUser.Mail);
            return email.User.ToLower();
        }

        public async Task<string> GetUserRootFolder()
        {
            var domain = await GetUserEmailDomain();
            var prefix = await GetUserEmailPrefix();
            return $"{domain}/{prefix}";
        }

        private async Task<User> GetUserAsyncInternal()
        {
            try
            {               
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                var claimsList = authState.User.Claims.ToList();
                var email = authState.User.Identity.Name;
                if (email is null)
                {
                    throw new InvalidOperationException("Cannot resolve user email");
                }

                PrepareAuthenticatedClient();
                CurrentUser = await graphServiceClient.Users[email].Request().GetAsync();

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

        public async Task<User> GetUserAsync()
        {
            await CheckUser();
            return CurrentUser;
        }

        //public async Task<string> GetMePhotoAsync()
        //{
        //    try
        //    {

        //        //var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        //        //var claimsList = authState.User.Claims.ToList();
        //        //var email = authState.User.Identity.Name;
        //        //PrepareAuthenticatedClient();
        //        //Stream photoresponse = await graphServiceClient.Users[email].Photo.Content.Request().GetAsync();

        //        //if (photoresponse != null)
        //        //{
        //        //    MemoryStream ms = new MemoryStream();
        //        //    photoresponse.CopyTo(ms);
        //        //    System.Drawing.Image i = System.Drawing.Image.FromStream(ms);

        //        //    byte[] imgBytes = turnImageToByteArray(i);
        //        //    string imgString = Convert.ToBase64String(imgBytes);
        //        //    return String.Format($@"<img src=""data:image/Bmp;base64,{imgString}"">");
        //        //}
        //        //else
        //        //{
        //        //    return string.Empty;
        //        //}

        //        byte[] imgBytes = await GetStreamWithAuthAsync();
        //        string imgString = Convert.ToBase64String(imgBytes);
        //        return String.Format($@"<img src=""data:image/Bmp;base64,{imgString}"">");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error getting signed-in user profilephoto: {ex.Message}");
        //        throw;
        //    }
        //}

        //private async Task<byte[]> GetStreamWithAuthAsync()
        //{
        //    //TODO figure out length of time of authtoken
        //    PrepareAuthenticatedClient();
        //    var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        //    var claimsList = authState.User.Claims.ToList();
        //    var email = authState.User.Identity.Name;
        //    var endpoint = @$"https://graph.microsoft.com/v1.0/users/0403528c-5abc-423f-9201-9c945f628595/photo/$value";
        //    var client = _httpClient.CreateClient();
        //    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
        //    client.DefaultRequestHeaders.Add("Accept", "application/json");
        //    using (var response = await client.GetAsync(endpoint))
        //    {
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var stream = await response.Content.ReadAsStreamAsync();
        //            byte[] bytes = new byte[stream.Length];
        //            stream.Read(bytes, 0, (int)stream.Length);
        //            return bytes;
        //        }
        //        else
        //            return null;
        //    }
        //}

        private byte[] turnImageToByteArray(System.Drawing.Image img)
        {
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            return ms.ToArray();
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
                using var eFCoreDatahubContext = contextFactory.CreateDbContext();
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
            using var eFCoreDatahubContext = contextFactory.CreateDbContext();
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
            using var eFCoreDatahubContext = contextFactory.CreateDbContext();
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
    }    
}
