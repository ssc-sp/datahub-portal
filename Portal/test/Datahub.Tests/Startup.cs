using BlazorDownloadFile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.JSInterop;
using Datahub.Portal.Services;
using Datahub.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Tewr.Blazor.FileReader;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services.Security;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Infrastructure.Services.UserManagement;
using Datahub.Portal.Services.Api;

namespace Datahub.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserInformationService, OfflineUserInformationService>();
        services.AddSingleton<IMSGraphService, MSGraphService>();
        services.AddSingleton<IKeyVaultService, KeyVaultCoreService>();
        services.AddScoped<DataLakeClientService>();
        services.AddScoped<IWebHostEnvironment, FakeWebHostEnvironment>();
        services.AddScoped<IJSRuntime, FakeJSRuntime>();
        //services.AddScoped<DataUpdatingService>();
        //services.AddScoped<DataSharingService>();
        services.AddScoped<DataCreatorService>();
        //services.AddScoped<DataRetrievalService>();
        //services.AddScoped<DataRemovalService>();
        services.AddSingleton<DatahubTools>();
        services.AddScoped<NotificationsService>();
        services.AddScoped<UiControlsService>();
        services.AddHttpClient();
        services.AddFileReaderService();
        services.AddBlazorDownloadFile();
        services.AddScoped<NotifierService>();
    }
}

public class FakeWebHostEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ContentRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string EnvironmentName { get => "Development"; set => throw new NotImplementedException(); }
}

public class FakeJSRuntime : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args)
    {
        throw new NotImplementedException();
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args)
    {
        throw new NotImplementedException();
    }
}