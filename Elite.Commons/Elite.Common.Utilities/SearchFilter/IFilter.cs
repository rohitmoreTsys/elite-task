
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Elite.Common.Utilities.SearchFilter
{
    public interface IFilter<T> where T : class
    {      
        Expression<Func<T, bool>> Predicate { get; set; }
    }
}
