using System;
using System.Threading;
using System.Threading.Tasks;
using Elite.Auth.Token.Lib.Entities;
using Elite.Auth.Token.Lib.Models.Entities;
using Elite.Common.Utilities.SecretVault;
using Elite.Auth.Token.Lib.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Elite.Auth.Token.Lib.Models
{
    public partial class EliteAuthTokenContext : DbContext, IUnitOfWork
    {
        public virtual DbSet<EliteAuthToken> EliteAuthToken { get; set; }
        public virtual DbSet<TrackRequestSessions> TrackRequestSessions { get; set; }
        public virtual DbSet<LegalPropertyContent> LegalPropertyContent { get; set; }
        public EliteAuthTokenContext()
        {

        }
        public EliteAuthTokenContext(DbContextOptions<EliteAuthTokenContext> options)
          : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("elite");
            modelBuilder.Entity<EliteAuthToken>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AccessToken).IsRequired();

                entity.Property(e => e.Idtoken).HasColumnName("IDToken");

                entity.Property(e => e.IsRefreshTokenGenerated).HasDefaultValueSql("false");

                entity.Property(e => e.RefreshToken).IsRequired();

                entity.Property(e => e.Uid).IsRequired();

                
            });

            modelBuilder.Entity<TrackRequestSessions>(entity =>
            {
                entity.HasKey(e => e.RequestId);

                entity.Property(e => e.RequestId).ValueGeneratedNever();
            });
            modelBuilder.Entity<LegalPropertyContent>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("Id");

                entity.Property(e => e.BundleId).IsRequired();

                entity.Property(e => e.Version).HasColumnName("Version");

            });
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken)
        {
            return Convert.ToBoolean(await base.SaveChangesAsync());
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var result = await base.SaveChangesAsync();
            return result;
        }


    }
}
