using System.Text.Json;
using Datahub.Core.Model.Projects;
using Datahub.Core.Utils;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;

namespace Datahub.Portal.Components
{
    public class KeyValuePair
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public partial class EnvironmentVariablesTable
    {
        private bool FilterFunc(KeyValuePair item)
        {
            if (string.IsNullOrWhiteSpace(_filterString))
                return true;
            if (item.Key.Contains(_filterString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (item.Value.Contains(_filterString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private async Task<List<KeyValuePair>> GetEnvironmentVariables()
        {
            var keys = TerraformVariableExtraction.ExtractEnvironmentVariableKeys(resource);

            if (projectUser == null)
            {
                throw new Exception("Could not find project user");
            }

            var role = projectUser.Role;

            if (role == null)
            {
                throw new Exception("Could not find role");
            }

            envVars = new();

            foreach (var key in keys)
            {
                try
                {
                    var value = await GetEnvironmentVariable(key, role);
                    KeyValuePair envVar = new() { Key = key, Value = value };
                    envVars.Add(envVar);
                }
                catch (KeyVaultErrorException e)
                {
                    _logger.LogError(e, $"Error getting environment variable {key} from KeyVault.");
                    _snackbar.Add(Localizer["Error getting environment variable \"{0}\" from KeyVault.", key],
                        Severity.Error);
                }
            }

            return envVars;
        }

        private async Task<string> GetEnvironmentVariable(string key, Project_Role role)
        {
            var value = role.IsAtLeastAdmin
                ? (await keyVaultUserService.GetSecretAsync(projectAcronym, key)) ?? string.Empty
                : string.Empty;
            return value;
        }

        private void HandleCommitEditClicked(MouseEventArgs args)
        {
            _logger.LogInformation("Commit edit button clicked.");
        }

        private void HandleRowEditCommit(object element)
        {
            var item = element as KeyValuePair;
            if (item is not null)
            {
                CreateOrUpdateEnvironmentVariable(item);
                _snackbar.Add(Localizer["Environment variable has been updated."], Severity.Success);
                _logger.LogInformation($"Item has been committed: {item?.Key}");
            }
            else
            {
                _snackbar.Add(Localizer["Error updating environment variable."], Severity.Error);
                _logger.LogError("Error updating environment variable.");
            }
        }

        private async Task AddNewEnvironmentVariable()
        {
            var newKey = await _jsRuntime.InvokeAsync<string>("prompt",
                Localizer[
                        "Enter the key of your new environment variable. Environment variable keys cannot be changed and environment variables cannot be deleted."]
                    .ToString());
            var newValue = await _jsRuntime.InvokeAsync<string>("prompt",
                Localizer["Enter the value of your new environment variable. You may edit this value later."]
                    .ToString());

            if (!string.IsNullOrWhiteSpace(newKey) && !string.IsNullOrWhiteSpace(newValue))
            {
                KeyValuePair newKVP = new() { Key = newKey, Value = newValue };
                await CreateOrUpdateEnvironmentVariable(newKVP);
                _snackbar.Add(Localizer["Environment variable {0} has been added.", newKey], Severity.Success);
            }
            else
            {
                _snackbar.Add(Localizer["Error adding environment variable. Key and value must not be empty."],
                    Severity.Error);
            }
        }

        private async Task CreateOrUpdateEnvironmentVariable(KeyValuePair item)
        {
            try
            {
                await SyncEnvironmentVariables(item);
                var existingItem = envVars.FirstOrDefault(x => x.Key == item.Key);
                if (existingItem is not null)
                {
                    existingItem.Value = item.Value;
                }
                else
                {
                    envVars.Add(item);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating or updating environment variable.");
            }
        }

        private void BackupItem(object item)
        {
            var element = item as KeyValuePair;
            _elementBeforeEdit = new()
            {
                Key = element.Key,
                Value = element.Value
            };
        }

        private void HandleRowEditCancel(object element)
        {
            var item = element as KeyValuePair;
            if (item is not null)
            {
                item = new KeyValuePair
                {
                    Key = _elementBeforeEdit.Key,
                    Value = _elementBeforeEdit.Value
                };

                _logger.LogInformation($"Item has been reset to original values: {item.Key}");
            }
        }

        public string GetHiddenValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return new string('*', value.Length);
        }

        private async Task SyncEnvironmentVariables(KeyValuePair kvp)
        {
            try
            {
                _logger.LogInformation("Syncing KeyVault with new environment variables.");
                await UpdateKeyVault(kvp);
                _logger.LogInformation("KeyVault updated successfully. Now syncing local environment variables.");
                await UpdateLocal(kvp);
            }
            catch (Exception e)
            {
                _logger.LogError("Error syncing environment variables." + e.Message);
                _snackbar.Add(Localizer["Error syncing environment variables."], Severity.Error);
            }
        }

        private async Task UpdateKeyVault(KeyValuePair kvp)
        {
            _logger.LogInformation($"Storing or updating secret {kvp.Key} in KeyVault.");
            await keyVaultUserService.StoreOrUpdateSecret(projectAcronym, kvp.Key, kvp.Value);
            _logger.LogInformation($"Secret {kvp.Key} stored or updated successfully.");
        }

        private async Task UpdateLocal(KeyValuePair kvp)
        {
            _logger.LogInformation($"Updating local environment variables.");
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
            if (project is null)
            {
                _logger.LogError($"Project {projectAcronym} not found in database.");
                _snackbar.Add(Localizer["Error updating local environment variables."], Severity.Error);
                return;
            }

            var webAppResourceType = TerraformTemplate.AzureAppService;
            var webAppResource = ctx.Project_Resources2.FirstOrDefault(r =>
                r.ProjectId == project.Project_ID &&
                r.ResourceType == TerraformTemplate.GetTerraformServiceType(webAppResourceType));
            if (webAppResource is null)
            {
                _logger.LogError($"WebApp resource not found in database.");
                _snackbar.Add(Localizer["Error updating local environment variables."], Severity.Error);
                return;
            }

            var currentEnvVarKeys = TerraformVariableExtraction.ExtractEnvironmentVariableKeys(webAppResource);
            if (!currentEnvVarKeys.Contains(kvp.Key))
            {
                currentEnvVarKeys.Add(kvp.Key);
            }

            var newEnvVarKeys = JsonSerializer.Serialize(currentEnvVarKeys);
            var inputJson = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(webAppResource.InputJsonContent);
            inputJson["environment_variables_keys"] = newEnvVarKeys;
            webAppResource.InputJsonContent = JsonSerializer.Serialize(inputJson);
            await ctx.SaveChangesAsync();

            _logger.LogInformation($"Local environment variables updated successfully.");
        }
    }
}