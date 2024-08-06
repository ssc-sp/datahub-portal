#nullable enable
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.UserTracking;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.UserManagement
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IUserInformationService userInformationService;
        private readonly IDbContextFactory<DatahubProjectDBContext> datahubContextFactory;
        private readonly ILogger<UserSettingsService> logger;
        private readonly NavigationManager navigationManager;

        public UserSettingsService(
            IUserInformationService userInformationService,
            IDbContextFactory<DatahubProjectDBContext> datahubContextFactory, ILogger<UserSettingsService> logger, NavigationManager navigationManager)
        {
            this.userInformationService = userInformationService;
            this.datahubContextFactory = datahubContextFactory;
            this.logger = logger;
            this.navigationManager = navigationManager;
        }

        public async Task<bool> HasUserAcceptedTAC()
        {
            await using var context = await datahubContextFactory.CreateDbContextAsync();
            var userSetting = await GetUserSettingsAsync();
            if (userSetting != null)
            {
                return userSetting.AcceptedDate != null;
            }
            return false;
        }

        public async Task<bool> RegisterUserTAC()
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();
            logger.LogInformation($"User: {currentUser.DisplayName} has accepted Terms and Conditions.");

            try
            {
                await using var context = await datahubContextFactory.CreateDbContextAsync();
                var userSetting = await GetUserSettingsAsync();

                if (userSetting == null)
                {
                    logger.LogError(
                        $"User: {currentUser.DisplayName} with user id: {currentUser.Id} is not in DB to register TAC.");
                    return false;
                }

                userSetting.AcceptedDate = DateTime.UtcNow;
                context.UserSettings.Update(userSetting);

                if (await context.SaveChangesAsync() > 0)
                    return true;

                logger.LogInformation(
                    $"User: {currentUser.DisplayName} has accepted Terms and Conditions. Changes NOT saved");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"User: {currentUser.DisplayName} registering TAC failed.");
            }

            return false;
        }

        public async Task<bool> ClearUserSettingsAsync()
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();

            try
            {
                await using var context = await datahubContextFactory.CreateDbContextAsync();
                var userSetting = await GetUserSettingsAsync();
                if (userSetting == null)
                {
                    logger.LogError(
                        "User: {CurrentUserDisplayName} with user id: {PortalUserId} is not in DB to clear settings",
                        currentUser.DisplayName, currentUser.Id);
                    return false;
                }

                context.UserSettings.Remove(userSetting);

                if (await context.SaveChangesAsync() > 0)
                {
                    return true;
                }

                logger.LogInformation(
                    "User: {CurrentUserDisplayName} has not cleared their settings. Changes NOT saved",
                    currentUser.DisplayName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User: {CurrentUserDisplayName} clearing settings has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        public async Task<List<string>> GetHiddenAlerts()
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();

            await using var context = await datahubContextFactory.CreateDbContextAsync();
            var userSetting = await GetUserSettingsAsync();
            if (userSetting == null)
            {
                logger.LogError(
                    "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB.",
                    currentUser.DisplayName, currentUser.Id);
                return new List<string>();
            }

            if (userSetting.HiddenAlerts == null)
                return new List<string>();

            return userSetting.HiddenAlerts;
        }

        public async Task<bool> AddHiddenAlert(string alertKey)
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();

            try
            {
                await using var context = await datahubContextFactory.CreateDbContextAsync();
                var userSetting = await GetUserSettingsAsync();

                if (userSetting == null)
                {
                    logger.LogError(
                        "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB to add hidden alert",
                        currentUser.DisplayName, currentUser.Id);
                    return false;
                }

                if (userSetting.HiddenAlerts == null)
                    userSetting.HiddenAlerts = new List<string>();

                userSetting.HiddenAlerts.Add(alertKey);
                context.UserSettings.Update(userSetting);

                if (await context.SaveChangesAsync() > 0)
                {
                    return true;
                }

                logger.LogInformation(
                    "User: {CurrentUserDisplayName} Alert {AlertKey} has not been added. Changes NOT saved.",
                    currentUser.DisplayName, alertKey);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User: {CurrentUserDisplayName} Adding hidden alert has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        public async Task<bool> GetHideAlerts()
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();

            await using var context = await datahubContextFactory.CreateDbContextAsync();
            var userSetting = await GetUserSettingsAsync();
            if (userSetting == null)
            {
                logger.LogError(
                    "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB.",
                    currentUser.DisplayName, currentUser.Id);
                return false;
            }

            return userSetting.HideAlerts;
        }

        public async Task<bool> SetHideAlerts(bool hideAlerts)
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();

            try
            {
                await using var context = await datahubContextFactory.CreateDbContextAsync();
                var userSetting = await GetUserSettingsAsync();

                if (userSetting == null)
                {
                    logger.LogError(
                        "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB to set hide alerts",
                        currentUser.DisplayName, currentUser.Id);
                    return false;
                }

                userSetting.HideAlerts = hideAlerts;
                context.UserSettings.Update(userSetting);

                if (await context.SaveChangesAsync() > 0)
                {
                    return true;
                }

                logger.LogInformation(
                    "User: {CurrentUserDisplayName} Hide alerts has not been set. Changes NOT saved.",
                    currentUser.DisplayName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User: {CurrentUserDisplayName} Setting hide alerts has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        public async Task<bool> SetHideAchievements(bool hideAchievements)
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();

            try
            {
                await using var context = await datahubContextFactory.CreateDbContextAsync();
                var userSetting = await GetUserSettingsAsync();

                if (userSetting == null)
                {
                    logger.LogError(
                        "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB to set hide achievements",
                        currentUser.DisplayName, currentUser.Id);
                    return false;
                }

                userSetting.HideAchievements = hideAchievements;
                context.UserSettings.Update(userSetting);

                if (await context.SaveChangesAsync() > 0)
                {
                    return true;
                }

                logger.LogInformation(
                    "User: {CurrentUserDisplayName} Hide achievements has not been set. Changes NOT saved.",
                    currentUser.DisplayName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User: {CurrentUserDisplayName} Setting hide achievements has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        public async Task<bool> RegisterUserLanguage(string language)
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();

            logger.LogInformation(
                "User: {DisplayName} has selected language: {Language}",
                currentUser.DisplayName, language);

            try
            {
                await using var context = await datahubContextFactory.CreateDbContextAsync();
                var userSetting = await GetUserSettingsAsync();

                if (userSetting == null)
                {
                    userSetting = new UserSettings { PortalUserId = currentUser.Id, UserName = currentUser.DisplayName, Language = language };
                    context.UserSettings.Add(userSetting);
                }
                else
                {
                    userSetting.Language = language;
                    context.UserSettings.Update(userSetting);
                }

                if (await context.SaveChangesAsync() > 0)
                    return true;

                logger.LogInformation(
                    "User: {DisplayName} has selected language: {Language}. Changes NOT saved",
                    currentUser.DisplayName, language);
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User: {DisplayName} registering language failed", currentUser.DisplayName);
            }

            return false;
        }

        public async Task<bool> SetLanguage(string language, string redirectUrl = "")
        {
            await using var context = await datahubContextFactory.CreateDbContextAsync();
            var userSetting = await GetUserSettingsAsync();

            if (userSetting != null)
            {
                userSetting.Language = language;
                context.UserSettings.Update(userSetting);
                await context.SaveChangesAsync();
            }

            if (Thread.CurrentThread.CurrentCulture.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                return false;

            var uri = new Uri(navigationManager.Uri).GetComponents(
                UriComponents.PathAndQuery,
                UriFormat.Unescaped);

            if (redirectUrl != string.Empty)
                uri = redirectUrl;

            var query = $"?culture={Uri.EscapeDataString(language)}&" +
                        $"redirectionUri={Uri.EscapeDataString(uri)}";
            navigationManager.NavigateTo($"/Culture/SetCulture{query}", forceLoad: true);

            return true;
        }

        public async Task<string> GetUserLanguage()
        {
            var userSetting = await GetUserSettingsAsync();

            return userSetting != null ? userSetting.Language : string.Empty;
        }

        public async Task<bool> IsFrench()
        {
            var lang = await GetUserLanguage();
            return !lang.ToLower().Contains("en");
        }

        public async Task<UserSettings?> GetUserSettingsAsync()
        {
            try
            {
                var currentUser = await userInformationService.GetCurrentPortalUserAsync();
                await using var context = await datahubContextFactory.CreateDbContextAsync();
                var userSettings = await context.UserSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.PortalUserId == currentUser.Id);
                return userSettings;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to fetch current user at this time.");
                return null;
            }
        }
    }
}