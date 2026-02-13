using System.Net.Mail;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace Datahub.Infrastructure.Offline;

public class OfflineMSGraphService : IMSGraphService
{
    public Dictionary<string, GraphUser> UsersDict { get; set; }

    public OfflineMSGraphService(IConfiguration configuration)
    {
        //clientSecret = configuration["ClientAppSecret"];
    }

    public async Task LoadUsersAsync()
    {
        await Task.Run(() =>
        {
            if (UsersDict == null)
            {
                UsersDict = new Dictionary<string, GraphUser>
                {
                    { "1", new GraphUser() { Id = "1", DisplayName = "Offline User", MailAddress = new MailAddress("offlineuser@example.com") } },
                    { "2", new GraphUser() { Id = "2", DisplayName = "Mennie, Todd", MailAddress = new MailAddress("todd.mennie@example.com") } },
                    { "3", new GraphUser() { Id = "3", DisplayName = "Shelat, Yask", MailAddress = new MailAddress("yask.shelat@example.com") } },
                    { "4", new GraphUser() { Id = "4", DisplayName = "Wang, Simon", MailAddress = new MailAddress("simon.wang@example.com") } },
                    { "5", new GraphUser() { Id = "5", DisplayName = "Yuldhev, Alisher", MailAddress = new MailAddress("alisher.yuldhev@example.com") } }
                };
            }
        });
    }

    public Task<string> GetUserName(string userId, CancellationToken tkn)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            if (UsersDict != null && UsersDict.ContainsKey(userId))
            {
                return Task.FromResult(UsersDict[userId].DisplayName);
            }
        }
        return Task.FromResult("...");
    }

    public async Task<string> GetUserEmail(string userId, CancellationToken tkn)
    {
        var user = await GetUserAsync(userId, CancellationToken.None);
        return user?.Mail;
    }
      
    public Task<string> GetUserIdFromEmailAsync(string email, CancellationToken tkn)
    {
        throw new NotImplementedException();
    }

    public Task<GraphUser> GetUserAsync(string userId, CancellationToken tkn)
    {
        throw new NotImplementedException();
    }


    public Task<Dictionary<string, GraphUser>> GetUsersListAsync(string filterText, CancellationToken tkn)
    {
        return Task.FromResult(UsersDict);
    }

    public Task<GraphUser> GetUserFromEmailAsync(string email, CancellationToken tkn) 
    {
        return Task.FromResult((GraphUser)null);
    }

    public Task<GraphUser> GetUserFromSamAccountNameAsync(string account, CancellationToken tkn)
    {
        return Task.FromResult((GraphUser)null);
    }

    public GraphServiceClient GetAuthenticatedClient()
    {
        throw new NotImplementedException();
    }
}
