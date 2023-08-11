using ResourceProvisioner.Application.Config;

namespace ResourceProvisioner.Infrastructure.Common;

public static class DirectoryUtils
{
    public static void VerifyDirectoryDoesNotExist(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var dir = new DirectoryInfo(path);
        SetAttributesNormal(dir);

        try
        {
            dir.Delete(true);
        }
        catch (IOException)
        {
            // wait for one second and try again
            Thread.Sleep(1000);
            dir.Delete(true);
        }
    }

    private static void SetAttributesNormal(DirectoryInfo dir)
    {
        foreach (var subDir in dir.GetDirectories())
            SetAttributesNormal(subDir);
        foreach (var file in dir.GetFiles())
        {
            file.Attributes = FileAttributes.Normal;
        }
    }


    public static string GetInfrastructureRepositoryPath(
        ResourceProvisionerConfiguration resourceProvisionerConfiguration)
    {
        return Path.Join(Environment.CurrentDirectory,
            resourceProvisionerConfiguration.InfrastructureRepository.LocalPath);
    }

    public static string GetModuleRepositoryPath(ResourceProvisionerConfiguration resourceProvisionerConfiguration)
    {
        return Path.Join(Environment.CurrentDirectory,
            resourceProvisionerConfiguration.ModuleRepository.LocalPath);
    }

    public static string GetTemplatePath(ResourceProvisionerConfiguration resourceProvisionerConfiguration,
        string? templateName)
    {
        if (templateName == null)
            throw new ArgumentNullException(nameof(templateName));

        return Path.Join(GetModuleRepositoryPath(resourceProvisionerConfiguration),
            resourceProvisionerConfiguration.ModuleRepository.TemplatePathPrefix, templateName);
    }

    public static string GetProjectPath(ResourceProvisionerConfiguration resourceProvisionerConfiguration,
        string? workspaceAcronym)
    {
        if (workspaceAcronym == null)
            throw new ArgumentNullException(nameof(workspaceAcronym));

        return Path.Join(GetInfrastructureRepositoryPath(resourceProvisionerConfiguration),
            resourceProvisionerConfiguration.InfrastructureRepository.ProjectPathPrefix, workspaceAcronym);
    }
}