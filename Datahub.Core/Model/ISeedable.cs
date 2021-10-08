using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Core.EFCore
{
    public interface ISeedable<T>
    {
        public void Seed(T context, IConfiguration configuration);
    }
}
