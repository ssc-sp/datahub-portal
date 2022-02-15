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
}
