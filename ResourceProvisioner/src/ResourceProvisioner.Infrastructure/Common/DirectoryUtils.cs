using Polly;
using Polly.Retry;
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
        RetryDelete(dir);
    }

    private static void RetryDelete(DirectoryInfo dir)
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions {MaxRetryAttempts = 5, Delay = TimeSpan.FromSeconds(1)})
            .AddTimeout(TimeSpan.FromSeconds(20))
            .Build();
        pipeline.Execute(() => dir.Delete(true));
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

    public static string GetTempDirectoryPath(ResourceProvisionerConfiguration resourceProvisionerConfiguration,
        string tempDirectory)
    {
        return Path.Join(Environment.CurrentDirectory,
            resourceProvisionerConfiguration.InfrastructureRepository.LocalPath,
            tempDirectory);
    }

    public static string GetInfrastructureRepositoryPath(
        ResourceProvisionerConfiguration resourceProvisionerConfiguration, string tempDirectory)
    {
        return Path.Join(
            Environment.CurrentDirectory, // i.e. /home/site/wwwroot
            resourceProvisionerConfiguration.InfrastructureRepository.LocalPath, // i.e. /../tmp
            tempDirectory, // i.e. caf2aaaf-a3d6-4518-ad8b-06c78fa8dc40 
            resourceProvisionerConfiguration.InfrastructureRepository.Name // i.e. datahub-project-infrastructure-poc
        ); // i.e. /home/site/wwwroot/../tmp/caf2aaaf-a3d6-4518-ad8b-06c78fa8dc40/datahub-project-infrastructure-poc
    }

    public static string GetModuleRepositoryPath(ResourceProvisionerConfiguration resourceProvisionerConfiguration,
        string tempDirectory)
    {
        return Path.Join(
            Environment.CurrentDirectory, // i.e. /home/site/wwwroot
            resourceProvisionerConfiguration.ModuleRepository.LocalPath, // i.e. /../tmp
            tempDirectory, // i.e. caf2aaaf-a3d6-4518-ad8b-06c78fa8dc40
            resourceProvisionerConfiguration.ModuleRepository.Name // i.e. datahub-resource-modules
        ); // i.e. /home/site/wwwroot/../tmp/caf2aaaf-a3d6-4518-ad8b-06c78fa8dc40/datahub-resource-modules
    }

    public static string GetTemplatePath(ResourceProvisionerConfiguration resourceProvisionerConfiguration,
        string? templateName, string tempDirectory)
    {
        if (templateName == null)
            throw new ArgumentNullException(nameof(templateName));

        return Path.Join(GetModuleRepositoryPath(resourceProvisionerConfiguration, tempDirectory),
            resourceProvisionerConfiguration.ModuleRepository.TemplatePathPrefix, templateName);
    }

    public static string GetProjectPath(ResourceProvisionerConfiguration resourceProvisionerConfiguration,
        string? workspaceAcronym, string tempDirectory)
    {
        if (workspaceAcronym == null)
            throw new ArgumentNullException(nameof(workspaceAcronym));

        return Path.Join(GetInfrastructureRepositoryPath(resourceProvisionerConfiguration, tempDirectory),
            resourceProvisionerConfiguration.InfrastructureRepository.ProjectPathPrefix, workspaceAcronym);
    }
}