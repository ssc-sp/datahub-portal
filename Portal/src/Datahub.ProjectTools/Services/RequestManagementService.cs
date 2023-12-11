using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.Notification;
using Datahub.Core.Services.Projects;
using Datahub.Core.Services.Security;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Transactions;
using Datahub.Application.Services;
using Datahub.Core.Model.Projects;
using Datahub.ProjectTools.Utils;
using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using Datahub.Shared.Exceptions;

namespace Datahub.ProjectTools.Services;

public class RequestManagementService : IRequestManagementService
{
    private readonly ILogger<RequestManagementService> _logger;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly ProjectToolsEmailService _emailNotificationService;
    private readonly ISystemNotificationService _systemNotificationService;
    private readonly IUserInformationService _userInformationService;
    private readonly IDatahubAuditingService _datahubAuditingService;
    private readonly IResourceMessagingService _resourceMessagingService;
    private readonly IMiscStorageService _miscStorageService;

    private const string RESOURCE_REQUEST_INPUT_JSON_PREFIX = "ResourceInput";

    public RequestManagementService(
        ILogger<RequestManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        ProjectToolsEmailService emailNotificationService,
        ISystemNotificationService systemNotificationService,
        IUserInformationService userInformationService,
        IDatahubAuditingService datahubAuditingService,
        IResourceMessagingService resourceMessagingService,
        IMiscStorageService miscStorageService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _emailNotificationService = emailNotificationService;
        _systemNotificationService = systemNotificationService;
        _userInformationService = userInformationService;
        _datahubAuditingService = datahubAuditingService;
        _resourceMessagingService = resourceMessagingService;
        _miscStorageService = miscStorageService;
    }


    public async Task HandleUserUpdatesToExternalPermissions(Datahub_Project project)
    {
        var currentPortalUser = await _userInformationService.GetCurrentPortalUserAsync();
        var workspaceDefinition = await _resourceMessagingService.GetWorkspaceDefinition(project.Project_Acronym_CD, currentPortalUser.Email);
        await _resourceMessagingService.SendToUserQueue(workspaceDefinition);
    }

    // TODO: Remove unused parameter "inputParams"
    public async Task RequestService(Datahub_ProjectRequestAudit request,
        Dictionary<string, string> inputParams = null!)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        ctx.Projects.Attach(request.Project);
            
        // load any ServiceRequests that may have been added since the project was loaded
        
        await ctx.Entry(request.Project)
            .Collection(p => p.ServiceRequests)
            .LoadAsync();

        if (request.Project.ServiceRequests?.Any(a => a.RequestType == request.RequestType) ?? false)
        {
            _logger.LogInformation(
                "Service request already exists for project {Acronym} and service type {ServiceType}",
                request.Project.Project_Acronym_CD, request.RequestType);
            return;
        }
        
        await ctx.Project_Requests.AddAsync(request);
        // var projectResource = CreateEmptyProjectResource(request, inputParams);
        // await ctx.Project_Resources2.AddAsync(projectResource);

        await ctx.TrackSaveChangesAsync(_datahubAuditingService);

        // await NotifyProjectAdminsOfServiceRequest(request);
    }

    public static Project_Resources2 CreateEmptyProjectResource(Datahub_ProjectRequestAudit request,
        Dictionary<string, string> inputParams)
    {
        var resource = new Project_Resources2
        {
            Project = request.Project,
            ResourceType = request.RequestType,
            TimeRequested = DateTime.UtcNow
        };

        if (inputParams != null)
        {
            resource.SetInputParameters(inputParams);
        }

        switch (request.RequestType)
        {
            case IRequestManagementService.POSTGRESQL:
            case IRequestManagementService.SQLSERVER:
                resource.SetResourceObject(default(ProjectResource_Database));
                break;
            case IRequestManagementService.STORAGE:
                resource.SetResourceObject(default(ProjectResource_Storage));
                break;
            case IRequestManagementService.DATABRICKS:
                resource.SetResourceObject(default(ProjectResource_Databricks));
                break;
            default:
                resource.SetResourceObject(default(ProjectResource_Blank));
                break;
        }

        return resource;
    }

    public async Task<List<Project_Resources2>> GetResourcesByRequest(Datahub_ProjectRequestAudit request)
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync();

        var resources = await ctx.Project_Resources2
            .Where(r => r.Project == request.Project && r.ResourceType == request.RequestType)
            .ToListAsync();

        return resources;
    }

    public async Task<bool> UpdateResourceInputParameters(Guid resourceId, Dictionary<string, string> inputParams)
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync();
        var resource = await ctx.Project_Resources2.FirstOrDefaultAsync(r => r.ResourceId == resourceId);
        if (resource != null)
        {
            resource.SetInputParameters(inputParams);
            await ctx.TrackSaveChangesAsync(_datahubAuditingService);
            return true;
        }
        else
        {
            return false;
        }
    }

    private string GetResourceInputDefinitionIdentifier(string resourceType) =>
        $"{RESOURCE_REQUEST_INPUT_JSON_PREFIX}-{resourceType}";

    public async Task<string> GetResourceInputDefinitionJson(string resourceType)
    {
        var id = GetResourceInputDefinitionIdentifier(resourceType);
        var result = await _miscStorageService.GetObject<string>(id);
        return result;
    }

    public async Task SaveResourceInputDefinitionJson(string resourceType, string jsonContent)
    {
        var id = GetResourceInputDefinitionIdentifier(resourceType);
        await _miscStorageService.SaveObject(jsonContent, id);
    }

    private async Task NotifyProjectAdminsOfServiceRequest(Datahub_ProjectRequestAudit request)
    {
        var admins = await GetProjectAdministratorEmailsAndIds(request.Project.Project_ID);

        await _emailNotificationService.SendServiceCreationRequestNotification(request.UserEmail, request.RequestType,
            request.Project.ProjectInfo, admins);

        var adminUserIds = admins.Where(a => Guid.TryParse(a, out _))
            .ToList();
        var user = await _userInformationService.GetCurrentGraphUserAsync();

        await _systemNotificationService.CreateSystemNotificationsWithLink(adminUserIds, $"/administration",
            "SYSTEM-NOTIFICATION.GoToAdminPage",
            "SYSTEM-NOTIFICATION.NOTIFICATION-TEXT.ServiceCreationRequested",
            user.UserPrincipalName, request.RequestType,
            new BilingualStringArgument(request.Project.ProjectInfo.ProjectNameEn,
                request.Project.ProjectInfo.ProjectNameFr));
    }

    private async Task<List<string>> GetProjectAdministratorEmailsAndIds(int projectId)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();

        var project = await ctx.Projects
            .Include(p => p.Users)
                .ThenInclude(u => u.PortalUser)
            .FirstAsync(p => p.Project_ID == projectId);

        var adminEmails = ServiceAuthManager.ExtractEmails(project.Project_Admin ?? string.Empty);

        var adminUsers = project.Users
            .Where(u => u.RoleId is (int)Project_Role.RoleNames.Admin or (int)Project_Role.RoleNames.WorkspaceLead) 
            .Select(u => u.PortalUser.Email);

        return adminEmails
            .Concat(adminUsers)
            .ToList();
    }

    public async Task<Dictionary<string, string>> GetDefaultValues(string resourceType)
    {
        var formParams = await CreateResourceInputFormParams(resourceType);
        return GetDefaultValues(formParams);
    }

    public async Task RequestServiceWithDefaults(Datahub_ProjectRequestAudit request)
    {
        var defaultValues = await GetDefaultValues(request.RequestType);
        if (defaultValues?.Count > 0)
        {
            await RequestService(request, defaultValues);
        }
        else
        {
            await RequestService(request);
        }
    }


    public async Task HandleRequestService(Datahub_Project project, string serviceType)
    {
        var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
        var serviceRequest = new Datahub_ProjectRequestAudit
        {
            RequestType = serviceType,
            RequestedDateTime = DateTime.Now,
            Project = project,
            UserEmail = currentUser.Email
        };

        await RequestServiceWithDefaults(serviceRequest);
    }

    public static string GetTerraformServiceType(string templateName) => $"terraform:{templateName}";

    public async Task<bool> HandleTerraformRequestServiceAsync(Datahub_Project datahubProject, string terraformTemplate)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .Include(p => p.Users)
                .ThenInclude(u => u.PortalUser).Include(datahubProject => datahubProject.Resources)
                .FirstOrDefaultAsync(p => p.Project_ID == datahubProject.Project_ID);
            if(project == null)
            {
                return false;
            }

            var currentPortalUser = await _userInformationService.GetCurrentPortalUserAsync();

            var newTemplates = TerraformTemplate.LatestFromNameWithDependencies(terraformTemplate);

            if (terraformTemplate != TerraformTemplate.VariableUpdate)
            {
                foreach (var template in newTemplates)
                {
                    var serviceRequest = new Datahub_ProjectRequestAudit
                    {
                        RequestType = GetTerraformServiceType(template.Name),
                        RequestedDateTime = DateTime.Now,
                        Project = project,
                        UserEmail = currentPortalUser.Email
                    };

                    await RequestServiceWithDefaults(serviceRequest);
                }
            }

            var allTemplates = project.Resources
                .Select(r => r.ResourceType)
                .Select(s => s.Replace("terraform:", ""))
                .Select(TerraformTemplate.LatestFromNameWithDependencies)
                .SelectMany(t => t)
                .Concat(newTemplates)
                .DistinctBy(t => t.Name)
                .ToList();
            
            var workspaceDefinition = await _resourceMessagingService.GetWorkspaceDefinition(project.Project_Acronym_CD, currentPortalUser.Email);
            workspaceDefinition.Templates = allTemplates;
            
            await _resourceMessagingService.SendToTerraformQueue(workspaceDefinition);
            scope.Complete();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource {TerraformTemplate} for {DatahubProjectProjectAcronymCd}", terraformTemplate, datahubProject.Project_Acronym_CD);
            return false;
        }
    }

    public static Role GetTerraformUserRole(Datahub_Project_User projectUser)
    {
        return projectUser.RoleId switch
        {
            (int)Project_Role.RoleNames.Remove => Role.Removed,
            (int)Project_Role.RoleNames.WorkspaceLead => Role.Owner,
            (int)Project_Role.RoleNames.Admin => Role.Admin,
            (int)Project_Role.RoleNames.Collaborator => Role.User,
            (int)Project_Role.RoleNames.Guest => Role.Guest,
            _ => Role.Guest
        };
    }

    public static FieldValueContainer BuildFieldValues(FieldDefinitions fieldDefinitions,
        Dictionary<string, string> existingValues, string objectId = null)
    {
        if (string.IsNullOrEmpty(objectId))
        {
            objectId = Guid.NewGuid().ToString();
        }

        var vals = fieldDefinitions.Fields.Select(f =>
        {
            var val = new ObjectFieldValue
            {
                FieldDefinition = f,
                FieldDefinitionId = f.FieldDefinitionId
            };

            if (existingValues.ContainsKey(f.Field_Name_TXT))
            {
                val.Value_TXT = existingValues[f.Field_Name_TXT];
            }

            return val;
        }).ToList();

        return new(objectId, fieldDefinitions, vals);
    }

    private static FieldDefinitions BuildFieldDefsFromDynamic(dynamic fields)
    {
        var defs = new FieldDefinitions();
        var nextId = 1;

        foreach (dynamic field in fields)
        {
            var def = new FieldDefinition
            {
                Field_Name_TXT = field.field_name,
                Name_English_TXT = field.name_en,
                Name_French_TXT = field.name_fr,
                Required_FLAG = field.required ?? false,
                Default_Value_TXT = field.default_value,
                FieldDefinitionId = nextId++
            };

            if (field.choices != null)
            {
                foreach (dynamic choice in field.choices)
                {
                    var fc = new FieldChoice
                    {
                        FieldChoiceId = nextId++,
                        Value_TXT = choice.value,
                        Label_English_TXT = choice.label_en,
                        Label_French_TXT = choice.label_fr,
                        FieldDefinition = def,
                        FieldDefinitionId = def.FieldDefinitionId
                    };

                    def.Choices.Add(fc);
                }
            }

            defs.Add(def);
        }

        return defs;
    }

    private static MetadataProfile BuildProfileFromDefsAndDynamic(FieldDefinitions defs, dynamic profileDyn)
    {
        var profileSections = new List<MetadataSection>();
        var nextId = 1;

        var profile = new MetadataProfile
        {
            Name = profileDyn.name,
            Sections = profileSections,
            ProfileId = nextId++
        };

        foreach (var secDyn in profileDyn.sections)
        {
            var section = new MetadataSection
            {
                SectionId = nextId++,
                Name_English_TXT = secDyn.label_en,
                Name_French_TXT = secDyn.label_fr,
                Fields = new List<SectionField>(),
                Profile = profile,
                ProfileId = profile.ProfileId
            };

            foreach (string fieldName in secDyn.fields)
            {
                var fd = defs.Get(fieldName);
                var sf = new SectionField
                {
                    FieldDefinition = fd,
                    FieldDefinitionId = fd.FieldDefinitionId,
                    Section = section,
                    SectionId = section.SectionId
                };
                section.Fields.Add(sf);
            }

            profileSections.Add(section);
        }


        return profile;
    }

    public async Task<ProjectResourceFormParams> CreateResourceInputFormParams(string resourceType)
    {
        var resourceJson = await GetResourceInputDefinitionJson(resourceType);
        var paramDef = BuildFormParamsFromJson(resourceJson);
        return await Task.FromResult(paramDef);
    }

    private static ProjectResourceFormParams BuildFormParamsFromJson(string inputJson)
    {
        if (string.IsNullOrEmpty(inputJson))
        {
            return default;
        }

        var anonObject = JsonConvert.DeserializeObject<dynamic>(inputJson);

        var defs = BuildFieldDefsFromDynamic(anonObject.fields);
        var profile = BuildProfileFromDefsAndDynamic(defs, anonObject.profile);

        return new(defs, profile);
    }

    public static Dictionary<string, string> GetDefaultValues(ProjectResourceFormParams formParams)
    {
        return formParams?.FieldDefinitions.Fields
            .Where(f => !string.IsNullOrEmpty(f.Default_Value_TXT))
            .ToDictionary(f => f.Field_Name_TXT, f => f.Default_Value_TXT);
    }
}