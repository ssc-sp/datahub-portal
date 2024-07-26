#nullable enable

using Datahub.Core.Model.UserTracking;

namespace Datahub.Application.Services.UserManagement
{
    public interface IUserSettingsService
    {
        Task<bool> HasUserAcceptedTAC();
        Task<bool> RegisterUserTAC();
        Task<bool> ClearUserSettingsAsync();
        Task<bool> GetHideAlerts();
        Task<bool> SetHideAlerts(bool hideAlerts);
        Task<List<string>> GetHiddenAlerts();
        Task<bool> AddHiddenAlert(string alertKey);
        Task<bool> SetHideAchievements(bool hideAchievements);
        Task<bool> RegisterUserLanguage(string language);
        Task<bool> SetLanguage(string language, string redirectUrl = "");
        Task<string> GetUserLanguage();
        Task<bool> IsFrench();
        Task<UserSettings?> GetUserSettingsAsync();
    }
}