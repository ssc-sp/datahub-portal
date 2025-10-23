using Datahub.Core.Model.Context;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Datahub.SpecflowTests.Utils;

public static class GenerateWorkspaceHelper
{
    public static async Task GenerateWorkspace(IDbContextFactory<DatahubProjectDBContext> dbContextFactory, string projectAcronym, string? resourceType = null, string? resourceStatus = null, string? cbrid = null)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = projectAcronym,
        };
        var datahubAzureSubscription = new DatahubAzureSubscription()
        {
            Nickname = "Test Subscription",
            TenantId = "00000000-0000-0000-0000-000000000000",
            SubscriptionId = "00000000-0000-0000-0000-000000000000",
            SubscriptionName = "Test Subscription Name"
        };
            
        project.DatahubAzureSubscription = datahubAzureSubscription;

        if (resourceType != null)
        {
            var resource = new Project_Resources2()
            {
                ResourceType = TerraformTemplate.GetTerraformServiceType(resourceType),
                JsonContent = "{}",
                Project = project,
                Status = resourceStatus
            };
                
            ctx.Project_Resources2.Add(resource);
        }

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
        ctx.AzureSubscriptions.Add(datahubAzureSubscription);
        ctx.Projects.Add(project);
        await ctx.SaveChangesAsync();
    }
}