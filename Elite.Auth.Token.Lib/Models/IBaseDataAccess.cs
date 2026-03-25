using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Auth.Token.Lib.Models
{
    public interface IBaseDataAccess
    {
        DbSet<TEntity> GetDBSet<TEntity>() where TEntity : class, IEntity;
    }
}
