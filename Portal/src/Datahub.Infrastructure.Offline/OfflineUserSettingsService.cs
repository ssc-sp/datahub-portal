#nullable enable
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.UserTracking;

namespace Datahub.Core.Services.Offline
{
    public class OfflineUserSettingsService : IUserSettingsService
    {
        public Task<bool> HasUserAcceptedTAC()
        {
            return Task.FromResult(true);
        }

        public Task<bool> RegisterUserTAC()
        {
            return Task.FromResult(true);
        }

        public Task<bool> ClearUserSettingsAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> GetHideAlerts()
        {
            return Task.FromResult(false);
        }

        public Task<bool> SetHideAlerts(bool hideAlerts)
        {
            return Task.FromResult(true);
        }

        public Task<List<string>> GetHiddenAlerts()
        {
            return Task.FromResult(new List<string>());
        }

        public Task<bool> AddHiddenAlert(string alertKey)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SetHideAchievements(bool hideAchievements)
        {
            return Task.FromResult(true);
        }

        public Task<bool> RegisterUserLanguage(string language)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SetLanguage(string language, string redirectUrl = "")
        {
            return Task.FromResult(true);
        }

        public Task<string> GetUserLanguage()
        {
            return Task.FromResult("en");
        }

        public Task<bool> IsFrench()
        {
            return Task.FromResult(false);
        }

        public Task<UserSettings?> GetUserSettingsAsync()
        {
            return Task.FromResult<UserSettings?>(null);
        }
    }
}