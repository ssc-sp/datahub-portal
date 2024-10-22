using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;
using Datahub.Application.Services;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using Datahub.Core.Model.Context;

namespace Datahub.Infrastructure.Services;

public class RequestManagementService : IRequestManagementService
{
    private readonly ILogger<RequestManagementService> _logger;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly IDatahubAuditingService _datahubAuditingService;
    private readonly IResourceMessagingService _resourceMessagingService;

    public RequestManagementService(
        ILogger<RequestManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IDatahubAuditingService datahubAuditingService,
        IResourceMessagingService resourceMessagingService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _datahubAuditingService = datahubAuditingService;
        _resourceMessagingService = resourceMessagingService;
    }


    public async Task HandleUserUpdatesToExternalPermissions(Datahub_Project project, PortalUser currentPortalUser)
    {
        var workspaceDefinition =
            await _resourceMessagingService.GetWorkspaceDefinition(project.Project_Acronym_CD, currentPortalUser.Email);
        await _resourceMessagingService.SendToUserQueue(workspaceDefinition);
    }

    /// <summary>
    /// Processes a request to create a project resource.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="requestingUser">The requesting user.</param>
    /// <param name="requestedTemplate">The requested template.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ProcessRequest(Datahub_Project project, PortalUser requestingUser,
        TerraformTemplate requestedTemplate)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        ctx.Projects.Attach(project);

        await ctx.Entry(project)
            .Collection(p => p.Resources)
            .LoadAsync();

        var resource = project.Resources
            .FirstOrDefault(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(requestedTemplate.Name));

        // TODO: Add delete logic in here
        
        
        if (resource is not null)
        {
            _logger.LogInformation(
                "Project resource already exists for project {Acronym} and resource type {ServiceType}",
                project.Project_Acronym_CD, requestedTemplate.Name);
            return;
        }

        resource = new Project_Resources2
        {
            ProjectId = project.Project_ID,
            RequestedById = requestingUser.Id,
            ResourceType = TerraformTemplate.GetTerraformServiceType(requestedTemplate.Name),
            Status = requestedTemplate.Status,
        };
        
        await ctx.Project_Resources2.AddAsync(resource);
        await ctx.TrackSaveChangesAsync(_datahubAuditingService);
    }

    /// <summary>
    /// Handles a Terraform request asynchronously.
    /// </summary>
    /// <param name="datahubProject">The Datahub project.</param>
    /// <param name="terraformTemplate">The Terraform template.</param>
    /// <param name="requestingUser">The user making the request.</param>
    /// <returns>True if the Terraform request was handled successfully; otherwise, false.</returns>
    public async Task<bool> HandleTerraformRequestServiceAsync(Datahub_Project datahubProject, TerraformTemplate terraformTemplate,
        PortalUser requestingUser)
    {
        try
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .Include(p => p.Resources)
                .Include(p => p.Users)
                .ThenInclude(u => u.PortalUser)
                .FirstOrDefaultAsync(p => p.Project_ID == datahubProject.Project_ID);

            if (project == null)
            {
                return false;
            }
            
            
            var dependencyTemplates = TerraformTemplate.GetDependenciesToCreate(terraformTemplate.Name);
            if (terraformTemplate.Name != TerraformTemplate.VariableUpdate)
            {
                await ProcessRequest(project, requestingUser, terraformTemplate);
                foreach (var template in dependencyTemplates)
                {
                    await ProcessRequest(project, requestingUser, template);
                }
            }

            var workspaceDefinition =
                await _resourceMessagingService.GetWorkspaceDefinition(project.Project_Acronym_CD,
                    requestingUser.Email);

            await _resourceMessagingService.SendToTerraformQueue(workspaceDefinition);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource {TerraformTemplate} for {DatahubProjectProjectAcronymCd}",
                terraformTemplate, datahubProject.Project_Acronym_CD);
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
    
    

    // public static FieldValueContainer BuildFieldValues(FieldDefinitions fieldDefinitions,
    //     Dictionary<string, string> existingValues, string objectId = null)
    // {
    //     if (string.IsNullOrEmpty(objectId))
    //     {
    //         objectId = Guid.NewGuid().ToString();
    //     }
    //
    //     var vals = fieldDefinitions.Fields.Select(f =>
    //     {
    //         var val = new ObjectFieldValue
    //         {
    //             FieldDefinition = f,
    //             FieldDefinitionId = f.FieldDefinitionId
    //         };
    //
    //         if (existingValues.ContainsKey(f.Field_Name_TXT))
    //         {
    //             val.Value_TXT = existingValues[f.Field_Name_TXT];
    //         }
    //
    //         return val;
    //     }).ToList();
    //
    //     return new(objectId, fieldDefinitions, vals);
    // }
    //
    // private static FieldDefinitions BuildFieldDefsFromDynamic(dynamic fields)
    // {
    //     var defs = new FieldDefinitions();
    //     var nextId = 1;
    //
    //     foreach (dynamic field in fields)
    //     {
    //         var def = new FieldDefinition
    //         {
    //             Field_Name_TXT = field.field_name,
    //             Name_English_TXT = field.name_en,
    //             Name_French_TXT = field.name_fr,
    //             Required_FLAG = field.required ?? false,
    //             Default_Value_TXT = field.default_value,
    //             FieldDefinitionId = nextId++
    //         };
    //
    //         if (field.choices != null)
    //         {
    //             foreach (dynamic choice in field.choices)
    //             {
    //                 var fc = new FieldChoice
    //                 {
    //                     FieldChoiceId = nextId++,
    //                     Value_TXT = choice.value,
    //                     Label_English_TXT = choice.label_en,
    //                     Label_French_TXT = choice.label_fr,
    //                     FieldDefinition = def,
    //                     FieldDefinitionId = def.FieldDefinitionId
    //                 };
    //
    //                 def.Choices.Add(fc);
    //             }
    //         }
    //
    //         defs.Add(def);
    //     }
    //
    //     return defs;
    // }
    //
    // private static MetadataProfile BuildProfileFromDefsAndDynamic(FieldDefinitions defs, dynamic profileDyn)
    // {
    //     var profileSections = new List<MetadataSection>();
    //     var nextId = 1;
    //
    //     var profile = new MetadataProfile
    //     {
    //         Name = profileDyn.name,
    //         Sections = profileSections,
    //         ProfileId = nextId++
    //     };
    //
    //     foreach (var secDyn in profileDyn.sections)
    //     {
    //         var section = new MetadataSection
    //         {
    //             SectionId = nextId++,
    //             Name_English_TXT = secDyn.label_en,
    //             Name_French_TXT = secDyn.label_fr,
    //             Fields = new List<SectionField>(),
    //             Profile = profile,
    //             ProfileId = profile.ProfileId
    //         };
    //
    //         foreach (string fieldName in secDyn.fields)
    //         {
    //             var fd = defs.Get(fieldName);
    //             var sf = new SectionField
    //             {
    //                 FieldDefinition = fd,
    //                 FieldDefinitionId = fd.FieldDefinitionId,
    //                 Section = section,
    //                 SectionId = section.SectionId
    //             };
    //             section.Fields.Add(sf);
    //         }
    //
    //         profileSections.Add(section);
    //     }
    //
    //
    //     return profile;
    // }
    //
    //
    // private static ProjectResourceFormParams BuildFormParamsFromJson(string inputJson)
    // {
    //     if (string.IsNullOrEmpty(inputJson))
    //     {
    //         return default;
    //     }
    //
    //     var anonObject = JsonConvert.DeserializeObject<dynamic>(inputJson);
    //
    //     var defs = BuildFieldDefsFromDynamic(anonObject.fields);
    //     var profile = BuildProfileFromDefsAndDynamic(defs, anonObject.profile);
    //
    //     return new(defs, profile);
    // }
}