using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Datahub.Infrastructure.Services.ReverseProxy
{
    public static class URLTranslationTransform
    {
        public static RouteConfig WithTransformTranslateURLs(this RouteConfig route, string prefixToAdd)
        {
            if (string.IsNullOrEmpty(prefixToAdd))
            {
                return route;
            }
            
            
            return route;
        }
    }
}
