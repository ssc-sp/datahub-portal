using System.Text.Json;
using Datahub.Core.Model.Projects;
using Datahub.Core.Utils;
using Datahub.Shared.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Severity = MudBlazor.Severity;

namespace Datahub.Portal.Components
{
    public class KeyValuePair
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class KeyValuePairValidator : AbstractValidator<KeyValuePair>
    {
        public KeyValuePairValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty()
                .WithMessage("Key cannot be empty");
            RuleFor(x => x.Key)
                .MaximumLength(200)
                .WithMessage("Key cannot be longer than 200 characters");
            RuleFor(x => x.Key)
                .Matches(@"^[a-zA-Z_]+$").WithMessage("Key can only contain letters and underscores");

            RuleFor(x => x.Value)
                .NotEmpty()
                .WithMessage("Value cannot be empty");
            RuleFor(x => x.Value)
                .MaximumLength(200)
                .WithMessage("Value cannot be longer than 200 characters");
        }
    }

    public partial class EnvironmentVariablesTable
    {
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
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error getting environment variable {key} from KeyVault.");
                    _snackbar.Add(Localizer["Error getting environment variable {0} from KeyVault.", key],
                        Severity.Error);
                }
            }

            return envVars;
        }

        private async Task<string> GetEnvironmentVariable(string key, Project_Role role)
        {
            var secretName = ToKeyVaultName(key);
            var value = role.IsAtLeastAdmin
                ? (await keyVaultUserService.GetSecretAsync(projectAcronym, secretName)) ?? string.Empty
                : string.Empty;
            return value;
        }

        private string ToKeyVaultName(string key)
        {
            return key.ToLower().Replace("_", "-");
        }

        private void ToggleValues()
        {
            _show = !_show;
        }

        private void StartEditingEnvironmentVariable(string? key)
        {
            _selectedEnvironmentVariableKey = key;
            var environmentVariable = envVars.FirstOrDefault(variable => variable.Key == key);
            if (environmentVariable is null)
            {
                return;
            }

            _isAdding = false;
            _editorVisible = true;
            _editorKey = environmentVariable.Key;
            _editorValue = environmentVariable.Value;
            ClearEditorErrors();
        }

        private void StartAddingEnvironmentVariable()
        {
            _selectedEnvironmentVariableKey = null;
            _isAdding = true;
            _editorVisible = true;
            _editorKey = null;
            _editorValue = null;
            ClearEditorErrors();
        }

        private void CancelEditingEnvironmentVariable()
        {
            _selectedEnvironmentVariableKey = null;
            _editorVisible = false;
            _isAdding = false;
            _editorKey = null;
            _editorValue = null;
            ClearEditorErrors();
        }

        private async Task SaveEnvironmentVariableAsync()
        {
            var item = new KeyValuePair
            {
                Key = (_editorKey ?? string.Empty).Trim(),
                Value = _editorValue ?? string.Empty
            };

            if (_isAdding)
            {
                item.Key = item.Key.ToUpperInvariant();
            }

            var validationResult = new KeyValuePairValidator().Validate(item);
            _keyError = validationResult.Errors
                .Where(error => error.PropertyName == nameof(KeyValuePair.Key))
                .Select(error => Localizer[error.ErrorMessage].ToString())
                .FirstOrDefault();
            _valueError = validationResult.Errors
                .Where(error => error.PropertyName == nameof(KeyValuePair.Value))
                .Select(error => Localizer[error.ErrorMessage].ToString())
                .FirstOrDefault();

            if (!validationResult.IsValid)
            {
                return;
            }

            _isSaving = true;
            try
            {
                if (!await CreateOrUpdateEnvironmentVariable(item))
                {
                    _snackbar.Add(Localizer["Error updating environment variable."], Severity.Error);
                    return;
                }

                _snackbar.Add(
                    _isAdding
                        ? Localizer["Environment variable {0} has been added.", item.Key]
                        : Localizer["Environment variable has been updated."],
                    Severity.Success);
                CancelEditingEnvironmentVariable();
            }
            finally
            {
                _isSaving = false;
            }
        }

        private void ClearEditorErrors()
        {
            _keyError = null;
            _valueError = null;
        }

        private async Task<bool> CreateOrUpdateEnvironmentVariable(KeyValuePair item)
        {
            try
            {
                if (!await SyncEnvironmentVariables(item))
                {
                    return false;
                }

                var existingItem = envVars.FirstOrDefault(x => x.Key == item.Key);
                if (existingItem is not null)
                {
                    existingItem.Value = item.Value;
                }
                else
                {
                    envVars.Add(item);
                }

                needsRestart = true;
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating or updating environment variable.");
                return false;
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

        private async Task<bool> SyncEnvironmentVariables(KeyValuePair kvp)
        {
            try
            {
                _logger.LogInformation("Syncing KeyVault with new environment variables.");
                await UpdateKeyVault(kvp);
                _logger.LogInformation("KeyVault updated successfully. Now syncing local environment variables.");
                return await UpdateLocal(kvp);
            }
            catch (Exception e)
            {
                _logger.LogError("Error syncing environment variables." + e.Message);
                _snackbar.Add(Localizer["Error syncing environment variables."], Severity.Error);
                return false;
            }
        }

        private async Task UpdateKeyVault(KeyValuePair kvp)
        {
            _logger.LogInformation($"Storing or updating secret {kvp.Key} in KeyVault.");
            await keyVaultUserService.StoreOrUpdateSecret(projectAcronym, ToKeyVaultName(kvp.Key), kvp.Value);
            _logger.LogInformation($"Secret {kvp.Key} stored or updated successfully.");
        }

        private async Task<bool> UpdateLocal(KeyValuePair kvp)
        {
            _logger.LogInformation($"Updating local environment variables.");
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
            if (project is null)
            {
                _logger.LogError($"Project {projectAcronym} not found in database.");
                _snackbar.Add(Localizer["Error updating local environment variables."], Severity.Error);
                return false;
            }

            var webAppResourceType = TerraformTemplate.AzureAppService;
            var webAppResource = ctx.Project_Resources2.FirstOrDefault(r =>
                r.ProjectId == project.Project_ID &&
                r.ResourceType == TerraformTemplate.GetTerraformServiceType(webAppResourceType));
            if (webAppResource is null)
            {
                _logger.LogError($"WebApp resource not found in database.");
                _snackbar.Add(Localizer["Error updating local environment variables."], Severity.Error);
                return false;
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
            return true;
        }
    }
}
