using Datahub.Application.Services.ReverseProxy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Datahub.Infrastructure.Services.ReverseProxy
{
    public class WorkspaceACLTransformFactory : ITransformFactory
    {

        private readonly ILogger logger;

        public WorkspaceACLTransformFactory(ILogger<WorkspaceACLTransformFactory> logger)
        {
            this.logger = logger;
        }

        public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
        {
            if (transformValues.TryGetValue(IReverseProxyConfigService.WorkspaceACLTransform, out var workspaceAcronym))
            {
                context.RequestTransforms.Add(new ContextRequestHeaderTransform(workspaceAcronym,logger));                   
            }
            else
            { 
                return false;
            }
            return true;
        }

        public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues)
        {
            if (transformValues.TryGetValue(IReverseProxyConfigService.WorkspaceACLTransform, out var workspaceAcronym))
            {
                if (string.IsNullOrEmpty(workspaceAcronym))
                {
                    context.Errors.Add(new ArgumentException($"Invalid workspace acronym"));
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
