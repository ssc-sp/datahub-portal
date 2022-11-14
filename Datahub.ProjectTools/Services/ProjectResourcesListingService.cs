using Datahub.Core.Data;
using Datahub.Core.EFCore;
using Datahub.Core.Services;
using Datahub.ProjectTools;
using Datahub.ProjectTools.Catalog;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.ProjectTools.Services
{

    public static class ProjectResourcesListingServiceExtensions
    {
        public static void AddProjectResources(this IServiceCollection services)
        {
            ProjectResourcesListingService.RegisterResources(services);
            services.AddScoped<ProjectResourcesListingService>();
        }
    }

    public class ProjectResourcesListingService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IUserInformationService userInformationService;

        public ProjectResourcesListingService(IServiceProvider serviceProvider, IUserInformationService userInformationService)
        {
            this.serviceProvider = serviceProvider;
            this.userInformationService = userInformationService;
        }

        //private static Type[] ResourceProviders = new[] { typeof(DHDatabricksResource), typeof(DHPublicSharing), typeof(DHStorageResource)}; 
        private static Type[] ResourceProviders = new[] { typeof(DHPublicSharing)}; 

        public static void RegisterResources(IServiceCollection services)
        {
            foreach (var resource in ResourceProviders)
                services.AddScoped(resource);
        }

        public async Task<List<IProjectResource>> GetResourcesForProject(Datahub_Project project)
        {
            var services = new ServiceCollection();            
            
            using var serviceScope = serviceProvider.CreateScope();
            var authUser = await userInformationService.GetAuthenticatedUser();
            var output = new List<IProjectResource>();
            var isUserAdmin = await userInformationService.IsUserProjectAdmin(project.Project_Acronym_CD);
            var isUserDHAdmin = await userInformationService.IsUserDatahubAdmin();
            foreach (var item in ResourceProviders)
            {
                var dhResource = serviceScope.ServiceProvider.GetRequiredService(item) as IProjectResource;
                await dhResource.InitializeAsync(project, await userInformationService.GetUserIdString(), await userInformationService.GetCurrentGraphUserAsync(), isUserAdmin || isUserDHAdmin);
                output.Add(dhResource);
            }
            return output;

        }


    }
}
