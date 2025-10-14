using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration;

public interface IWorkspaceToolConfiguration
{
    /// <summary>
    /// Creates a new copy of the current <see cref="IWorkspaceToolConfiguration"/> instance.
    /// </summary>
    /// <returns>A new <see cref="IWorkspaceToolConfiguration"/> instance that is a duplicate of the current instance.</returns>
    IWorkspaceToolConfiguration Clone();

    /// <summary>
    /// Writes the current state of the object to the specified workspace definition.
    /// </summary>
    /// <param name="workspaceDefinition">The workspace definition to which the state will be written. Cannot be <see langword="null"/>.</param>
    void WriteToWorkspaceDefinition(WorkspaceDefinition workspaceDefinition);

    /// <summary>
    /// Retrieves the workspace tool configuration based on the specified workspace definition, or default configuration if none exists.
    /// </summary>
    /// <param name="workspaceDefinition">The definition of the workspace used to determine the tool configuration.</param>
    /// <returns>An instance of <see cref="IWorkspaceToolConfiguration"/> representing the configuration for the specified
    /// workspace.</returns>
    static abstract IWorkspaceToolConfiguration ReadFromWorkspaceDefinition(WorkspaceDefinition workspaceDefinition);

    /// <summary>
    /// Retrieves the display label associated with the specified property name.
    /// Each display label should have corresponding entries in localization files.
    /// </summary>
    /// <param name="propertyName">The name of the property for which to retrieve the label. Cannot be null or empty.</param>
    /// <returns>The display label for the specified property name.</returns>
    static abstract string GetPropertyLabel(string propertyName);

    /// <summary>
    /// Generates a JSON string representing the input data for a resource based on the current tool configuration.
    /// </summary>
    /// <returns>A JSON-formatted string containing the resource input data.</returns>
    string GenerateResourceInputJson();
}
