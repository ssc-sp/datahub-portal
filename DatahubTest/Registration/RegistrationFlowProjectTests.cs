using System;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.EFCore;
using Datahub.Portal.Data.Forms;
using Datahub.Portal.Services;
using Datahub.Tests.Portal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Datahub.Tests.Registration;

public class RegistrationFlowProjectTests
{
    private readonly MockProjectDbContextFactory _dbFactory;
    private readonly RegistrationService _registrationService;

    public RegistrationFlowProjectTests()
    {
        _dbFactory = new MockProjectDbContextFactory();
        var logger = Mock.Of<ILogger<RegistrationService>>();
        _registrationService = new RegistrationService(_dbFactory, logger, null);
    }
    
    [Fact]
    public async Task RegistrationFlow_EnsureDatabaseSharedTest()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        
        context.Registration_Requests.Add(
            new Datahub_Registration_Request
            {
                ProjectName = "EnsureDatabaseSharedTest Project",
                Email = "EnsureDatabaseSharedTest@email.com",
                DepartmentName = "EnsureDatabaseSharedTest Department",
                CreatedBy = RegistrationService.SELF_SIGNUP,
                CreatedAt = DateTime.Now
            });
        await context.SaveChangesAsync();
        
        await using var context2 = await _dbFactory.CreateDbContextAsync();
        Assert.NotEmpty(await context2.Registration_Requests.ToListAsync());
    }

    [Fact]
    public async Task RegistrationFlow_Submit_Test()
    {
        var intakeForm = new BasicIntakeForm()
        {
            DepartmentName = "Test department",
            ProjectName = "Test project",
            Email = "test@email.com",
        };
        
        await _registrationService.SubmitRegistration(intakeForm, RegistrationService.SELF_SIGNUP);
        
        await using var db = await _dbFactory.CreateDbContextAsync();
        var result = await db.Registration_Requests
            .FirstOrDefaultAsync(r => r.Email == intakeForm.Email);
        
        Assert.NotNull(result);
        Assert.Equal(intakeForm.Email, result.Email);
        Assert.Equal(intakeForm.DepartmentName, result.DepartmentName);
        Assert.Equal(intakeForm.ProjectName, result.ProjectName);
        Assert.NotNull(result.CreatedAt);
        Assert.Equal(RegistrationService.SELF_SIGNUP, result.CreatedBy);
        Assert.Equal(Datahub_Registration_Request.STATUS_REQUESTED, result.Status);
        Assert.True(result.Id > 0, "Id should be set");
    }
    
    [Fact]
    public async Task RegistrationFlow_ResubmitTest()
    {
        var intakeForm = new BasicIntakeForm()
        {
            DepartmentName = "Test department",
            ProjectName = "Test project",
            Email = "test@email.com",
        };
        
        await _registrationService.SubmitRegistration(intakeForm, RegistrationService.SELF_SIGNUP);
        
        intakeForm.ProjectName = "Test project 2";
        intakeForm.DepartmentName = "Test department 2";
        
        await _registrationService.SubmitRegistration(intakeForm, RegistrationService.SELF_SIGNUP);
        
        await using var db = await _dbFactory.CreateDbContextAsync();
        var result = await db.Registration_Requests
            .FirstOrDefaultAsync(r => r.Email == intakeForm.Email);
        
        Assert.NotNull(result);
        Assert.NotNull(result.UpdatedAt);
        Assert.Equal(RegistrationService.SELF_SIGNUP, result.UpdatedBy);
        Assert.Equal(intakeForm.DepartmentName, result.DepartmentName);
        Assert.Equal(intakeForm.ProjectName, result.ProjectName);
    }

    [Fact]
    public async Task RegistrationFlow_Invalid_DoesNotExist_Test()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        const string testName = "RegistrationInvalidDoesNotExistTest";
        var request = new Datahub_Registration_Request
        {
            ProjectName = $"{testName} Project",
            Email = $"{testName}@email.com",
            DepartmentName = $"{testName} Department",
            CreatedBy = RegistrationService.SELF_SIGNUP,
            CreatedAt = DateTime.Now
        };
        
        Assert.False(await context.Registration_Requests.AnyAsync(r => r.Email == request.Email));
        Assert.False(await _registrationService.IsValidRegistrationRequest(request));
    }

    [Theory]
    [InlineData("", "exists")]
    [InlineData(" ", "exists")]
    [InlineData(null, "exists")]
    [InlineData("eXiSTs", "exists")]
    public async Task RegistrationFlow_Invalid_Acronym_Test(string testAcronym, string existingAcronym)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        await context.AddAsync(new Datahub_Registration_Request
        {
            ProjectAcronym = existingAcronym
        });
        await context.SaveChangesAsync();

        const string testName = "RegistrationInvalidAcronymTest";
        var request = new Datahub_Registration_Request
        {
            ProjectName = $"{testName} Project",
            Email = $"{testName}@email.com",
            DepartmentName = $"{testName} Department",
            CreatedBy = RegistrationService.SELF_SIGNUP,
            CreatedAt = DateTime.Now,
            ProjectAcronym = testAcronym,
        };
        
        await context.Registration_Requests.AddAsync(request);
        await context.SaveChangesAsync();
        
        Assert.True(await context.Registration_Requests.AnyAsync(r => r.Email == request.Email));
        Assert.False(await _registrationService.IsValidRegistrationRequest(request));
    }

    [Fact]
    public async Task RegistrationFlow_CreateProject_Test()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        const string testName = "RegistrationCreateProjectTest";
        var request = new Datahub_Registration_Request
        {
            ProjectName = $"{testName} Project",
            Email = $"{testName}@email.com",
            DepartmentName = $"{testName} Department",
            CreatedBy = RegistrationService.SELF_SIGNUP,
            CreatedAt = DateTime.Now,
            ProjectAcronym = "RegistrationCreateProjectTest"
        };
        
        await context.Registration_Requests.AddAsync(request);
        await context.SaveChangesAsync();

        await _registrationService.CreateProject(request, testName);
        var requestResult = await context.Registration_Requests
            .FirstOrDefaultAsync(r => r.Email == request.Email);
        
        var projectResult = await context.Projects
            .FirstOrDefaultAsync(r => r.Project_Name == request.ProjectName);
        Assert.NotNull(projectResult);
        Assert.NotNull(requestResult);

        Assert.Equal(requestResult.ProjectName, projectResult.Project_Name);
        Assert.Equal(requestResult.Email, projectResult.Contact_List);
        Assert.Equal(requestResult.DepartmentName, projectResult.Branch_Name);
        
        Assert.Equal(testName, requestResult.UpdatedBy);
        Assert.True(requestResult.UpdatedAt > DateTime.Now.AddMinutes(-5));
        Assert.Equal(Datahub_Registration_Request.STATUS_CREATED, requestResult.Status);

        BasicIntakeForm.DepartmentDictionary.TryGetValue(request.DepartmentName, out var sectorName);
        
        Assert.Equal(sectorName ?? requestResult.DepartmentName, projectResult.Sector_Name);
    }
    
    [Fact]
    public async Task RegistrationFlow_CreateProject_Invalid_Test()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        const string testName = "RegistrationCreateProjectInvalidTest";
        var request = new Datahub_Registration_Request
        {
            ProjectName = $"{testName} Project",
            Email = $"{testName}@email.com",
            DepartmentName = $"{testName} Department",
            CreatedBy = RegistrationService.SELF_SIGNUP,
            CreatedAt = DateTime.Now,
        };
        
        await context.Registration_Requests.AddAsync(request);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _registrationService.CreateProject(request, testName);
        });
    }

    [Fact]
    public async Task RegistrationCreateProjectSectorNameTest()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        const string testName = "RegistrationCreateProjectSectorNameTest";
        var (department, sector) = BasicIntakeForm.DepartmentDictionary.First();
        var request = new Datahub_Registration_Request
        {
            ProjectName = $"{testName} Project",
            Email = $"{testName}@email.com",
            DepartmentName = department,
            CreatedBy = RegistrationService.SELF_SIGNUP,
            CreatedAt = DateTime.Now,
            ProjectAcronym = "RegistrationCreateProjectSectorNameTest"
        };
        
        await context.Registration_Requests.AddAsync(request);
        await context.SaveChangesAsync();
        await _registrationService.CreateProject(request, testName);
        
        var projectResult = await context.Projects
            .FirstOrDefaultAsync(r => r.Project_Name == request.ProjectName);
        Assert.NotNull(projectResult);
        Assert.Equal(sector, projectResult.Sector_Name);
    }
}