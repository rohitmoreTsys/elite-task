using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elite.Common.Utilities.SearchFilter
{
    public interface IOrderByExpression<TEntity> where TEntity : class
    {
        IOrderedQueryable<TEntity> ApplyOrderBy(IQueryable<TEntity> query);
        IOrderedQueryable<TEntity> ApplyThenBy(IOrderedQueryable<TEntity> query);
    }


}
