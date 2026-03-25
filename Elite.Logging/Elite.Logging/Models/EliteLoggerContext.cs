using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Elite.Logging.Models
{
    public partial class EliteLoggerContext : DbContext
    {

        public EliteLoggerContext()
        {
        }

        public EliteLoggerContext(DbContextOptions<EliteLoggerContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Logs> Logs { get; set; }       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("elite");
            modelBuilder.Entity<Logs>(entity =>
            {
				entity.HasKey(e => e.LogId);
				


				entity.Property(e => e.LogId).HasDefaultValueSql("nextval('agendasattachment_seq'::regclass)");
				entity.Property(e => e.LogDescription).HasColumnType("varchar");
				entity.Property(e => e.LogUserId).HasColumnType("varchar");
				entity.Property(e => e.LogDateTime).HasColumnType("timestamp");
				entity.Property(e => e.LogServiceName).HasColumnType("varchar");
			});

            modelBuilder.HasSequence("Log_LogId_seq");
        }
    }
}
