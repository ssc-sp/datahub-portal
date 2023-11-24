using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.UserTracking;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Core.Services.UserManagement
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IUserInformationService _userInformationService;
        private readonly IDbContextFactory<DatahubProjectDBContext> _datahubContextFactory;
        private readonly ILogger<UserSettingsService> _logger;
        private readonly NavigationManager _navigationManager;

        public UserSettingsService(IUserInformationService userInformationService,
            IDbContextFactory<DatahubProjectDBContext> datahubContextFactory, ILogger<UserSettingsService> logger, NavigationManager navigationManager)
        {
            _userInformationService = userInformationService;
            _datahubContextFactory = datahubContextFactory;
            _logger = logger;
            _navigationManager = navigationManager;
        }

        public async Task<bool> HasUserAcceptedTAC()
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
            await using var context = await _datahubContextFactory.CreateDbContextAsync();
            var userSetting = context.UserSettings.FirstOrDefault(u => u.UserId == currentUser.Id);


            return userSetting is { AcceptedDate: { } };
        }

        public async Task<bool> RegisterUserTAC()
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
            _logger.LogInformation($"User: {currentUser.DisplayName} has accepted Terms and Conditions.");

            try
            {
                await using var context = await _datahubContextFactory.CreateDbContextAsync();
                var userSetting = context.UserSettings.FirstOrDefault(u => u.UserId == currentUser.Id);

                if (userSetting == null)
                {
                    _logger.LogError(
                        $"User: {currentUser.DisplayName} with user id: {currentUser.Id} is not in DB to register TAC.");
                    return false;
                }

                userSetting.UserName = currentUser.DisplayName;
                userSetting.AcceptedDate = DateTime.UtcNow;

                if (await context.SaveChangesAsync() > 0)
                    return true;

                _logger.LogInformation(
                    $"User: {currentUser.DisplayName} has accepted Terms and Conditions. Changes NOT saved");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"User: {currentUser.DisplayName} registering TAC failed.");
            }

            return false;
        }

        public async Task<bool> ClearUserSettingsAsync()
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

            try
            {
                await using var context = await _datahubContextFactory.CreateDbContextAsync();
                var userSetting = context.UserSettings.FirstOrDefault(u => u.UserId == currentUser.Id);
                if (userSetting == null)
                {
                    _logger.LogError(
                        "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB to clear settings",
                        currentUser.DisplayName, currentUser.Id);
                    return false;
                }

                context.UserSettings.Remove(userSetting);

                if (await context.SaveChangesAsync() > 0)
                {
                    return true;
                }

                _logger.LogInformation(
                    "User: {CurrentUserDisplayName} has not cleared their settings. Changes NOT saved",
                    currentUser.DisplayName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User: {CurrentUserDisplayName} clearing settings has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        public async Task<List<string>> GetHiddenAlerts()
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

            await using var context = await _datahubContextFactory.CreateDbContextAsync();
            var userSetting = context.UserSettings.AsNoTracking().FirstOrDefault(u => u.UserId == currentUser.Id);
            if (userSetting == null)
            {
                _logger.LogError(
                    "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB.",
                    currentUser.DisplayName, currentUser.Id);
                return new List<string>();
            }

            return userSetting.HiddenAlerts;
        }

        public async Task<bool> AddHiddenAlert(string alertKey)
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

            try
            {
                await using var context = await _datahubContextFactory.CreateDbContextAsync();
                var userSetting = context.UserSettings.FirstOrDefault(u => u.UserId == currentUser.Id);

                if (userSetting == null)
                {
                    _logger.LogError(
                        "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB to add hidden alert",
                        currentUser.DisplayName, currentUser.Id);
                    return false;
                }

                userSetting.HiddenAlerts.Add(alertKey);

                if (await context.SaveChangesAsync() > 0)
                {
                    return true;
                }

                _logger.LogInformation(
                    "User: {CurrentUserDisplayName} Alert {AlertKey} has not been added. Changes NOT saved.",
                    currentUser.DisplayName, alertKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User: {CurrentUserDisplayName} Adding hidden alert has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        public async Task<bool> GetHideAlerts()
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

            await using var context = await _datahubContextFactory.CreateDbContextAsync();
            var userSetting = context.UserSettings.AsNoTracking().FirstOrDefault(u => u.UserId == currentUser.Id);
            if (userSetting == null)
            {
                _logger.LogError(
                    "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB.",
                    currentUser.DisplayName, currentUser.Id);
                return false;
            }

            return userSetting.HideAlerts;
        }

        public async Task<bool> SetHideAlerts(bool hideAlerts)
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

            try
            {
                await using var context = await _datahubContextFactory.CreateDbContextAsync();
                var userSetting = context.UserSettings.FirstOrDefault(u => u.UserId == currentUser.Id);

                if (userSetting == null)
                {
                    _logger.LogError(
                        "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB to set hide alerts",
                        currentUser.DisplayName, currentUser.Id);
                    return false;
                }

                userSetting.HideAlerts = hideAlerts;

                if (await context.SaveChangesAsync() > 0)
                {
                    return true;
                }

                _logger.LogInformation(
                    "User: {CurrentUserDisplayName} Hide alerts has not been set. Changes NOT saved.",
                    currentUser.DisplayName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User: {CurrentUserDisplayName} Setting hide alerts has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        public async Task<bool> SetHideAchievements(bool hideAchievements)
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

            try
            {
                await using var context = await _datahubContextFactory.CreateDbContextAsync();
                var userSetting = context.UserSettings.FirstOrDefault(u => u.UserId == currentUser.Id);

                if (userSetting == null)
                {
                    _logger.LogError(
                        "User: {CurrentUserDisplayName} with user id: {UserId} is not in DB to set hide achievements",
                        currentUser.DisplayName, currentUser.Id);
                    return false;
                }

                userSetting.HideAchievements = hideAchievements;

                if (await context.SaveChangesAsync() > 0)
                {
                    return true;
                }

                _logger.LogInformation(
                    "User: {CurrentUserDisplayName} Hide achievements has not been set. Changes NOT saved.",
                    currentUser.DisplayName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User: {CurrentUserDisplayName} Setting hide achievements has failed",
                    currentUser.DisplayName);
            }

            return false;
        }

        public async Task<bool> RegisterUserLanguage(string language)
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

            _logger.LogInformation(
                "User: {DisplayName} has selected language: {Language}",
                currentUser.DisplayName, language);

            try
            {
                await using var context = await _datahubContextFactory.CreateDbContextAsync();
                var userSetting = context.UserSettings.FirstOrDefault(u => u.UserId == currentUser.Id);

                if (userSetting == null)
                {
                    userSetting = new UserSettings { UserId = currentUser.Id };
                    context.UserSettings.Add(userSetting);
                }

                userSetting.UserName = currentUser.DisplayName;
                userSetting.Language = language;
                if (await context.SaveChangesAsync() > 0)
                    return true;

                _logger.LogInformation(
                    "User: {DisplayName} has selected language: {Language}. Changes NOT saved",
                    currentUser.DisplayName, language);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User: {DisplayName} registering language failed", currentUser.DisplayName);
            }

            return false;
        }

        public async Task<bool> SetLanguage(string language)
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
            
            if (language == null ||
                Thread.CurrentThread.CurrentCulture.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                return false;
        
            await using var context = await _datahubContextFactory.CreateDbContextAsync();
            var userSetting = context.UserSettings.FirstOrDefault(u => u.UserId == currentUser.Id);

            if (userSetting != null)
            {
                userSetting.Language = language;
                await context.SaveChangesAsync();
            }

            var uri = new Uri(_navigationManager.Uri).GetComponents(UriComponents.PathAndQuery,
                UriFormat.Unescaped);
            var query = $"?culture={Uri.EscapeDataString(language)}&" +
                        $"redirectionUri={Uri.EscapeDataString(uri)}";
            _navigationManager.NavigateTo($"/Culture/SetCulture{query}", forceLoad: true);

            //                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            return true;
        }

        public async Task<string> GetUserLanguage()
        {
            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
            await using var context = await _datahubContextFactory.CreateDbContextAsync();
            var userSetting = context.UserSettings.FirstOrDefault(u => u.UserId == currentUser.Id);

            return userSetting != null ? userSetting.Language : string.Empty;
        }

        public async Task<bool> IsFrench()
        {
            var lang = await GetUserLanguage();
            return !lang.ToLower().Contains("en");
        }
    }
}