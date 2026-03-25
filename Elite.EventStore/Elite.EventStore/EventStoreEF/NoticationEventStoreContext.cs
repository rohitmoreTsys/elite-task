using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.EventBus.EventStoreEF
{
    public partial class NoticationEventStoreContext : DbContext
    {
        public NoticationEventStoreContext(DbContextOptions<NoticationEventStoreContext> options) : base(options)
        {
        }

        public DbSet<NoticationEventStore> NoticationEventStore { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("elite");
            builder.Entity<NoticationEventStore>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.IsFailed).HasColumnName("isFailed");

                entity.Property(e => e.IsProcessed).HasColumnName("isProcessed");

                entity.Property(e => e.JsonMessage)
                    .IsRequired()
                    .HasColumnType("json");

                entity.Property(e => e.Sourcetypeid).HasColumnName("sourcetypeid");
            });

            builder.HasSequence("NoticationEventStore_ID_seq");
        }
    }
}
