using Bunit;
using Bunit.TestDoubles;
using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;

namespace Datahub.SpecflowTests.Steps.Workspace
{
    public static class CommonCbrTestUtils
    {
        public static readonly PortalUser CbrOwnerUser = new()
        {
            GraphGuid = Guid.NewGuid().ToString(),
            Id = 1,
            DisplayName = "CBR Owner",
            Email = "cbrowner@example.com",
        };

        public static readonly PortalUser OtherWorkspaceLead = new()
        {
            GraphGuid = Guid.NewGuid().ToString(),
            Id = 2,
            DisplayName = "Other Lead",
            Email = "wlead@example.com"
        };

        public async static Task<SpecFlowDbContextFactory> GenerateCbrTestDatabase()
        {
            var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContextFactory = new SpecFlowDbContextFactory(options);

            await using var context = await dbContextFactory.CreateDbContextAsync();

            var workspaceLeadRole = new Project_Role()
            {
                Id = (int)Project_Role.RoleNames.WorkspaceLead,
                Name = RoleConstants.WORKSPACE_LEAD_ROLE,
                Description = RoleConstants.WORKSPACE_LEAD_ROLE
            };

   

            var mainCbrWorkspace = new Datahub_Project()
            {
                Project_ID = 1,
                Project_Acronym_CD = Testing.WorkspaceAcronym,
                Project_Name = "Main CBR Workspace",
                Project_Budget = 1000
            };

            var otherWorkspace = new Datahub_Project()
            {
                Project_ID = 2,
                Project_Acronym_CD = Testing.WorkspaceAcronym2,
                Project_Name = "Additional Workspace",
                Project_Budget = 1000
            };

            var w1cbrOwnerUser = new Datahub_Project_User()
            {
                PortalUser = CbrOwnerUser,
                Role = workspaceLeadRole,
                Project = mainCbrWorkspace
            };

            var w2LeadUser = new Datahub_Project_User()
            {
                Role = workspaceLeadRole,
                PortalUser = OtherWorkspaceLead,
                Project = otherWorkspace
            };

            var gcHosting = new GCHostingWorkspaceDetails()
            {
                Datahub_Project = mainCbrWorkspace,
                GcHostingId = Guid.NewGuid().ToString(),
                LeadEmail = CbrOwnerUser.Email,
                WorkspaceBudget = 10000,
                CBRName = "Example CBR",
                CBRID = "ABC123",
                DepartmentName = "Example",
                FinancialAuthorityCommitmentIsOrg = "1",
                FinancialAuthorityCommitmentIsRef = "1",
                FinancialAuthorityEmail = "financial@example.com",
                FinancialAuthorityFirstName = "Uncle",
                FinancialAuthorityLastName = "Pennybags",
                Keywords = "example",
                LeadFirstName = "Joe",
                LeadLastName = "Bloggs",
                RetentionValue = "123",
                Subject = "example",
                WorkspaceDescription = "Description",
                WorkspaceName = "Name"
            };

            mainCbrWorkspace.ParentGCHostingBudget = gcHosting;
            otherWorkspace.ParentGCHostingBudget = gcHosting;

            await context.Project_Roles.AddAsync(workspaceLeadRole);
            await context.PortalUsers.AddRangeAsync(CbrOwnerUser, OtherWorkspaceLead);
            await context.Projects.AddRangeAsync(mainCbrWorkspace, otherWorkspace);
            await context.Project_Users.AddRangeAsync(w1cbrOwnerUser, w2LeadUser);
            await context.GCHostingWorkspaceDetails.AddAsync(gcHosting);

            await context.SaveChangesAsync();

            return dbContextFactory;
        }

        public static void AddLoggedInUserAuthorization(TestContextBase testContext, string workspaceName, bool isCbrOwner, bool isDhAdmin)
        {
            var roleNames = new List<string>
            {
                $"{workspaceName}{RoleConstants.WORKSPACE_LEAD_SUFFIX}",
                "default"
            };

            if (isCbrOwner)
            {
                roleNames.Add($"{workspaceName}{RoleConstants.CBR_OWNER_SUFFIX}");
                roleNames.Add(RoleConstants.CBR_OWNER_ROLE);
            }

            if (isDhAdmin)
            {
                roleNames.Add(RoleConstants.DATAHUB_ROLE_ADMIN);
            }

            var authContext = testContext.AddTestAuthorization();
            authContext.SetAuthorized("TEST USER");
            authContext.SetRoles([.. roleNames]);
        }

    }
}
