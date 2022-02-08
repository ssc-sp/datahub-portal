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
}
