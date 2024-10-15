using System.Security.Claims;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;

namespace Datahub.Infrastructure.Offline;

public class OfflineUserInformationService : IUserInformationService
{
    public static readonly Guid UserGuid = new Guid(); 

    readonly ILogger<IUserInformationService> _logger;
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    
    private User AnonymousUser => UserInformationServiceConstants.GetAnonymousUser();

    public OfflineUserInformationService(ILogger<IUserInformationService> logger, IDbContextFactory<DatahubProjectDBContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public Task<User> GetCurrentGraphUserAsync()
    {
        return Task.FromResult(new User
        {
            DisplayName = "Offline User",
            Id = UserGuid.ToString(),
            UserPrincipalName = "me@me.com",
            Mail = "nabeel.bader@nrcan-rncan.gc.ca"
        });
    }

    public Task<string> GetDisplayName()
    {
        return Task.FromResult("Me");
    }

    public Task<string> GetUserEmail()
    {
        return Task.FromResult("me@me.com");
    }

    public Task<string> GetUserEmailDomain()
    {
        return Task.FromResult("me@me.com");
    }

    public Task<string> GetUserEmailPrefix()
    {
        return Task.FromResult("");
    }

    public Task<User> GetAnonymousGraphUserAsync()
    {
        return Task.FromResult(AnonymousUser);
    }

    public Task<string> GetUserIdString()
    {
        return Task.FromResult(UserGuid.ToString());
    }

    public Task<string> GetUserLanguage()
    {
        return Task.FromResult("en-CA");
    }

    public Task<string> GetUserRootFolder()
    {
        return Task.FromResult("/");
    }

    public Task<bool> IsFrench()
    {
        return Task.FromResult(false);
    }

    public Task<bool> RegisterUserLanguage(string language)
    {
        return Task.FromResult(true);
    }
        
    public bool SetLanguage(string language)
    {
        return true;
    }

    public Task<PortalUser> GetPortalUserWithAchievementsAsync(string userGraphId)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetGraphUserAsync(string userId)
    {
        return Task.FromResult(new User
        {
            DisplayName = "Offline User random",
            Id = userId,
            UserPrincipalName = "me@me.com"
        });
    }

    public async Task<PortalUser> GetCurrentPortalUserAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // get yan
        // var yan = await context.PortalUsers.FirstOrDefaultAsync(u => u.DisplayName == "Yannick Robert");
        // return yan;
        
        // get total number of users
        var totalUsers = await context.PortalUsers.CountAsync();
        
        // return a random one
        var randomUser = await context.PortalUsers.Skip(new Random().Next(0, totalUsers)).FirstOrDefaultAsync();
        
        return randomUser!;
    }

    public async Task<PortalUser> GetPortalUserAsync(string userGraphId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return (await context.PortalUsers.FirstOrDefaultAsync(u => u.GraphGuid == userGraphId))!;
    }

    public async Task HandleDeletedUserRegistration(string email, string graphId)
    {
        return;
    }
    public async Task<PortalUser> GetCurrentPortalUserWithAchievementsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // get total number of users
        var totalUsers = await context.PortalUsers.CountAsync();
        
        // return a random one
        var randomUser = await context.PortalUsers
            .Include(u => u.Achievements)
                .ThenInclude(a => a.Achievement)
            .Skip(new Random().Next(0, totalUsers))
            .FirstOrDefaultAsync();
        
        return randomUser!;
    }
    public async Task<ExtendedPortalUser?> GetPortalUserByEmailAsync(string email)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.PortalUsers.FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower())!;
        if (user != null)
        {
            return new ExtendedPortalUser(user);
        }
        return null;
    }
    public Task<bool> IsUserWithoutInitiatives()
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsViewingAsGuest()
    {
        return Task.FromResult(false);
    }

    public Task SetViewingAsGuest(bool isGuest)
    {
        // do nothing
        return Task.Delay(0);
    }

    public Task<bool> IsViewingAsVisitor()
    {
        return Task.FromResult(false);
    }

    public Task SetViewingAsVisitor(bool isVisitor)
    {
        return Task.CompletedTask;
    }

    public Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false)
    {
        return Task.FromResult(new ClaimsPrincipal());
    }

    public Task<bool> IsUserProjectAdmin(string projectAcronym)
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsUserProjectWorkspaceLead(string projectAcronym)
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsUserDatahubAdmin()
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsUserProjectMember(string projectAcronym)
    {
        return Task.FromResult(false);
    }

    public Task RegisterAuthenticatedPortalUser()
    {
        return Task.FromResult(true);
    }

    public Task CreatePortalUserAsync(string userGraphId)
    {
        throw new NotImplementedException();
    }

    public Task<PortalUser> GetAuthenticatedPortalUser()
    {
        return Task.FromResult(new PortalUser(){GraphGuid = AnonymousUser.Id});
    }
    
    public Task<bool> UpdatePortalUserAsync(PortalUser updatedUser)
    {
        PortalUserUpdated?.Invoke(this, new PortalUserUpdatedEventArgs(updatedUser));
        throw new NotImplementedException();
    }
    public event EventHandler<PortalUserUpdatedEventArgs>? PortalUserUpdated;
    public Task<bool> IsDailyLogin()
    {
        return Task.FromResult(false);
    }
}