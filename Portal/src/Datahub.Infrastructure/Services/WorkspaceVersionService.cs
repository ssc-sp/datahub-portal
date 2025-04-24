using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.Services
{
    public class WorkspaceVersionService(
        IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        ILogger<WorkspaceCreationService> logger) : IWorkspaceVersionService
    {
        public async Task<string> GetLatestVersion()
        {
            await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
            var versionTags = await db.VersionTags
                                .Select(t => t.Tag)
                                .ToListAsync();
         
            var latest = versionTags
                 .Select(v => Version.Parse(v.TrimStart('v')))
                 .OrderByDescending(v => v)
                 .First();

            return latest.ToString();
        }
    }
}
