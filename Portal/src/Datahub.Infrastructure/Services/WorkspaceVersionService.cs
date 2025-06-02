using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using MassTransit;
using MassTransit.Transports;
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
                                .Where(t => t.IsActive)
                                .Select(t => t.Tag)                                
                                .ToListAsync();
         
            var latest = versionTags
                 .Select(v => Version.Parse(v.TrimStart('v')))
                 .OrderByDescending(v => v)
                 .First();
            var latestStr = $"v{latest.ToString()}";

            return latestStr;
        }

        public async Task<List<VersionTag>> GetAllVersionsAsync()
        {
            await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
            var versionTags = await db.VersionTags
                .ToListAsync();

            var orderedVersionTags = versionTags
                .OrderByDescending(v => Version.Parse(v.Tag.TrimStart('v')))
                .ToList();

            return orderedVersionTags;
        }


        public async Task<bool> AddNewVersion(VersionTag versionTag)
        {
            await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
            await db.VersionTags.AddAsync(versionTag);
            var isSaved = await db.SaveChangesAsync();
            return isSaved > 0;
        }

        public async Task<bool> UpdateVersionTag(VersionTag versionTag)
        {
            await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
            db.VersionTags.Update(versionTag);
            var isSaved = await db.SaveChangesAsync();
            return isSaved > 0;
        }

        public async Task<bool> DeleteVersion(VersionTag versionTag)
        {
            await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
            db.VersionTags.Remove(versionTag);
            var isDeleted = await db.SaveChangesAsync();
            return isDeleted > 0;
        }
        
    }
}
