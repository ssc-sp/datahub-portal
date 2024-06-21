using Datahub.Core.Model.Projects;
using Datahub.Core.Utils;
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
            _snackbar.Add(Localizer["Environment variable updated"], Severity.Info);
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
                Localizer["Enter the key of your new environment variable:"].ToString());
            var newValue = await _jsRuntime.InvokeAsync<string>("prompt",
                Localizer["Enter the value of your new environment variable:"].ToString());

            if (!string.IsNullOrWhiteSpace(newKey) && !string.IsNullOrWhiteSpace(newValue))
            {
                KeyValuePair newKVP = new() { Key = newKey, Value = newValue };
                CreateOrUpdateEnvironmentVariable(newKVP);
                _snackbar.Add(Localizer["Environment variable {0} has been added.", newKey], Severity.Success);
            }
            else
            {
                _snackbar.Add(Localizer["Error adding environment variable. Key and value must not be empty."],
                    Severity.Error);
            }
        }

        private void CreateOrUpdateEnvironmentVariable(KeyValuePair item)
        {
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

        public Dictionary<string, string> GetEnvironmentVariablesDictionary()
        {
            return envVars.ToDictionary(x => x.Key, x => x.Value);
        }

        public string GetHiddenValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            return new string('*', value.Length);
        }
    }
}