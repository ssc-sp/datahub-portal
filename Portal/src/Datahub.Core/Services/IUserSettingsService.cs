using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datahub.Core.Model.UserTracking;
#nullable enable

namespace Datahub.Core.Services
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
        Task<bool> SetLanguage(string language);
        Task<string> GetUserLanguage();
        Task<bool> IsFrench();
        Task<UserSettings?> GetUserSettingsAsync();

    }
}