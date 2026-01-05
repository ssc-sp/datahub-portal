using Datahub.Core.Model.Context;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Core.Model.Users;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Datahub.SpecflowTests.Utils;

public static class GenerateWorkspaceHelper
{
    public const string JSON_RG = "{\"resource_group_name\":\"fsdh_proj_abc_dev_rg\"}";

    public static async Task GenerateWorkspace(IDbContextFactory<DatahubProjectDBContext> dbContextFactory, 
        string projectAcronym, 
        string? resourceType = null,
        string? resourceStatus = null,
        string? cbrid = null,
        bool generateResourceGroup = true)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        
        // First, add and save the subscription
        var datahubAzureSubscription = new DatahubAzureSubscription()
        {
            Nickname = "Test Subscription",
            TenantId = Testing.WorkspaceTenantGuid,
            SubscriptionId = Testing.WorkspaceSubscriptionGuid,
            SubscriptionName = Testing.SubscriptionName
        };
        ctx.AzureSubscriptions.Add(datahubAzureSubscription);
        await ctx.SaveChangesAsync();
        
        // Then create and add the project with the subscription ID reference
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = projectAcronym,
            DatahubAzureSubscription = datahubAzureSubscription,
            DatahubAzureSubscriptionId = datahubAzureSubscription.Id,
            MetadataAdded = true,
            Project_Budget = 100.0M
        };
        var credits = new Project_Credits
        {
            Project = project,
            Current = 0
        };
        ctx.Project_Credits.Add(credits);
        if (resourceType != null)
        {
            var resource = new Project_Resources2()
            {
                ResourceType = TerraformTemplate.GetTerraformServiceType(resourceType),
                JsonContent = JSON_RG,
                Project = project,
                Status = resourceStatus,
                CreatedAt = DateTime.Now
            };
                
            ctx.Project_Resources2.Add(resource);
        }
        if (resourceType != TerraformTemplate.NewProjectTemplate && generateResourceGroup)
        {
            var rg = new Project_Resources2()
            {
                ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate),
                JsonContent = "{\"resource_group_name\":\"fsdh_proj_abc_dev_rg\"}",
                Project = project,
                Status = TerraformStatus.Completed,
                CreatedAt = DateTime.Now
            };
            ctx.Project_Resources2.Add(rg);
        }

        var user = new PortalUser
        {
            Email = Testing.CurrentUserEmail,
            EntraUser = new() { GraphGuid = "00000000-0000-0000-0000-000000000000", PortalUser = null! },
            DisplayName = "Test User"
        };
        ctx.PortalUsers.Add(user);

        var projectUser = new UserRoleLinks
        {
            PortalUser = user,
            Project = project   
        };
        ctx.UserRolesLinks.Add(projectUser);

        //{'DepartmentName', 'FinancialAuthorityCommitmentIsOrg', 'FinancialAuthorityCommitmentIsRef',
        //'FinancialAuthorityEmail', 'FinancialAuthorityFirstName', 'FinancialAuthorityLastName',
        //'Keywords', 'LeadEmail', 'LeadFirstName', 'LeadLastName', 'RetentionValue', 'Subject', 'WorkspaceDescription', 'WorkspaceName
        if (cbrid is not null)
        {
            var gchostingDetails = new GCHostingWorkspaceDetails()
            {
                GcHostingId = "1",
                CBRID = cbrid,
                CBRName = "Test CBR",
                DepartmentName = "Test Department",
                FinancialAuthorityCommitmentIsOrg = "1",
                FinancialAuthorityCommitmentIsRef = "1",
                FinancialAuthorityEmail = "test@test.gc.ca",
                FinancialAuthorityFirstName = "John",
                FinancialAuthorityLastName = "Accounting",
                Keywords = "test,workspace",
                LeadEmail = "lead@test.gc.ca",
                LeadFirstName = "Jane",
                LeadLastName = "Manager",
                RetentionValue = "5 years",
                Subject = "Test Subject",
                WorkspaceDescription = "This is a test workspace",
                WorkspaceName = "Test Workspace"                
            };
            project.ParentGCHostingBudget = gchostingDetails;
        }
        ctx.Projects.Add(project);
        await ctx.SaveChangesAsync();
    }
}