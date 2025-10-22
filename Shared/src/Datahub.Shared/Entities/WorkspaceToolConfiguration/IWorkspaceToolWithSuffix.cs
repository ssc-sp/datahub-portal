using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration;

public interface IWorkspaceToolWithSuffix
{
    string ResourceNameSuffix { get; set; }
}
