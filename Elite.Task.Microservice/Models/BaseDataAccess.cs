using Elite.Common.Utilities.SecretVault;
using Elite_Task.Microservice.Core;
using Elite_Task.Microservice.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Elite_Task.Microservice.Models
{
    public abstract class BaseDataAccess : IBaseDataAccess
    {
        protected EliteTaskContext _context;

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }

        }

        protected void SetDbContext(IConfiguration configuration)
        {
            var secretVault = SecretVault.Instance;

            var optionsBuilder = new DbContextOptionsBuilder<EliteTaskContext>();
            optionsBuilder.UseNpgsql(secretVault.GetValuesFromVault("eliteTaskConnectionString"));
            _context = new EliteTaskContext(optionsBuilder.Options);
        }

        public BaseDataAccess(EliteTaskContext context)
        {
            _context = context;
        }
        public DbSet<TEntity> GetDBSet<TEntity>() where TEntity : class, IEntity
        {
            return this._context.Set<TEntity>();
        }


    }
}
