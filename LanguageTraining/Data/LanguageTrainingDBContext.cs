using Microsoft.EntityFrameworkCore;

namespace Datahub.Portal.Data.LanguageTraining
{
    public class LanguageTrainingDBContext : DbContext
    {
        public LanguageTrainingDBContext(DbContextOptions<LanguageTrainingDBContext> options) : base(options)
        { 
        }

        public DbSet<LanguageTrainingApplication> LanguageTrainingApplications { get; set; } = null!;
    }
}
