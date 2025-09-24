using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.UserTracking;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.UserManagement
{
    public class UserSettingsService(
        IUserInformationService userInformationService,
        IDbContextFactory<DatahubProjectDBContext> datahubContextFactory,
        ILogger<UserSettingsService> logger,
        NavigationManager navigationManager)
        : IUserSettingsService
    {
        /// <summary>
        /// Registers the current user's settings.
        /// </summary>
        /// <returns>True if new user setting record was created, false otherwise</returns>
        public async Task<bool> RegisterUserSettingsAsync()
        {
            await using var ctx = await datahubContextFactory.CreateDbContextAsync();
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();
            if (currentUser.UserSettings is null)
            {
                var userSettings = new UserSettings
                {
                    PortalUserId = currentUser.Id,
                    UserName = currentUser.DisplayName
                };
                ctx.UserSettings.Add(userSettings);
                await ctx.SaveChangesAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks that the user has accepted the Terms and Conditions.
        /// </summary>
        /// <returns>True if they have, false otherwise</returns>
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

        /// <summary>
        /// Registers that the user has accepted the Terms and Conditions.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise</returns>
        public async Task<bool> RegisterUserTAC()
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();
            logger.LogInformation("User: {CurrentUserDisplayName} has accepted Terms and Conditions",
                currentUser.DisplayName);

            try
            {
                await using var context = await datahubContextFactory.CreateDbContextAsync();
                var userSetting = await GetUserSettingsAsync();

                if (userSetting == null)
                {
                    logger.LogError(
                        "User: {CurrentUserDisplayName} with user id: {CurrentUserId} is not in DB to register TAC",
                        currentUser.DisplayName, currentUser.Id);
                    return false;
                }

                userSetting.AcceptedDate = DateTime.UtcNow;
                context.UserSettings.Update(userSetting);

                if (await context.SaveChangesAsync() > 0)
                    return true;

                logger.LogInformation(
                    "User: {CurrentUserDisplayName} has accepted Terms and Conditions. Changes NOT saved",
                    currentUser.DisplayName);
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User: {CurrentUserDisplayName} registering TAC failed", currentUser.DisplayName);
            }

            return false;
        }

        /// <summary>
        /// Deletes the user's settings.
        /// </summary>
        /// <returns>True if a record was deleted, false if no record was deleted</returns>
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

        /// <summary>
        /// Gets a list of keys of alerts that were hidden by the user.
        /// </summary>
        /// <returns>The list of keys of hidden alerts. Empty list if nothing found</returns>
        public async Task<List<string>> GetHiddenAlerts()
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();

            await using var context = await datahubContextFactory.CreateDbContextAsync();
            var userSetting = await GetUserSettingsAsync();
            if (userSetting == null)
            {
                logger.LogError(
                    "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB",
                    currentUser.DisplayName, currentUser.Id);
                return new List<string>();
            }

            if (userSetting.HiddenAlerts == null)
                return new List<string>();

            return userSetting.HiddenAlerts;
        }

        /// <summary>
        /// Adds an alert key to the list of hidden alerts for the user.
        /// </summary>
        /// <param name="alertKey">The key of the alert to hide</param>
        /// <returns>True if the alert was added, false otherwise</returns>
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

                userSetting.HiddenAlerts ??= new List<string>();

                userSetting.HiddenAlerts.Add(alertKey);
                context.UserSettings.Update(userSetting);

                if (await context.SaveChangesAsync() > 0)
                {
                    return true;
                }

                logger.LogInformation(
                    "User: {CurrentUserDisplayName} Alert {AlertKey} has not been added. Changes NOT saved",
                    currentUser.DisplayName, alertKey);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User: {CurrentUserDisplayName} Adding hidden alert has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        /// <summary>
        /// Gets the user's preference for hiding alerts.
        /// </summary>
        /// <returns>True if all alerts should be hidden, false otherwise</returns>
        public async Task<bool> GetHideAlerts()
        {
            var currentUser = await userInformationService.GetCurrentPortalUserAsync();

            await using var context = await datahubContextFactory.CreateDbContextAsync();
            var userSetting = await GetUserSettingsAsync();
            if (userSetting == null)
            {
                logger.LogError(
                    "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB",
                    currentUser.DisplayName, currentUser.Id);
                return false;
            }

            return userSetting.HideAlerts;
        }

        /// <summary>
        /// Sets the user's preference for hiding alerts.
        /// </summary>
        /// <param name="hideAlerts">Whether all alerts should be hidden</param>
        /// <returns>True if the setting was properly set, false otherwise</returns>
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
                    "User: {CurrentUserDisplayName} Hide alerts has not been set. Changes NOT saved",
                    currentUser.DisplayName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User: {CurrentUserDisplayName} Setting hide alerts has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        /// <summary>
        /// Sets the user's preference for hiding achievements.
        /// </summary>
        /// <param name="hideAchievements">Whether to hide achievements</param>
        /// <returns>True if the setting was set, false otherwise</returns>
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
                    "User: {CurrentUserDisplayName} Hide achievements has not been set. Changes NOT saved",
                    currentUser.DisplayName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User: {CurrentUserDisplayName} Setting hide achievements has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        /// <summary>
        /// Registers the user's selected language.
        /// </summary>
        /// <param name="language">The two letter language code, i.e. "en" or "fr"</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
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
                    await RegisterUserSettingsAsync();
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

        /// <summary>
        /// Sets the user's language preference.
        /// </summary>
        /// <param name="language">The two letter language code, i.e. "en" or "fr"</param>
        /// <param name="redirectUrl">The url to redirect to once the language is set</param>
        /// <returns>True if the language was changed, false otherwise</returns>
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

        /// <summary>
        /// Gets the user's selected language.
        /// </summary>
        /// <returns>The two letter language code, or empty string if no language found</returns>
        public async Task<string> GetUserLanguage()
        {
            var userSetting = await GetUserSettingsAsync();

            return userSetting != null ? userSetting.Language : string.Empty;
        }

        /// <summary>
        /// Checks if the user's selected language is French.
        /// </summary>
        /// <returns>True if french, false otherwise</returns>
        public async Task<bool> IsFrench()
        {
            try
            { 
                var lang = await GetUserLanguage();
                return !lang.ToLower().Contains("en");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to check if user is French");
                return false;
            }
        }

        /// <summary>
        /// Sets theme preference
        /// </summary>
        /// <param name="language"></param>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        public async Task<bool> SetTheme(string theme, string redirectUrl = "")
        {
            await using var context = await datahubContextFactory.CreateDbContextAsync();
            var userSetting = await GetUserSettingsAsync();

            if (userSetting != null)
            {
                userSetting.Theme = theme;
                context.UserSettings.Update(userSetting);
                await context.SaveChangesAsync();
            }

            var language = Thread.CurrentThread.CurrentCulture.Name;
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

        /// <summary>
        /// Gets the user's selected theme.
        /// </summary>
        /// <returns>Returns theme</returns>
        public async Task<string> GetTheme()
        {
            var userSetting = await GetUserSettingsAsync();

            return userSetting != null ? userSetting.Theme : "";
        }         

        /// <summary>
        /// Gets the user's settings. Not tracked.
        /// </summary>
        /// <returns>The UserSettings of the current user, null if none found if there was an error</returns>
        public async Task<UserSettings?> GetUserSettingsAsync()
        {
            try
            {
                var currentUser = await userInformationService.GetCurrentPortalUserAsync();
                if (currentUser == null)
                {
                    // this is legitimate , if the user is not logged in
                    return null;
                }
                await using var context = await datahubContextFactory.CreateDbContextAsync();
                var userSettings = await context.UserSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.PortalUserId == currentUser.Id);
                return userSettings;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to fetch current user at this time");
                return null;
            }
        }
    }
}