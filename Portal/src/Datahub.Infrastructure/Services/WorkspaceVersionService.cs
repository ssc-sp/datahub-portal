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
        ILogger<WorkspaceCreationService> logger)
    {
        public async Task<string> GetLatestVersion()
        {
            await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
            return db.VersionTags.OrderByDescending(v => v.VersionTagId).First().Tag ?? "latest";
        }
    }
}
