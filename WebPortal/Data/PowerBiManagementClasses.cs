using Datahub.Core.EFCore;
using Microsoft.PowerBI.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Portal.Data
{
    public class PowerBiManagementConstants
    {
        public static readonly string POWERBI_MANAGEMENT_LOCALIZATION_ROOT_KEY = "POWER_BI_MANAGEMENT";
        public readonly static string SANDBOX_WORKSPACE_SUFFIX = "[Development]";
    }

    public class PowerBiAdminDatasetTreeItem
    {
        public PowerBiAdminDatasetTreeItem(Dataset pbiDataset, PowerBi_DataSet dbDataset, Guid? workspaceId)
        {
            _dbDataset = dbDataset;
            _pbiDataset = pbiDataset;

            if (_pbiDataset != null)
            {
                _datasetId = Guid.Parse(pbiDataset.Id);
            }
            else
            {
                _datasetId = dbDataset.DataSet_ID;
            }

            _pbiDatasetName = pbiDataset?.Name;
            _dbDatasetName = dbDataset?.DataSet_Name;
            _pbiWorkspaceId = workspaceId;
            _dbWorkspaceId = dbDataset?.Workspace_Id;
        }

        private Guid _datasetId;
        private string _pbiDatasetName;
        private string _dbDatasetName;
        private Guid? _pbiWorkspaceId;
        private Guid? _dbWorkspaceId;

        private readonly Dataset _pbiDataset;
        private readonly PowerBi_DataSet _dbDataset;

        public bool IsInPowerBi => _pbiDataset != null;
        public bool IsInDb => _dbDataset != null;
        public bool NeedsUpdate => !IsInDb || _nameWasChanged;
        private bool _nameWasChanged => PbiDatasetName != null && DbDatasetName != null && PbiDatasetName != DbDatasetName;

        public Guid DatasetId
        {
            get => _datasetId;
            set => _datasetId = value;
        }

        public string PbiDatasetName
        {
            get => _pbiDatasetName;
            set => _pbiDatasetName = value;
        }

        public string DbDatasetName
        {
            get => _dbDatasetName;
            set => _dbDatasetName = value;
        }

        public Guid? DbWorkspaceId
        {
            get => _dbWorkspaceId;
            set => _dbWorkspaceId = value;
        }

        public Guid? PbiWorkspaceId
        {
            get => _pbiWorkspaceId;
            set => _pbiWorkspaceId = value;
        }

        public Guid AnyWorkspaceId => _pbiWorkspaceId ?? _dbWorkspaceId ?? Guid.Empty;
        public PowerBi_DataSetDefinition Definition => new(_datasetId, _pbiDatasetName ?? _dbDatasetName, AnyWorkspaceId);
        public PowerBiAdminTreeItem ManagementTreeItem => new(DbDatasetName, PowerBiAdminTreeItemType.Dataset, DatasetId);
    }

    public class PowerBiAdminReportTreeItem
    {
        public PowerBiAdminReportTreeItem(Report pbiReport, PowerBi_Report dbReport, Guid? workspaceId)
        {
            _pbiReport = pbiReport;
            _dbReport = dbReport;

            _reportId = pbiReport?.Id ?? dbReport?.Report_ID ?? Guid.Empty;
            _pbiReportName = pbiReport?.Name;
            _dbReportName = dbReport?.Report_Name;
            _pbiWorkspaceId = workspaceId;
            _dbWorkspaceId = dbReport?.Workspace_Id;
        }

        private Guid _reportId;
        private string _pbiReportName;
        private string _dbReportName;
        private Guid? _pbiWorkspaceId;
        private Guid? _dbWorkspaceId;

        private readonly Report _pbiReport;
        private readonly PowerBi_Report _dbReport;

        public bool IsInPowerBi => _pbiReport != null;
        public bool IsInDb => _dbReport != null;
        public bool NeedsUpdate => !IsInDb || _nameWasChanged;
        private bool _nameWasChanged => PbiReportName != null && DbReportName != null && PbiReportName != DbReportName;

        public Guid ReportId
        {
            get => _reportId;
            set => _reportId = value;
        }

        public string PbiReportName
        {
            get => _pbiReportName;
            set => _pbiReportName = value;
        }

        public string DbReportName
        {
            get => _dbReportName;
            set => _dbReportName = value;
        }

        public Guid? PbiWorkspaceId
        {
            get => _pbiWorkspaceId;
            set => _pbiWorkspaceId = value;
        }

        public Guid? DbWorkspaceId
        {
            get => _dbWorkspaceId;
            set => _dbWorkspaceId = value;
        }

        public Guid AnyWorkspaceId => _pbiWorkspaceId ??_dbWorkspaceId ?? Guid.Empty;
        public PowerBi_ReportDefinition Definition => new(_reportId, _pbiReportName ?? _dbReportName, AnyWorkspaceId);
        public PowerBiAdminTreeItem ManagementTreeItem => new(DbReportName, PowerBiAdminTreeItemType.Report, ReportId);
    }

    public class PowerBiAdminWorkspaceTreeItem
    {
        public PowerBiAdminWorkspaceTreeItem(Group pbiWorkspace, PowerBi_Workspace dbWorkspace)
        {
            _dbWorkspace = dbWorkspace;
            _pbiWorkspace = pbiWorkspace;

            // should never be empty, since at least one workspace will be provided
            _workspaceId = pbiWorkspace?.Id ?? dbWorkspace?.Workspace_ID ?? Guid.Empty;
            _dbWorkspaceName = dbWorkspace?.Workspace_Name;
            _pbiWorkspaceName = pbiWorkspace?.Name;
            _sandboxFlag = dbWorkspace?.Sandbox_Flag ?? false;
            _projectId = dbWorkspace?.Project_Id;
        }

        private Guid _workspaceId;
        private string _dbWorkspaceName;
        private string _pbiWorkspaceName;
        private bool _sandboxFlag;
        private int? _projectId;

        private readonly Group _pbiWorkspace;
        private readonly PowerBi_Workspace _dbWorkspace;

        private List<PowerBiAdminDatasetTreeItem> _datasets = new();
        private List<PowerBiAdminReportTreeItem> _reports = new();


        public bool IsInPowerBi => _pbiWorkspace != null;
        public bool IsInDb => _dbWorkspace != null;
        public bool NeedsUpdate => !IsInDb || ChildrenNeedUpdate || _nameWasChanged;
        public bool ChildrenNeedUpdate => Datasets.Any(d => d.NeedsUpdate) || Reports.Any(r => r.NeedsUpdate);
        private bool _nameWasChanged => PbiWorkspaceName != null && DbWorkspaceName != null && PbiWorkspaceName != DbWorkspaceName;

        public Guid WorkspaceId
        {
            get => _workspaceId;
            set => _workspaceId = value;
        }

        public string DbWorkspaceName
        {
            get => _dbWorkspaceName;
            set => _dbWorkspaceName = value;
        }

        public string PbiWorkspaceName
        {
            get => _pbiWorkspaceName; 
            set => _pbiWorkspaceName = value;
        }

        public bool SandboxFlag
        {
            get => _sandboxFlag;
            set => _sandboxFlag = value;
        }

        public int ProjectId
        {
            get => _projectId ?? -1;
            set => _projectId = (value < 0) ? null : value;
        }

        public List<PowerBiAdminDatasetTreeItem> Datasets
        {
            get => _datasets;
            private set => _datasets = value;
        }

        public List<PowerBiAdminReportTreeItem> Reports
        {
            get => _reports;
            private set => _reports = value;
        }

        public PowerBi_WorkspaceDefinition Definition => new(_workspaceId, _pbiWorkspaceName ?? _dbWorkspaceName, _sandboxFlag, _projectId);
        public PowerBiAdminTreeItem ManagementTreeItem => new(DbWorkspaceName, PowerBiAdminTreeItemType.Workspace, WorkspaceId);
    }


    public enum PowerBiAdminTreeItemType
    {
        Project,
        Workspace,
        Container,
        Dataset,
        Report
    }

    /*
     *  root
     *      project
     *          workspace
     *              container (datasets)
     *                  dataset
     *              container (reports)
     *                  report
     * */

    public class PowerBiAdminTreeItem
    {
        public PowerBiAdminTreeItem(string label, PowerBiAdminTreeItemType itemType, Guid id = default)
        {
            Label = label;
            ItemType = itemType;
            Children = new();
            Id = id;
        }

        public Guid Id { get; private set; }
        public string Label { get; private set; }
        public PowerBiAdminTreeItemType ItemType { get; private set; }
        public List<PowerBiAdminTreeItem> Children { get; private set; }
        public void AddChild(PowerBiAdminTreeItem item) => Children.Add(item);
        public void AddChildren(IEnumerable<PowerBiAdminTreeItem> items) => Children.AddRange(items);
        public bool HasChildren => Children.Count > 0;
    }

    public class PowerBiAdminGroupUser
    {
        public PowerBiAdminGroupUser(string userEmail, bool isAdmin)
        {
            UserEmail = userEmail;
            IsAdmin = isAdmin;
        }
        public string UserEmail { get; private set; }
        public bool IsAdmin { get; private set; }
    }

    public class PowerBiAdminWorkspaceName
    {
        public PowerBiAdminWorkspaceName(Datahub_Project project)
        {
            _originalBranch = project.Branch_Name;
            _originalName = project.ProjectName;

            ProjectAcronym = project.Project_Acronym_CD;
            Branch = _originalBranch;
            Name = _originalName;
        }

        

        private readonly string _originalBranch;
        private readonly string _originalName;
        public string Branch { get; set; }
        public string Name { get; set; }
        public string ProjectAcronym { get; set; }
        private bool _missingField => string.IsNullOrWhiteSpace(Branch) || string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(ProjectAcronym);
        public bool HasConflict { get; set; }
        public bool HasProblem => _missingField || HasConflict;
        public bool IsChanged => _originalName != Name || _originalBranch != Branch;
        public void Revert()
        {
            Name = _originalName;
            Branch = _originalBranch;
        }
        public string ProductionName => $"{Branch} - {ProjectAcronym} - {Name}";
        public string SandboxName => $"{ProductionName} {PowerBiManagementConstants.SANDBOX_WORKSPACE_SUFFIX}";
    }
}
