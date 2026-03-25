
using Elite.Auth.Token.Lib.Repository;
using Elite.Common.Utilities.SecretVault;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Auth.Token.Lib.Models
{
    public abstract class BaseDataAccess : IBaseDataAccess
    {
        protected EliteAuthTokenContext _context;

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }

        }

        protected void SetDbContext(IConfiguration configuration)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EliteAuthTokenContext>();
            optionsBuilder.UseNpgsql(SecretVault.Instance.GetValuesFromVault("eliteUsersConnectionString"));
            _context = new EliteAuthTokenContext(optionsBuilder.Options);
        }

        public BaseDataAccess(EliteAuthTokenContext context)
        {
            _context = context;
        }
        public DbSet<TEntity> GetDBSet<TEntity>() where TEntity : class, IEntity
        {
            return this._context.Set<TEntity>();
        }
    }
}
