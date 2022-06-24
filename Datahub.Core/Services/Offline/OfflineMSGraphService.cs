using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Datahub.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;

namespace Datahub.Core.Services
{
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
                        { "1", new GraphUser() { Id = "1", DisplayName = "Offline User"} },
                        { "2", new GraphUser() { Id = "2", DisplayName = "Mennie, Todd"} },
                        { "3", new GraphUser() { Id = "3", DisplayName = "Shelat, Yask"} },
                        { "4", new GraphUser() { Id = "4", DisplayName = "Wang, Simon"} },
                        { "5", new GraphUser() { Id = "5", DisplayName = "Yuldashev, Alisher"} }
                      };
                  }
              });
        }

        //public async Task<Dictionary<string, GraphUser>> GetUsersAsync()
        //{
        //    if (UsersDict != null)
        //    {
        //        return UsersDict;
        //    }

        //    await LoadUsersAsync();
        //    return UsersDict;
        //}

        private void PrepareAuthenticatedClient()
        {
            // Create Microsoft Graph client.
            try
            {

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot Prepare Authenticated Request", ex);
            }
        }

        //public GraphUser GetUser(string userId)
        //{
        //    if (!string.IsNullOrWhiteSpace(userId))
        //    {
        //        if (UsersDict != null && UsersDict.ContainsKey(userId))
        //        {
        //            return UsersDict[userId];
        //        }
        //    }

        //    return null;
        //}

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
    }
}