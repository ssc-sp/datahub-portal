using Microsoft.Extensions.Configuration;

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
        dir.Delete(true);
    }

    public static void SetAttributesNormal(DirectoryInfo dir)
    {
        foreach (var subDir in dir.GetDirectories())
            SetAttributesNormal(subDir);
        foreach (var file in dir.GetFiles())
        {
            file.Attributes = FileAttributes.Normal;
        }
    }


    public static string GetInfrastructureRepositoryPath(IConfiguration configuration)
    {
        return Path.Join(Environment.CurrentDirectory,
            configuration["InfrastructureRepository:LocalPath"]);
    }

    public static string GetModuleRepositoryPath(IConfiguration configuration)
    {
        return Path.Join(Environment.CurrentDirectory,
            configuration["ModuleRepository:LocalPath"]);
    }

    public static string GetTemplatePath(IConfiguration configuration, string templateName)
    {
        return Path.Join(GetModuleRepositoryPath(configuration), configuration["ModuleRepository:TemplatePathPrefix"], templateName);
    }

    public static string GetProjectPath(IConfiguration configuration, string workspaceAcronym)
    {
        return Path.Join(GetInfrastructureRepositoryPath(configuration),
            configuration["InfrastructureRepository:ProjectPathPrefix"], workspaceAcronym);
    }
}