﻿using Microsoft.EntityFrameworkCore;

namespace Datahub.Portal.Data.WebAnalytics
{
    public class WebAnalyticsContext : DbContext
    {
        public WebAnalyticsContext(DbContextOptions<WebAnalyticsContext> options) : base(options)
        { }

        public DbSet<WebAnalytics> WebAnalytics { get; set; }
    }
}
