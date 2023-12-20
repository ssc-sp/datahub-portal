using Datahub.Core.Model.Projects;
using Datahub.Stories.Utils;
using Microsoft.AspNetCore.Components;

namespace Datahub.Stories.Stories.Workspace;

/// <inheritdoc />
public abstract class WorkspaceBaseStory : ComponentBase
{
        
    /// <summary>
    /// The project to display
    /// </summary>
    protected Datahub_Project _project = null!;
    
    /// <summary>
    /// Placeholder service to get random projects
    /// </summary>
    [Inject] protected PlaceholderService PlaceholderService { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _project = await PlaceholderService.GetRandomProjectAsync();
    }
}