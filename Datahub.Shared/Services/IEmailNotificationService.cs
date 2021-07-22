using System.Collections.Generic;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public interface IEmailNotificationService
    {
        Task<string> RenderTestTemplate();
        Task<string> RenderTemplate<T>(IDictionary<string, object> parameters = null) where T: Microsoft.AspNetCore.Components.IComponent;
        Task SendEmailMessage(string subject, string body, string recipientName, string recipientAddress, bool isHtml = true);
        bool IsDevTestMode();
    }
}