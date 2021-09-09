using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.Portal.Data.LanguageTraining
{
    public class LanguageTrainingDBContext : DbContext
    {
        public LanguageTrainingDBContext(DbContextOptions<LanguageTrainingDBContext> options) : base(options)
        { }

        public DbSet<LanguageTrainingApplication> LanguageTrainingApplications { get; set; }
    }
}
