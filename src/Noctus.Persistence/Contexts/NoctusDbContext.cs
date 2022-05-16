using Microsoft.EntityFrameworkCore;
using Noctus.Domain.Entities;

namespace Noctus.Persistence.Contexts
{
    public class NoctusDbContext : DbContext
    {
        public DbSet<LicenseKey> LicenseKeys { get; set; }
        public DbSet<LicenseKeyFlag> LicenseKeyFlags { get; set; }
        public DbSet<LicenseKeyEvent> LicenseKeyEvents { get; set; }
        public DbSet<IdentifiersInfo> IdentifiersInfo { get; set; }
        public DbSet<PipelineRun> PipelineRuns { get; set; }
        public DbSet<PipelineEvent> PipelineEvents { get; set; }
        public DbSet<GenBucket> AccountsGenBuckets { get; set; }
        public DbSet<GenBucketConfig> GenBucketConfigs { get; set; }

        public NoctusDbContext(DbContextOptions<NoctusDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
