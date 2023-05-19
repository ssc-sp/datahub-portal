namespace Datahub.Application.Services.Notifications;

public interface IDatahubEmailService
{
    Task<bool> SendAll(string sender, string subject, string body);
    Task<bool> SendToProjects(string sender, List<string> projects, string subject, string body);
    Task<bool> SendToRecipients(string sender, List<string> recipients, string subject, string body);
}
