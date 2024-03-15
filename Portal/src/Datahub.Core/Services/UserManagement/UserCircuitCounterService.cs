﻿namespace Datahub.Core.Services.UserManagement;

public class UserCircuitCounterService : IDisposable, IUserCircuitCounterService
{
    readonly IGlobalSessionManager sessionManager;
    readonly IUserInformationService userInformationService;
    private string sessionId;
    private bool? enabled;

    public UserCircuitCounterService(IGlobalSessionManager sessionManager, IUserInformationService userInformationService)
    {
        this.sessionManager = sessionManager;
        this.userInformationService = userInformationService;
        enabled = null;
    }

    public async Task<bool> IsSessionEnabled()
    {
        if (enabled is null)
        {
            sessionId = await userInformationService.GetUserIdString();
            enabled = sessionManager.TryAddSession(sessionId);
        }
        return enabled.Value;
    }

    public int GetSessionCount()
    {
        return sessionManager.GetSessionCount(sessionId);
    }

    public void Dispose()
    {
        if (enabled == true)
        {
            sessionManager.RemoveSession(sessionId);
            sessionId = null;
        }
    }
}