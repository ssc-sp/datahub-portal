using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Application.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ExternalUserPageAttribute : Attribute
    {
        public bool IsAllowed { get; }

        public ExternalUserPageAttribute(bool isAllowed = true)
        {
            IsAllowed = isAllowed;
        }
    }
}
