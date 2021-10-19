using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data.WebAnalytics
{
    public class WebAnalyticsContext : DbContext
    {
        public WebAnalyticsContext(DbContextOptions<WebAnalyticsContext> options) : base(options)
        { }

        public DbSet<WebAnalytics> WebAnalytics { get; set; }
    }
}
