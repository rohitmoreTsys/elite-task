using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Core
{
    public interface IBaseDataAccess
    {
        DbSet<TEntity> GetDBSet<TEntity>() where TEntity : class, IEntity;
    }
}
