using Datahub.Core.Model.Projects;
using Datahub.Core.Utils;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;

namespace Datahub.Portal.Components
{
	public partial class EnvironmentVariablesTable
	{
		private bool FilterFunc((string Key, string Value) item)
		{
			if (string.IsNullOrWhiteSpace(_filterString))
				return true;
			if (item.Key.Contains(_filterString, StringComparison.OrdinalIgnoreCase))
				return true;
			if (item.Value.Contains(_filterString, StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		private async Task<List<(string Key, string Value)>> GetEnvironmentVariables()
		{
			var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
			var keys = TerraformVariableExtraction.ExtractEnvironmentVariableKeys(resource);
			var ctx = await _dbContextFactory.CreateDbContextAsync();
			var projectUser = ctx.Project_Users.Include(datahubProjectUser => datahubProjectUser.Role)
				.FirstOrDefault(u => u.PortalUser.Id == currentUser.Id);

			if (projectUser == null)
			{
				throw new Exception("Could not find project user");
			}

			var role = projectUser.Role;

			if (role == null)
			{
				throw new Exception("Could not find role");
			}

			foreach (var key in keys)
			{
				var value = await GetEnvironmentVariable(key, role);
				var envVar = (key, value);
				envVars.Add(envVar);
			}

			return envVars;
		}

		private async Task<string> GetEnvironmentVariable(string key, Project_Role role)
		{
			var value = role.IsAtLeastAdmin ? await _keyVaultUserService.GetSecretAsync(projectAcronym, key) : "";
			return value;
		}

		private void HandleCommitEditClicked(MouseEventArgs args)
		{
			_logger.LogInformation("Commit edit button clicked.");
			_snackbar.Add(Localizer["Environment variable updated"], Severity.Info);
		}

		private void HandleRowEditCommit(object element)
		{
			var item = element as (string Key, string Value)?;
			if (item is not null)
			{
				CreateOrUpdateEnvironmentVariable(item!.Value);
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
				await CreateOrUpdateEnvironmentVariable(new(newKey, newValue));
				_snackbar.Add(Localizer["Environment variable {0} has been added.", newKey], Severity.Success);
			}
			else
			{
				_snackbar.Add(Localizer["Error adding environment variable. Key and value must not be empty."],
					Severity.Error);
			}
		}

		private async Task DeleteEnvironmentVariable((string Key, string Value) item, bool showSnackbar)
		{
			if (envVars.Remove(item))
				_snackbar.Add(Localizer["Environment variable {0} has been deleted.", item.Key], Severity.Success);
			else
				_snackbar.Add(Localizer["Error deleting environment variable {0}.", item.Key], Severity.Error);
		}

		private async Task CreateOrUpdateEnvironmentVariable((string Key, string Value) item)
		{
			envVars.Add(item);
		}

		private void BackupItem(object item)
		{
			var e = item as (string Key, string Value)?;
			if (e is not null)
			{
				_elementBeforeEdit = new()
				{
					Key = e?.Key,
					Value = e?.Value
				};

				_logger.LogInformation($"Item has been backed up: {_elementBeforeEdit.Key}");
			}
			else
			{
				_logger.LogInformation("Unable to backup item.");
			}
		}

		private void HandleRowEditCancel(object element)
		{
			var item = element as (string Key, string Value)?;
			if (item is not null)
			{
				item = new(_elementBeforeEdit.Key, _elementBeforeEdit.Value);
				_logger.LogInformation($"Item has been reset to original values: {item?.Key}");
			}
		}

		public Dictionary<string, string> GetEnvironmentVariablesDictionary()
		{
			return envVars.ToDictionary(x => x.Key, x => x.Value);
		}

		public string GetHiddenValue(string value)
		{
			return new string('*', value.Length);
		}

	}
}