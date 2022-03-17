using Datahub.Core.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Datahub.Core.Services
{
    public class SystemNotificationService : ISystemNotificationService, IDisposable
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly IStringLocalizer<SystemNotificationService> _localizer;
        private readonly ILogger<SystemNotificationService> _logger;
        private readonly IPropagationService _propagationService;

        private readonly CultureInfo enCulture;
        private readonly CultureInfo frCulture;

        private int _notificationPageSize;

        public event Func<string, Task> Notify;

        public SystemNotificationService(
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
            IStringLocalizer<SystemNotificationService> localizer,
            ILogger<SystemNotificationService> logger,
            IPropagationService propagationService
            )
        {
            _dbContextFactory = dbContextFactory;
            _localizer = localizer;
            _logger = logger;
            _propagationService = propagationService;

            _propagationService.UpdateSystemNotifications += DoNotifyUsers;

            enCulture = new CultureInfo("en-ca");
            frCulture = new CultureInfo("fr-ca");

            _notificationPageSize = ISystemNotificationService.DEFAULT_PAGE_SIZE;
        }

        private string GetLocalizedString(CultureInfo culture, string textKey, object[] arguments)
        {
            try
            {
                var currentThread = Thread.CurrentThread;
                var oldCulture = currentThread.CurrentUICulture;
                currentThread.CurrentUICulture = culture;

                var localizedArguments = arguments.Select(a => ConvertLocalizableArgument(a)).ToArray();
                var localizedText = _localizer[textKey, localizedArguments];

                currentThread.CurrentUICulture = oldCulture;
                return localizedText;
            }
            catch (JsonReaderException e)
            {
                _logger.LogWarning("Missing translation for key {TextKey}", textKey);
                return $"Missing translation for {textKey}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError("Unexpected exception when getting localized string for {TextKey}", textKey);
                throw;
            }
        }

        public async Task<int> CreateSystemNotifications(List<string> userIds, string textKey, params object[] arguments)
        {
            return await DoCreateSystemNotifications(userIds, textKey, arguments);
        }
        public async Task<int> CreateSystemNotificationsWithLink(List<string> userIds, string actionLink,  string linkKey, string textKey, params object[] arguments)
        {
            return await DoCreateSystemNotifications(userIds, textKey, arguments, actionLink, linkKey);
        }

        private string ConvertLocalizableArgument(object argument)
        {
            if (argument is string)
            {
                return argument as string;
            }
            else if (argument is BilingualStringArgument)
            {
                var bilingualArg = argument as BilingualStringArgument;
                var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
                var isFrench = culture == frCulture;
                return isFrench ? bilingualArg.French : bilingualArg.English;
            }
            else if (argument is LocalizedKeyStringArgument)
            {
                var keyArg = argument as LocalizedKeyStringArgument;
                return _localizer[keyArg.Key];
            }
            else 
            {
                return argument?.ToString();
            }
        }

        private async Task<int> DoCreateSystemNotifications(List<string> userIds, string textKey, object[] arguments, string actionLink = null, string linkKey = null)
        {
            var textEn = GetLocalizedString(enCulture, textKey, arguments);
            var textFr = GetLocalizedString(frCulture, textKey, arguments);
            var now = DateTime.UtcNow;

            var notifications = userIds
                .Select(userId => new SystemNotification()
                {
                    Generated_TS = now,
                    NotificationTextEn_TXT = textEn,
                    NotificationTextFr_TXT = textFr,
                    ReceivingUser_ID = userId,
                    Read_FLAG = false,
                    ActionLink_URL = actionLink,
                    ActionLink_Key = linkKey
                });

            using var ctx = _dbContextFactory.CreateDbContext();
            await ctx.SystemNotifications.AddRangeAsync(notifications);
            var result = await ctx.SaveChangesAsync();

            await NotifyUsers(userIds);

            return result;
        }

        private async Task NotifyUsers(List<string> userIds)
        {
            await _propagationService.PropagateSystemNotificationUpdate(userIds);
        }

        private async Task DoNotifyUsers(IEnumerable<string> userIds)
        {
            if (Notify != null)
            {
                var tasks = userIds.Select(u => Notify.Invoke(u));
                await Task.WhenAll(tasks);
            }
        }

        private static Expression<Func<SystemNotification, bool>> CreateUserNotificationCondition(string userId, bool unreadOnly)
        {
            var lowerUserId = userId.ToLower();
            return (unreadOnly ?
                n => n.ReceivingUser_ID.ToLower() == userId && !n.Read_FLAG :
                n => n.ReceivingUser_ID.ToLower() == userId);
        }

        public async Task<int> GetNotificationCountForUser(string userId, bool unreadOnly = false)
        {
            var condition = CreateUserNotificationCondition(userId, unreadOnly);

            using var ctx = _dbContextFactory.CreateDbContext();
            return await ctx.SystemNotifications.CountAsync(condition);
        }

        public async Task<List<SystemNotification>> GetNotificationsForUser(string userId, bool unreadOnly = false, int pageNumber = 0)
        {
            var condition = CreateUserNotificationCondition(userId, unreadOnly);

            using var ctx = _dbContextFactory.CreateDbContext();
            return await ctx.SystemNotifications
                .Where(condition)
                .OrderByDescending(n => n.Generated_TS)
                .Skip(pageNumber * _notificationPageSize)
                .Take(_notificationPageSize)
                .ToListAsync();
        }

        public async Task SetReadStatus(long notificationId, bool readStatus)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var notification = await ctx.SystemNotifications.FirstOrDefaultAsync(n => n.Notification_ID == notificationId);
            if (notification != null)
            {
                notification.Read_FLAG = readStatus;
                await ctx.SaveChangesAsync();
                await NotifyUsers(new List<string>() { notification.ReceivingUser_ID });
            }
        }

        public int GetNotificationPageSize()
        {
            return _notificationPageSize;
        }

        public void Dispose()
        {
            _propagationService.UpdateSystemNotifications -= DoNotifyUsers;
        }
    }
}
