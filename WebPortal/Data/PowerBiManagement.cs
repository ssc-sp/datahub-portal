using Datahub.Core.EFCore;
using Microsoft.PowerBI.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Portal.Data
{
    public class PowerBiAdminWorkspaceItem
    {
        public PowerBiAdminWorkspaceItem(Group pbiWorkspace, PowerBi_Workspace dbWorkspace)
        {
            _workspaceId = pbiWorkspace.Id;
            _workspaceName = pbiWorkspace.Name;
            WorkspaceFromDb = dbWorkspace;
            _sandboxFlag = dbWorkspace?.Sandbox_Flag ?? false;
            _projectId = dbWorkspace?.Project_Id;
            _originalDefinition = Definition;
        }

        private Guid _workspaceId;
        private string _workspaceName;
        private bool _sandboxFlag;
        private int? _projectId;

        private bool _isChanged = false;
        private readonly PowerBi_WorkspaceDefinition _originalDefinition;

        public Guid WorkspaceId
        {
            get { return _workspaceId; }
            set
            {
                _workspaceId = value;
                _isChanged = true;
            }
        }

        public string WorkspaceName
        {
            get { return _workspaceName; }
            set
            {
                _workspaceName = value;
                _isChanged = true;
            }
        }

        public bool SandboxFlag
        {
            get { return _sandboxFlag; }
            set
            {
                _sandboxFlag = value;
                _isChanged = true;
            }
        }

        public int ProjectId
        {
            get { return _projectId ?? -1; }
            set
            {
                _projectId = (value < 0) ? null : value;
                _isChanged = true;
            }
        }

        public bool IsChanged
        {
            get { return _isChanged; }
            set
            {
                _isChanged = value;
                if (!_isChanged)
                {
                    _workspaceId = _originalDefinition.WorkspaceId;
                    _workspaceName = _originalDefinition.WorkspaceName;
                    _projectId = _originalDefinition.ProjectId;
                    _sandboxFlag = _originalDefinition.SandboxFlag;
                }
            }
        }
        
        public PowerBi_Workspace WorkspaceFromDb { get; set; }
        public bool IsAlreadyCatalogued => WorkspaceFromDb != null;
        public string ProjectName => WorkspaceFromDb?.Project?.ProjectName;
        public PowerBi_WorkspaceDefinition Definition => new(_workspaceId, _workspaceName, _sandboxFlag, _projectId);
    }

    public class PowerBiAdminDataSetItem
    {
        public PowerBiAdminDataSetItem(Dataset pbiDataset, Guid workspaceId, PowerBi_DataSet dbDataset)
        {
            _datasetId = Guid.Parse(pbiDataset.Id);
            _workspaceId = workspaceId;
            _datasetName = pbiDataset.Name;

            DatasetFromDb = dbDataset;

            _originalDefinition = Definition;
        }

        private Guid _datasetId;
        private Guid _workspaceId;
        private string _datasetName;

        private bool _isChanged = false;
        private readonly PowerBi_DataSetDefinition _originalDefinition;

        public Guid DatasetId
        {
            get => _datasetId;
            set
            {
                _datasetId = value;
                _isChanged = true;
            }
        }

        public Guid WorkspaceId
        {
            get => _workspaceId;
            set
            {
                _workspaceId = value;
                _isChanged = true;
            }
        }

        public string DatasetName
        {
            get => _datasetName;
            set
            {
                _datasetName = value;
                _isChanged = true;
            }
        }

        public bool IsChanged
        {
            get => _isChanged;
            set
            {
                _isChanged = value;
                if (!_isChanged)
                {
                    _datasetId = _originalDefinition.DataSetId;
                    _workspaceId = _originalDefinition.WorkspaceId;
                    _datasetName = _originalDefinition.DataSetName;
                }
            }
        }

        public PowerBi_DataSet DatasetFromDb { get; set; }
        public bool IsAlreadyCatalogued => DatasetFromDb != null;
        public PowerBi_DataSetDefinition Definition => new(_datasetId, _datasetName, _workspaceId);
    }

    public class PowerBiAdminReportItem
    {
        public PowerBiAdminReportItem(Report report, Guid workspaceId, PowerBi_Report dbReport)
        {
            _reportId = report.Id;
            _workspaceId = workspaceId;
            _reportName = report.Name;

            ReportFromDb = dbReport;

            _originalDefinition = Definition;
        }

        private Guid _reportId;
        private string _reportName;
        private Guid _workspaceId;

        private bool _isChanged;
        private readonly PowerBi_ReportDefinition _originalDefinition;

        public Guid ReportId
        {
            get => _reportId;
            set
            {
                _reportId = value;
                _isChanged = true;
            }
        }

        public string ReportName
        {
            get => _reportName;
            set
            {
                _reportName = value;
                _isChanged = true;
            }
        }

        public Guid WorkspaceId
        {
            get => _workspaceId;
            set
            {
                _workspaceId = value;
                _isChanged = true;
            }
        }

        public bool IsChanged
        {
            get => _isChanged;
            set
            {
                _isChanged = value;
                if (!_isChanged)
                {
                    _workspaceId = _originalDefinition.WorkspaceId;
                    _reportId = _originalDefinition.ReportId;
                    _reportName = _originalDefinition.ReportName;
                }
            }
        }

        public PowerBi_Report ReportFromDb { get; set; }
        public bool IsAlreadyCatalogued => ReportFromDb != null;
        public PowerBi_ReportDefinition Definition => new(_reportId, _reportName, _workspaceId);
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
        public bool NeedsUpdate => !IsInDb; //TODO

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
        public bool NeedsUpdate => !IsInDb; //TODO

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
        public bool NeedsUpdate => !IsInDb || Datasets.Any(d => d.NeedsUpdate) || Reports.Any(r => r.NeedsUpdate); //TODO

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
    }
}
