using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Microsoft.PowerBI.Api.Models;

namespace Datahub.PowerBI.Data;

public class PowerBiManagementConstants
{
    public const string POWERBIMANAGEMENTLOCALIZATIONROOTKEY = "POWER_BI_MANAGEMENT";
    public const string SANDBOXWORKSPACESUFFIX = "[Development]";

    public const string TITLEENMETADATAFIELD = "title_translated_en";
    public const string TITLEFRMETADATAFIELD = "title_translated_fr";
    public const string CONTACTINFOMETADATAFIELD = "contact_information";
}

public class PowerBiAdminDatasetTreeItem
{
    public PowerBiAdminDatasetTreeItem(Dataset pbiDataset, PowerBiDataSet dbDataset, Guid? workspaceId)
    {
        _dbDataset = dbDataset;
        _pbiDataset = pbiDataset;

        if (_pbiDataset != null)
        {
            _datasetId = Guid.Parse(pbiDataset.Id);
        }
        else
        {
            _datasetId = dbDataset.DataSetID;
        }

        _pbiDatasetName = pbiDataset?.Name;
        _dbDatasetName = dbDataset?.DataSetName;
        _pbiWorkspaceId = workspaceId;
        _dbWorkspaceId = dbDataset?.WorkspaceId;
    }

    private Guid _datasetId;
    private string _pbiDatasetName;
    private string _dbDatasetName;
    private Guid? _pbiWorkspaceId;
    private Guid? _dbWorkspaceId;

    private int? _projectId;

    private readonly Dataset _pbiDataset;
    private readonly PowerBiDataSet _dbDataset;

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

    public int ProjectId
    {
        get => _projectId ?? -1;
        set => _projectId = (value < 0) ? null : value;
    }

    public bool IsLinkedToProject => _projectId != null;

    public Guid AnyWorkspaceId => _pbiWorkspaceId ?? _dbWorkspaceId ?? Guid.Empty;
    public PowerBiDataSetDefinition Definition => new(_datasetId, _pbiDatasetName ?? _dbDatasetName, AnyWorkspaceId);
    public PowerBiAdminTreeItem ManagementTreeItem => new(DbDatasetName, PowerBiAdminTreeItemType.Dataset, DatasetId, _projectId);
}

public class PowerBiAdminReportTreeItem
{
    public PowerBiAdminReportTreeItem(Report pbiReport, PowerBiReport dbReport, Guid? workspaceId)
    {
        _pbiReport = pbiReport;
        _dbReport = dbReport;

        _reportId = pbiReport?.Id ?? dbReport?.ReportID ?? Guid.Empty;
        _pbiReportName = pbiReport?.Name;
        _dbReportName = dbReport?.ReportName;
        _pbiWorkspaceId = workspaceId;
        _dbWorkspaceId = dbReport?.WorkspaceId;
    }

    private Guid _reportId;
    private string _pbiReportName;
    private string _dbReportName;
    private Guid? _pbiWorkspaceId;
    private Guid? _dbWorkspaceId;

    private int? _projectId;

    private readonly Report _pbiReport;
    private readonly PowerBiReport _dbReport;

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

    public int ProjectId
    {
        get => _projectId ?? -1;
        set => _projectId = (value < 0) ? null : value;
    }

    public bool IsLinkedToProject => _projectId != null;

    public Guid? DatasetId
    {
        get
        {
            if (Guid.TryParse(_pbiReport?.DatasetId, out var datasetId))
            {
                return datasetId;
            }
            else
            {
                return null;
            }
        }
    }

    public Guid AnyWorkspaceId => _pbiWorkspaceId ?? _dbWorkspaceId ?? Guid.Empty;
    public PowerBiReportDefinition Definition => new(_reportId, _pbiReportName ?? _dbReportName, AnyWorkspaceId);
    public PowerBiAdminTreeItem ManagementTreeItem => new(DbReportName, PowerBiAdminTreeItemType.Report, ReportId, _projectId);

    public bool InCatalog { get; set; }
    public List<PowerBiReport> SiblingReports { get; set; } = new();
}

public class PowerBiAdminWorkspaceTreeItem
{
    public PowerBiAdminWorkspaceTreeItem(Group pbiWorkspace, PowerBiWorkspace dbWorkspace)
    {
        _dbWorkspace = dbWorkspace;
        _pbiWorkspace = pbiWorkspace;

        // should never be empty, since at least one workspace will be provided
        _workspaceId = pbiWorkspace?.Id ?? dbWorkspace?.WorkspaceID ?? Guid.Empty;
        _dbWorkspaceName = dbWorkspace?.WorkspaceName;
        _pbiWorkspaceName = pbiWorkspace?.Name;
        RevertProjectAssignment();
    }

    private Guid _workspaceId;
    private string _dbWorkspaceName;
    private string _pbiWorkspaceName;
    private bool _sandboxFlag;
    private int? _projectId;

    private readonly Group _pbiWorkspace;
    private readonly PowerBiWorkspace _dbWorkspace;

    private List<PowerBiAdminDatasetTreeItem> _datasets = new();
    private List<PowerBiAdminReportTreeItem> _reports = new();


    public bool IsInPowerBi => _pbiWorkspace != null;
    public bool IsInDb => _dbWorkspace != null;
    public bool NeedsUpdate => !IsInDb || ChildrenNeedUpdate || _nameWasChanged;
    public bool ChildrenNeedUpdate => Datasets.Any(d => d.NeedsUpdate) || Reports.Any(r => r.NeedsUpdate);
    private bool _nameWasChanged => PbiWorkspaceName != null && DbWorkspaceName != null && PbiWorkspaceName != DbWorkspaceName;

    public bool ProjectAssignmentChanged => _dbWorkspace == null || _dbWorkspace.SandboxFlag != _sandboxFlag || _dbWorkspace.ProjectId != _projectId;

    public void RevertProjectAssignment()
    {
        _sandboxFlag = _dbWorkspace?.SandboxFlag ?? false;
        _projectId = _dbWorkspace?.ProjectId;
    }

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

    public PowerBiWorkspaceDefinition Definition => new(_workspaceId, _pbiWorkspaceName ?? _dbWorkspaceName, _sandboxFlag, _projectId);
    public PowerBiAdminTreeItem ManagementTreeItem => new(DbWorkspaceName, PowerBiAdminTreeItemType.Workspace, WorkspaceId, _projectId);
}


public enum PowerBiAdminTreeItemType
{
    Project,
    Workspace,
    Container,
    Dataset,
    Report
}

public class PowerBiAdminTreeItem
{
    public PowerBiAdminTreeItem(string label, PowerBiAdminTreeItemType itemType, Guid id = default, int? projectId = null)
    {
        Label = label;
        ItemType = itemType;
        Children = new();
        Id = id;
        _projectId = projectId;
    }

    private static readonly Dictionary<PowerBiAdminTreeItemType, string> ICON_MAP = new()
    {
        { PowerBiAdminTreeItemType.Project, "project-diagram" },
        { PowerBiAdminTreeItemType.Workspace, "folder-open" },
        { PowerBiAdminTreeItemType.Dataset, "table" },
        { PowerBiAdminTreeItemType.Report, "chart-bar" },
        { PowerBiAdminTreeItemType.Container, "inbox" }
    };

    public Guid Id { get; private set; }
    public string Label { get; private set; }
    public PowerBiAdminTreeItemType ItemType { get; private set; }
    public List<PowerBiAdminTreeItem> Children { get; private set; }
    public void AddChild(PowerBiAdminTreeItem item) => Children.Add(item);
    public void AddChildren(IEnumerable<PowerBiAdminTreeItem> items) => Children.AddRange(items);
    public bool HasChildren => Children.Count > 0;
    private int? _projectId;
    public int ProjectId
    {
        get => _projectId ?? -1;
        set => _projectId = (value < 0) ? null : value;
    }
    public string Icon => ICON_MAP[ItemType];
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

    public override string ToString()
    {
        return $"{UserEmail}{(IsAdmin ? " (admin)" : string.Empty)}";
    }
}

public class PowerBiAdminWorkspaceName
{
    public PowerBiAdminWorkspaceName(DatahubProject project)
    {
        _originalBranch = project.BranchName;
        _originalName = project.ProjectName;

        ProjectAcronym = project.ProjectAcronymCD;
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
    public string SandboxName => $"{ProductionName} {PowerBiManagementConstants.SANDBOXWORKSPACESUFFIX}";
}