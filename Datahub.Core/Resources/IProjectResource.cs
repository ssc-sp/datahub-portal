using Datahub.Core.EFCore;
using Microsoft.AspNetCore.Components;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Resources
{
#nullable enable
    public interface IProjectResource
    {
        (Type type, IDictionary<string, object> parameters)[] GetActiveResources();
        (Type type, IDictionary<string, object> parameters)? GetInactiveResource();

        public string? GetCostEstimatorLink();

        public string[] GetTags();
        Task Initialize(Datahub_Project project, string userId, User graphUser);
    }
}
