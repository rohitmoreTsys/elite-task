using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace Elite.Filters.Lib.FiltersEF
{
    public partial class FiltereContext : DbContext
    {
        public FiltereContext(DbContextOptions<FiltereContext> options) : base(options)
        {
        }

        public DbSet<EliteFilters> EliteFilters { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("elite");
            builder.Entity<EliteFilters>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("nextval('elitefilters_seq'::regclass)");

                entity.Property(e => e.CreatedBy).IsRequired();

                entity.Property(e => e.FilterJson)
                    .IsRequired()
                    .HasColumnType("json");

                entity.Property(e => e.IsActive).HasDefaultValueSql("true");

                entity.Property(e => e.Uid)
                    .IsRequired()
                    .HasColumnName("UID");
                    
            });

            builder.HasSequence("elitefilters_seq");
        }
    }
}
