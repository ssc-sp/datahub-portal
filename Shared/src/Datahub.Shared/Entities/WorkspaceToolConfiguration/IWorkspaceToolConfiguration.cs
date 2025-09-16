using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration;

public interface IWorkspaceToolConfiguration
{
    IWorkspaceToolConfiguration Clone();
}
