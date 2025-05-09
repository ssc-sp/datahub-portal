using Datahub.Core.Model.Projects;

namespace Datahub.Portal.Pages.Workspace.Settings
{
    public class WorkspaceBudgetManagementItem(Datahub_Project workspace)
    {
        private decimal _budget = workspace.Project_Budget ?? 0;
        public Datahub_Project Workspace => workspace;
        public decimal Budget
        {
            get => _budget;
            set 
            { 
                _budget = value;
                RequiresTerraformUpdate = HasChanged;
            }
        }
        public bool HasChanged => _budget != workspace.Project_Budget;
        public bool RequiresTerraformUpdate { get; private set; }
        public void ResetBudget()
        {
            _budget = workspace.Project_Budget ?? 0;
            RequiresTerraformUpdate = false;
        }
        public void TerraformUpdated() => RequiresTerraformUpdate = false;
    }
}
