using Datahub.Application.Services.Notifications;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services.Queues;
using MassTransit;
using MassTransit.Transports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Notifications;

public class DatahubEmailService : IDatahubEmailService
{
    private readonly ILogger<DatahubEmailService> _logger;
    private readonly ISendEndpointProvider sendEndpointProvider;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

    public DatahubEmailService(ILogger<DatahubEmailService> logger, 
        ISendEndpointProvider sendEndpointProvider,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        _logger = logger;
        this.sendEndpointProvider = sendEndpointProvider;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<bool> SendAll(string sender, string subject, string body)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        var recipients = await ctx.Project_Users.Select(u => u.User_Name).Distinct().ToListAsync();
        return await SendMessage(ToList(sender), default, recipients, subject, body);
    }

    public async Task<bool> SendToProjects(string sender, List<string> projects, string subject, string body)
    {
        var projectSet = new HashSet<string>(projects);
        
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        var recipients = await ctx.Project_Users
           .Include(e => e.Project)
           .Where(e => projectSet.Contains(e.Project.Project_Acronym_CD))
           .Select(e => e.User_Name)
           .Distinct()
           .ToListAsync();

        return await SendMessage(ToList(sender), default, recipients, subject, body);
    }

    public async Task<bool> SendToRecipients(string sender, List<string> recipients, string subject, string body)
    {
        return await SendMessage(ToList(sender), default, recipients, subject, body);
    }

    private List<string> ToList(string value) => new() { value };

    private async Task<bool> SendMessage(List<string>? toRecipients, List<string>? ccRecipients, List<string>? bccRecipients, string subject, string body)
    {
        if (toRecipients is null && ccRecipients is null)
        {
            _logger.LogError($"No recipients specified for message '{subject}'");
            return false;
        }
        try
        {
            EmailRequestMessage message = new()
            {
                To = toRecipients ?? new(),
                CcTo = ccRecipients ?? new(),
                BccTo = bccRecipients ?? new(),
                Subject = subject,
                Body = body
            };
            var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{QueueConstants.QUEUE_EMAILS}"));
            await endpoint.Send(message);            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }        
    }
}
